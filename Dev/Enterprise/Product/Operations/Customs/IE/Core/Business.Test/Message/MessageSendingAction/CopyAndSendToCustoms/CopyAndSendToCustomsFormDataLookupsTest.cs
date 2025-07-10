using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	class CopyAndSendToCustomsFormDataLookupsTest : TestCaseWithFactory
	{
		public void TestSendingActionTypeListType()
		{
			AssertType<AESOutgoingMessageTypeList>(new CopyAndSendToCustomsFormData().Lookups.SendingActionTypeList);
		}
	}
}
