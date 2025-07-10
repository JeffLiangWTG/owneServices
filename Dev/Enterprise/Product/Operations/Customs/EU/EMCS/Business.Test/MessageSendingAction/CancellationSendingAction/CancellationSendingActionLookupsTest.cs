using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class CancellationSendingActionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAttachmentTypeList()
		{
			var jobDeclaration = Factory.New<EMCSJobDeclaration>();
			var cancellation = new CancellationSendingAction(jobDeclaration);
			var list = cancellation.Lookups.ReasonList;
			AssertEquals("0, 1, 2, 3, 4", list.CodesAsString);
			AssertSame("Should be cached", list, cancellation.Lookups.ReasonList);
		}
	}
}
