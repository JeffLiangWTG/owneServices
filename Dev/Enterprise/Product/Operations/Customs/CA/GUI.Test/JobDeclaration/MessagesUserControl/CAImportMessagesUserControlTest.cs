using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CAImportMessagesUserControlTest : TestCaseWithFactory
	{
		public void TestEntryLinesTabUserControl()
		{
			var testDec = Factory.New<JobDeclaration>();

			using (var form = new ZForm(testDec))
			using (var userControl = new CAImportMessagesUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				AssertNotNull(userControl.CADEntryLinesTabUseControl);
			}
		}

		public void TestSetupEntryHeaderColumns()
		{
			var testDec = Factory.New<JobDeclaration>();

			using (var form = new ZForm(testDec))
			using (var userControl = new CAImportMessagesUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				CombineAssertions(() =>
				{
					AssertNotNull("User control should have CH_EntryStatus column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryStatus]);
					AssertNotNull("User control should have CH_Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_Status]);
					AssertNotNull("User control should have CustomsValue column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CustomsValue]);
					AssertNotNull("User control should have TransactionValue column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.TransactionValue]);
					AssertNotNull("User control should have TotalAmountPayable column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.TotalAmountPayable]);
					AssertNotNull("User control should have TotalDutyAmount column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.TotalDutyAmount]);
					AssertNotNull("User control should have GSTAmount column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.GSTAmount]);
					AssertNotNull("User control should not have CH_MessageType column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageType]);
					AssertNull("User control should have no CH_MessageTypeDescription column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription]);
					AssertNotNull("User control should have CH_EntrySubmittedDate column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntrySubmittedDate]);
					AssertNotNull("User control should have EffectivePortOfClearance column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EffectivePortOfClearance]);
					AssertNotNull("User control should have CH_EntryReleaseDate column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryReleaseDate]);
					AssertNotNull("User control should have EffectiveValuationDate column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EffectiveValuationDate]);
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageDetailsTabPageVisible()
		{
			JobDeclaration declaration = GetDeclaration();

			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Factory.Save();

				ZTabPage messagesTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).MessagesTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = messagesTabPage;

				var userControl = (CAImportMessagesUserControl)messagesTabPage.Controls[0];
				var userControlMessages = (MessagesTabUserControl)userControl.BaseMessageUserControl.HostedControl;
				CombineAssertions(() =>
				{
					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					Application.DoEvents();
					AssertEquals("MessageDetailsEDITabPage should not be visible", false, userControlMessages.MessageDetailsEDITabPage.TabVisible);
					AssertEquals("MessageTextEDITabPage should not be visible", false, userControlMessages.MessageTextEDITabPage.TabVisible);

					userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(1, 0);
					Application.DoEvents();

					AssertEquals("MessageDetailsEDITabPage should be visible", true, userControlMessages.MessageDetailsEDITabPage.TabVisible);
					AssertEquals("MessageTextEDITabPage should be visible", true, userControlMessages.MessageTextEDITabPage.TabVisible);
				});
			}
		}

		public void TestMessagesUserControlBindingPathIsMessagesForDisplay()
		{
			using (var form = new ZForm(Factory.New<JobDeclaration>()))
			using (var userControl = new CAImportMessagesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				AssertEquals("MessagesTab has the correct binding path", "CustomsEntryHeaders.MessagesForDisplay", userControl.BaseMessageUserControl.BindingSource.DataMember);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		JobDeclaration GetDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.ActiveEntryHeaders.AddNew();
			declaration.ActiveEntryHeaders.AddNew();
			var message1 = Factory.New<UniversalEventMessage>();
			message1.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message1.EM_MessageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\EDIMessageDetails.xml");
			declaration.ActiveEntryHeaders[1].Messages.Add(message1);
			return declaration;
		}
	}
}
