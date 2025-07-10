using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	public class CusTempStorageRegLineItemDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestGoodsItemNumberCalcEdit()
		{
			var goodsItemNumberCalcEdit = control.GoodsItemNumberCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", goodsItemNumberCalcEdit);
				AssertBindingMember(CusTempStorageRegLineItem.Schema.SRI_GoodsItemNumber, goodsItemNumberCalcEdit.GetBindingMember(), ItemPrefix);
			});
		}

		public void TestTariffTextBox()
		{
			var tariffTextBox = control.TariffTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", tariffTextBox);
				AssertBindingMember(CusTempStorageRegLineItem.Schema.FormattedTariff, tariffTextBox.GetBindingMember(), ItemPrefix);
			});
		}

		public void TestCusC4NumberTextBox()
		{
			var cusC4NumberTextBox = control.CusC4NumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", cusC4NumberTextBox);
				AssertBindingMember(CusTempStorageRegLineItem.Schema.SRI_CusC4Number, cusC4NumberTextBox.GetBindingMember(), ItemPrefix);
			});
		}

		public void TestGoodsDescriptionTextBox()
		{
			var goodsDescriptionTextBox = control.GoodsDescriptionTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", goodsDescriptionTextBox);
				AssertEquals("IsMultiline", expected: true, goodsDescriptionTextBox.Multiline);
				AssertBindingMember(CusTempStorageRegLineItem.Schema.SRI_GoodsDescription, goodsDescriptionTextBox.GetBindingMember(), ItemPrefix);
			});
		}

		public void TestGrossWeightCalcDropEdit()
		{
			var grossWeightCalcDropEdit = control.GrossWeightCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>("Type", grossWeightCalcDropEdit);
				AssertEquals("BindingMember", ".", grossWeightCalcDropEdit.GetBindingMember());
				AssertBindingMember(CusTempStorageRegLineItemPivot.Schema.SRV_GrossWeight, grossWeightCalcDropEdit.BindToAmount);
				AssertBindingMember(CusTempStorageRegLine.Schema.SRL_GrossWeightUQ, grossWeightCalcDropEdit.BindToUnit, LinePrefix);
			});
		}

		const string LinePrefix = "RegLine";
		const string ItemPrefix = "RegLineItem";

		void AssertBindingMember(string columnName, string bindingMember, string columnPrefix = "")
		{
			var columnStyleName = string.IsNullOrEmpty(columnPrefix) ? columnName : $"{columnPrefix}.{columnName}";
			AssertEquals("BindingMember", $"CusTempStorageRegLines.RegLineItemPivots.{columnStyleName}", bindingMember);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CusTempStorageRegLineItemDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		CusTempStorageRegLineItemDetailsUserControl control;
	}
}
