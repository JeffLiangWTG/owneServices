using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class OtherCustomsInformationControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var form = new ZForm())
			using (var control = new OtherCustomsInformationControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("CustomsOfficeOfDestinationOrExitDropEdit", true, control.FindSingle<ZDropEdit>("CustomsOfficeOfDestinationOrExitDropEdit").Visible);
					AssertEquals("LocalReferenceNumberTextBox", true, control.FindSingle<ZTextBox>("LocalReferenceNumberTextBox").Visible);
				});
			}
		}

		public void TestLocalReferenceNumber_NoMergeConfirmation()
		{
			var instruction = Factory.New<Business.CusEntryInstruction>();
			using (var form = new ZForm(instruction))
			using (var otherCustomsInformationControl = new OtherCustomsInformationControl())
			{
				form.Controls.Add(otherCustomsInformationControl);
				form.Show();
				CombineAssertions("Local Reference Number Changed", () =>
				{
					instruction.ASY_LocalReferenceNumber = "NUMBER1";
					AssertEquals("No message popup if no linked declaration", null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("ASY_LocalReferenceNumber was set to NUMBER1", "NUMBER1", instruction.ASY_LocalReferenceNumber);

					instruction.ASY_LocalReferenceNumber = "NUMBER2";
					AssertEquals("No message popup if no linked declaration", null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("ASY_LocalReferenceNumber was set to NUMBER2", "NUMBER2", instruction.ASY_LocalReferenceNumber);
				});
			}
		}

		public void TestLocalReferenceNumber_MergeConfirmation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_DeclarationReference = "B001000";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.ASY_LocalReferenceNumber = "DUPLICATE";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("DUPLICATE", entry.CH_BGMReference);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration2.JE_DeclarationReference = "B002000";
			var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction2.PK;
			AssertEquals(0, declaration2.ActiveEntryHeaders.Count);

			var message = "Please note that changing the Local Reference Number may invalidate various external documents. Please take the necessary precautions to ensure compliance. The entries will be re-merged.\r\n\r\nSelect 'Yes' to change the Local Reference Number";

			using (var form = new ZForm(declaration2))
			using (var entryInstructionDetailsUserControl = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(entryInstructionDetailsUserControl);
				form.Show();

				CombineAssertions("Local Reference Number Changed", () =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					instruction2.ASY_LocalReferenceNumber = "NUMBER1";
					AssertEquals("No message popup if no entry yet", null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("ASY_LocalReferenceNumber was set to NUMBER1", "NUMBER1", instruction2.ASY_LocalReferenceNumber);
					declaration2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
					declaration2.DoMerge();

					var entry2 = declaration2.ActiveEntryHeaders[0];
					AssertEquals("DoMerge and 1 entry", 1, declaration2.ActiveEntryHeaders.Count);
					AssertEquals("Entry number was set", "NUMBER1", entry2.CH_BGMReference);
					Factory.Save();

					instruction2.ASY_LocalReferenceNumber = "NUMBER1";
					AssertEquals("No message popup if ASY_LocalReferenceNumber not changed", null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("ASY_LocalReferenceNumber was unchanged", "NUMBER1", instruction2.ASY_LocalReferenceNumber);
					instruction2.ASY_LocalReferenceNumber = "NUMBER2";
					AssertEquals("Message popup and select YES", message, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("ASY_LocalReferenceNumber was set to NUMBER2", "NUMBER2", instruction2.ASY_LocalReferenceNumber);
					AssertEquals("Entry number was changed to ASY_LocalReferenceNumber", "NUMBER2", entry2.CH_BGMReference);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					instruction2.ASY_LocalReferenceNumber = "duplicate";
					AssertEquals("No message popup if there's validation error", null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertHasErrors("Has validation error", instruction2.ASY_LocalReferenceNumberInfo);
					AssertEquals("ASY_LocalReferenceNumber was changed and has error", "duplicate", instruction2.ASY_LocalReferenceNumber);
					AssertEquals("Entry reference not changed due to ASY_LocalReferenceNumber validation error", "NUMBER2", entry2.CH_BGMReference);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					instruction2.ASY_LocalReferenceNumber = "NUMBER3";
					AssertEquals("Message popup and select No", message, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("ASY_LocalReferenceNumber was not set to NUMBER3", "duplicate", instruction2.ASY_LocalReferenceNumber);
					AssertEquals("Entry reference not changed to NUMBER3", "NUMBER2", entry2.CH_BGMReference);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					instruction2.ASY_LocalReferenceNumber = "NUMBER4";
					AssertEquals("Message popup and select Cancel", message, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("ASY_LocalReferenceNumber was not set to NUMBER4", "duplicate", instruction2.ASY_LocalReferenceNumber);
					AssertEquals("Entry reference not changed to NUMBER4", "NUMBER2", entry2.CH_BGMReference);
				});
			}
		}

		public void TestLocalReferenceNumber_MergeConfirmation_ForJobWithDefaultBGMReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_DeclarationReference = "B001000";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("B001000/1", entry.CH_BGMReference);
			AssertEquals("ASY_LocalReferenceNumber was empty", "", instruction.ASY_LocalReferenceNumber);

			using (var form = new ZForm(declaration))
			using (var entryInstructionDetailsUserControl = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(entryInstructionDetailsUserControl);
				form.Show();

				CombineAssertions("Local Reference Number Changed", () =>
				{
					instruction.ASY_LocalReferenceNumber = "B001000/1";
					AssertEquals("No message popup if ASY_LocalReferenceNumber is equal as current entry reference", null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("ASY_LocalReferenceNumber was set", "B001000/1", instruction.ASY_LocalReferenceNumber);
					AssertEquals("CH_BGMReference not changed", "B001000/1", entry.CH_BGMReference);

					instruction.ASY_LocalReferenceNumber = "";
					AssertEquals("Message popup if ASY_LocalReferenceNumber change to empty", "Local Reference Number cannot be empty as it may invalidate external documents.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("ASY_LocalReferenceNumber not changed", "B001000/1", instruction.ASY_LocalReferenceNumber);
					AssertEquals("Entry BGMReference still the same", "B001000/1", entry.CH_BGMReference);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					entry.CH_WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated;
					instruction.ASY_LocalReferenceNumber = "change";
					AssertEquals("Message popup if current entry has WHSTransaction", "Local Reference Number cannot be changed as the entry header linked with this entry instruction has been synchronized to Bonded Warehouse.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("ASY_LocalReferenceNumber can not change if entry header has warehouse transcation", "B001000/1", instruction.ASY_LocalReferenceNumber);
					AssertEquals("Entry BGMReference still the same", "B001000/1", entry.CH_BGMReference);
				});
			}
		}
	}
}
