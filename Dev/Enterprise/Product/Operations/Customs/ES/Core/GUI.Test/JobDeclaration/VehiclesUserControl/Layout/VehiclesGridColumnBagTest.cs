using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(VehiclesGridColumnBag))]
	class VehiclesGridColumnBagTest : TestCase
	{
		public void TestVinTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.VinTextBoxColumn);
				var columnInfo = ColumnsBag.VinTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CVH_VehicleIdentificationNumber", columnInfo.ColumnName);
				AssertEquals("Width", 60, columnInfo.Width);
			});
		}

		public void TestBrandTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.BrandTextBoxColumn);
				var columnInfo = ColumnsBag.BrandTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CVH_BrandName", columnInfo.ColumnName);
				AssertEquals("Width", 60, columnInfo.Width);
			});
		}

		public void TestModelTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.ModelTextBoxColumn);
				var columnInfo = ColumnsBag.ModelTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CVH_ModelName", columnInfo.ColumnName);
				AssertEquals("Width", 60, columnInfo.Width);
			});
		}

		VehiclesGridColumnBag ColumnsBag => VehiclesGridColumnBag.Instance;
	}
}
