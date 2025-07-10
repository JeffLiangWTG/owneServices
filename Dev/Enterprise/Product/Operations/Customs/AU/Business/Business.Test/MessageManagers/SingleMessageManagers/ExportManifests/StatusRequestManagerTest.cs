using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class StatusRequestManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestBusinessObject()
		{
			AssertEquals(ExportLine, Manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			ExportLine.EL_CAN = "123";
			ExportLine.EL_LineNo = 2;
			ExportLine.EL_AirWayBill = "AWB";
			ExportLine.EL_UserReferenceNum = "456";
			AssertEquals("MessageFriendlyName", "Status Request for: CAN: 123 Line: 2 Master: AWB Ref: 456", Manager.MessageFriendlyName);
		}

		public void TestGetMessages()
		{
			AssertEquals("Messages", ExportLine.Messages, ((StatusRequestManager)Manager).GetMessages(ExportLine));
		}

		public new void TestCanSendOriginal()
		{
			Assert(true);
		}
		public new void TestCanSendWithdrawal()
		{
			Assert(true);
		}

		public new void TestNotificationsWhenWaitingForResponse()
		{
			Assert(true);
		}

		protected override CMRMessageManager GetManager() => new StatusRequestManager(ExportLine);

		protected override void SetStatus(ZString status)
		{
		}

		ExportCustomsManifestLines exportLine;
		ExportCustomsManifestLines ExportLine
		{
			get
			{
				if (exportLine == null)
				{
					var header = Factory.New<ExportCustomsManifestHeader>();
					exportLine = header.Lines.AddNew();
				}
				return exportLine;
			}
		}
	}
}
