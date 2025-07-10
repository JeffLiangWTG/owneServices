using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Module;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class CusClassificationFilterControlTest : TestCaseWithFactory
	{
		public void TestFilteredGridDetails()
		{
			var filterObject = new CusClassificationFilterBusinessObject();
			var gridCollection = new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Canada);
			using (var control = new CusClassificationFilterControl(gridCollection, filterObject))
			{
				control.Show();
				AssertEquals("CC_LookupCode", ControlDpiScalingHelper.ScaleToCurrentDpiX(90), control.FilteredGrid.GetColumnWidth(CusClassification.Schema.CC_LookupCode));
				AssertEquals("CC_FormattedTariffNum", ControlDpiScalingHelper.ScaleToCurrentDpiX(90), control.FilteredGrid.GetColumnWidth(CusClassification.Schema.CC_FormattedTariffNum));
				AssertEquals("CC_Description", ControlDpiScalingHelper.ScaleToCurrentDpiX(200), control.FilteredGrid.GetColumnWidth(CusClassification.Schema.CC_Description));
				AssertEquals("CC_IsActive", ControlDpiScalingHelper.ScaleToCurrentDpiX(70), control.FilteredGrid.GetColumnWidth(CusClassification.Schema.CC_IsActive));
				AssertEquals("CC_LastAuditedUser", ControlDpiScalingHelper.ScaleToCurrentDpiX(90), control.FilteredGrid.GetColumnWidth(CusClassification.Schema.CC_LastAuditedUser));
				AssertEquals("CC_LastAuditedDate", ControlDpiScalingHelper.ScaleToCurrentDpiX(95), control.FilteredGrid.GetColumnWidth(CusClassification.Schema.CC_LastAuditedDate));
				AssertNull("CC_TariffNum should not appear", control.FilteredGrid.Columns[CusClassification.Schema.CC_TariffNum]);
			}
		}
	}
}
