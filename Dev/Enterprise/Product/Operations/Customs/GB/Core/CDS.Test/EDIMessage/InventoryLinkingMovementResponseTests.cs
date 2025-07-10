using System;
using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class InventoryLinkingMovementResponseTests : TestCase
	{
		public void TestAllProperties()
		{
			var xml = CDSInventoryLinkingMovementResponseEDIMessage.Serialize(InventoryLinkingMovementResponseXMLForTest);
			var ilmr = new InventoryLinkingMovementResponse(xml);
			AssertInventoryLinkingMovementResponseProperties(ilmr);
		}

		internal static void AssertInventoryLinkingMovementResponseProperties(InventoryLinkingMovementResponse ilmr)
		{
			AssertEquals("CRC", ilmr.Crc);
			AssertEquals(Enum.GetName(typeof(messageCodeMovement), messageCodeMovement.EAA), ilmr.MessageCode);
			AssertEquals("2018-08-08T08:08:08", ilmr.GoodsArrivalDateTime);
			AssertEquals("LOC", ilmr.GoodsLocation);
			AssertEquals("shed", ilmr.ShedOPID);
			AssertEquals("123", ilmr.MovementReference);
			AssertEquals("rol", ilmr.SubmitRole);
			AssertEquals("UCR", ilmr.UCR);
			AssertEquals("D", ilmr.UCRType);
			AssertEquals(2, ilmr.GoodsItem.Count);
			AssertEquals("123", ilmr.GoodsItem[0].CommodityCode);
			AssertEquals("1", ilmr.GoodsItem[0].TotalPackages);
			AssertEquals(1.2m, ilmr.GoodsItem[0].TotalNetMass);
			AssertEquals("456", ilmr.GoodsItem[1].CommodityCode);
			AssertEquals("2", ilmr.GoodsItem[1].TotalPackages);
			AssertEquals(2.3m, ilmr.GoodsItem[1].TotalNetMass);
			AssertEquals("ICS", ilmr.ICS);
			AssertEquals("6", ilmr.ROE);
			AssertEquals("3", ilmr.SOE);
		}

		#region InventoryLinkingMovementResponseXMLForTest

		internal static inventoryLinkingMovementResponse InventoryLinkingMovementResponseXMLForTest
		{
			get
			{
				return new inventoryLinkingMovementResponse
				{
					crc = "CRC",
					entryStatus = new entryStatus
					{
						ics = "ICS",
						roe = "6",
						soe = "3"
					},
					goodsArrivalDateTime = new DateTime(2018, 8, 8, 8, 8, 8),
					goodsArrivalDateTimeSpecified = true,
					goodsItem = new[]
					{
						new goodsItem
						{
							commodityCode = "123",
							totalNetMass = 1.2m,
							totalNetMassSpecified = true,
							totalPackages = "1"
						},
						new goodsItem
						{
							commodityCode = "456",
							totalNetMass = 2.3m,
							totalNetMassSpecified = true,
							totalPackages = "2"
						}
					},
					goodsLocation = "LOC",
					messageCode = messageCodeMovement.EAA,
					movementReference = "123",
					shedOPID = "shed",
					submitRole = "rol",
					ucrBlock = new ucrBlock
					{
						ucr = "UCR",
						ucrType = ucrType.D
					}
				};
			}
		}
		#endregion

		internal static string ExpectedHTMLInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>A response to a movement request message of type (EAL, EAA, EDL)</H3><p><strong>Message Code: </strong>EAA<br><strong>CRC: </strong>CRC<br><strong>Goods Arrival Date Time: </strong>2018-08-08T08:08:08<br><strong>Goods Location: </strong>LOC<br><strong>Shed Operator: </strong>shed<br><strong>Movement Reference Number: </strong>123<br><strong>Submit Role: </strong>rol<br><strong>UCR: </strong>UCR<br><strong>UCR Type: </strong>D<br><strong>Entry Status ICS: </strong>ICS<br><strong>Entry Status ROE: </strong>6 - No control required (The declaration has been risked and no control has been required.). CHIEF Equivalent: 6<br><strong>Entry Status SOE: </strong>3 - Non-Blocking Documentary Control (A documentary check has been requested, but the goods do not need to be held.). CHIEF Equivalent: 3</p><p><strong>Goods Items</strong>:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Commodity Code</strong></td><td><strong>Total Packages</strong></td><td><strong>Total Net Mass</strong></td></tr><tr><td>123</td><td>1</td><td>1.2</td></tr><tr><td>456</td><td>2</td><td>2.3</td></tr></table></p>";
	}
}
