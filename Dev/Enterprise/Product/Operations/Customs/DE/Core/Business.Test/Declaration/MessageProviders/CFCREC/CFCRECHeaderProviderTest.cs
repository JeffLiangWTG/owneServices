using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CFCRECHeaderProvider))]
	sealed class CFCRECHeaderProviderTest : ImportHeaderProviderAbstractTest<CFCRECHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CFCRECHeaderProvider(null));
		}

		public void TestTaxOffice_SEL()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._1Self);
			declaration.Declarant.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000666", Constants.CountryCodes.Germany);
			declaration.Representative.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000777", Constants.CountryCodes.Germany);
			AssertEquals("DE000666", Provider.TaxOffice);
		}

		public void TestTaxOffice_DIR()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._2Direct);
			declaration.Declarant.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000666", Constants.CountryCodes.Germany);
			declaration.Representative.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000777", Constants.CountryCodes.Germany);
			AssertEquals("DE000777", Provider.TaxOffice);
		}

		public void TestTaxOffice_IND()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._3Indirect);
			declaration.Declarant.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000666", Constants.CountryCodes.Germany);
			declaration.Representative.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000777", Constants.CountryCodes.Germany);
			AssertEquals("DE000666", Provider.TaxOffice);
		}

		public void TestTaxOffice_DeclarationSenderNull()
		{
			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertNull(Provider.TaxOffice);
		}

		public void TestArrivalTransportMeansIdentity_FIX()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.ZG_Box18TransportID = "AA-BB123";
			AssertNull(Provider.ArrivalTransportMeansIdentity);
		}

		public void TestArrivalTransportMeansIdentity_Other()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.ZG_Box18TransportID = "AA-BB123";
			AssertEquals("AA-BB123", Provider.ArrivalTransportMeansIdentity);
		}

		public void TestAdditionalDutyReferences()
		{
			var fiscalReference1 = entryInstruction.FiscalReferences.AddNew();
			fiscalReference1.CFR_Code = "RF1";
			fiscalReference1.CFR_Reference = "RN001";
			var fiscalReference2 = entryInstruction.FiscalReferences.AddNew();
			fiscalReference2.CFR_Code = "RF2";
			fiscalReference2.CFR_Reference = "RN002";
			AssertEquals(2, Provider.AdditionalDutyReferences.Count);
		}

		public void TestAdditionalDutyReferences_Empty()
		{
			AssertEquals(0, Provider.AdditionalDutyReferences.Count);
		}

		public void TestLocalClearanceDate_NotPopulated()
		{
			entryInstruction.CEI_LocalClearanceDate = ZDateTime.Today;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;
			AssertNull(Provider.LocalClearanceDate);
		}

		public void TestLocalClearanceDate_Populated()
		{
			entryInstruction.CEI_LocalClearanceDate = ZDateTime.BrettsBirthday;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			AssertEquals(ZDateTime.BrettsBirthday, Provider.LocalClearanceDate);
		}

		public void TestLocalClearanceProcedure_CEI_StyleIsVZA()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			authorizationUsage.AGC_Number = "111";

			CombineAssertions(() =>
			{
				AssertEquals("AGC_Code is SDE", "111", Provider.LocalClearanceProcedure);

				authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				var newProvider = new CFCRECHeaderProvider(entryHeader) as ICFCRECHeader;
				AssertNull("AGC_Code is invalid", newProvider.LocalClearanceProcedure);
			});
		}

		public void TestLocalClearanceProcedure_CEI_StyleIsAZ()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			authorizationUsage.AGC_Number = "111";

			CombineAssertions(() =>
			{
				AssertEquals("AGC_Code is EIR", "111", Provider.LocalClearanceProcedure);

				authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				var newProvider = new CFCRECHeaderProvider(entryHeader) as ICFCRECHeader;
				AssertNull("AGC_Code is invalid", newProvider.LocalClearanceProcedure);
			});
		}

		public void TestLocalClearanceProcedure_CEI_StyleIsInvalid()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			authorizationUsage.AGC_Number = "111";
			var authorizationUsage2 = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			authorizationUsage2.AGC_Number = "222";

			AssertNull(Provider.LocalClearanceProcedure);
		}

		public void TestDeclarant_DeclarantUnequalsImporter()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var importerAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			var declarantAddress = GetOrgWithEORNumberAndEORIBranch("EOR2", "EBS1");
			CombineAssertions(() =>
			{
				declaration.JE_OH_Importer = importerAddress.Header.PK;
				declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
				var declarant = Provider.Declarant;
				AssertEquals("Declarant's EORI", "GREOR2", declarant.Identification.EoriNumber);
				AssertSame("Declarant Cached", declarant, Provider.Declarant);
			});
		}

		public void TestDeclarant_DeclarantEqualsImporter()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress = GetOrgWithEORNumberAndEORIBranch("EOR2", "EBS1");
			declaration.JE_OH_Importer = orgAddress.Header.PK;
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			AssertEquals("Decalarant == Importer: declarant's EORI", "GREOR2", Provider.Declarant.Identification.EoriNumber);
		}

		public void TestProcedureAuthorisation()
		{
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EndUse;
			authorizationUsage.AGC_Number = "111";

			CombineAssertions(() =>
			{
				AssertEquals("AGC_Code is EUS", "111", Provider.ProcedureAuthorisation);

				authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				var newProvider = new CFCRECHeaderProvider(entryHeader) as ICFCRECHeader;
				AssertNull("AGC_Code is invalid", newProvider.ProcedureAuthorisation);
			});
		}

		public void TestLines()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryHeader.MergedLines.AddNew().PK;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryHeader.MergedLines.AddNew().PK;
			AssertEquals(2, Provider.Lines.Count);
		}

		protected override CFCRECHeaderProvider GetProvider() => new CFCRECHeaderProvider(entryHeader);

		void SetDeclarantAndRepresentative(ZString declarantType)
		{
			declaration.JE_OA_DeclarantAddress = CreateNewAddress().PK;
			declaration.JE_OA_Representative = CreateNewAddress().PK;
			declaration.JE_OA_BuyingAgentAddress = CreateNewAddress().PK;
			declaration.JE_DeclarantType = declarantType;

			OrgAddress CreateNewAddress()
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var address = orgHeader.Addresses.AddNew();
				address.OA_Address1 = "dummy address";
				return address;
			}
		}

		new ICFCRECHeader Provider => base.Provider;
	}
}
