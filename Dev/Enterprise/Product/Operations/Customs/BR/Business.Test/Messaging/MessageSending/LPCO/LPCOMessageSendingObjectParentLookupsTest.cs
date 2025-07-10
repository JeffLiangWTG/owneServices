using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class LPCOMessageSendingObjectParentLookupsTest : TestCaseWithFactory
	{
		public void TestLookups()
		{
			AssertType(typeof(GlbStaffCollection), lookups.BrokerList);
		}

		LPCOMessageSendingObjectParentLookups lookups;

		protected override void SetUp()
		{
			header = Factory.NewWithValidTestData<CusLPCOHeader>();
			sendingObjectParent = new LPCOMessageSendingObjectParent(header);
			lookups = new LPCOMessageSendingObjectParentLookups(sendingObjectParent);
		}

		CusLPCOHeader header;
		LPCOMessageSendingObjectParent sendingObjectParent;
	}
}
