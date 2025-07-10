using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5GULineMessageDataCollection))]
	sealed class GOVCBR5GULineMessageDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GOVCBR5GULineMessageDataCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => new GOVCBR5GULineMessageData(Factory);

		protected override GOVCBR5GULineMessageDataCollection GetCollectionToTest() => new GOVCBR5GULineMessageDataCollection(Factory);
	}
}
