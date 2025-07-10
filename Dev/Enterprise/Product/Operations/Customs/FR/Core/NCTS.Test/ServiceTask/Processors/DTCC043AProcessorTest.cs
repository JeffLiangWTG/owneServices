using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC043A;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC043AProcessorTest : DTBaseProcessorTest<Cc043AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT043A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewArrivalStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted,
					ExpectedNewDetailedDepartureStatus = ZString.Empty,
					ExpectedNewMessageInterpretation = @"
<p>New arrival status: Unloading Permission Granted</p>
<p>Status granted on: 11/04/2019 11:28</p>",
					HeaderAssertion = (x) =>
					{
						AssertEquals("11 TT 75", x.ArrivalMovementHeader.BM_TransportAtDeparture);
						AssertEquals("FR", x.ArrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry);

						AssertEquals(20m, x.ArrivalMovementHeader.BM_GrossWeight);

						AssertEquals(0, x.DepartureHeaderContainers.Count);
						AssertEquals(2, x.ArrivalMovementHeader.Seals.Count);
						AssertEquals("SEAL001", x.ArrivalMovementHeader.Seals[0].CY_Data);
						AssertEquals("SEAL002", x.ArrivalMovementHeader.Seals[1].CY_Data);

						AssertEquals(2, x.ArrivalMovementHeader.GoodsItems.Count);
						AssertEquals(2, x.UnloadingMovementHeader.GoodsItems.Count);
						var goodsItem1 = x.ArrivalMovementHeader.GoodsItems[0];
						var unloadinggoodsItem1 = x.UnloadingMovementHeader.GoodsItems[0];
						AssertEquals((ZShort)1, goodsItem1.BY_LineNo);
						AssertEquals("400200500", goodsItem1.BY_HarmonisedTariff);
						AssertEquals("T1", goodsItem1.BY_Type);
						AssertEquals("souliers rouges", goodsItem1.BY_Description);
						AssertEquals(10m, goodsItem1.BY_GrossWeight);
						AssertEquals(Core.Constants.Weight.Kilograms, goodsItem1.BY_GrossWeightUnit);
						AssertEquals(8m, goodsItem1.BY_NetWeight);
						AssertEquals(Core.Constants.Weight.Kilograms, goodsItem1.BY_NetWeightUnit);
						AssertEquals("AU", goodsItem1.BY_RN_NKCountryOfDispatch);
						AssertEquals("FR", goodsItem1.BY_RN_NKCountryOfDestination);

						AssertEquals(goodsItem1.BY_Description, unloadinggoodsItem1.BY_Description);

						var pds = goodsItem1.SupportingDocuments;
						AssertEquals(1, pds.Count);
						var pd1 = pds[0];
						AssertEquals("714", pd1.CSI_Code);
						AssertEquals("RX4", pd1.CSI_ReferenceNumber);
						AssertEquals("RX4 Details", pd1.CSI_Description);

						var sms = goodsItem1.SpecialMentions.ToList();
						AssertEquals(1, sms.Count);
						var sm1 = sms[0];
						AssertEquals("SM1", sm1.Statement);
						AssertEquals(true, sm1.ExportFromEC);
						AssertEquals("DE", sm1.ExportFromCountry);

						var cnts = goodsItem1.Containers.Select(y => y.ContainerNumber);
						AssertEquals("1111, 2222", ZString.Join(", ", cnts.ToArray()));

						var pkgs = goodsItem1.Packages;
						AssertEquals(1, pkgs.Count);
						var pkg1 = pkgs[0];
						AssertEquals("noir", pkg1.B5_MarksAndNumbers);
						AssertEquals("BX", pkg1.B5_UnitType);
						AssertEquals(10, pkg1.B5_UnitCount);

						var sgis = goodsItem1.SgiCodes.ToList();
						AssertEquals(1, sgis.Count);
						var sgi1 = sgis[0];
						AssertEquals("1", sgi1.Code);
						AssertEquals(12.4m, sgi1.Qty);

						var goodsItem2 = x.ArrivalMovementHeader.GoodsItems[1];
						AssertEquals(ZString.Replicate('A', 280), goodsItem2.BY_Description);
					}
				};
			}
		}
	}
}
