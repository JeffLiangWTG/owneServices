using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineNexDocNotificationCreatorTest : TestCaseWithFactory
	{
		public void TestCreateFA()
		{
			var notification = new QuarantineNexDocNotificationCreator(Factory).Create(
				"FA",
				"Forward Notification requires approval: REX0000028829 (Exporter Reference: WZG190607REF2)",
				"You have received a forward request that requires acceptance.  The forward was sent by CGAA00312."
			);
			AssertEquals("QN_RexNumber", "REX0000028829", notification.QN_RexNumber);
			AssertEquals("QN_ExporterReference", "WZG190607REF2", notification.QN_ExporterReference);
			AssertEquals("QN_ForwardingGroupID", "CGAA00312", notification.QN_ForwardingGroupID);
			AssertEquals("QN_NotificationType", "FA", notification.QN_NotificationType);
			AssertEquals("QN_AcknowledgeStatus", "NOT", notification.QN_AcknowledgeStatus);
		}

		public void TestCreateTA()
		{
			var notification = new QuarantineNexDocNotificationCreator(Factory).Create(
				"TA",
				"Transfer Notification requires approval: REX0000028928 (Exporter Reference: DDH190607REF3)",
				"You have received a transfer request that requires acceptance.  The new exporter is AA0220. The transfer was sent by CGAA00312 and exporter AB45624"
			);
			AssertEquals("QN_RexNumber", "REX0000028928", notification.QN_RexNumber);
			AssertEquals("QN_ExporterReference", "DDH190607REF3", notification.QN_ExporterReference);
			AssertEquals("QN_ForwardingGroupID", "CGAA00312", notification.QN_ForwardingGroupID);
			AssertEquals("QN_TransferringExporterID", "AB45624", notification.QN_TransferringExporterID);
			AssertEquals("QN_ReceivingExporterID", "AA0220", notification.QN_ReceivingExporterID);
			AssertEquals("QN_NotificationType", "TA", notification.QN_NotificationType);
			AssertEquals("QN_AcknowledgeStatus", "NOT", notification.QN_AcknowledgeStatus);
		}
	}
}
