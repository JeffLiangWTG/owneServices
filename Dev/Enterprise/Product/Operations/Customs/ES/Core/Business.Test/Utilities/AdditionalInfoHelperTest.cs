using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Testing
{
	sealed class AdditionalInfoHelperTest : TestCaseWithFactory
	{
		public void TestGetAdditionalInfoByCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = AdditionalInfoHelper.GetAdditionalInfoByCode(declaration.AdditionalInfos, "1003");
			CombineAssertions(() =>
			{
				AssertEquals("List empty return nothing", null, supportingDocument);

				var expectedDocument = declaration.AdditionalInfos.AddNew();
				expectedDocument.CSI_Code = "N380";
				expectedDocument.CSI_ReferenceNumber = "Invoice Document";

				supportingDocument = AdditionalInfoHelper.GetAdditionalInfoByCode(declaration.AdditionalInfos, "1003");
				AssertEquals("No document match in the list return nothing", null, supportingDocument);

				supportingDocument = AdditionalInfoHelper.GetAdditionalInfoByCode(declaration.AdditionalInfos, "N380");
				AssertEquals("Document found in the list return the document", expectedDocument, supportingDocument);
			});
		}

		public void TestProcessExportAcceptedMessageCreateNewEntryLineAdditionalInfos()
		{
			var entryLine = GetExportTestEntryLineWithAddInfos();
			CombineAssertions(() =>
			{
				AssertEquals("There are no EntryLine AdditionalInfos before calling ProcessExportEntryLineAdditionalInfos", 0, GetEntryLineAdditionalInfos(entryLine).Length);

				entryLine.ProcessExportEntryLineAdditionalInfos();
				var entryLineAddInfos = GetEntryLineAdditionalInfos(entryLine);
				AssertEquals("There are 4 EntryLine AdditionalInfos after calling ProcessExportEntryLineAdditionalInfos", 4, entryLineAddInfos.Length);
				AssertContainsExactElementsInAnyOrder("The 4 EntryLine AdditionalInfos after calling ProcessExportEntryLineAdditionalInfos have the correct CSI_Codes", new ZString[] { "X001", "X002", "X003", "N380" }, entryLineAddInfos.Select(x => x.CSI_Code).ToArray());
				AssertEquals("The 4 EntryLine AdditionalInfos after calling ProcessExportEntryLineAdditionalInfos have the correct CSI_Status", false, entryLineAddInfos.Any(x => x.CSI_Status != "ACC"));
			});
		}

		public void TestProcessExportAcceptedMessageDeleteAndCreateNewEntryLineAdditionalInfos()
		{
			var entryLine = GetExportTestEntryLineWithAddInfos();
			CombineAssertions(() =>
			{
				entryLine.AddEntryLineDocument<AdditionalInfo>("X004", "ES3600000004");
				entryLine.AddEntryLineDocument<AdditionalInfo>("X005", "ES3600000005");
				var entryLineAddInfos = GetEntryLineAdditionalInfos(entryLine);
				AssertEquals("There are 2 EntryLine AdditionalInfos before calling ProcessExportEntryLineAdditionalInfos", 2, entryLineAddInfos.Length);
				AssertContainsExactElementsInAnyOrder("The 2 EntryLine AdditionalInfos before calling ProcessExportEntryLineAdditionalInfos have the correct CSI_Codes", new ZString[] { "X004", "X005" }, entryLineAddInfos.Select(x => x.CSI_Code).ToArray());

				entryLine.ProcessExportEntryLineAdditionalInfos();
				entryLineAddInfos = GetEntryLineAdditionalInfos(entryLine);
				AssertEquals("There are only 4 EntryLine AdditionalInfos after calling ProcessExportEntryLineAdditionalInfos (previous 2 have been deleted)", 4, entryLineAddInfos.Length);
				AssertContainsExactElementsInAnyOrder("The 4 EntryLine AdditionalInfos after calling ProcessExportEntryLineAdditionalInfos have the correct CSI_Codes", new ZString[] { "X001", "X002", "X003", "N380" }, entryLineAddInfos.Select(x => x.CSI_Code).ToArray());
			});
		}

		CusEntryLine GetExportTestEntryLineWithAddInfos()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_DataModel = Core.Constants.CountryCodes.Spain;
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var suppDoc1 = declaration.AdditionalInfos.AddNew();
			suppDoc1.CSI_Code = "X001";
			suppDoc1.CSI_ReferenceNumber = "ES3600000001";
			suppDoc1.CSI_Status = ZString.Empty;

			var suppDoc2 = entryInstruction.AdditionalInfos.AddNew();
			suppDoc2.CSI_Code = "X002";
			suppDoc2.CSI_ReferenceNumber = "ES3600000002";
			suppDoc2.CSI_Status = ZString.Empty;

			var suppDoc3 = invoice.AdditionalInfos.AddNew();
			suppDoc3.CSI_Code = "X003";
			suppDoc3.CSI_ReferenceNumber = "ES3600000003";
			suppDoc3.CSI_Status = ZString.Empty;

			var suppDoc4 = invoiceLine.AdditionalInfos.AddNew();
			suppDoc4.CSI_Code = "N380";
			suppDoc4.CSI_ReferenceNumber = "ES36000N3801";
			suppDoc4.CSI_Status = ZString.Empty;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "EntryNum";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			Factory.Save();
			return entryLine;
		}

		AdditionalInfo[] GetEntryLineAdditionalInfos(CusEntryLine entryLine)
		{
			var query = new ZQuery(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, entryLine.PK);
			return Factory.Load<AdditionalInfo>(query);
		}
	}
}
