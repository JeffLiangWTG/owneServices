using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class OutwardProcessingUserControlTest : TestCaseWithFactory
	{
		public void TestInitializeGrid()
		{
			using (var form = new ZForm())
			using (var control = new OutwardProcessingUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var productGrid = control.FindSingle<ZGrid>("ProductGrid");
				AssertEquals(ProductSupportingInfo.Schema.FormattedTariff, ((ZGridColumnInfo)productGrid.ColumnStyles[0]).ColumnName);
			}
		}

		public void TestProductDescriptionLowercaseAllowed()
		{
			using (var form = new ZForm())
			using (var control = new OutwardProcessingUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var productGrid = control.FindSingle<ZGrid>("ProductGrid");
				var descriptionColumn = (ZGridColumnInfo)productGrid.ColumnStyles[1];
				AssertEquals(ProductSupportingInfo.Schema.CSI_Description, descriptionColumn.ColumnName);
				AssertEquals(CharacterCasing.Normal, descriptionColumn.CharacterCasing);
			}
		}

		public void TestIdentificationMeansDescriptionLowercaseAllowed()
		{
			using (var form = new ZForm())
			using (var control = new OutwardProcessingUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var identificationMeansGrid = control.FindSingle<ZGrid>("IdentificationMeansGrid");
				var descriptionColumn = (ZGridColumnInfo)identificationMeansGrid.ColumnStyles[1];
				AssertEquals(IdentificationMeansCode.Schema.CY_Data, descriptionColumn.ColumnName);
				AssertEquals(CharacterCasing.Normal, descriptionColumn.CharacterCasing);
			}
		}

		public void TestReimportCountryGroupBox()
		{
			using (var control = new OutwardProcessingUserControl())
			{
				AssertEquals("Reimport Country/Region", control.ReimportCountryGroupBox.CaptionResourceString.Caption);
			}
		}
	}
}
