using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CargoGroupMapperTest : BaseMapperTest
	{
		public void TestFromCargoGroupItemsRow()
		{
			Xsd.CusImportManifestOceanBillOceanBillDetail oceanBillDetail = new Xsd.CusImportManifestOceanBillOceanBillDetail();
			CargoGroupItemsDataRow row = new CargoGroupItemsDataRow("CIT0001       730BG                                 POLYVINYL ALCOHOL                   15219.000      12899.000         20.000");
			Mapper.goodsDescription = "foo";
			Mapper.marksAndNumbers = "bar";
			Mapper.containerMode = Core.Constants.ContainerModes.FCL;
			Mapper.containerSizeOrISOCode = "2000";
			Mapper.FromCargoGroupItemsRow(row, oceanBillDetail);

			AssertEquals("KG", oceanBillDetail.Weight.DimensionType);
			AssertEquals(15219m, oceanBillDetail.Weight.Value);
			AssertEquals("CU", oceanBillDetail.Volume.DimensionType);
			AssertEquals(20m, oceanBillDetail.Volume.Value);
			AssertEquals(false, oceanBillDetail.Indicators.HazardousGoods);
			AssertEquals(false, oceanBillDetail.Indicators.Fumigation);
			AssertEquals(false, oceanBillDetail.Indicators.ReportableDocuments);
			AssertEquals(730, oceanBillDetail.NumberOfPackages);
			AssertEquals("BG", oceanBillDetail.PackageType.Trim());

			AssertEquals("foo", oceanBillDetail.GoodsDescription);
			AssertEquals("bar", oceanBillDetail.MarksAndNumbers);
			AssertEquals(Core.Constants.ContainerModes.FCL, oceanBillDetail.ContainerMode);
			AssertEquals("2000", oceanBillDetail.ContainerSizeOrISOCode);
		}

		public void TestFromCargoGroupDescriptionsRow()
		{
			CargoDescriptionsDataRow row = new CargoDescriptionsDataRow("CGD0001M AND A 7476 MELBOURNE NO.1 MADE IN SINGAPORE AN JAPAN                                                                                                                                                                                                    730 BAGS POLYVINYL ALCOHOL GM14S     10,000KGS PACKING:20KGS NET IN 500 BAGS GH23       3,000KGS PACKING:20KGS NET IN  150 BAGS");
			Mapper.FromCargoGroupDescriptionsRow(row);

			AssertEquals("730 BAGS POLYVINYL ALCOHOL GM14S     10,000KGS PACKING:20KGS NET IN 500 BAGS GH23       3,000KGS PACKING:20KGS NET IN  150 BAGS", Mapper.goodsDescription.Trim());
			AssertEquals("M AND A 7476 MELBOURNE NO.1 MADE IN SINGAPORE AN JAPAN", Mapper.marksAndNumbers.Trim());
		}

		public void TestFromCargoGroupRow()
		{
			CargoGroupDataRow row = new CargoGroupDataRow("CGR0001         12210 5      15219.000      12899.000         20.000                      730       2320.000");
			Mapper.FromCargoGroupRow(row);

			AssertEquals(Core.Constants.ContainerModes.FCL, Mapper.containerMode);
			AssertEquals("2210", Mapper.containerSizeOrISOCode.Trim());
		}

		CargoGroupMapper mapper;
		CargoGroupMapper Mapper => mapper ?? (mapper = new CargoGroupMapper("1"));
	}
}
