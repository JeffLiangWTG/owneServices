using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5TWLineMessageDataCollection))]
	sealed class GOVCBR5TWLineMessageDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GOVCBR5TWLineMessageDataCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => new GOVCBR5TWLineMessageData(Factory);

		protected override GOVCBR5TWLineMessageDataCollection GetCollectionToTest() => new GOVCBR5TWLineMessageDataCollection(Factory);
	}
}
