using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.GUI.PlugIn.Testing
{
	class AdditionalInfosUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			var additionalInfo = Factory.New<AdditionalInfo>();
			var collection = new AdditionalInfoCollection(additionalInfo);

			using (var control = new AdditionalInfosUserControlForTest())
			{
				control.AdditionalInfosGrid.SetDataBinding(collection, "");
				control.Show();

				AssertEquals(6, control.AdditionalInfosGrid.ColumnStyles.Count);

				Assert(control.AdditionalInfosGrid.Columns.Contains(SupportingDocument.Schema.CSI_Code));
				Assert(control.AdditionalInfosGrid.Columns.Contains(SupportingDocument.Schema.CSI_Description));
				Assert(control.AdditionalInfosGrid.Columns.Contains(SupportingDocument.Schema.CSI_DateOfIssue));
				Assert(!control.AdditionalInfosGrid.Columns.Contains(SupportingDocument.Schema.CSI_RN_NKCountryCode));
				Assert(!control.AdditionalInfosGrid.Columns.Contains(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo.Schema.CSI_NctsExportFromEC));
				Assert(!control.AdditionalInfosGrid.Columns.Contains(SupportingDocument.Schema.CSI_Status));
			}
		}

		public void TestBindingSourceType()
		{
			using (var control = new AdditionalInfosUserControlForTest())
			{
				AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
			}
		}
	}

	class AdditionalInfosUserControlForTest : AdditionalInfosUserControl
	{
		public new ZArchitecture.ZGrid AdditionalInfosGrid => base.AdditionalInfosGrid;
	}
}
