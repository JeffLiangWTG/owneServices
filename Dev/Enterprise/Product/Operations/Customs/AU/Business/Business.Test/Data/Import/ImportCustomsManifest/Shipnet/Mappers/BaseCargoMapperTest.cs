using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BaseCargoMapperTest : BaseMapperTest
	{
		public void TestCargoType()
		{
			AssertEquals("cargo type", "FOO", TestHelper.cargoType);
		}

		public void TestFromCargoGroupDescriptionsRow()
		{
			CargoDescriptionsDataRow row = new CargoDescriptionsDataRow("CGD0001M AND A 7476 MELBOURNE NO.1 MADE IN SINGAPORE AN JAPAN                                                                                                                                                                                                    730 BAGS POLYVINYL ALCOHOL GM14S     10,000KGS PACKING:20KGS NET IN 500 BAGS GH23       3,000KGS PACKING:20KGS NET IN  150 BAGS");
			TestHelper.FromCargoGroupDescriptionsRow(row);

			AssertEquals("730 BAGS POLYVINYL ALCOHOL GM14S     10,000KGS PACKING:20KGS NET IN 500 BAGS GH23       3,000KGS PACKING:20KGS NET IN  150 BAGS", TestHelper.goodsDescription.Trim());
			AssertEquals("M AND A 7476 MELBOURNE NO.1 MADE IN SINGAPORE AN JAPAN", TestHelper.marksAndNumbers.Trim());
		}

		public void TestForDescriptionRow()
		{
			CargoDescriptionsDataRow row = new CargoDescriptionsDataRow("CGD0001FOOND A 7476 MELBOURNE NO.1 MADE IN SINGAPORE AN JAPAN                                                                                                                                                                                                    BAR BAGS POLYVINYL ALCOHOL GM14S     10,000KGS PACKING:20KGS NET IN 500 BAGS GH23       3,000KGS PACKING:20KGS NET IN  150 BAGS");
			CargoDescriptionsDataRow row2 = new CargoDescriptionsDataRow("CID0001M AND A 7476 MELBOURNE NO.1 MADE IN SINGAPORE AN JAPAN                                                                                                                                                                                                    730 BAGS POLYVINYL ALCOHOL GM14S     10,000KGS PACKING:20KGS NET IN 500 BAGS GH23       3,000KGS PACKING:20KGS NET IN  150 BAGS");

			TestHelper.ForDescriptionRow(row2);

			Assert("marks and numbers from cid", TestHelper.marksAndNumbers.StartsWith("M AND A"));
			Assert("goods desc from cid", TestHelper.goodsDescription.StartsWith("730"));

			TestHelper.ForDescriptionRow(row);

			Assert("marks and numbers from cgd", TestHelper.marksAndNumbers.StartsWith("FOO"));
			Assert("goods desc from cgd", TestHelper.goodsDescription.StartsWith("BAR"));

			TestHelper.ForDescriptionRow(row2);

			Assert("marks and numbers still from cgd", TestHelper.marksAndNumbers.StartsWith("FOO"));
			Assert("goods desc still from cgd", TestHelper.goodsDescription.StartsWith("BAR"));
		}

		BaseCargoMapperTestHelper testHelper;
		BaseCargoMapperTestHelper TestHelper => testHelper ?? (testHelper = new BaseCargoMapperTestHelper("FOO"));

		sealed class BaseCargoMapperTestHelper : BaseCargoMapper
		{
			public BaseCargoMapperTestHelper(ZString cargoType)
				: base(cargoType)
			{
			}

			public override void Map(FlatFileDataRowCollection rows, Enterprise.DataTransfer.Xml.IValueObject value)
			{
			}
		}
	}
}
