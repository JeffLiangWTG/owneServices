using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5WNLineMessageDataCollection))]
	sealed class GOVCBR5WNLineMessageDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GOVCBR5WNLineMessageDataCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => new GOVCBR5WNLineMessageData(Factory);

		protected override GOVCBR5WNLineMessageDataCollection GetCollectionToTest() => new GOVCBR5WNLineMessageDataCollection(Factory);
	}
}
