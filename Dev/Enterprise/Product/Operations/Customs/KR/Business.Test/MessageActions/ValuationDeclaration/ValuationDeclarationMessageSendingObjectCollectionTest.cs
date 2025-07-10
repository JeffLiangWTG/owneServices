using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ValuationDeclarationMessageSendingObjectCollection))]
	sealed class ValuationDeclarationMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ValuationDeclarationMessageSendingObjectCollection>
	{
		protected override ValuationDeclarationMessageSendingObjectCollection GetCollectionToTest()
		{
			return new ValuationDeclarationMessageSendingObjectCollection(Declaration);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			return new ValuationDeclarationMessageSendingObject(entry);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}
	}
}
