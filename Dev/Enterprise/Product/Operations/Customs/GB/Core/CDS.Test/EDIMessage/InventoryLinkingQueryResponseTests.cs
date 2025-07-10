using System;
using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class InventoryLinkingQueryResponseTests : TestCase
	{
		public void TestAllProperties()
		{
			var xml = CDSInventoryLinkingQueryResponseEDIMessage.Serialize(InventoryLinkingQueryResponseXMLForTest);
			var ilqr = new InventoryLinkingQueryResponse(xml);
			AssertInventoryLinkingQueryResponseProperties(ilqr);
		}

		internal static void AssertInventoryLinkingQueryResponseProperties(InventoryLinkingQueryResponse ilqr)
		{
			AssertEquals("QUCR-1", ilqr.QueriedDUCR.UCR);
			AssertEquals("Declaration001", ilqr.QueriedDUCR.DeclarationID);
			AssertEquals("PARENTMUCR", ilqr.QueriedDUCR.ParentMUCR);
			AssertEquals("ICS", ilqr.QueriedDUCR.ICS);
			AssertEquals("6", ilqr.QueriedDUCR.ROE);
			AssertEquals("3", ilqr.QueriedDUCR.SOE);

			AssertEquals(2, ilqr.QueriedDUCR.Movement.Count);
			AssertEquals(nameof(messageCodeMovement.EAA), ilqr.QueriedDUCR.Movement[0].MessageCode);
			AssertEquals("LOC1", ilqr.QueriedDUCR.Movement[0].GoodsLocation);
			AssertEquals("2018-08-08T08:08:08", ilqr.QueriedDUCR.Movement[0].GoodsArrivalDateTime);
			AssertEquals("2018-09-09T09:09:09", ilqr.QueriedDUCR.Movement[0].GoodsDepartureDateTime);
			AssertEquals("MRN1", ilqr.QueriedDUCR.Movement[0].MovementReference);
			AssertEquals("TID1", ilqr.QueriedDUCR.Movement[0].TransportDetails.TransportID);
			AssertEquals("TMODE1", ilqr.QueriedDUCR.Movement[0].TransportDetails.TransportMode);
			AssertEquals("GB", ilqr.QueriedDUCR.Movement[0].TransportDetails.TransportNationality);
			AssertEquals(nameof(messageCodeMovement.EAL), ilqr.QueriedDUCR.Movement[1].MessageCode);
			AssertEquals("LOC2", ilqr.QueriedDUCR.Movement[1].GoodsLocation);
			AssertEquals("2018-08-08T08:08:08", ilqr.QueriedDUCR.Movement[1].GoodsArrivalDateTime);
			AssertEquals("2018-09-09T09:09:09", ilqr.QueriedDUCR.Movement[1].GoodsDepartureDateTime);
			AssertEquals("MRN2", ilqr.QueriedDUCR.Movement[1].MovementReference);
			AssertEquals("TID2", ilqr.QueriedDUCR.Movement[1].TransportDetails.TransportID);
			AssertEquals("TMODE2", ilqr.QueriedDUCR.Movement[1].TransportDetails.TransportMode);
			AssertEquals("FR", ilqr.QueriedDUCR.Movement[1].TransportDetails.TransportNationality);

			AssertEquals(4, ilqr.QueriedDUCR.GoodsItem.Count);
			AssertEquals("1", ilqr.QueriedDUCR.GoodsItem[0].TotalPackages);
			AssertEquals("2", ilqr.QueriedDUCR.GoodsItem[1].TotalPackages);
			AssertEquals("3", ilqr.QueriedDUCR.GoodsItem[2].TotalPackages);
			AssertEquals("4", ilqr.QueriedDUCR.GoodsItem[3].TotalPackages);

			AssertEquals(6, ilqr.ChildMUCR.Count);
			AssertEquals("MUCR-11", ilqr.ChildMUCR[0].UCR);
			AssertEquals(true, ilqr.ChildMUCR[0].Shut);
			AssertEquals("QUCR-1", ilqr.ChildMUCR[0].ParentMUCR);
			AssertEquals("ICS", ilqr.ChildMUCR[0].ICS);
			AssertEquals("6", ilqr.ChildMUCR[0].ROE);
			AssertEquals("3", ilqr.ChildMUCR[0].SOE);
			AssertEquals(1, ilqr.ChildMUCR[0].Movement.Count);
			AssertEquals(nameof(messageCodeMovement.EAA), ilqr.ChildMUCR[0].Movement[0].MessageCode);
			AssertEquals("LOC3", ilqr.ChildMUCR[0].Movement[0].GoodsLocation);
			AssertEquals("2018-08-08T08:08:08", ilqr.ChildMUCR[0].Movement[0].GoodsArrivalDateTime);
			AssertEquals("2018-09-09T09:09:09", ilqr.ChildMUCR[0].Movement[0].GoodsDepartureDateTime);
			AssertEquals("MRN3", ilqr.ChildMUCR[0].Movement[0].MovementReference);
			AssertEquals("TID3", ilqr.ChildMUCR[0].Movement[0].TransportDetails.TransportID);
			AssertEquals("TMODE3", ilqr.ChildMUCR[0].Movement[0].TransportDetails.TransportMode);
			AssertEquals("GB", ilqr.ChildMUCR[0].Movement[0].TransportDetails.TransportNationality);
			AssertEquals("MUCR-X1", ilqr.ChildMUCR[1].UCR);
			AssertEquals(true, ilqr.ChildMUCR[1].Shut);
			AssertEquals("MUCR-11", ilqr.ChildMUCR[1].ParentMUCR);
			AssertEquals("MUCR-X2", ilqr.ChildMUCR[2].UCR);
			AssertEquals(true, ilqr.ChildMUCR[2].Shut);
			AssertEquals("MUCR-11", ilqr.ChildMUCR[2].ParentMUCR);
			AssertEquals("MUCR-22", ilqr.ChildMUCR[3].UCR);
			AssertEquals(true, ilqr.ChildMUCR[3].Shut);
			AssertEquals("QUCR-1", ilqr.ChildMUCR[3].ParentMUCR);
			AssertEquals("MUCR-Y1", ilqr.ChildMUCR[4].UCR);
			AssertEquals(true, ilqr.ChildMUCR[4].Shut);
			AssertEquals("MUCR-22", ilqr.ChildMUCR[4].ParentMUCR);
			AssertEquals("MUCR-Y2", ilqr.ChildMUCR[5].UCR);
			AssertEquals(true, ilqr.ChildMUCR[5].Shut);
			AssertEquals("MUCR-22", ilqr.ChildMUCR[5].ParentMUCR);

			AssertEquals(6, ilqr.ChildDUCR.Count);
			AssertEquals("DUCR-X3", ilqr.ChildDUCR[0].UCR);
			AssertEquals("DEC-X3", ilqr.ChildDUCR[0].DeclarationID);
			AssertEquals("MUCR-11", ilqr.ChildDUCR[0].ParentMUCR);
			AssertEquals("ICS", ilqr.ChildDUCR[0].ICS);
			AssertEquals("H", ilqr.ChildDUCR[0].ROE);
			AssertEquals("9", ilqr.ChildDUCR[0].SOE);
			AssertEquals(1, ilqr.ChildDUCR[0].Movement.Count);
			AssertEquals(nameof(messageCodeMovement.EAA), ilqr.ChildDUCR[0].Movement[0].MessageCode);
			AssertEquals("LOC4", ilqr.ChildDUCR[0].Movement[0].GoodsLocation);
			AssertEquals("2018-08-08T08:08:08", ilqr.ChildDUCR[0].Movement[0].GoodsArrivalDateTime);
			AssertEquals("2018-09-09T09:09:09", ilqr.ChildDUCR[0].Movement[0].GoodsDepartureDateTime);
			AssertEquals("MRN4", ilqr.ChildDUCR[0].Movement[0].MovementReference);
			AssertEquals("TID4", ilqr.ChildDUCR[0].Movement[0].TransportDetails.TransportID);
			AssertEquals("TMODE4", ilqr.ChildDUCR[0].Movement[0].TransportDetails.TransportMode);
			AssertEquals("GB", ilqr.ChildDUCR[0].Movement[0].TransportDetails.TransportNationality);
			AssertEquals(3, ilqr.ChildDUCR[0].GoodsItem.Count);
			AssertEquals("5", ilqr.ChildDUCR[0].GoodsItem[0].TotalPackages);
			AssertEquals("6", ilqr.ChildDUCR[0].GoodsItem[1].TotalPackages);
			AssertEquals("7", ilqr.ChildDUCR[0].GoodsItem[2].TotalPackages);
			AssertEquals("DUCR-X4", ilqr.ChildDUCR[1].UCR);
			AssertEquals("DEC-X4", ilqr.ChildDUCR[1].DeclarationID);
			AssertEquals("MUCR-11", ilqr.ChildDUCR[1].ParentMUCR);
			AssertEquals("DUCR-Y3", ilqr.ChildDUCR[2].UCR);
			AssertEquals("DEC-Y3", ilqr.ChildDUCR[2].DeclarationID);
			AssertEquals("MUCR-22", ilqr.ChildDUCR[2].ParentMUCR);
			AssertEquals("DUCR-Y4", ilqr.ChildDUCR[3].UCR);
			AssertEquals("DEC-Y4", ilqr.ChildDUCR[3].DeclarationID);
			AssertEquals("MUCR-22", ilqr.ChildDUCR[3].ParentMUCR);
			AssertEquals("DUCR111", ilqr.ChildDUCR[4].UCR);
			AssertEquals("DEC111", ilqr.ChildDUCR[4].DeclarationID);
			AssertEquals("QUCR-1", ilqr.ChildDUCR[4].ParentMUCR);
			AssertEquals("DUCR222", ilqr.ChildDUCR[5].UCR);
			AssertEquals("DEC222", ilqr.ChildDUCR[5].DeclarationID);
			AssertEquals("QUCR-1", ilqr.ChildDUCR[5].ParentMUCR);

			AssertEquals("UCR-X", ilqr.ParentMUCR.UCR);
			AssertEquals(true, ilqr.ParentMUCR.Shut);
			AssertEquals("UCR-Y", ilqr.ParentMUCR.ParentMUCR);
			AssertEquals("ICS", ilqr.ParentMUCR.ICS);
			AssertEquals("0", ilqr.ParentMUCR.ROE);
			AssertEquals("E", ilqr.ParentMUCR.SOE);
			AssertEquals(2, ilqr.ParentMUCR.Movement.Count);
			AssertEquals(nameof(messageCodeMovement.EAA), ilqr.ParentMUCR.Movement[0].MessageCode);
			AssertEquals("LOC5", ilqr.ParentMUCR.Movement[0].GoodsLocation);
			AssertEquals("2018-08-08T08:08:08", ilqr.ParentMUCR.Movement[0].GoodsArrivalDateTime);
			AssertEquals("2018-09-09T09:09:09", ilqr.ParentMUCR.Movement[0].GoodsDepartureDateTime);
			AssertEquals("MRN-X1", ilqr.ParentMUCR.Movement[0].MovementReference);
			AssertEquals("TID5", ilqr.ParentMUCR.Movement[0].TransportDetails.TransportID);
			AssertEquals("TMODE5", ilqr.ParentMUCR.Movement[0].TransportDetails.TransportMode);
			AssertEquals("GB", ilqr.ParentMUCR.Movement[0].TransportDetails.TransportNationality);
			AssertEquals(nameof(messageCodeMovement.EAL), ilqr.ParentMUCR.Movement[1].MessageCode);
			AssertEquals("LOC6", ilqr.ParentMUCR.Movement[1].GoodsLocation);
			AssertEquals("2018-08-08T08:08:08", ilqr.ParentMUCR.Movement[1].GoodsArrivalDateTime);
			AssertEquals("2018-09-09T09:09:09", ilqr.ParentMUCR.Movement[1].GoodsDepartureDateTime);
			AssertEquals("MRN-X2", ilqr.ParentMUCR.Movement[1].MovementReference);
			AssertEquals("TID6", ilqr.ParentMUCR.Movement[1].TransportDetails.TransportID);
			AssertEquals("TMODE6", ilqr.ParentMUCR.Movement[1].TransportDetails.TransportMode);
			AssertEquals("FR", ilqr.ParentMUCR.Movement[1].TransportDetails.TransportNationality);
		}

		#region InventoryLinkingQueryResponseXMLForTest
		internal static inventoryLinkingQueryResponse InventoryLinkingQueryResponseXMLForTest
		{
			get
			{
				return new inventoryLinkingQueryResponse
				{
					queriedDUCR = new DUCRObject
					{
						UCR = "QUCR-1",
						declarationID = "Declaration001",
						parentMUCR = "PARENTMUCR",
						entryStatus = new entryStatus
						{
							ics = "ICS",
							roe = "6",
							soe = "3"
						},
						movement = new[]
						{
							new movementObject
							{
								messageCode = messageCodeMovement.EAA,
								goodsLocation = "LOC1",
								goodsArrivalDateTime = new DateTime(2018, 8, 8, 8, 8, 8),
								goodsArrivalDateTimeSpecified = true,
								goodsDepartureDateTime = new DateTime(2018, 9, 9, 9, 9, 9),
								goodsDepartureDateTimeSpecified = true,
								movementReference = "MRN1",
								transportDetails = new transportDetails
								{
									transportID = "TID1",
									transportMode = "TMODE1",
									transportNationality = "GB"
								},
							},
							new movementObject
							{
								messageCode = messageCodeMovement.EAL,
								goodsLocation = "LOC2",
								goodsArrivalDateTime = new DateTime(2018, 8, 8, 8, 8, 8),
								goodsArrivalDateTimeSpecified = true,
								goodsDepartureDateTime = new DateTime(2018, 9, 9, 9, 9, 9),
								goodsDepartureDateTimeSpecified = true,
								movementReference = "MRN2",
								transportDetails = new transportDetails
								{
									transportID = "TID2",
									transportMode = "TMODE2",
									transportNationality = "FR"
								},
							},
						},
						goodsItem = new[]
						{
							new goodsItemObject
							{
								totalPackages = "1"
							},
							new goodsItemObject
							{
								totalPackages = "2"
							},
							new goodsItemObject
							{
								totalPackages = "3"
							},
							new goodsItemObject
							{
								totalPackages = "4"
							}
						},
					},
					childMUCR = new[]
					{
						new MUCRObject
						{
							UCR = "MUCR-11",
							shut = true,
							parentMUCR = "QUCR-1",
							entryStatus = new entryStatus
							{
								ics = "ICS",
								roe = "6",
								soe = "3"
							},
							movement = new[]
							{
								new movementObject
								{
									messageCode = messageCodeMovement.EAA,
									goodsLocation = "LOC3",
									goodsArrivalDateTime = new DateTime(2018, 8, 8, 8, 8, 8),
									goodsArrivalDateTimeSpecified = true,
									goodsDepartureDateTime = new DateTime(2018, 9, 9, 9, 9, 9),
									goodsDepartureDateTimeSpecified = true,
									movementReference = "MRN3",
									transportDetails = new transportDetails
									{
										transportID = "TID3",
										transportMode = "TMODE3",
										transportNationality = "GB"
									},
								},
							},
						},
						new MUCRObject
						{
							UCR = "MUCR-X1",
							shut = true,
							parentMUCR = "MUCR-11"
						},
						new MUCRObject
						{
							UCR = "MUCR-X2",
							shut = true,
							parentMUCR = "MUCR-11"
						},
						new MUCRObject
						{
							UCR = "MUCR-22",
							shut = true,
							parentMUCR = "QUCR-1"
						},
						new MUCRObject
						{
							UCR = "MUCR-Y1",
							shut = true,
							parentMUCR = "MUCR-22"
						},
						new MUCRObject
						{
							UCR = "MUCR-Y2",
							shut = true,
							parentMUCR = "MUCR-22"
						},
					},
					childDUCR = new[]
					{
						new DUCRObject
						{
							UCR = "DUCR-X3",
							declarationID = "DEC-X3",
							parentMUCR = "MUCR-11",
							entryStatus = new entryStatus
							{
								ics = "ICS",
								roe = "H",
								soe = "9"
							},
							movement = new[]
							{
								new movementObject
								{
									messageCode = messageCodeMovement.EAA,
									goodsLocation = "LOC4",
									goodsArrivalDateTime = new DateTime(2018, 8, 8, 8, 8, 8),
									goodsArrivalDateTimeSpecified = true,
									goodsDepartureDateTime = new DateTime(2018, 9, 9, 9, 9, 9),
									goodsDepartureDateTimeSpecified = true,
									movementReference = "MRN4",
									transportDetails = new transportDetails
									{
										transportID = "TID4",
										transportMode = "TMODE4",
										transportNationality = "GB"
									},
								},
							},
							goodsItem = new[]
							{
								new goodsItemObject
								{
									totalPackages = "5"
								},
								new goodsItemObject
								{
									totalPackages = "6"
								},
								new goodsItemObject
								{
									totalPackages = "7"
								},
							},
						},
						new DUCRObject
						{
							UCR = "DUCR-X4",
							declarationID = "DEC-X4",
							parentMUCR = "MUCR-11"
						},
						new DUCRObject
						{
							UCR = "DUCR-Y3",
							declarationID = "DEC-Y3",
							parentMUCR = "MUCR-22"
						},
						new DUCRObject
						{
							UCR = "DUCR-Y4",
							declarationID = "DEC-Y4",
							parentMUCR = "MUCR-22"
						},
						new DUCRObject {
							UCR = "DUCR111",
							declarationID = "DEC111",
							parentMUCR = "QUCR-1"
						},
						new DUCRObject
						{
							UCR = "DUCR222",
							declarationID = "DEC222",
							parentMUCR = "QUCR-1"
						},
					},
					parentMUCR = new MUCRObject
					{
						UCR = "UCR-X",
						shut = true,
						parentMUCR = "UCR-Y",
						entryStatus = new entryStatus
						{
							ics = "ICS",
							roe = "0",
							soe = "E"
						},
						movement = new[]
						{
							new movementObject
							{
								messageCode = messageCodeMovement.EAA,
								goodsLocation = "LOC5",
								goodsArrivalDateTime = new DateTime(2018, 8, 8, 8, 8, 8),
								goodsArrivalDateTimeSpecified = true,
								goodsDepartureDateTime = new DateTime(2018, 9, 9, 9, 9, 9),
								goodsDepartureDateTimeSpecified = true,
								movementReference = "MRN-X1",
								transportDetails = new transportDetails
								{
									transportID = "TID5",
									transportMode = "TMODE5",
									transportNationality = "GB"
								},
							},
							new movementObject
							{
								messageCode = messageCodeMovement.EAL,
								goodsLocation = "LOC6",
								goodsArrivalDateTime = new DateTime(2018, 8, 8, 8, 8, 8),
								goodsArrivalDateTimeSpecified = true,
								goodsDepartureDateTime = new DateTime(2018, 9, 9, 9, 9, 9),
								goodsDepartureDateTimeSpecified = true,
								movementReference = "MRN-X2",
								transportDetails = new transportDetails
								{
									transportID = "TID6",
									transportMode = "TMODE6",
									transportNationality = "FR"
								},
							},
						},
					},
				};
			}
		}
		#endregion

		internal static string ExpectedHTMLInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>A response to an inventory linking query</H3><H4>The response includes the queried MUCR: 1/ children (the whole subtree with all MUCRs and DUCRs with declarations); 2/ all movements of the queried MUCR; 3/ all parents with their movements</H4><p><strong>Queried UCR: </strong>QUCR-1<br><strong>Queried UCR Type: </strong>D<br><strong>Declaration ID: </strong>Declaration001<br><strong>Parent UCR: </strong>PARENTMUCR<br><strong>Entry Status ICS: </strong>ICS<br><strong>Entry Status ROE: </strong>6 - No control required (The declaration has been risked and no control has been required.). CHIEF Equivalent: 6<br><strong>Entry Status SOE: </strong>3 - Declaration Clearance</p><H4>Movement: 1</H4><p><strong>Message Code: </strong>EAA<br><strong>Goods Location: </strong>LOC1<br><strong>Goods Arrival Date: </strong>2018-08-08T08:08:08<br><strong>Goods Departure Date: </strong>2018-09-09T09:09:09<br><strong>Movement Reference: </strong>MRN1<br><strong>Transport ID: </strong>TID1<br><strong>Transport Mode: </strong>TMODE1<br><strong>Transport Nationality: </strong>GB</p><H4>Movement: 2</H4><p><strong>Message Code: </strong>EAL<br><strong>Goods Location: </strong>LOC2<br><strong>Goods Arrival Date: </strong>2018-08-08T08:08:08<br><strong>Goods Departure Date: </strong>2018-09-09T09:09:09<br><strong>Movement Reference: </strong>MRN2<br><strong>Transport ID: </strong>TID2<br><strong>Transport Mode: </strong>TMODE2<br><strong>Transport Nationality: </strong>FR</p><p><strong>Goods Items</strong>:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Total Packages</strong></td></tr><tr><td>1</td></tr><tr><td>2</td></tr><tr><td>3</td></tr><tr><td>4</td></tr></table></p><ul><p><strong>Child MUCRs</strong>:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>MUCR</strong></td><td><strong>Shut</strong></td><td><strong>Parent MUCR</strong></td><td><strong>Entry Status ICS</strong></td><td><strong>Entry Status ROE</strong></td><td><strong>Entry Status SOE</strong></td></tr><tr><td>MUCR-11</td><td>Y</td><td>QUCR-1</td><td>ICS</td><td>6 - No control required (The declaration has been risked and no control has been required.). CHIEF Equivalent: 6</td><td>3 - The consolidation is closed and all underlying declarations have P2P.</td></tr><tr><td colspan=\"6\"><H4>Movement: 1</H4><p><strong>Message Code: </strong>EAA<br><strong>Goods Location: </strong>LOC3<br><strong>Goods Arrival Date: </strong>2018-08-08T08:08:08<br><strong>Goods Departure Date: </strong>2018-09-09T09:09:09<br><strong>Movement Reference: </strong>MRN3<br><strong>Transport ID: </strong>TID3<br><strong>Transport Mode: </strong>TMODE3<br><strong>Transport Nationality: </strong>GB</p></td></tr><tr><td>MUCR-X1</td><td>Y</td><td>MUCR-11</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>MUCR-X2</td><td>Y</td><td>MUCR-11</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>MUCR-22</td><td>Y</td><td>QUCR-1</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>MUCR-Y1</td><td>Y</td><td>MUCR-22</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>MUCR-Y2</td><td>Y</td><td>MUCR-22</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table></p></ul><ul><p><strong>Child DUCRs</strong>:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>DUCR</strong></td><td><strong>Declaration ID</strong></td><td><strong>Parent MUCR</strong></td><td><strong>Entry Status ICS</strong></td><td><strong>Entry Status ROE</strong></td><td><strong>Entry Status SOE</strong></td></tr><tr><td>DUCR-X3</td><td>DEC-X3</td><td>MUCR-11</td><td>ICS</td><td>H - Pre-Lodge Prefix (The declaration is an advance/pre-lodged type, and as such the routing is provisional only.). CHIEF Equivalent: H</td><td>9 - Declaration Acceptance</td></tr><tr><td colspan=\"6\"><H4>Movement: 1</H4><p><strong>Message Code: </strong>EAA<br><strong>Goods Location: </strong>LOC4<br><strong>Goods Arrival Date: </strong>2018-08-08T08:08:08<br><strong>Goods Departure Date: </strong>2018-09-09T09:09:09<br><strong>Movement Reference: </strong>MRN4<br><strong>Transport ID: </strong>TID4<br><strong>Transport Mode: </strong>TMODE4<br><strong>Transport Nationality: </strong>GB</p></td></tr><tr><td colspan=\"6\"><p><strong>Goods Items</strong>:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Total Packages</strong></td></tr><tr><td>5</td></tr><tr><td>6</td></tr><tr><td>7</td></tr></table></p></td></tr><tr><td>DUCR-X4</td><td>DEC-X4</td><td>MUCR-11</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>DUCR-Y3</td><td>DEC-Y3</td><td>MUCR-22</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>DUCR-Y4</td><td>DEC-Y4</td><td>MUCR-22</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>DUCR111</td><td>DEC111</td><td>QUCR-1</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td>DUCR222</td><td>DEC222</td><td>QUCR-1</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table></p></ul><H4>Parent MUCR:</H4><p><strong>Queried UCR: </strong>UCR-X<br><strong>Queried UCR Type: </strong>M<br><strong>Shut: </strong>Y<br><strong>Parent UCR: </strong>UCR-Y<br><strong>Entry Status ICS: </strong>ICS<br><strong>Entry Status ROE: </strong>0 - Risking not yet performed (The declaration is in a processing state which means that risking has not been performed yet.). CHIEF Equivalent: N/A<br><strong>Entry Status SOE: </strong>E - The MUCR has been created but contains no declarations</p><H4>Movement: 1</H4><p><strong>Message Code: </strong>EAA<br><strong>Goods Location: </strong>LOC5<br><strong>Goods Arrival Date: </strong>2018-08-08T08:08:08<br><strong>Goods Departure Date: </strong>2018-09-09T09:09:09<br><strong>Movement Reference: </strong>MRN-X1<br><strong>Transport ID: </strong>TID5<br><strong>Transport Mode: </strong>TMODE5<br><strong>Transport Nationality: </strong>GB</p><H4>Movement: 2</H4><p><strong>Message Code: </strong>EAL<br><strong>Goods Location: </strong>LOC6<br><strong>Goods Arrival Date: </strong>2018-08-08T08:08:08<br><strong>Goods Departure Date: </strong>2018-09-09T09:09:09<br><strong>Movement Reference: </strong>MRN-X2<br><strong>Transport ID: </strong>TID6<br><strong>Transport Mode: </strong>TMODE6<br><strong>Transport Nationality: </strong>FR</p>";
	}
}
