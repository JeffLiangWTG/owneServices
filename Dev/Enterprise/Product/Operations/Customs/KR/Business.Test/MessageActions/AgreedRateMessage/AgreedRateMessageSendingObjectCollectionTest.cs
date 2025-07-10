using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(AgreedRateMessageSendingObjectCollection))]
	sealed class AgreedRateMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AgreedRateMessageSendingObjectCollection>
	{
		protected override AgreedRateMessageSendingObjectCollection GetCollectionToTest()
		{
			return new AgreedRateMessageSendingObjectCollection(Declaration);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			return new AgreedRateMessageSendingObject(entry);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}
	}
}
