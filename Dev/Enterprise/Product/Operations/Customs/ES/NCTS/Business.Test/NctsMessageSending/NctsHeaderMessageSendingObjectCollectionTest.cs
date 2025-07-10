using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderMessageSendingObjectCollection))]
	class NctsHeaderMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NctsHeaderMessageSendingObjectCollection>
	{
		protected override NctsHeaderMessageSendingObjectCollection GetCollectionToTest() => new NctsHeaderMessageSendingObjectCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new NctsHeaderMessageSendingObject(Factory.NewDepartureNctsHeader());
	}
}
