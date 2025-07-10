using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class RF415HeaderTypeProviderTest : DataProviderTestCase<RF415HeaderTypeProvider>
	{
		public void TestApplicationReferenceId()
		{
			AssertEquals(AISOutboundEDIMessage.RF415ApplicationReferenceIdPlaceHolder, Provider.ApplicationReferenceId);
		}

		public void TestApplicationDecisionCodeType()
		{
			SendingAction.RefundType = AISRefundTypeList.Codes.REP;
			AssertEquals(AISRefundTypeList.Codes.REP, Provider.ApplicationDecisionCodeType);
		}

		public void TestSignature()
		{
			AssertNull(Provider.Signature);
		}

		public void TestTotalNumberOfDocuments()
		{
			AssertEquals(0, Provider.TotalNumberOfDocuments);

			sendingAction.DocumentSendingObjectCollection.AddNew();
			AssertEquals(1, GetProvider().TotalNumberOfDocuments);
		}

		protected override RF415HeaderTypeProvider GetProvider() => new RF415HeaderTypeProvider(SendingAction);

		RefundApplicationMessageSendingAction SendingAction
		{
			get
			{
				if (sendingAction == null)
				{
					var entryHeader = Factory.New<CusEntryHeader>();
					sendingAction = new RefundApplicationMessageSendingAction(entryHeader);
				}

				return sendingAction;
			}
		}
		RefundApplicationMessageSendingAction sendingAction;
	}
}
