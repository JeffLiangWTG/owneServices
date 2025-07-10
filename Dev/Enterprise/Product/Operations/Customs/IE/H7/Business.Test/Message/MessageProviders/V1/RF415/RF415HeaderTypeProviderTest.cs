using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class RF415HeaderTypeProviderTest : DataProviderTestCase<RF415HeaderTypeProvider>
	{
		public void TestApplicationReferenceId()
		{
			AssertEquals(AISOutboundEDIMessage.RF415ApplicationReferenceIdPlaceHolder, Provider.ApplicationReferenceId);
		}

		public void TestApplicationDecisionCodeType()
		{
			SendingObject.RefundType = AISRefundTypeList.Codes.REP;
			AssertEquals(AISRefundTypeList.Codes.REP, Provider.ApplicationDecisionCodeType);
		}

		public void TestSignature()
		{
			AssertNull(Provider.Signature);
		}

		public void TestTotalNumberOfDocuments()
		{
			AssertEquals("0", Provider.TotalNumberOfDocuments);

			SendingObject.DocumentSendingObjectCollection.AddNew();
			AssertEquals("1", GetProvider().TotalNumberOfDocuments);
		}

		protected override RF415HeaderTypeProvider GetProvider() => new RF415HeaderTypeProvider(SendingObject);

		RF415MessageSendingObject SendingObject => sendingObject ?? (sendingObject = new RF415MessageSendingObject(Factory.New<AsycudaManifestHeader>().Bills.AddNew()));
		RF415MessageSendingObject sendingObject;
	}
}
