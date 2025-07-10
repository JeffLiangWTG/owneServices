using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(NexDocNotificationForm))]
	sealed class NexDocNotificationFormTest : ZFormBasherTest
	{
		public void TestMenuItem()
		{
			var notification = Factory.New<QuarantineNexDocNotification>();
			notification.QN_AcknowledgeStatus = "ACC";
			using (var form = new NexDocNotificationForm(notification))
			{
				form.Show();
				var acknowledgeMenu = form.FindMenuItem_ForTest("Acknowledge Forward/Transfer Request");
				AssertNotNull("Acknowledge menu item should have been added.", acknowledgeMenu);
				acknowledgeMenu.PerformClick();
				AssertEquals("Should have shown Acknowledge status incorrect message.", "Acknowledge status must be NOT - Not Actioned or ERR - Error.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			notification.QN_AcknowledgeStatus = "NOT";
			using (var form = new NexDocNotificationForm(notification))
			{
				form.Show();
				var acknowledgeMenu = form.FindMenuItem_ForTest("Acknowledge Forward/Transfer Request");
				acknowledgeMenu.PerformClick();
				Assert("Should have shown NEXDOCAcknowledgeForm.", ZFormModaliser.LastFormShownDialogForTest is NEXDOCAcknowledgeForm);
			}
		}

		public void TestFormCaption()
		{
			var notification = Factory.New<QuarantineNexDocNotification>();
			using (var form = new NexDocNotificationForm(notification))
			{
				form.Show();
				AssertEquals("NEXDOC Notification", form.FormCaption);
			}

			notification.QN_RexNumber = "REX12345678";
			using (var form = new NexDocNotificationForm(notification))
			{
				form.Show();
				AssertEquals("NEXDOC Notification - REX12345678", form.FormCaption);
			}

			notification.QN_ExporterReference = "EXP00112233";
			using (var form = new NexDocNotificationForm(notification))
			{
				form.Show();
				AssertEquals("NEXDOC Notification - REX12345678 - EXP00112233", form.FormCaption);
			}
		}

		public void TestQuarantineFieldsDisplayed()
		{
			var notification = Factory.New<QuarantineNexDocNotification>();
			using (var form = new NexDocNotificationForm(notification))
			{
				form.Show();
				AssertTextFieldExistsOnForm(form, "NotificationTypeTextBox", "QN_NotificationType");
				AssertTextFieldExistsOnForm(form, "AcknowledgementStatusTextBox", "AcknowledgeStatusAndDescription");
				AssertTextFieldExistsOnForm(form, "MessageStatusTextBox", "MessageStatusAndDescription");
				AssertTextFieldExistsOnForm(form, "ForwardingGroupIDTextBox", "QN_ForwardingGroupID");
				AssertTextFieldExistsOnForm(form, "ReceivingExporterIDTextBox", "QN_ReceivingExporterID");
				AssertTextFieldExistsOnForm(form, "TransferringExporterIDTextBox", "QN_TransferringExporterID");
				var receivedDateField = form.FindSingle<ZDateEdit>("ReceivedDateTimeDateEdit");
				AssertEquals("ReceivedDateTimeDateEdit", true, receivedDateField.ReadOnly);
				AssertEquals("QN_ReceivedDate", ((IDataBoundControl)receivedDateField).DataMember);
			}
		}

		public void TestColumnsInGrid()
		{
			var notification = Factory.New<QuarantineNexDocNotification>();
			using (var form = new NexDocNotificationForm(notification))
			{
				form.Show();
				var messageGrid = form.FindSingle<ZArchitecture.ZGrid>("MessagesGrid");
				AssertContainsExactElementsInAnyOrder(new string[] { "EM_MessageNum", "EM_MessageDateTime", "EM_InterchangeNumber", "EM_ReceiveTransmit", "EM_Status" }, messageGrid.Columns.GetVisibleColumnMappingNames());
				AssertEquals("InterchangeStatus is hidden", false, messageGrid.GetColumnStyle("EM_InterchangeStatus").IsVisible);
				AssertEquals("eHubID is hidden", false, messageGrid.GetColumnStyle("Interchange+eHubID").IsVisible);
			}
		}

		public void TestShowMessageSummary()
		{
			var notification = Factory.New<QuarantineNexDocNotification>();
			notification.QN_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "~1";
			interchange.EI_From = "NEXDOCS";
			var message = interchange.ContainedMessages.AddNew();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_MessageText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataProvider>NEXDOCS</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>B00001307</Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2017-09-29T03:20:49Z</EventTime>
		<EventType>MRR</EventType>
		<EventReference>MST=NOTIF</EventReference>
		<ContextCollection>
			<Context>
				<Type>NotificationDate</Type>
				<Value>2018-01-18T07:51:44.219+11:00</Value>
			</Context>
			<Context>
				<Type>NotificationTitle</Type>
				<Value>Certificate Notification: REX0000012476</Value>
			</Context>
			<Context>
				<Type>NotificationText</Type>
				<Value>Certification for the REX issued!</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = AutoEvents.MessageReceivedCode;
				log.SL_Table = notification.TableName;
				log.SL_Parent = notification.PK;
			}

			var pivot = Factory.New<GenPivot>();
			pivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
			pivot.Relation1Object = log;
			pivot.Relation2Object = message;
			Factory.Save();
			using (var form = new NexDocNotificationForm(notification))
			{
				form.Show();
				var messageGrid = form.FindSingle<ZArchitecture.ZGrid>("MessagesGrid");
				messageGrid.Select(0);
				var messageSummaryText = form.FindSingle<ZArchitecture.ZTextBox>("MessageSummaryBox");
				var expectedSummary = @"Notification Title: Certificate Notification: REX0000012476
Notification Text: Certification for the REX issued!";
				AssertEquals("MessageSummaryBox", expectedSummary, messageSummaryText.Text);
			}
		}

		protected override Form GetFormToBashCore() => new NexDocNotificationForm(Factory.New<QuarantineNexDocNotification>());

		protected override bool ShouldIgnoreMissingBindingMember(Control control) => control.Name == "MessageSummaryBox";

		void AssertTextFieldExistsOnForm(NexDocNotificationForm form, string fieldName, string boundData)
		{
			var fieldToFind = form.FindSingle<ZArchitecture.ZTextBox>(fieldName);
			AssertEquals(fieldName, true, fieldToFind.ReadOnly);
			AssertEquals(boundData, ((IDataBoundControl)fieldToFind).DataMember);
		}
	}
}
