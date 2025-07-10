using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCIRECHeaderProvider))]
	sealed class SCIRECHeaderProviderTest : ImportHeaderProviderAbstractTest<SCIRECHeaderProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("CusEntryHeader null", () => new SCIRECHeaderProvider(null));

				AssertExceptionThrown<ArgumentException>("CusEntryHeader.Declaration null", () => new SCIRECHeaderProvider(Factory.New<CusEntryHeader>()));

				entryHeader.CH_CEI_Instruction = ZGuid.Empty;
				AssertExceptionThrown<ArgumentException>("CusEntryHeader.EntryInstruction null", () => new SCIRECHeaderProvider(entryHeader));
			});
		}

		public void TestLocalClearanceDate_AAV()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2020, 10, 15);
				AssertEquals("CEI_LocalClearanceDate set", new DateTime(2020, 10, 15), Provider.LocalClearanceDate);
			});
		}

		public void TestLocalClearanceDate_NotAAV()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2020, 10, 15);

			AssertNull(Provider.LocalClearanceDate);
		}

		public void TestLocalClearanceProcedure_CEI_StyleIsVAV()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			authorizationUsage.AGC_Number = "111";

			CombineAssertions(() =>
			{
				AssertEquals("AGC_Code is SDE", "111", Provider.LocalClearanceProcedure);

				authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				var newProvider = new SCIRECHeaderProvider(entryHeader) as ISCIRECHeader;

				AssertNull("AGC_Code is invalid", newProvider.LocalClearanceProcedure);
			});
		}

		public void TestLocalClearanceProcedure_CEI_StyleIsAAV()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			authorizationUsage.AGC_Number = "111";

			CombineAssertions(() =>
			{
				AssertEquals("AGC_Code is EIR", "111", Provider.LocalClearanceProcedure);

				authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				var newProvider = new SCIRECHeaderProvider(entryHeader) as ISCIRECHeader;

				AssertNull("AGC_Code is invalid", newProvider.LocalClearanceProcedure);
			});
		}

		public void TestLocalClearanceProcedure_CEI_StyleIsInvalid()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			authorizationUsage.AGC_Number = "111";
			var authorizationUsage2 = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			authorizationUsage2.AGC_Number = "222";

			AssertNull(Provider.LocalClearanceProcedure);
		}

		public void TestProcedureAuthorisation()
		{
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			authorizationUsage.AGC_Number = "111";

			CombineAssertions(() =>
			{
				AssertEquals("AGC_Code is IPO", "111", Provider.ProcedureAuthorisation);

				authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				var newProvider = new SCIRECHeaderProvider(entryHeader) as ISCIRECHeader;

				AssertNull("AGC_Code is invalid", newProvider.ProcedureAuthorisation);
			});
		}

		public void TestArrivalTransportMeansIdentity()
		{
			declaration.ZG_Box18TransportID = "AB123";
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("JE_TransportMode <> 'FIX'", "AB123", Provider.ArrivalTransportMeansIdentity);

				declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;

				AssertNull("JE_TransportMode = 'FIX'", Provider.ArrivalTransportMeansIdentity);
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

		protected override SCIRECHeaderProvider GetProvider() => new SCIRECHeaderProvider(entryHeader);

		new ISCIRECHeader Provider => base.Provider;
	}
}
