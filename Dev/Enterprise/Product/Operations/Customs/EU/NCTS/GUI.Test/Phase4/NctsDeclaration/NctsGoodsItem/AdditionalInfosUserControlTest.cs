using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class AdditionalInfosUserControlTest : TestCaseWithFactory
	{
		public void TestUserControlDataSourceType()
		{
			using (var control = new AdditionalInfosUserControl())
			{
				AssertEquals(typeof(NctsCommonCargoDesc), control.BindingSource.DataSourceType);
			}
		}

		public void TestGridColumnSizes()
		{
			using (var control = new AdditionalInfosUserControl())
			{
				var additionalInfosGrid = control.FindSingle<ZGrid>("AdditionalInfosGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code", 70, additionalInfosGrid.GetColumnStyle(NctsAdditionalInfo.Schema.CSI_Code).Width);
					AssertEquals("CSI_Description", 610, additionalInfosGrid.GetColumnStyle(NctsAdditionalInfo.Schema.CSI_Description).Width);
					AssertEquals("CSI_RN_NKCountryCode", 149, additionalInfosGrid.GetColumnStyle(NctsAdditionalInfo.Schema.CSI_RN_NKCountryCode).Width);
					AssertEquals("CSI_NctsExportFromEC", 97, additionalInfosGrid.GetColumnStyle(NctsAdditionalInfo.Schema.CSI_NctsExportFromEC).Width);
					AssertEquals("CSI_Status", 53, additionalInfosGrid.GetColumnStyle(NctsAdditionalInfo.Schema.CSI_Status).Width);
				});
			}
		}

		public void TestGridColumCharcterCasing()
		{
			using (var control = new AdditionalInfosUserControl())
			{
				var additionalInfosGrid = control.FindSingle<ZGrid>("AdditionalInfosGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code", CharacterCasing.Upper, additionalInfosGrid.GetColumnStyle(NctsAdditionalInfo.Schema.CSI_Code).CharacterCasing);
					AssertEquals("CSI_Description", CharacterCasing.Upper, additionalInfosGrid.GetColumnStyle(NctsAdditionalInfo.Schema.CSI_Description).CharacterCasing);
					AssertEquals("CSI_RN_NKCountryCode", CharacterCasing.Upper, additionalInfosGrid.GetColumnStyle(NctsAdditionalInfo.Schema.CSI_RN_NKCountryCode).CharacterCasing);
					AssertEquals("CSI_Status", CharacterCasing.Upper, additionalInfosGrid.GetColumnStyle(NctsAdditionalInfo.Schema.CSI_Status).CharacterCasing);
				});
			}
		}
	}
}
