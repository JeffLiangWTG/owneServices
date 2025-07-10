using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5GVLineMessageDataCollection))]
	sealed class GOVCBR5GVLineMessageDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GOVCBR5GVLineMessageDataCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => new GOVCBR5GVLineMessageData(Factory);

		protected override GOVCBR5GVLineMessageDataCollection GetCollectionToTest() => new GOVCBR5GVLineMessageDataCollection(Factory);
	}
}
