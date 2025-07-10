using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class RF415HeaderTypeProviderTest : DataProviderTestCase<RF415HeaderTypeProvider>
	{
		public void TestApplicationReferenceId()
		{
			AssertEquals(AISCommonOutboundEDIMessage.RF415ApplicationReferenceIdPlaceHolder, Provider.ApplicationReferenceId);
		}

		public void TestApplicationDecisionCodeType()
		{
			sendingObject.RefundType = AISRefundTypeList.Codes.REP;
			AssertEquals(AISRefundTypeList.Codes.REP, Provider.ApplicationDecisionCodeType);
		}

		public void TestSignature()
		{
			AssertNull(Provider.Signature);
		}

		public void TestTotalNumberOfDocuments()
		{
			AssertEquals(0, Provider.TotalNumberOfDocuments);

			sendingObject.DocumentSendingObjectCollection.AddNew();
			AssertEquals(1, GetProvider().TotalNumberOfDocuments);
		}

		protected override RF415HeaderTypeProvider GetProvider()
		{
			return new RF415HeaderTypeProvider(sendingObject);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			sendingObject = new RF415MessageSendingObject(bill);
		}

		RF415MessageSendingObject sendingObject;
		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
