using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(UpdateCSVClearanceForm))]
	class UpdateCSVClearanceFormTest : ZFormBasherTest
	{
		#region Overrides

		protected override Form GetFormToBashCore() => new UpdateCSVClearanceForm(CsvCodeInfo);

		#endregion

		CsvCodeInfo CsvCodeInfo
		{
			get
			{
				if (csvCodeInfo == null)
				{
					csvCodeInfo = CsvCodeInfo.LoadNew(EntryHeader);
				}
				return csvCodeInfo;
			}
		}
		CsvCodeInfo csvCodeInfo;

		CusEntryHeader EntryHeader
		{
			get
			{
				if (entryHeader == null)
				{
					entryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Export);
				}
				return entryHeader;
			}
		}
		CusEntryHeader entryHeader;

		public void TestInitializeUpdateCSVClearanceLayoutImport()
		{
			var newEntryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Import);
			using (var updateCSVForm = new UpdateCSVClearanceForm(CsvCodeInfo.LoadNew(newEntryHeader)))
			{
				var messageLabel = (ZLabel)(updateCSVForm.Controls.Find("MessageLabel", true).Single());
				var csvClearanceTextBox = (ZTextBox)(updateCSVForm.Controls.Find("CSVClearance", true).Single());
				var clearanceDateDateEdit = (ZDateEdit)(updateCSVForm.Controls.Find("ClearanceDate", true).Single());
				var secondaryCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("SecondaryCSVNumber", true).Single());
				var thirdCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("ThirdCSVNumber", true).Single());
				var expectedMessageLabelText = string.Format(@"You are about to change the clearance number of entry {0}.
Please make sure that the number that you are entering is the right clearance number.", newEntryHeader.CH_BGMReference);

				updateCSVForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Message Label has the correct Text", expectedMessageLabelText, messageLabel.Text);
					AssertEquals("CSV Clearance is visible cause is an Import Declaration", true, csvClearanceTextBox.Visible);
					AssertEquals("Clearance Date is visible cause is an Import Declaration", true, clearanceDateDateEdit.Visible);
					AssertEquals("Secondary CSV Number is visible cause is an Import Declaration", true, secondaryCsvNumberTextBox.Visible);
					AssertEquals("Secondary CSV Number Caption is CSV ImportCertificate cause is an Import Declaration", "CSV Import Certificate", secondaryCsvNumberTextBox.CaptionResourceString.Caption);
					AssertEquals("Third CSV Number is not visible cause is a Import Declaration", false, thirdCsvNumberTextBox.Visible);
				});
			}
		}

		public void TestInitializeUpdateCSVClearanceLayoutExport()
		{
			var newEntryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Export);
			using (var updateCSVForm = new UpdateCSVClearanceForm(CsvCodeInfo.LoadNew(newEntryHeader)))
			{
				var messageLabel = (ZLabel)(updateCSVForm.Controls.Find("MessageLabel", true).Single());
				var csvClearanceTextBox = (ZTextBox)(updateCSVForm.Controls.Find("CSVClearance", true).Single());
				var clearanceDateDateEdit = (ZDateEdit)(updateCSVForm.Controls.Find("ClearanceDate", true).Single());
				var secondaryCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("SecondaryCSVNumber", true).Single());
				var thirdCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("ThirdCSVNumber", true).Single());
				var expectedMessageLabelText = string.Format(@"You are about to change the clearance number of entry {0}.
Please make sure that the number that you are entering is the right clearance number.", newEntryHeader.CH_BGMReference);

				updateCSVForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Message Label has the correct Text", expectedMessageLabelText, messageLabel.Text);
					AssertEquals("CSV Clearance is visible cause is an UCC6 Export Declaration", true, csvClearanceTextBox.Visible);
					AssertEquals("Clearance Date is visible cause is an UCC6 Export Declaration", true, clearanceDateDateEdit.Visible);
					AssertEquals("Secondary CSV Number is visible cause is an Export Declaration", true, secondaryCsvNumberTextBox.Visible);
					AssertEquals("Secondary CSV Number Caption is CSV T2L cause is an Export Declaration", "CSV T2L", secondaryCsvNumberTextBox.CaptionResourceString.Caption);
					AssertEquals("Third CSV Number is visible cause is an UCC6 Export Declaration", true, thirdCsvNumberTextBox.Visible);
				});
			}
		}

		public void TestInitializeUpdateCSVClearanceLayoutT2L()
		{
			var newEntryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Export, EntrySubStyleList.Codes.T2L);
			using (var updateCSVForm = new UpdateCSVClearanceForm(CsvCodeInfo.LoadNew(newEntryHeader)))
			{
				var messageLabel = (ZLabel)(updateCSVForm.Controls.Find("MessageLabel", true).Single());
				var csvClearanceTextBox = (ZTextBox)(updateCSVForm.Controls.Find("CSVClearance", true).Single());
				var clearanceDateDateEdit = (ZDateEdit)(updateCSVForm.Controls.Find("ClearanceDate", true).Single());
				var secondaryCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("SecondaryCSVNumber", true).Single());
				var thirdCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("ThirdCSVNumber", true).Single());
				var expectedMessageLabelText = string.Format(@"You are about to change the clearance number of entry {0}.
Please make sure that the number that you are entering is the right clearance number.", newEntryHeader.CH_BGMReference);

				updateCSVForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Message Label has the correct Text", expectedMessageLabelText, messageLabel.Text);
					AssertEquals("CSV Clearance is visible cause is a T2Lport Declaration", true, csvClearanceTextBox.Visible);
					AssertEquals("Clearance Date is visible cause is a T2L Declaration", true, clearanceDateDateEdit.Visible);
					AssertEquals("Secondary CSV Number is not visible cause is a T2L Declaration", false, secondaryCsvNumberTextBox.Visible);
					AssertEquals("Third CSV Number is not visible cause is a T2L Declaration", false, thirdCsvNumberTextBox.Visible);
				});
			}
		}

		public void TestInitializeUpdateCSVClearanceLayoutT2C()
		{
			var newEntryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2C);
			using (var updateCSVForm = new UpdateCSVClearanceForm(CsvCodeInfo.LoadNew(newEntryHeader)))
			{
				var messageLabel = (ZLabel)(updateCSVForm.Controls.Find("MessageLabel", true).Single());
				var csvClearanceTextBox = (ZTextBox)(updateCSVForm.Controls.Find("CSVClearance", true).Single());
				var clearanceDateDateEdit = (ZDateEdit)(updateCSVForm.Controls.Find("ClearanceDate", true).Single());
				var secondaryCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("SecondaryCSVNumber", true).Single());
				var thirdCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("ThirdCSVNumber", true).Single());
				var expectedMessageLabelText = string.Format(@"You are about to change the clearance number of entry {0}.
Please make sure that the number that you are entering is the right clearance number.", newEntryHeader.CH_BGMReference);

				updateCSVForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Message Label has the correct Text", expectedMessageLabelText, messageLabel.Text);
					AssertEquals("CSV Clearance is visible cause is a T2C Declaration", true, csvClearanceTextBox.Visible);
					AssertEquals("Clearance Date is visible cause is a T2C Declaration", true, clearanceDateDateEdit.Visible);
					AssertEquals("Secondary CSV Number is not visible cause is a T2C Declaration", false, secondaryCsvNumberTextBox.Visible);
					AssertEquals("Third CSV Number is not visible cause is a T2C Declaration", false, thirdCsvNumberTextBox.Visible);
				});
			}
		}

		public void TestInitializeUpdateCSVClearanceLayoutEXS()
		{
			var newEntryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS);
			using (var updateCSVForm = new UpdateCSVClearanceForm(CsvCodeInfo.LoadNew(newEntryHeader)))
			{
				var messageLabel = (ZLabel)(updateCSVForm.Controls.Find("MessageLabel", true).Single());
				var csvClearanceTextBox = (ZTextBox)(updateCSVForm.Controls.Find("CSVClearance", true).Single());
				var clearanceDateDateEdit = (ZDateEdit)(updateCSVForm.Controls.Find("ClearanceDate", true).Single());
				var secondaryCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("SecondaryCSVNumber", true).Single());
				var thirdCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("ThirdCSVNumber", true).Single());
				var expectedMessageLabelText = string.Format(@"You are about to change the clearance number of entry {0}.
Please make sure that the number that you are entering is the right clearance number.", newEntryHeader.CH_BGMReference);

				updateCSVForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Message Label has the correct Text", expectedMessageLabelText, messageLabel.Text);
					AssertEquals("CSV Clearance is visible cause is a EXS Declaration", true, csvClearanceTextBox.Visible);
					AssertEquals("Clearance Date is visible cause is a EXS Declaration", true, clearanceDateDateEdit.Visible);
					AssertEquals("Secondary CSV Number is not visible cause is a EXS Declaration", false, secondaryCsvNumberTextBox.Visible);
					AssertEquals("Third CSV Number is not visible cause is a EXS Declaration", false, thirdCsvNumberTextBox.Visible);
				});
			}
		}

		public void TestInitializeUpdateCSVClearanceLayoutExportAES()
		{
			var newEntryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Export);
			newEntryHeader.ZG_UCC6Version = 1;
			using (var updateCSVForm = new UpdateCSVClearanceForm(CsvCodeInfo.LoadNew(newEntryHeader)))
			{
				var messageLabel = (ZLabel)(updateCSVForm.Controls.Find("MessageLabel", true).Single());
				var csvClearanceTextBox = (ZTextBox)(updateCSVForm.Controls.Find("CSVClearance", true).Single());
				var clearanceDateDateEdit = (ZDateEdit)(updateCSVForm.Controls.Find("ClearanceDate", true).Single());
				var secondaryCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("SecondaryCSVNumber", true).Single());
				var thirdCsvNumberTextBox = (ZTextBox)(updateCSVForm.Controls.Find("ThirdCSVNumber", true).Single());
				var expectedMessageLabelText = string.Format(@"You are about to change the clearance number of entry {0}.
Please make sure that the number that you are entering is the right clearance number.", newEntryHeader.CH_BGMReference);

				updateCSVForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Message Label has the correct Text", expectedMessageLabelText, messageLabel.Text);
					AssertEquals("CSV Clearance is visible cause is an AES Export Declaration", true, csvClearanceTextBox.Visible);
					AssertEquals("Clearance Date is visible cause is an AES Export Declaration", true, clearanceDateDateEdit.Visible);
					AssertEquals("Secondary CSV Number is visible cause is an AES Export Declaration", true, secondaryCsvNumberTextBox.Visible);
					AssertEquals("Secondary CSV Number Caption is CSV T2L cause is an AES Export Declaration", "CSV T2L", secondaryCsvNumberTextBox.CaptionResourceString.Caption);
					AssertEquals("Third CSV Number is visible cause is an AES Export Declaration", true, thirdCsvNumberTextBox.Visible);
				});
			}
		}

		CusEntryHeader CreateEntryHeader(ZString declarationType, string entrySubStyle = "A")
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = declarationType;
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.ZG_IsTrainingDeclaration = true;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = entrySubStyle;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var newEntryHeader = declaration.CustomsEntryHeaders[0];

			Factory.Save();

			return newEntryHeader;
		}
	}
}
