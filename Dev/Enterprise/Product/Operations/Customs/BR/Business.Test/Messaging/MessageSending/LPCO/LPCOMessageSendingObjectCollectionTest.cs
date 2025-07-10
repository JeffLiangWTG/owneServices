using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LPCOMessageSendingObjectCollection))]
	public class LPCOMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LPCOMessageSendingObjectCollection>
	{
		protected override LPCOMessageSendingObjectCollection GetCollectionToTest()
		{
			return new LPCOMessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var lpcoHeader = Factory.New<CusLPCOHeader>();
			var parent = new LPCOMessageSendingObjectParent(lpcoHeader);
			return new LPCOMessageSendingObject(parent);
		}

		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
			AssertEquals(false, Collection.AllowRemove);
		}
	}
}
