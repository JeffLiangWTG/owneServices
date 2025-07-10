using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class InventoryLinkingControlResponseTests : TestCase
	{
		public void TestAllProperties()
		{
			var xml = CDSInventoryLinkingControlResponseEDIMessage.Serialize(new inventoryLinkingControlResponse
			{
				actionCode = "01",
				error = new errorBlock[]
				{
					new errorBlock
					{
						errorCode = "1"
					}
				},
				messageCode = messageCodeAll.EAA,
				movementReference = "123",
				ucr = new ucrBlock
				{
					ucr = "UCR",
					ucrType = ucrType.M
				}
			});

			var ilcr = new InventoryLinkingControlResponse(xml);
			AssertEquals("01", ilcr.ActionCode);
			AssertEquals("1", ilcr.ErrorCodes[0]);
			AssertEquals("EAA", ilcr.MessageCode);
			AssertEquals("123", ilcr.MovementReference);
			AssertEquals("UCR", ilcr.UCR);
			AssertEquals("M", ilcr.UCRType);
		}
	}
}
