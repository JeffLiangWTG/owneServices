using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	public class UCC6TemporaryStoragePackedItemDetailsControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6TemporaryStoragePackedItemDetailsControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull(control.FindSingle<ZCalcEdit>("SeqCalcEdit"));
					AssertNotNull(control.FindSingle<Universal.GUI.TariffFindBox>("TariffCodeFindBox"));
					AssertNotNull(control.FindSingle<ZTextBox>("GoodsDescriptionTextBox"));
					AssertNotNull(control.FindSingle<ZCodeFindBox>("CusCodeFindBox"));
					AssertNotNull(control.FindSingle<ZCalcDropEdit>("GrossWeightCalcDropEdit"));
					AssertNotNull(control.FindSingle<ZDropEdit>("CountryOfOriginDropEdit"));
					AssertNotNull(control.FindSingle<ZCalcDropEdit>("CustomsValueCalcDropEdit"));
					AssertNotNull(control.FindSingle<ZCalcDropEdit>("SupplementaryUnitsCalcDropEdit"));
					AssertNotNull(control.FindSingle<ZCalcDropEdit>("CustomsSecondQuantityDropEdit"));
					AssertNotNull(control.FindSingle<ZCalcDropEdit>("CustomsThirdQuantityDropEdit"));
					AssertNotNull(control.FindSingle<AdditionalSupplementaryCodesUserControl>("AdditionalSupplementaryCodesUserControl"));
					AssertNotNull(control.FindSingle<ZLabel>("DutiesAndTaxesLabel"));
					AssertNotNull(control.FindSingle<ZGrid>("DutiesAndTaxesGrid"));
				});
			}
		}

		public void TestNetWeightCalcDropEdit()
		{
			using (var control = new UCC6TemporaryStoragePackedItemDetailsControl())
			{
				CombineAssertions("NetWeightCalcDropEdit", () =>
				{
					var element = control.FindSingle<ZCalcDropEdit>("NetWeightCalcDropEdit");

					AssertEquals("BindToAmount", "Bills.PackedItems.API_NetWeight", element.BindToAmount);
					AssertEquals("BindToUnit", "Bills.PackedItems.API_NetWeightUQ", element.BindToUnit);
				});
			}
		}

		public void TestGrossWeightCalcDropEdit()
		{
			using (var control = new UCC6TemporaryStoragePackedItemDetailsControl())
			{
				CombineAssertions("GrossWeightCalcDropEdit", () =>
				{
					var element = control.FindSingle<ZCalcDropEdit>("GrossWeightCalcDropEdit");
					AssertEquals("BindToAmount", "Bills.PackedItems.API_GrossWeight", element.BindToAmount);
					AssertEquals("BindToUnit", "Bills.PackedItems.API_GrossWeightUQ", element.BindToUnit);
				});
			}
		}

		public void TestCustomsSecondQuantityDropEdit()
		{
			using (var control = new UCC6TemporaryStoragePackedItemDetailsControl())
			{
				CombineAssertions("CustomsSecondQuantityDropEdit", () =>
				{
					var element = control.FindSingle<ZCalcDropEdit>("CustomsSecondQuantityDropEdit");

					AssertEquals("BindToAmount", "Bills.PackedItems.API_CustomsQty2", element.BindToAmount);
					AssertEquals("BindToUnit", "Bills.PackedItems.API_CustomsUQ2", element.BindToUnit);
				});
			}
		}

		public void TestDutyAndTaxGrid() => CombineAssertions(() =>
		{
			using (var control = new UCC6TemporaryStoragePackedItemDetailsControl())
			{
				var grid = control.DutiesAndTaxesGrid;
				var columns = grid.ColumnStyles.Cast<ZGridColumnInfo>().ToList();
				var expectedIndex = 0;

				AssertColumn<ZTextBoxColumnStyleInfo>(TemporaryStorageDutyAndTax.Schema.AET_ChargeType);
				AssertColumn<ZTextBoxColumnStyleInfo>(TemporaryStorageDutyAndTax.Schema.AET_MethodOfCalculation);
				AssertColumn<ZCalcEditColumnStyleInfo>(TemporaryStorageDutyAndTax.Schema.AET_BaseValue);
				AssertColumn<ZCalcEditColumnStyleInfo>(TemporaryStorageDutyAndTax.Schema.AET_Rate);
				AssertColumn<ZCalcEditColumnStyleInfo>(TemporaryStorageDutyAndTax.Schema.AET_ChargeAmount);
				AssertEquals("Column count", expectedIndex, columns.Count);

				AssertEquals("AllowNavigation", false, grid.AllowNavigation);

				void AssertColumn<T>(string name)
				{
					var column = columns.First(x => x.ColumnName == name);
					AssertType<T>($"{name} Type", column);
					AssertEquals($"{name} Index", expectedIndex, columns.IndexOf(column));
					expectedIndex++;
				}
			}
		});
	}
}
