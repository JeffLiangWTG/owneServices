using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(SubscriberInfo))]
	class SubscriberInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SubscriberInfo("Code");
		}

		public void TestSubscriberInfo()
		{
			var subscriber = new SubscriberInfo("Code");
			AssertNotNull(subscriber);
		}
	}
}
