using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCWRECHeaderProvider))]
	sealed class SCWRECHeaderProviderTest : ImportHeaderProviderAbstractTest<SCWRECHeaderProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("Necessary arguments exist", () => new SCWRECHeaderProvider(entryHeader));
				declaration = Factory.New<JobDeclaration>();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				AssertExceptionThrown<ArgumentException>("EntryInstruction missing", () => new SCWRECHeaderProvider(entryHeader));
			});
		}

		public void TestLocalClearanceDate_AZL()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			entryInstruction.CEI_LocalClearanceDate = new DateTime(2021, 04, 21);
			AssertEquals(new DateTime(2021, 04, 21), Provider.LocalClearanceDate);
		}

		public void TestLocalClearanceDate_NotAZL()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			entryInstruction.CEI_LocalClearanceDate = new DateTime(2021, 04, 21);
			AssertNull(Provider.LocalClearanceDate);
		}

		public void TestForeignTradeImportEarlyClearanceFlag()
		{
			entryInstruction.CEI_EarlyClearanceFlag = EarlyClearanceFlagsList.Codes.J;
			AssertEquals(EarlyClearanceFlagsList.Codes.J, Provider.ForeignTradeImportEarlyClearanceFlag);
		}

		public void TestForeignTradeImportEarlyClearanceFlag_Empty()
		{
			AssertNull("ForeignTradeImportEarlyClearanceFlag returns null by default", Provider.ForeignTradeImportEarlyClearanceFlag);
		}

		public void TestLocalClearanceProcedure_ValidAuthorization_VZL()
		{
			SetupAuthorizationUsages();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;

			AssertEquals($"Valid VZL-authorization", "SDE111", Provider.LocalClearanceProcedure);
		}

		public void TestLocalClearanceProcedure_ValidAuthorization_AZL()
		{
			SetupAuthorizationUsages();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;

			AssertEquals($"Valid AZL-authorization", "EIR222", Provider.LocalClearanceProcedure);
		}

		public void TestLocalClearanceProcedure_ValidAuthorization_Empty()
		{
			AssertNull("LocalClearanceProcedure returns null by default", Provider.LocalClearanceProcedure);
		}

		public void TestCurrentProcedure_CWP()
		{
			CreateUsage(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "CWP333");
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;

			AssertEquals($"Valid procedure CWP", "CWP333", Provider.CurrentProcedure);
		}

		public void TestCurrentProcedure_CW1()
		{
			CreateUsage(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "CW1444");
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;

			AssertEquals($"Valid procedure CW1", "CW1444", Provider.CurrentProcedure);
		}

		public void TestCurrentProcedure_Empty()
		{
			AssertNull("Current Procedure returns null by default", Provider.CurrentProcedure);
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
			Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
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
			Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress = GetOrgWithEORNumberAndEORIBranch("EOR2", "EBS1");
			declaration.JE_OH_Importer = orgAddress.Header.PK;
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			AssertEquals("Decalarant == Importer: declarant's EORI", "GREOR2", Provider.Declarant.Identification.EoriNumber);
		}

		protected override SCWRECHeaderProvider GetProvider() => new SCWRECHeaderProvider(entryHeader);

		void SetupAuthorizationUsages()
		{
			CreateUsage(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "SDE111");
			CreateUsage(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "EIR222");
		}

		void CreateUsage(ZString code, ZString number)
		{
			var authorizationUsageSDE = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsageSDE.AGC_Code = code;
			authorizationUsageSDE.AGC_Number = number;
		}

		new ISCWRECHeader Provider => base.Provider;
	}
}
