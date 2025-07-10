using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class NctsTaxOrFeeUserControlTest : TestCaseWithFactory
{
	public void TestTaxOrFeesGrid()
	{
		using (var control = new NctsTaxOrFeeUserControl())
		{
			var taxOrFeesGrid = control.FindSingle<ZGrid>("TaxOrFeesGrid");
			AssertNotNull("Not null", taxOrFeesGrid);
			var allColumnStyles = taxOrFeesGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			AssertEquals("ColumnStyles", 7, allColumnStyles.Length);

			CombineAssertions(() =>
			{
				AssertColumnVisibleAndAtSpecificIndex<ZDropEditColumnStyleInfo>(NctsCargoDescFee.Schema.BFE_ChargeType, 0);
				AssertColumnVisibleAndAtSpecificIndex<ZDropEditColumnStyleInfo>(NctsCargoDescFee.Schema.BFE_RateOverrideReasonCode, 1);
				AssertColumnVisibleAndAtSpecificIndex<ZCalcEditColumnStyleInfo>(NctsCargoDescFee.Schema.BFE_BaseValue, 2);
				AssertColumnVisibleAndAtSpecificIndex<ZDropEditColumnStyleInfo>(NctsCargoDescFee.Schema.BFE_MethodOfCalculation, 3);
				AssertColumnVisibleAndAtSpecificIndex<ZCalcEditColumnStyleInfo>(NctsCargoDescFee.Schema.BFE_Rate, 4);
				AssertColumnVisibleAndAtSpecificIndex<ZCalcEditColumnStyleInfo>(NctsCargoDescFee.Schema.BFE_ChargeAmount, 5);
				AssertColumnVisibleAndAtSpecificIndex<ZDropEditColumnStyleInfo>(NctsCargoDescFee.Schema.BFE_MethodOfPayment, 6);
			});

			void AssertColumnVisibleAndAtSpecificIndex<TColumnInfo>(ZString columnName, ZInt expectedIndex)
				where TColumnInfo : ZGridColumnInfo
			{
				var columnStyle = allColumnStyles.SingleOrDefault(x => x.ColumnName == columnName);
				AssertNotNull($"{columnName} not null", columnStyle);
				AssertType<TColumnInfo>($"Expected type for {columnStyle}", columnStyle);
				Assert($"{columnName} is visible", columnStyle.IsVisible);
				AssertEquals($"{columnName} is at specific index", expectedIndex, Array.IndexOf(allColumnStyles, columnStyle));
			}
		}
	}
}
