using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class MessagesUserControlTest : TestCaseWithFactory
	{
		public void TestMessagesGridHasResendInterchangeContextMenu()
		{
			using (var control = new MessagesUserControl())
			{
				control.SetDataBinding(storageDec.Messages, ZString.Empty);
				var messagesGrid = (MessageZGrid)control.Controls.Find("MessagesGrid", true).First();
				var menuOption = messagesGrid.ContextMenu.MenuItems.FindByText("Resend Interchange");
				AssertNotNull(menuOption);
			}
		}

		public void TestMessagesGrid()
		{
			using (var form = new ZForm())
			using (var control = new MessagesUserControl())
			{
				control.Dock = DockStyle.Fill;
				control.SetDataBinding(storageDec.Messages, ZString.Empty);
				form.Controls.Add(control);
				form.Show();

				var messagesGrid = (MessageZGrid)control.Controls.Find("MessagesGrid", true).First();
				var columns = messagesGrid.Columns;
				CombineAssertions(() =>
				{
					AssertEquals("Only 11 columns avaialble", 11, columns.Count);
					AssertEquals("Created column visible", true, columns[EDIMessage.Schema.EM_MessageDateTime].IsVisible);
					AssertEquals("User column visible", true, columns[EDIMessage.Schema.EM_SystemCreateUser].IsVisible);
					AssertEquals("Created (UTC) column visible", true, columns[EDIMessage.Schema.EM_SystemCreateTimeUtc].IsVisible);
					AssertEquals("Direction column visible", true, columns[EDIMessage.Schema.EM_SendOrReceiveHumanReadable].IsVisible);
					AssertEquals("Message Type column visible", true, columns[EDIMessage.Schema.EM_MessageType].IsVisible);
					AssertEquals("Message Sub Type column visible", true, columns[EDIMessage.Schema.EM_MessageSubType].IsVisible);
					AssertEquals("Application Ref. column visible", true, columns[EDIMessage.Schema.EM_ApplicationReference].IsVisible);
					AssertEquals("Message Number column visible", true, columns[EDIMessage.Schema.EM_MessageNum].IsVisible);
					AssertEquals("Status column visible", true, columns[EDIMessage.Schema.EM_Status].IsVisible);
					AssertEquals("Interchange Number column visible", true, columns[EDIMessage.Schema.EM_InterchangeNumber].IsVisible);
					AssertEquals("Interchange Status column visible", true, columns[EDIMessage.Schema.EM_InterchangeStatus].IsVisible);
				});
			}
		}

		public void TestSplitterExists()
		{
			using (var form = new ZForm())
			using (var control = new MessagesUserControl())
			{
				control.SetDataBinding(storageDec.Messages, ZString.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertNotNull(control.Controls.Find("MessagesAndTextSplitContainer", true).FirstOrDefault());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageDec = Factory.New<CUSPRLCusTempStorageDec>();
			storageDec.Messages.AddNew();
			storageDec.Messages.AddNew();
		}
		CUSPRLCusTempStorageDec storageDec;
	}
}
