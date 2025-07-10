using System;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCalcEditColumnItemTemplateTest : ZItemTemplateTest
	{
		public void TestGetControl()
		{
			TestColumn.HeaderText = "Header Text";

			ISelfBindingWebControl control = TestItemTemplate.GetControl();
			ZNumericLabel numericLabel = control as ZNumericLabel;
			AssertNotNull("Control has expected ZNumericLabel type", numericLabel);

			AssertEquals("NumericLabel BindToDecimals", TestColumn.BindToDecimals, numericLabel.BindToDecimals);
			AssertEquals("NumericLabel ShowGroupSeparators", TestColumn.ShowGroupSeparators, numericLabel.ShowGroupSeparators);

			TestColumn.BindToCurrencySymbol = "CurerencySymbol";
			control = TestItemTemplate.GetControl();
			ZMoneyLabel moneyLabel = control as ZMoneyLabel;
			AssertNotNull("Control has expected ZMoneyLabel type", moneyLabel);
			AssertEquals("MoneyLabel BindToDecimals", TestColumn.BindToDecimals, moneyLabel.BindToDecimals);
			AssertEquals("MoneyLabel BindToCurrencySymbol", TestColumn.BindToCurrencySymbol, moneyLabel.BindToCurrencySymbol);
		}

		public void TestRightAlignmentForNumberCells()
		{
			using (TableCell cell = new TableCell())
			{
				TestItemTemplate.InstantiateIn(cell);
				AssertNotNull("Precondition: Horizontal Align set", cell.HorizontalAlign);
				AssertEquals("Right alignment", HorizontalAlign.Right, cell.HorizontalAlign);
			}
		}

		#region Implementation

		new ZCalcEditColumn TestColumn
		{
			get { return base.TestColumn as ZCalcEditColumn; }
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZCalcEditColumn); }
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZCalcEditColumnItemTemplate); }
		}

		#endregion
	}
}
