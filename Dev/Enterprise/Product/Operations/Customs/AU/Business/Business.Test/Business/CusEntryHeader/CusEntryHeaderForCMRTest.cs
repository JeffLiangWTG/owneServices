using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderForCMRTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNatureForImportCMRForNature10()
		{
			JobDeclaration declaration = GetJobDeclaration();
			declaration.DoMerge();

			AssertEquals("Nature 10", CusEntryHeader.NatureTypesForImportCMR.Nature10, declaration.CustomsEntryHeaders[0].Nature);
		}

		public void TestNatureForImportCMRForNature20OnInvoiceLines()
		{
			JobDeclaration declaration = GetJobDeclaration();
			header1.JZ_BondPackCount = 20;
			header1.JZ_Nature10PackCount = 20;
			header1.JobComInvoiceLines[0].AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";

			header2.JZ_BondPackCount = 20;
			header2.JZ_Nature10PackCount = 20;
			header2.JobComInvoiceLines[0].AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";

			declaration.DoMerge();

			AssertEquals("Nature 20", CusEntryHeader.NatureTypesForImportCMR.Nature20, declaration.CustomsEntryHeaders[0].Nature);
		}

		public void TestNatureForImportCMRForNature1020()
		{
			JobDeclaration declaration = GetJobDeclaration();
			header1.JobComInvoiceLines[0].AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			JobComInvoiceLine invoiceLine2 = header1.JobComInvoiceLines.AddNew();

			declaration.DoMerge();

			AssertEquals("Nature 10/20", CusEntryHeader.NatureTypesForImportCMR.Nature1020, declaration.CustomsEntryHeaders[0].Nature);
		}

		public void TestNatureForImportCMRForNature30()
		{
			JobDeclaration declaration = GetJobDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			JobComInvoiceLine invoiceLine2 = header1.JobComInvoiceLines.AddNew();

			declaration.DoMerge();
			AssertEquals("Nature 30", CusEntryHeader.NatureTypesForImportCMR.Nature30, declaration.CustomsEntryHeaders[0].Nature);
		}

		public void TestIsCMRNature10()
		{
			JobDeclaration declaration = GetJobDeclaration();
			declaration.DoMerge();

			AssertEquals("Is Nature 10", true, declaration.CustomsEntryHeaders[0].IsCMRNature10);
		}

		public void TestIsCMRNature1020()
		{
			JobDeclaration declaration = GetJobDeclaration();
			header1.JobComInvoiceLines[0].AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			declaration.DoMerge();
			AssertEquals("Nature 10/20", true, declaration.CustomsEntryHeaders[0].IsCMRNature1020);
		}

		public void TestIsCMRNature20()
		{
			JobDeclaration declaration = GetJobDeclaration();
			header1.JobComInvoiceLines[0].AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			header2.JobComInvoiceLines[0].AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			declaration.DoMerge();
			AssertEquals("Nature 20", true, declaration.CustomsEntryHeaders[0].IsCMRNature20);
		}

		public void TestIsCMRNature30()
		{
			JobDeclaration declaration = GetJobDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.DoMerge();
			AssertEquals("Nature 30", true, declaration.CustomsEntryHeaders[0].IsCMRNature30);
		}

		public void TestHasBeenMergedWithMessageErrorsForCMR()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			declaration.Validation.ValidateAll();

			CusEntryHeader header1 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader header2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader header3 = declaration.CustomsEntryHeaders.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			AssertEquals("HasBeenMergedWithMessageErrors", false, header1.HasBeenMergedWithMessageErrors);
			AssertEquals("HasBeenMergedWithMessageErrors", false, header2.HasBeenMergedWithMessageErrors);
			AssertEquals("HasBeenMergedWithMessageErrors", false, header3.HasBeenMergedWithMessageErrors);
		}

		public void TestPopulateCH_BGMReferenceIfNeededForCMR()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			declaration.JE_DeclarationReference = "B00000001";
			declaration.Validation.ValidateAll();

			CusEntryHeader header1 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader header2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryHeader header3 = declaration.CustomsEntryHeaders.AddNew();

			AssertEquals("CH_BGMReference", "", header1.CH_BGMReference);
			header1.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/1", header1.CH_BGMReference);

			header3.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/3", header3.CH_BGMReference);

			header2.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("CH_BGMReference", "B00000001/2", header2.CH_BGMReference);
		}

		public new void TestWorkflowSupportableBusinessObject()
		{
			Assert("We no longer support workflow on CusEntryHeader", true);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => (CusEntryHeader)GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			return entryHeader;
		}

		JobDeclaration GetJobDeclaration()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);

			header1 = declaration.Invoices.AddNew();
			header1.JobComInvoiceLines.AddNew();

			header2 = declaration.Invoices.AddNew();
			header2.JobComInvoiceLines.AddNew();

			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;

			return declaration;
		}

		JobComInvoiceHeader header2;
		JobComInvoiceHeader header1;
	}
}
