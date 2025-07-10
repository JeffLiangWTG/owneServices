using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks.Test
{
	class NctsIE43ArrivalItemsParserTest : TestCaseWithFactory
	{
		public void TestParse()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList();
			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			NCTSTestHelper.SetMrnForTest(nctsHeader, "14GB000060100C7574");

			var ie43Wrapper = new IE43Wrapper
			{
				MeansOfTransportAtDepartureIdentity = "AIR",
				MeansOfTransportAtDepartureNationality = "AU",
				TotalGrossMass = 1000,
				TotalGrossMassUQ = "KG",
				Seals = new ZString[] { "SEL1", "SEL2", "SEL3" },
				GoodsItems = new[]
				{
					new IE43GoodsItemWrapper
					{
						ItemNumber = 1,
						CommodityCode = "4000",
						DeclarationType = "IMP",
						DescriptionOfGoods = new string('X', 290),
						GrossWeight = 100,
						GrossWeightUQ = "KG",
						NetWeight = 90,
						NetWeightUQ = "KG",
						CountryOfDispatch = "AU",
						CountryOfDestination = "FR",
						ProducedDocumentsCertificates = new []
						{
							new IE43ProducedDocumentsCertificateWrapper
							{
								Code = "PD1",
								Description = "Produced Document 1",
								ReferenceNumber = "PD0001"
							},
							new IE43ProducedDocumentsCertificateWrapper
							{
								Code = "PD2",
								Description = "Produced Document 2",
								ReferenceNumber = "PD0002"
							}
						},
						SpecialMentions = new []
						{
							new IE43SpecialMentionWrapper
							{
								Code = "SM1",
								Description = "Special Mention 1",
								NctsExportFromEC = true,
								CountryCode = "US"
							},
							new IE43SpecialMentionWrapper
							{
								Code = "SM2",
								Description = "Special Mention 2",
								NctsExportFromEC = false,
								CountryCode = "CA"
							}
						},
						Containers = new ZString[] { "CNT1", "CNT2", "CNT3" },
						Packages = new []
						{
							new IE43PackageWrapper
							{
								MarksAndNumbers = "PKG1",
								UnitCount = 10,
								UnitType = "BG"
							},
							new IE43PackageWrapper
							{
								MarksAndNumbers = "PKG2",
								UnitType = "BG",
								NumberOfPieces = 20
							}
						},
						SgiCodes = new []
						{
							new IE43SgiCodeWrapper
							{
								Code = "SGI1",
								Description = "30"
							},
							new IE43SgiCodeWrapper
							{
								Code = "SGI2",
								Description = "60"
							}
						}
					},
					new IE43GoodsItemWrapper
					{
						ItemNumber = 2
					}
				}
			};
			new NctsIE43ArrivalItemsParser(nctsHeader, ie43Wrapper).Parse();

			AssertEquals("AIR", nctsHeader.ArrivalMovementHeader.BM_TransportAtDeparture);
			AssertEquals("AU", nctsHeader.ArrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry);
			AssertEquals(1000m, nctsHeader.ArrivalMovementHeader.BM_GrossWeight);
			AssertEquals("KG", nctsHeader.ArrivalMovementHeader.BM_GrossWeightUQ);

			AssertEquals(0, nctsHeader.DepartureHeaderContainers.Count);
			AssertEquals(3, nctsHeader.ArrivalMovementHeader.Seals.Count);
			AssertEquals("SEL1, SEL2, SEL3", string.Join(", ", nctsHeader.ArrivalMovementHeader.Seals.Cast<Seal>().Select(x => x.CY_Data)));

			AssertEquals(2, nctsHeader.ArrivalMovementHeader.GoodsItems.Count);
			var goodsItem1 = nctsHeader.ArrivalMovementHeader.GoodsItems[0];
			AssertEquals((ZShort)1, goodsItem1.BY_LineNo);
			AssertEquals("4000", goodsItem1.BY_HarmonisedTariff);
			AssertEquals("IMP", goodsItem1.BY_Type);
			AssertEquals(new string('X', 280), goodsItem1.BY_Description);
			AssertEquals(100m, goodsItem1.BY_GrossWeight);
			AssertEquals("KG", goodsItem1.BY_GrossWeightUnit);
			AssertEquals(90m, goodsItem1.BY_NetWeight);
			AssertEquals("KG", goodsItem1.BY_NetWeightUnit);
			AssertEquals("AU", goodsItem1.BY_RN_NKCountryOfDispatch);
			AssertEquals("FR", goodsItem1.BY_RN_NKCountryOfDestination);
			var goodsItem2 = nctsHeader.ArrivalMovementHeader.GoodsItems[1];
			AssertEquals((ZShort)2, goodsItem2.BY_LineNo);

			AssertEquals(2, goodsItem1.SupportingDocuments.Count);
			var sd1 = goodsItem1.SupportingDocuments[0];
			AssertEquals("PD1", sd1.CSI_Code);
			AssertEquals("Produced Document 1", sd1.CSI_Description);
			AssertEquals("PD0001", sd1.CSI_ReferenceNumber);
			var sd2 = goodsItem1.SupportingDocuments[1];
			AssertEquals("PD2", sd2.CSI_Code);
			AssertEquals("Produced Document 2", sd2.CSI_Description);
			AssertEquals("PD0002", sd2.CSI_ReferenceNumber);

			var sms = goodsItem1.SpecialMentions.ToList();
			AssertEquals(2, sms.Count);
			var sm1 = sms[0];
			AssertEquals("SM1", sm1.Statement);
			AssertEquals("Special Mention 1", sm1.StatementText);
			AssertEquals(true, sm1.ExportFromEC);
			AssertEquals("US", sm1.ExportFromCountry);
			var sm2 = sms[1];
			AssertEquals("SM2", sm2.Statement);
			AssertEquals("Special Mention 2", sm2.StatementText);
			AssertEquals(false, sm2.ExportFromEC);
			AssertEquals("CA", sm2.ExportFromCountry);

			AssertContainsExactElementsInAnyOrder(new ZString[] { "CNT1", "CNT2", "CNT3" }, goodsItem1.Containers.Select(x => x.ContainerNumber));

			AssertEquals(2, goodsItem1.Packages.Count);
			var pkg1 = goodsItem1.Packages[0];
			AssertEquals("PKG1", pkg1.B5_MarksAndNumbers);
			AssertEquals(10, pkg1.B5_UnitCount);
			AssertEquals("BG", pkg1.B5_UnitType);
			var pkg2 = goodsItem1.Packages[1];
			AssertEquals("PKG2", pkg2.B5_MarksAndNumbers);
			AssertEquals(20, pkg2.B5_UnitCount);
			AssertEquals("BG", pkg2.B5_UnitType);

			var sgis = goodsItem1.SgiCodes.ToList();
			AssertEquals(2, sgis.Count);
			var sgi1 = sgis[0];
			AssertEquals("1", sgi1.Code);
			AssertEquals(30m, sgi1.Qty);
			var sgi2 = sgis[1];
			AssertEquals("2", sgi2.Code);
			AssertEquals(60m, sgi2.Qty);
		}
	}
}
