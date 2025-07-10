using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(MailItemIDsMessageSendingObjectCollection))]
	sealed class MailItemIDsMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MailItemIDsMessageSendingObjectCollection>
	{
		protected override MailItemIDsMessageSendingObjectCollection GetCollectionToTest()
		{
			return new MailItemIDsMessageSendingObjectCollection(Declaration);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			return new MailItemIDsMessageSendingObject(entry);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}
	}
}
