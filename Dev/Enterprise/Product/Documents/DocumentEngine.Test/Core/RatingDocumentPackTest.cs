using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(RatingDocumentPack))]
	sealed class RatingDocumentPackTest : NonPersistentBusinessObjectCollectionTestCase<RatingDocumentPack>
	{
		protected override RatingDocumentPack GetCollectionToTest()
		{
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var docSupp = Factory.NewWithValidTestData<DummyBODocSupportable>();
			RatingDocPackBuilder builder = new RatingDocPackBuilder(command, docSupp, Callback);
			return new RatingDocumentPack(command, builder);
		}

		void Callback(RatingDocPackBuilder builder)
		{
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyDeliverable();
		}
	}
}
