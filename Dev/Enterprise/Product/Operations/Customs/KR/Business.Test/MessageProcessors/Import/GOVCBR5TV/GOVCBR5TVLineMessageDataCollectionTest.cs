using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5TVLineMessageDataCollection))]
	sealed class GOVCBR5TVLineMessageDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GOVCBR5TVLineMessageDataCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => new GOVCBR5TVLineMessageData(new GOVCBR5TVMessageData(Factory));

		protected override GOVCBR5TVLineMessageDataCollection GetCollectionToTest() => new GOVCBR5TVLineMessageDataCollection(new GOVCBR5TVMessageData(Factory));
	}
}
