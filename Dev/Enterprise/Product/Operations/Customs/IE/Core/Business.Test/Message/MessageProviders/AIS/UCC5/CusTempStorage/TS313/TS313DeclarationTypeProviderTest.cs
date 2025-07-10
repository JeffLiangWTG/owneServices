using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS313DeclarationTypeProviderTest : DataProviderTestCase<TS313DeclarationTypeProvider>
	{
		public void TestMRN()
		{
			header.MRN = "MRN2343234242";
			AssertEquals("MRN", "MRN2343234242", Provider.MRN);
		}

		public void TestRemarks()
		{
			messageSendingObject.CustomsJustification = "For Some Reason";
			AssertEquals("For Some Reason", Provider.Remarks);
		}

		protected override TS313DeclarationTypeProvider GetProvider() => new TS313DeclarationTypeProvider(messageSendingObject);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<TemporaryStorageHeader>();
			messageSendingObject = new TemporaryStorageMessageSendingObject(header);
		}
		TemporaryStorageMessageSendingObject messageSendingObject;
		TemporaryStorageHeader header;
	}
}
