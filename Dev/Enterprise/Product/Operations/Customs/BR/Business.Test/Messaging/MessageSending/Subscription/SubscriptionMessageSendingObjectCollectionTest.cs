using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SubscriptionMessageSendingObjectCollection))]
	public class SubscriptionMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SubscriptionMessageSendingObjectCollection>
	{
		protected override SubscriptionMessageSendingObjectCollection GetCollectionToTest()
		{
			return new SubscriptionMessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var externalPassword = Factory.New<GlbExternalPassword_BRS>();
			return new SubscriptionMessageSendingObject(externalPassword);
		}

		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}
	}
}
