using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class InvoiceHeaderAdditionalInfoDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new InvoiceHeaderAdditionalInfoDetailsUserControl())
		{
			AssertEquals(typeof(InvoiceHeaderAdditionalInfo), control.BindingSource.DataSourceType);
		}
	}

	public void TestControls()
	{
		using (var control = new InvoiceHeaderAdditionalInfoDetailsUserControl())
		{
			var descriptionTextBox = control.FindSingle<ZTextBox>("DescriptionTextBox");
			CombineAssertions(() =>
			{
				AssertEquals("DescriptionTextBox Visible", true, descriptionTextBox.Visible);
				AssertEquals("DescriptionTextBox BindTo", "CSI_Description", descriptionTextBox.BindTo);
			});
		}
	}
}
