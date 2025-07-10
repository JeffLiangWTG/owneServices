using System;
using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class InventoryLinkingMovementTotalsResponseTests : TestCase
	{
		public void TestAllProperties()
		{
			var xml = CDSInventoryLinkingMovementTotalsResponseEDIMessage.Serialize(InventoryLinkingMovementTotalsResponseXMLForTest);
			var ilmtr = new InventoryLinkingMovementTotalsResponse(xml);
			AssertInventoryLinkingMovementTotalsResponseProperties(ilmtr);
		}

		internal static void AssertInventoryLinkingMovementTotalsResponseProperties(InventoryLinkingMovementTotalsResponse ilmtr)
		{
			AssertEquals(Enum.GetName(typeof(messageCodeSend), messageCodeSend.EMR), ilmtr.MessageCode);
			AssertEquals("CRC", ilmtr.Crc);
			AssertEquals("LOC", ilmtr.GoodsLocation);
			AssertEquals("M-UCR", ilmtr.MasterUcr);
			AssertEquals("2", ilmtr.DeclarationCount);
			AssertEquals("2018-08-08T08:08:08", ilmtr.GoodsArrivalDateTime);
			AssertEquals("shed", ilmtr.ShedOPID);
			AssertEquals("123", ilmtr.MovementReference);
			AssertEquals("H", ilmtr.MasterROE);
			AssertEquals("3", ilmtr.MasterSOE);
			AssertEquals(2, ilmtr.Entry.Count);

			AssertEquals("UCR1", ilmtr.Entry[0].UCR);
			AssertEquals("D", ilmtr.Entry[0].UCRType);
			AssertEquals("123", ilmtr.Entry[0].GoodsItem[0].CommodityCode);
			AssertEquals("1", ilmtr.Entry[0].GoodsItem[0].TotalPackages);
			AssertEquals(1.2m, ilmtr.Entry[0].GoodsItem[0].TotalNetMass);
			AssertEquals("456", ilmtr.Entry[0].GoodsItem[1].CommodityCode);
			AssertEquals("2", ilmtr.Entry[0].GoodsItem[1].TotalPackages);
			AssertEquals(2.3m, ilmtr.Entry[0].GoodsItem[1].TotalNetMass);
			AssertEquals("role1", ilmtr.Entry[0].SubmitRole);
			AssertEquals("ICS1", ilmtr.Entry[0].ICS);
			AssertEquals("6", ilmtr.Entry[0].ROE);
			AssertEquals("3", ilmtr.Entry[0].SOE);

			AssertEquals("UCR2", ilmtr.Entry[1].UCR);
			AssertEquals("D", ilmtr.Entry[1].UCRType);
			AssertEquals("789", ilmtr.Entry[1].GoodsItem[0].CommodityCode);
			AssertEquals("3", ilmtr.Entry[1].GoodsItem[0].TotalPackages);
			AssertEquals(3.4m, ilmtr.Entry[1].GoodsItem[0].TotalNetMass);
			AssertEquals("987", ilmtr.Entry[1].GoodsItem[1].CommodityCode);
			AssertEquals("4", ilmtr.Entry[1].GoodsItem[1].TotalPackages);
			AssertEquals(4.3m, ilmtr.Entry[1].GoodsItem[1].TotalNetMass);
			AssertEquals("role2", ilmtr.Entry[1].SubmitRole);
			AssertEquals("ICS2", ilmtr.Entry[1].ICS);
			AssertEquals("0", ilmtr.Entry[1].ROE);
			AssertEquals("D", ilmtr.Entry[1].SOE);
		}

		#region InventoryLinkingMovementTotalsResponseXMLForTest

		internal static inventoryLinkingMovementTotalsResponse InventoryLinkingMovementTotalsResponseXMLForTest
		{
			get
			{
				return new inventoryLinkingMovementTotalsResponse
				{
					messageCode = messageCodeSend.EMR,
					crc = "CRC",
					goodsLocation = "LOC",
					masterUCR = "M-UCR",
					declarationCount = "2",
					goodsArrivalDateTime = new DateTime(2018, 8, 8, 8, 8, 8),
					goodsArrivalDateTimeSpecified = true,
					shedOPID = "shed",
					movementReference = "123",
					masterROE = "H",
					masterSOE = "3",
					entry = new[]
					{
						new entry
						{
							ucrBlock = new ucrBlock
							{
								ucr = "UCR1",
								ucrType = ucrType.D
							},
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
							submitRole = "role1",
							entryStatus = new entryStatus
							{
								ics = "ICS1",
								roe = "6",
								soe = "3"
							},
						},
						new entry
						{
							ucrBlock = new ucrBlock
							{
								ucr = "UCR2",
								ucrType = ucrType.D
							},
							goodsItem = new[]
							{
								new goodsItem
								{
									commodityCode = "789",
									totalNetMass = 3.4m,
									totalNetMassSpecified = true,
									totalPackages = "3"
								},
								new goodsItem
								{
									commodityCode = "987",
									totalNetMass = 4.3m,
									totalNetMassSpecified = true,
									totalPackages = "4"
								}
							},
							submitRole = "role2",
							entryStatus = new entryStatus
							{
								ics = "ICS2",
								roe = "0",
								soe = "D"
							},
						}
					}
				};
			}
		}
		#endregion

		internal static string ExpectedHTMLInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>An EMR / ERS sent by the Inventory Linking Component</H3><p><strong>Message Code: </strong>EMR<br><strong>CRC: </strong>CRC<br><strong>Goods Location: </strong>LOC<br><strong>Master UCR: </strong>M-UCR<br><strong>Declaration Count: </strong>2<br><strong>Goods Arrival Date Time: </strong>2018-08-08T08:08:08<br><strong>Shed Operator: </strong>shed<br><strong>Movement Reference Number: </strong>123<br><strong>Master ROE: </strong>H<br><strong>Master SOE: </strong>3</p><H4>Entry: 1</H4><p><strong>UCR: </strong>UCR1<br><strong>UCR Type: </strong>D<br><strong>Submit Role: </strong>role1<br><strong>Entry Status ICS: </strong>ICS1<br><strong>Entry Status ROE: </strong>6 - No control required (The declaration has been risked and no control has been required.). CHIEF Equivalent: 6<br><strong>Entry Status SOE: </strong>3 - Declaration Clearance</p><p><strong>Goods Items</strong>:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Commodity Code</strong></td><td><strong>Total Packages</strong></td><td><strong>Total Net Mass</strong></td></tr><tr><td>123</td><td>1</td><td>1.2</td></tr><tr><td>456</td><td>2</td><td>2.3</td></tr></table></p><H4>Entry: 2</H4><p><strong>UCR: </strong>UCR2<br><strong>UCR Type: </strong>D<br><strong>Submit Role: </strong>role2<br><strong>Entry Status ICS: </strong>ICS2<br><strong>Entry Status ROE: </strong>0 - Risking not yet performed (The declaration is in a processing state which means that risking has not been performed yet.). CHIEF Equivalent: N/A<br><strong>Entry Status SOE: </strong>D - Departed</p><p><strong>Goods Items</strong>:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Commodity Code</strong></td><td><strong>Total Packages</strong></td><td><strong>Total Net Mass</strong></td></tr><tr><td>789</td><td>3</td><td>3.4</td></tr><tr><td>987</td><td>4</td><td>4.3</td></tr></table></p>";
	}
}
