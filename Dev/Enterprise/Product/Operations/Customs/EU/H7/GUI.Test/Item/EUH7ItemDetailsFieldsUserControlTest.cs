using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7ItemDetailsFieldsUserControl))]
	sealed class EUH7ItemDetailsFieldsUserControlTest : TestCaseWithFactory
	{
		public void TestTariffFindBox()
		{
			using (var control = new EUH7ItemDetailsFieldsUserControl())
			{
				control.Show();

				var tariffFindBox = control.FindSingle<Universal.GUI.TariffFindBox>("TariffFindBox");

				AssertEquals("BindingMember", "API_FormattedTariff", tariffFindBox.GetBindingMember());
			}
		}

		[RequiresSTA]
		public void TestIntrinsicValueConvertToLocalCurrencyControl()
		{
			using (var control = new EUH7ItemDetailsFieldsUserControl())
			{
				control.Show();

				var intrinsicValueConvertToLocalCurrencyControl = control.FindSingle<ConvertToLocalCurrencyControl>("IntrinsicValueConvertToLocalCurrencyControl");

				CombineAssertions("IntrinsicValueConvertToLocalCurrencyControl", () =>
				{
					AssertEquals("BindToAmount", "API_GoodsValue", intrinsicValueConvertToLocalCurrencyControl.BindToAmount);
					AssertEquals("BindToUnit", "API_RX_NKGoodsValueCurrency", intrinsicValueConvertToLocalCurrencyControl.BindToUnit);
					AssertEquals("Decimals", 2, intrinsicValueConvertToLocalCurrencyControl.Decimals);
				});
			}
		}

		public void TestGoodsOriginCodeFindBox()
		{
			using (var control = new EUH7ItemDetailsFieldsUserControl())
			{
				control.Show();

				var goodsOriginCodeFindBox = control.FindSingle<ZCodeFindBox>("GoodsOriginCodeFindBox");

				AssertEquals("BindingMember", "API_RN_NKGoodsOrigin", goodsOriginCodeFindBox.GetBindingMember());
			}
		}

		public void TestSupplementaryDropEdit()
		{
			using (var control = new EUH7ItemDetailsFieldsUserControl())
			{
				control.Show();

				var supplementaryDropEdit = control.FindSingle<ZCalcDropEdit>("SupplementaryDropEdit");

				CombineAssertions("BindingMember", () =>
				{
					AssertEquals("BindToAmount", "API_CustomsQty2", supplementaryDropEdit.BindToAmount);
					AssertEquals("BindToUnit", "API_CustomsUQ2", supplementaryDropEdit.BindToUnit);
				});
			}
		}

		public void TestGrossWeightCalcDropEdit()
		{
			using (var control = new EUH7ItemDetailsFieldsUserControl())
			{
				control.Show();

				var grossWeightCalcDropEdit = control.FindSingle<ZCalcDropEdit>("GrossWeightCalcDropEdit");

				CombineAssertions("BindingMember", () =>
				{
					AssertEquals("BindToAmount", "API_GrossWeight", grossWeightCalcDropEdit.BindToAmount);
					AssertEquals("BindToUnit", "API_GrossWeightUQ", grossWeightCalcDropEdit.BindToUnit);
				});
			}
		}

		public void TestNetWeightCalcDropEdit()
		{
			using (var control = new EUH7ItemDetailsFieldsUserControl())
			{
				control.Show();

				var netWeightCalcDropEdit = control.FindSingle<ZCalcDropEdit>("NetWeightCalcDropEdit");

				CombineAssertions("BindingMember", () =>
				{
					AssertEquals("BindToAmount", "API_NetWeight", netWeightCalcDropEdit.BindToAmount);
					AssertEquals("BindToUnit", "API_NetWeightUQ", netWeightCalcDropEdit.BindToUnit);
				});
			}
		}

		public void TestGoodsDescriptionTextBox()
		{
			using (var control = new EUH7ItemDetailsFieldsUserControl())
			{
				control.Show();

				var goodsDescriptionTextBox = control.FindSingle<ZTextBox>("GoodsDescriptionTextBox");

				AssertEquals("BindingMember", "API_GoodsDescription", goodsDescriptionTextBox.GetBindingMember());
			}
		}

		public void TestQuantityCalcEdit()
		{
			using (var control = new EUH7ItemDetailsFieldsUserControl())
			{
				control.Show();

				var quantityCalcEdit = control.FindSingle<ZTextBox>("QuantityCalcEdit");

				AssertEquals("BindingMember", "API_CustomsQty", quantityCalcEdit.GetBindingMember());
			}
		}

		public void TestCustomEntriesGrid()
		{
			using (var control = new EUH7ItemDetailsFieldsUserControl())
			{
				control.Show();

				var customEntriesGrid = control.FindSingle<ZGrid>("CustomEntriesGrid");

				AssertEquals("BindingMember", "CustomsEntryNumbers", customEntriesGrid.GetBindingMember());

				EUH7GUITestHelper.AssertGridLayout(customEntriesGrid,
					[
						(AutoCusEntryNum.Schema.CE_EntryNum, typeof(ZTextBoxColumnStyleInfo)),
						(AutoCusEntryNum.Schema.CE_EntryType, typeof(ZDropEditColumnStyleInfo))
					]);
			}
		}
	}
}
