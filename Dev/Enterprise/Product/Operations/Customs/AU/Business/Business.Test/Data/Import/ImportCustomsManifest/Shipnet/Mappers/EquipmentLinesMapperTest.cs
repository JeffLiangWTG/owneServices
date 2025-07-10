using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EquipmentLinesMapperTest : BaseMapperTest
	{
		public void TestFromCargoGroupDescriptionsRow()
		{
			CargoDescriptionsDataRow row = new CargoDescriptionsDataRow("CGD0001M AND A 7476 MELBOURNE NO.1 MADE IN SINGAPORE AN JAPAN                                                                                                                                                                                                    730 BAGS POLYVINYL ALCOHOL GM14S     10,000KGS PACKING:20KGS NET IN 500 BAGS GH23       3,000KGS PACKING:20KGS NET IN  150 BAGS");
			Mapper.FromCargoGroupDescriptionsRow(row);

			AssertEquals("730 BAGS POLYVINYL ALCOHOL GM14S     10,000KGS PACKING:20KGS NET IN 500 BAGS GH23       3,000KGS PACKING:20KGS NET IN  150 BAGS", Mapper.goodsDescription.Trim());
			AssertEquals("M AND A 7476 MELBOURNE NO.1 MADE IN SINGAPORE AN JAPAN", Mapper.marksAndNumbers.Trim());
		}

		public void TestFromCargoGroupItemsRow()
		{
			CargoGroupItemsDataRow row = new CargoGroupItemsDataRow("CIT0001        57BX                                 PARTS OF GASMETER,ACTUATORS A       14740.000      12420.000         26.149                                                             Y Y");
			Mapper.FromCargoGroupItemsRow(row);

			AssertEquals(true, Mapper.fumigation);
			AssertEquals(false, Mapper.reportableDocuments);
			AssertEquals(true, Mapper.personalEffects);
		}

		public void TestFromEquipmentDetailsRow()
		{
			EquipmentDetailsDataRow row = new EquipmentDetailsDataRow("EQD0001GESU2715440 2210 5      14740.000      12420.000         26.149                      2320.000        57BX                                 Y                                                                                                                                                                                                                                      ");
			Xsd.CusImportManifestOceanBillOceanBillDetail oceanBillDetail = new Xsd.CusImportManifestOceanBillOceanBillDetail();

			Mapper.reportableDocuments = true;
			Mapper.fumigation = true;
			Mapper.personalEffects = true;
			Mapper.goodsDescription = "foo";
			Mapper.marksAndNumbers = "bar";
			Mapper.FromEquipmentDetailsRow(row, oceanBillDetail);

			AssertEquals(Core.Constants.ContainerModes.FCL, oceanBillDetail.ContainerMode);

			AssertEquals("foo", oceanBillDetail.GoodsDescription);
			AssertEquals("bar", oceanBillDetail.MarksAndNumbers);
			AssertEquals(true, oceanBillDetail.Indicators.Fumigation);
			AssertEquals(true, oceanBillDetail.Indicators.ReportableDocuments);

			AssertEquals("GESU2715440", oceanBillDetail.ContainerNumber.Trim());
			AssertEquals("2210", oceanBillDetail.ContainerSizeOrISOCode.Trim());
			AssertEquals("KG", oceanBillDetail.Weight.DimensionType);
			AssertEquals(14740m, oceanBillDetail.Weight.Value);
			AssertEquals("CU", oceanBillDetail.Volume.DimensionType);
			AssertEquals(26.149m, oceanBillDetail.Volume.Value);
			AssertEquals(57, oceanBillDetail.NumberOfPackages);
			AssertEquals("BX", oceanBillDetail.PackageType);
			AssertEquals("", oceanBillDetail.SealNumber.Trim());
			AssertEquals(true, oceanBillDetail.Indicators.HazardousGoods);
			AssertEquals(false, oceanBillDetail.Indicators.ShipperOwnedContainer);
			AssertEquals(true, oceanBillDetail.Indicators.PersonalEffects);
		}

		#region Implementation

		EquipmentLinesMapper Mapper
		{
			get
			{
				if (fMapper == null)
				{
					fMapper = new EquipmentLinesMapper("1");
				}
				return fMapper;
			}
		}
		EquipmentLinesMapper fMapper;

		#endregion
	}
}
