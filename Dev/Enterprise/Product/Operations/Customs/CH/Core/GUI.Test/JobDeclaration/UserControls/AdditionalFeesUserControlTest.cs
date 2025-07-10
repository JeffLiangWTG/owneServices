using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

sealed class AdditionalFeesUserControlTest : TestCaseWithFactory
{
	public void TestFeesGridColumns()
	{
		using (var control = new AdditionalFeesUserControl())
		{
			var additionalFeesGridColumnStyles = control.AdditionalFeesGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(additionalFeesGridColumnStyles, CusLineTariffDetail.Schema.BZ_Tariff, 0);
				UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(additionalFeesGridColumnStyles, CusLineTariffDetail.Schema.Description, 1);
				UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(additionalFeesGridColumnStyles, CusLineTariffDetail.Schema.BZ_Qty1, 2);
				UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(additionalFeesGridColumnStyles, CusLineTariffDetail.Schema.BZ_ManualRate, 3);
				UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(additionalFeesGridColumnStyles, CusLineTariffDetail.Schema.BZ_Value, 4);
			});
		}
	}

	public void TestMultipleKeysToUse()
	{
		using (var control = new AdditionalFeesUserControl())
		{
			AssertSequencesEqual(new[] { UniversalReferenceConstants.RateTypes.AdditionalFees }, control.SupportMultipleResourceStringData.MultipleKeysToUse);
		}
	}
}
