using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class ImportFTAMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new ImportFTAMessageSendingFormBuilder(ElectronicDocumentTypeList.Codes._DHR);
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 16);
			Assert(nameof(FTAMessageSendingObject.ShouldSend), true, 0);
			Assert(nameof(FTAMessageSendingObject.FormattedEntryNumber), true, 1);
			Assert(nameof(FTAMessageSendingObject.LawCode), true, 2);
			Assert(nameof(FTAMessageSendingObject.DepartureDate), true, 3);
			Assert(nameof(FTAMessageSendingObject.DeparturePort), true, 4);
			Assert(nameof(FTAMessageSendingObject.CustomsDisbursementBill), true, 5);
			Assert(nameof(FTAMessageSendingObject.ManufacturCompanyName), false, 6);
			Assert(nameof(FTAMessageSendingObject.ManufacturAddress), false, 7);
			Assert(nameof(FTAMessageSendingObject.ManufacturPostCode), false, 8);
			Assert(nameof(FTAMessageSendingObject.DepartureCountry), false, 9);
			Assert(nameof(FTAMessageSendingObject.TransshipmentYN), false, 10);
			Assert(nameof(FTAMessageSendingObject.TransshipmentDate), false, 11);
			Assert(nameof(FTAMessageSendingObject.TransshipmentCountry), false, 12);
			Assert(nameof(FTAMessageSendingObject.TransshipmentPort), false, 13);
			Assert(nameof(FTAMessageSendingObject.ImporterCompanyName), false, 14);
			Assert(nameof(FTAMessageSendingObject.ExporterCompanyName), false, 15);

			void Assert(object columnName, bool isVisible, int index)
			{
				AssertEquals(columnName, list[index].ColumnName);
				AssertEquals(isVisible, list[index].IsVisible);
			}
		}

		public void TestGetAmendmentUserControl()
		{
			var builder = new ImportFTAMessageSendingFormBuilder(ElectronicDocumentTypeList.Codes._DHR);

			using (var userControl = builder.GetUserControl())
			{
				AssertEquals(typeof(ImportFTAEntryLinesMessageSendingUserControl), userControl.GetType());
			}
		}

		public void TestGetAdditionalTabPages_5SC()
		{
			var builder = new ImportFTAMessageSendingFormBuilder(ElectronicDocumentTypeList.Codes._5SC);
			var tabPages = builder.GetAdditionalTabPages(null);
			AssertNotNull(tabPages);
			AssertEquals(2, tabPages.Length);
			AssertType(typeof(ImportFTAMessageDetailsUserControl), tabPages[0].Controls[0]);
			AssertEquals("FTA Entry Details", tabPages[0].CaptionResourceString.Caption);
			AssertEquals("ImportFTAMessageDetailsUserControl", tabPages[0].Controls[0].Name);

			AssertType(typeof(ImportFTAEntryLineDetailsUserControl), tabPages[1].Controls[0]);
			AssertEquals("FTA Entry Line Details", tabPages[1].CaptionResourceString.Caption);
			AssertEquals("ImportFTAEntryLineDetailsUserControl", tabPages[1].Controls[0].Name);

			foreach (var control in tabPages)
			{
				control.Dispose();
			}
		}
		public void TestGetAdditionalTabPages_DHR()
		{
			var builder = new ImportFTAMessageSendingFormBuilder(ElectronicDocumentTypeList.Codes._DHR);
			var tabPages = builder.GetAdditionalTabPages(null);
			AssertNotNull(tabPages);
			AssertEquals(3, tabPages.Length);
			AssertType(typeof(ImportFTAMessageDetailsUserControl), tabPages[0].Controls[0]);
			AssertEquals("FTA Entry Details", tabPages[0].CaptionResourceString.Caption);
			AssertEquals("ImportFTAMessageDetailsUserControl", tabPages[0].Controls[0].Name);

			AssertType(typeof(ImportFTAEntryLineDetailsUserControl), tabPages[1].Controls[0]);
			AssertEquals("FTA Entry Line Details", tabPages[1].CaptionResourceString.Caption);
			AssertEquals("ImportFTAEntryLineDetailsUserControl", tabPages[1].Controls[0].Name);

			AssertType(typeof(ImportFTAInvoiceLineDetailsUserControl), tabPages[2].Controls[0]);
			AssertEquals("FTA Invoice Line Details", tabPages[2].CaptionResourceString.Caption);
			AssertEquals("ImportFTAInvoiceLineDetailsUserControl", tabPages[2].Controls[0].Name);

			foreach (var control in tabPages)
			{
				control.Dispose();
			}
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	class ImportFTAMessageSendingActionForm5SCTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var parent = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5SC, MessageFunctions.MessageFunctionCode.Original);
			return new MessageSendingActionForm(parent, new ImportFTAMessageSendingFormBuilder(ElectronicDocumentTypeList.Codes._5SC));
		}
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_PrimaryPreference = "FEU1";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.MergedLines.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.CL_FTASequenceNumber = 1;
		}
		JobDeclaration declaration;
	}

	[TestedType(typeof(MessageSendingActionForm))]
	class ImportFTAMessageSendingActionFormDHRTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var parent = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._DHR, MessageFunctions.MessageFunctionCode.Original);
			return new MessageSendingActionForm(parent, new ImportFTAMessageSendingFormBuilder(ElectronicDocumentTypeList.Codes._DHR));
		}
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_PrimaryPreference = "FEU1";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.CL_FTASequenceNumber = 1;
		}
		JobDeclaration declaration;
	}
}
