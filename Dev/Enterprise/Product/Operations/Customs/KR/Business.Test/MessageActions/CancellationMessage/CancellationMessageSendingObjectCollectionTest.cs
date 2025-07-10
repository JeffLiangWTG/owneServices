using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CancellationMessageSendingObjectCollection))]
	sealed class CancellationMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CancellationMessageSendingObjectCollection>
	{
		protected override CancellationMessageSendingObjectCollection GetCollectionToTest()
		{
			return new CancellationMessageSendingObjectCollection(Declaration);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			return new CancellationMessageSendingObject(entry);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}
	}
}
