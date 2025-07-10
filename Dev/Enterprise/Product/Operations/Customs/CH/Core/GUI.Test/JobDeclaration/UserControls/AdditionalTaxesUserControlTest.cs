using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

sealed class AdditionalTaxesUserControlTest : TestCaseWithFactory
{
	public void TestTaxesGridColumns()
	{
		using (var control = new AdditionalTaxesUserControl())
		{
			var additionalTaxesGridColumnStyles = control.AdditionalTaxesGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions("AdditionalTaxesGrid", () =>
			{
				UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(additionalTaxesGridColumnStyles, CusLineTariffDetail.Schema.BZ_TaxType, 0);
				UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(additionalTaxesGridColumnStyles, CusLineTariffDetail.Schema.BZ_Tariff, 1);
				UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(additionalTaxesGridColumnStyles, CusLineTariffDetail.Schema.BZ_AlcoholPercentage, 2);
				UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(additionalTaxesGridColumnStyles, CusLineTariffDetail.Schema.BZ_Qty1, 3);
				UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(additionalTaxesGridColumnStyles, CusLineTariffDetail.Schema.BZ_UQ1, 4);
				UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(additionalTaxesGridColumnStyles, CusLineTariffDetail.Schema.BZ_BaseValue, 5);
				UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(additionalTaxesGridColumnStyles, CusLineTariffDetail.Schema.BZ_ManualRate, 6);
				UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(additionalTaxesGridColumnStyles, CusLineTariffDetail.Schema.BZ_Value, 7);
			});
		}
	}

	public void TestMultipleKeysToUse()
	{
		using (var control = new AdditionalTaxesUserControl())
		{
			AssertSequencesEqual(new[] { UniversalReferenceConstants.RateTypes.AdditionalTaxes }, control.SupportMultipleResourceStringData.MultipleKeysToUse);
		}
	}
}
