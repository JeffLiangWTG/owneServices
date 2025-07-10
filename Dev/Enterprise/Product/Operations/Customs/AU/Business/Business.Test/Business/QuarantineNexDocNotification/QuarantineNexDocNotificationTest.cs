using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineNexDocNotification))]
	sealed class QuarantineNexDocNotificationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestINEXDOCMessageParent()
		{
			var nexdocNotification = Factory.New<QuarantineNexDocNotification>();
			nexdocNotification.QN_RexNumber = "TEST";

			var messageParent = (INEXDOCMessageParent)nexdocNotification;

			AssertEquals("TEST", messageParent.RexNumber);
			AssertSame(nexdocNotification.Factory, messageParent.Factory);
		}

		public void TestLookups()
		{
			var nexdocNotification = Factory.New<QuarantineNexDocNotification>();
			AssertType<QuarantineNexDocNotificationLookups>(nexdocNotification.Lookups);
		}

		public void TestValidation()
		{
			var nexdocNotification = Factory.New<QuarantineNexDocNotification>();
			AssertType<QuarantineNexDocNotificationValidation>(nexdocNotification.Validation);
		}

		public void TestHumanReadableName()
		{
			var newNotification = Factory.New<QuarantineNexDocNotification>();
			AssertEquals("NEXDOC Notification", newNotification.HumanReadableName);
			newNotification.QN_RexNumber = "REX1234";
			AssertEquals("NEXDOC Notification - REX1234", newNotification.HumanReadableName);
		}

		public void TestHumanReadableShortcutName()
		{
			var notification = Factory.New<QuarantineNexDocNotification>();
			AssertEquals("NEXDOC Notification", notification.HumanReadableShortcutName);

			notification.QN_RexNumber = "REX12345678";
			AssertEquals("REX12345678", notification.HumanReadableShortcutName);

			notification.QN_ExporterReference = "EXP00112233";
			AssertEquals("REX12345678 - EXP00112233", notification.HumanReadableShortcutName);
		}

		public void TestObjectName()
		{
			var newNotification = Factory.New<QuarantineNexDocNotification>();
			AssertEquals("ObjectName should be empty", ZString.Empty, newNotification.ObjectName);

			newNotification.QN_NotificationType = "FA";
			AssertEquals("ObjectName", "Forward", newNotification.ObjectName);

			newNotification.QN_NotificationType = "TA";
			AssertEquals("ObjectName", "Transfer", newNotification.ObjectName);
		}

		public void TestINEXDOCResponseMembers()
		{
			var nexDocNotification = Factory.New<QuarantineNexDocNotification>();
			nexDocNotification.QN_RexNumber = "111";
			var response = nexDocNotification as INEXDOCResponse;
			AssertEquals(nameof(INEXDOCResponse.JobNumber), ZString.Empty, response.JobNumber);
			AssertEquals(nameof(INEXDOCResponse.RexNumber), "111", response.RexNumber);
			AssertEquals(nameof(INEXDOCResponse.RexStatus), ZString.Empty, response.RexStatus);
			AssertEquals(nameof(INEXDOCResponse.ExportPermitNumber), ZString.Empty, response.ExportPermitNumber);
			AssertEquals(nameof(INEXDOCResponse.CustomsAuthorityNumber), ZString.Empty, response.CustomsAuthorityNumber);
			AssertEquals(nameof(INEXDOCResponse.HtmlTemplatePath), "Enterprise.Customs.AU.Declaration.Business.Data.Xml.Universal.NEXDOC.HtmlTemplates.FATAResponse.html", response.HtmlTemplatePath);
		}

		[TestDate(2020, 2, 6, 13, 20, 20)]
		[TestUtcOffset(11, 0, 0)]
		public void TestQN_ReceivedDate()
		{
			var nexDocNotification = Factory.New<QuarantineNexDocNotification>();
			nexDocNotification.QN_SystemCreateTimeUtc = ZDateTime.Empty;
			AssertEquals("QN_ReceivedDate", ZDateTime.Empty, nexDocNotification.QN_ReceivedDate);

			nexDocNotification.QN_SystemCreateTimeUtc = ZDateTime.Now;
			ZDateTime localtime = nexDocNotification.QN_ReceivedDate;
			ZDateTime utcTime = nexDocNotification.QN_SystemCreateTimeUtc;
			AssertEquals(utcTime.AddHours(11), localtime);
		}

		public void TestMessageStatusAndDescription()
		{
			var nexDocNotification = Factory.New<QuarantineNexDocNotification>();
			nexDocNotification.QN_MessageStatus = "";
			AssertEquals("MessageStatusAndDescription", ZString.Empty, nexDocNotification.MessageStatusAndDescription);

			nexDocNotification.QN_MessageStatus = "FOH";
			AssertEquals("MessageStatusAndDescription", "FOH - Forward On Hold", nexDocNotification.MessageStatusAndDescription);
		}

		public void TestAcknowledgeStatusAndDescription()
		{
			var nexDocNotification = Factory.New<QuarantineNexDocNotification>();
			nexDocNotification.QN_AcknowledgeStatus = "";
			AssertEquals("AcknowledgeStatusAndDescription", ZString.Empty, nexDocNotification.AcknowledgeStatusAndDescription);

			nexDocNotification.QN_AcknowledgeStatus = "PAC";
			AssertEquals("AcknowledgeStatusAndDescription", "PAC - Pending Accept", nexDocNotification.AcknowledgeStatusAndDescription);
		}

		public void TestMessages()
		{
			var nexdocNotification = Factory.New<QuarantineNexDocNotification>();
			nexdocNotification.QN_SystemCreateTimeUtc = ZDateTime.UtcNow;

			var notificationMessage = CreateInboundUXMLMessage(nexdocNotification);
			Factory.Save();

			AssertEquals("Header messages contains 1", 1, nexdocNotification.Messages.Count);
			Assert("has declarationMessage", nexdocNotification.Messages.Contains(notificationMessage.PK));
		}

		EDIMessage CreateInboundUXMLMessage(BusinessObject parent)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "~1";
			interchange.EI_From = "NEXDOCS";

			var message = interchange.ContainedMessages.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			AssertEquals("interchange contains message", interchange.PK, message.EM_EI);

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = AutoEvents.MessageReceivedCode;
				log.SL_Table = parent.TableName;
				log.SL_Parent = parent.PK;
			}

			var pivot = Factory.New<GenPivot>();
			pivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
			pivot.Relation1Object = log;
			pivot.Relation2Object = message;

			return message;
		}
	}
}
