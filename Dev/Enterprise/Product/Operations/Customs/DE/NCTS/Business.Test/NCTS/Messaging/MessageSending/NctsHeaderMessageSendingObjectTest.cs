using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObject))]
sealed class NctsHeaderMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestValidation()
	{
		var messageSendingObject = (NctsHeaderMessageSendingObject)GetNewBusinessObject();

		AssertType<NctsHeaderMessageSendingObjectValidation>(messageSendingObject.Validation);
	}

	protected override BusinessObject GetNewBusinessObject() => new NctsHeaderMessageSendingObject(nctsHeader);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
	}

	NctsHeader nctsHeader;
}
