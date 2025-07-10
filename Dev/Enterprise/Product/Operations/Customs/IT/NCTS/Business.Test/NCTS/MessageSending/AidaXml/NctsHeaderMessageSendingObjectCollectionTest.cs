using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObjectCollection))]
sealed class NctsHeaderMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NctsHeaderMessageSendingObjectCollection>
{
	protected override NctsHeaderMessageSendingObjectCollection GetCollectionToTest() => new NctsHeaderMessageSendingObjectCollection(Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection() => new NctsHeaderMessageSendingObject(nctsHeader);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
	}

	NctsHeader nctsHeader;
}
