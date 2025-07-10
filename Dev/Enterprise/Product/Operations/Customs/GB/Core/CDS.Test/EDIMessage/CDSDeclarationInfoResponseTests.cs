using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSDeclarationInfoResponseTests : TestCase
	{
		public void TestAllProperties()
		{
			var xml = CDSDeclarationInfoResponseEDIMessage.Serialize(CDSDeclarationInfoResponseXMLForTest);
			var ilqr = new DeclarationStatusResponse(xml);
			AssertDeclarationInfoQueryResponseProperties(ilqr);
		}

		internal static void AssertDeclarationInfoQueryResponseProperties(DeclarationStatusResponse ilqr)
		{
			AssertEquals(1, ilqr.DeclarationStatusDetails.Count);
			AssertEquals("20190702110757Z", ilqr.DeclarationStatusDetails[0].Declaration.AcceptanceDateTime.Item.Value);
			AssertEquals("19GBL4592NCOI21NR9", ilqr.DeclarationStatusDetails[0].Declaration.ID.Value);
			AssertEquals("1", ilqr.DeclarationStatusDetails[0].Declaration.VersionID.Value);
			AssertEquals("20190702110757Z", ilqr.DeclarationStatusDetails[0].Declaration.ReceivedDateTime.Item.Value);
			AssertEquals("20190702110757Z", ilqr.DeclarationStatusDetails[0].Declaration.GoodsReleasedDateTime.Item.Value);
			AssertEquals("6", ilqr.DeclarationStatusDetails[0].Declaration.ROE);
			AssertEquals("15", ilqr.DeclarationStatusDetails[0].Declaration.ICS);
			AssertEquals("000", ilqr.DeclarationStatusDetails[0].Declaration.IRC);
			AssertEquals("9", ilqr.DeclarationStatusDetails[0].Declaration1.FunctionCode.Value);
			AssertEquals("IMZ", ilqr.DeclarationStatusDetails[0].Declaration1.TypeCode.Value);
			AssertEquals("100", ilqr.DeclarationStatusDetails[0].Declaration1.GoodsItemQuantity.Value.ToString());
			AssertEquals("10", ilqr.DeclarationStatusDetails[0].Declaration1.TotalPackageQuantity.Value.ToString());
			AssertEquals("GB123456789012000", ilqr.DeclarationStatusDetails[0].Declaration1.Submitter.ID.Value);
			AssertEquals(2, ilqr.DeclarationStatusDetails[0].Declaration1.GoodsShipment.PreviousDocument.Length);
			AssertEquals("18GBAKZ81EQJ2FGVR", ilqr.DeclarationStatusDetails[0].Declaration1.GoodsShipment.PreviousDocument[0].ID.Value);
			AssertEquals("DCR", ilqr.DeclarationStatusDetails[0].Declaration1.GoodsShipment.PreviousDocument[0].TypeCode.Value);
			AssertEquals("18GBAKZ81EQJ2FGVA", ilqr.DeclarationStatusDetails[0].Declaration1.GoodsShipment.PreviousDocument[1].ID.Value);
			AssertEquals("MCR", ilqr.DeclarationStatusDetails[0].Declaration1.GoodsShipment.PreviousDocument[1].TypeCode.Value);
			AssertEquals("20GBAKZ81EQJ2WXYZ", ilqr.DeclarationStatusDetails[0].Declaration1.GoodsShipment.UCR.TraderAssignedReferenceID.Value);
		}

		public void TestParsingOfRealDISResponseWithMultipleDeclarationResponses()
		{
			var xml =
				@"<p:DeclarationStatusResponse 
				xsi:schemaLocation=""http://gov.uk/customs/declarationInformationRetrieval/status/v2"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:p4=""urn:un:unece:uncefact:data:standard:UnqualifiedDataType:6"" xmlns:p3=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"" xmlns:p2=""urn:wco:datamodel:WCO:DEC-DMS:2"" xmlns:p1=""urn:wco:datamodel:WCO:Response_DS:DMS:2"" xmlns:p=""http://gov.uk/customs/declarationInformationRetrieval/status/v2"">
					<p:DeclarationStatusDetails>
						<p:Declaration>
							<p:AcceptanceDateTime>
								<p1:DateTimeString formatCode=""304"">20200528145050Z</p1:DateTimeString>
							</p:AcceptanceDateTime>
							<p:ID>20GB5WCZQ922CFGVR9</p:ID>
							<p:VersionID>1</p:VersionID>
							<p:ReceivedDateTime>
								<p:DateTimeString formatCode=""304"">20200528145050Z</p:DateTimeString>
							</p:ReceivedDateTime>
							<p:ROE>H</p:ROE>
							<p:ICS>14</p:ICS>
						</p:Declaration>
						<p2:Declaration>
							<p2:FunctionCode>9</p2:FunctionCode>
							<p2:TypeCode>IMD</p2:TypeCode>
							<p2:GoodsItemQuantity>1</p2:GoodsItemQuantity>
							<p2:TotalPackageQuantity>55.0</p2:TotalPackageQuantity>
							<p2:Submitter>
								<p2:ID>GB8172025690</p2:ID>
							</p2:Submitter>
							<p2:GoodsShipment>
								<p2:PreviousDocument>
									<p2:ID>0GB896458895023-B00031398</p2:ID>
									<p2:TypeCode>DCR</p2:TypeCode>
								</p2:PreviousDocument>
								<p2:UCR>
									<p2:TraderAssignedReferenceID>0GB896458895023-B00031398</p2:TraderAssignedReferenceID>
								</p2:UCR>
							</p2:GoodsShipment>
						</p2:Declaration>
					</p:DeclarationStatusDetails>
					<p:DeclarationStatusDetails>
						<p:Declaration>
							<p:ID>20GB5XKJE313MFGVR1</p:ID>
							<p:VersionID>1</p:VersionID>
							<p:ReceivedDateTime>
								<p:DateTimeString formatCode=""304"">20200529110951Z</p:DateTimeString>
							</p:ReceivedDateTime>
							<p:ROE>H</p:ROE>
							<p:ICS>5</p:ICS>
						</p:Declaration>
						<p2:Declaration>
							<p2:FunctionCode>9</p2:FunctionCode>
							<p2:TypeCode>IMD</p2:TypeCode>
							<p2:GoodsItemQuantity>1</p2:GoodsItemQuantity>
							<p2:TotalPackageQuantity>55.0</p2:TotalPackageQuantity>
							<p2:Submitter>
								<p2:ID>GB8172025690</p2:ID>
							</p2:Submitter>
							<p2:GoodsShipment>
								<p2:PreviousDocument>
									<p2:ID>0GB896458895023-B00031398</p2:ID>
									<p2:TypeCode>DCR</p2:TypeCode>
								</p2:PreviousDocument>
								<p2:UCR>
									<p2:TraderAssignedReferenceID>0GB896458895023-B00031398/1</p2:TraderAssignedReferenceID>
								</p2:UCR>
							</p2:GoodsShipment>
						</p2:Declaration>
					</p:DeclarationStatusDetails>
					<p:DeclarationStatusDetails>
						<p:Declaration>
							<p:AcceptanceDateTime>
								<p1:DateTimeString formatCode=""304"">20200529110947Z</p1:DateTimeString>
							</p:AcceptanceDateTime>
							<p:ID>20GB5XKJAT03OFGVR7</p:ID>
							<p:VersionID>1</p:VersionID>
							<p:ReceivedDateTime>
								<p:DateTimeString formatCode=""304"">20200529110947Z</p:DateTimeString>
							</p:ReceivedDateTime>
							<p:ROE>H</p:ROE>
							<p:ICS>14</p:ICS>
						</p:Declaration>
						<p2:Declaration>
							<p2:FunctionCode>9</p2:FunctionCode>
							<p2:TypeCode>IMD</p2:TypeCode>
							<p2:GoodsItemQuantity>1</p2:GoodsItemQuantity>
							<p2:TotalPackageQuantity>55.0</p2:TotalPackageQuantity>
							<p2:Submitter>
								<p2:ID>GB8172025690</p2:ID>
							</p2:Submitter>
							<p2:GoodsShipment>
								<p2:PreviousDocument>
									<p2:ID>0GB896458895023-B00031398</p2:ID>
									<p2:TypeCode>DCR</p2:TypeCode>
								</p2:PreviousDocument>
								<p2:UCR>
									<p2:TraderAssignedReferenceID>0GB896458895023-B00031398/1</p2:TraderAssignedReferenceID>
								</p2:UCR>
							</p2:GoodsShipment>
						</p2:Declaration>
					</p:DeclarationStatusDetails>
				</p:DeclarationStatusResponse>";

			var disResponse = new DeclarationStatusResponse(xml);
			AssertEquals("Expected 3 Declaration Status Details", 3, disResponse.DeclarationStatusDetails.Count);
			AssertRealDISResponseDataVariations(disResponse.DeclarationStatusDetails, 0, "20200528145050Z", "20GB5WCZQ922CFGVR9", "20200528145050Z", "14", "0GB896458895023-B00031398");
			AssertRealDISResponseDataVariations(disResponse.DeclarationStatusDetails, 1, "", "20GB5XKJE313MFGVR1", "20200529110951Z", "5", "0GB896458895023-B00031398/1");
			AssertRealDISResponseDataVariations(disResponse.DeclarationStatusDetails, 2, "20200529110947Z", "20GB5XKJAT03OFGVR7", "20200529110947Z", "14", "0GB896458895023-B00031398/1");
		}

		void AssertRealDISResponseDataVariations(List<DeclarationStatusResponseDeclarationStatusDetails> declarationStatusDetailsList, int index, ZString acceptanceDateTime, ZString id, ZString receivedDateTime, ZString ics, ZString traderAssignedReferenceID)
		{
			var declarationStatusDetail = declarationStatusDetailsList[index];

			CombineAssertions($"Declaration Status Detail index {index} failed:", () =>
			{
				AssertEquals(acceptanceDateTime, declarationStatusDetail.Declaration.AcceptanceDateTime.Item.Value);
				AssertEquals(id, declarationStatusDetail.Declaration.ID.Value);
				AssertEquals(receivedDateTime, declarationStatusDetail.Declaration.ReceivedDateTime.Item.Value);
				AssertEquals(ics, declarationStatusDetail.Declaration.ICS);
				AssertEquals(traderAssignedReferenceID, declarationStatusDetail.Declaration1.GoodsShipment.UCR.TraderAssignedReferenceID.Value);
			});
		}

		#region CDSDeclarationInfoResponseXMLForTest

		internal static CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.DeclarationStatusResponse CDSDeclarationInfoResponseXMLForTest
		{
			get
			{
				return new CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.DeclarationStatusResponse
				{
					DeclarationStatusDetails = new[]
					{
						new DeclarationStatusResponseDeclarationStatusDetails
						{
							Declaration = new  DeclarationStatusResponseDeclarationStatusDetailsDeclaration
							{
								AcceptanceDateTime = new DeclarationAcceptanceDateTimeType
								{
									Item = new DeclarationAcceptanceDateTimeTypeDateTimeString
									{
										formatCode = FormatCodeType.Item304,
										Value = "20190702110757Z"
									}
								},
								ID = new DeclarationIdentificationIDType1
								{
									Value = "19GBL4592NCOI21NR9"
								},
								VersionID = new DeclarationVersionIDType
								{
									Value = "1"
								},
								ReceivedDateTime = new DeclarationStatusResponseDeclarationStatusDetailsDeclarationReceivedDateTime
								{
									Item = new DeclarationStatusResponseDeclarationStatusDetailsDeclarationReceivedDateTimeDateTimeString
									{
										formatCode = FormatCodeType.Item304,
										Value = "20190702110757Z"
									}
								},
								GoodsReleasedDateTime = new DeclarationStatusResponseDeclarationStatusDetailsDeclarationGoodsReleasedDateTime
								{
									Item = new DeclarationStatusResponseDeclarationStatusDetailsDeclarationGoodsReleasedDateTimeDateTimeString
									{
										formatCode = FormatCodeType.Item304,
										Value = "20190702110757Z"
									}
								},
								ROE = "6",
								ICS = "15",
								IRC = "000"
							},
							Declaration1 = new CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration
							{
								FunctionCode = new DeclarationFunctionCodeType
								{
									Value = "9"
								},
								TypeCode = new DeclarationTypeCodeType
								{
									Value = "IMZ"
								},
								GoodsItemQuantity = new DeclarationGoodsItemQuantityType
								{
									Value = 100
								},
								TotalPackageQuantity = new DeclarationTotalPackageQuantityType
								{
									Value = 10
								},
								Submitter = new DeclarationSubmitter
								{
									ID = new SubmitterIdentificationIDType
									{
										Value = "GB123456789012000"
									}
								},
								GoodsShipment = new DeclarationGoodsShipment
								{
									PreviousDocument = new[]
									{
										new DeclarationGoodsShipmentPreviousDocument
										{
											ID = new PreviousDocumentIdentificationIDType
											{
												Value = "18GBAKZ81EQJ2FGVR"
											},
											TypeCode = new PreviousDocumentTypeCodeType
											{
												Value = "DCR"
											}
										},
										new DeclarationGoodsShipmentPreviousDocument
										{
											ID = new PreviousDocumentIdentificationIDType
											{
												Value = "18GBAKZ81EQJ2FGVA"
											},
											TypeCode = new PreviousDocumentTypeCodeType
											{
												Value = "MCR"
											}
										}
									},
									UCR = new DeclarationGoodsShipmentUCR
									{
										TraderAssignedReferenceID = new UCRTraderAssignedReferenceIDType
										{
											Value = "20GBAKZ81EQJ2WXYZ"
										}
									}
								}
							}
						}
					}
				};
			}
		}
		#endregion

		internal static string ExpectedHTMLInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>A response to a CDS Declaration Query</H3><p><strong>Acceptance Date Time: </strong>02-Jul-19 11:07:57<br><strong>ID: </strong>19GBL4592NCOI21NR9<br><strong>Version ID: </strong>1<br><strong>Received Date Time: </strong>02-Jul-19 11:07:57<br><strong>ROE: </strong>6<br><strong>ICS: </strong>15 - Customs Position Determined. CHIEF Equivalent: N/A<br><strong>IRC: </strong>000<br><strong>Goods Released Date Time: </strong>02-Jul-19 11:07:57<br><strong>Function Code: </strong>9<br><strong>Type Code: </strong>IMZ<br><strong>Goods Item Quantity: </strong>100<br><strong>Total Package Quantity: </strong>10<br><strong>Submitter: </strong>GB123456789012000<br><strong>UCR: </strong>20GBAKZ81EQJ2WXYZ</p><ul><p><strong>Previous Documents</strong>:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>ID</strong></td><td><strong>Type Code</strong></td></tr><tr><td>18GBAKZ81EQJ2FGVR</td><td>DCR</td></tr><tr><td>18GBAKZ81EQJ2FGVA</td><td>MCR</td></tr></table></p></ul>";
	}
}
