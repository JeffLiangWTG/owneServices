using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class AdditionalInfoUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestContentInfoTypeSeparator()
		{
			AssertType<SeparatorUserControl>(control.ContentInfoTypeSeparatorUserControl);
		}

		public void TestContentInfoTypeGrid()
		{
			AssertType<ZGrid>(control.ContentInfoTypeGrid);
		}

		public void TestContentInfoTypeGrid_ColumnDetails()
		{
			AssertGridColumnDetails(control.ContentInfoTypeGrid, ContentInfoTypeGridColumnDetails);
		}

		public void TestAdditionalTariffsSeparatorUserControl()
		{
			AssertType<SeparatorUserControl>(control.AdditionalTariffsSeparatorUserControl);
		}

		public void TestAdditionalTariffsGrid()
		{
			AssertType<ZGrid>(control.AdditionalTariffsGrid);
		}

		public void TestAdditionalTariffsGrid_ColumnDetails()
		{
			AssertGridColumnDetails(control.AdditionalTariffsGrid, AdditionalTariffsGridColumnDetails);
		}

		AdditionalInfoUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new AdditionalInfoUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		void AssertGridColumnDetails(ZGrid grid, (string ColumnName, string ColumnCaption, int ColumnWidth)[] expectedColumnDetails)
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.SetDataBinding(Factory.New<JobDeclaration>(), ".");
				form.Show();

				CombineAssertions(() =>
				{
					AssertSequencesEqual("Columns in order", expectedColumnDetails.Select(x => x.ColumnName), grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));

					foreach (var (columnName, columnCaption, columnWidth) in expectedColumnDetails)
					{
						AssertEquals($"Caption for {columnName}", columnCaption, grid.GetColumnCaption(columnName));
						AssertEquals($"Width for {columnName}", columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		(string ColumnName, string ColumnCaption, int ColumnWidth)[] ContentInfoTypeGridColumnDetails => new[]
		{
			(ContentInformationType.Schema.CY_Code, "Type", 80),
			(ContentInformationType.Schema.DegreePercentage, "Degree Percentage", 120)
		};

		(string ColumnName, string ColumnCaption, int ColumnWidth)[] AdditionalTariffsGridColumnDetails => new[]
		{
			(CusLineTariffDetail.Schema.BZ_Type, "Part", 43),
			(CusLineTariffDetail.Schema.BZ_Tariff, "Code", 86),
			(CusLineTariffDetail.Schema.BZ_Qty1, "Quantity", 80),
			(CusLineTariffDetail.Schema.BZ_UQ1, "UOM", 46),
			(CusLineTariffDetail.Schema.BZ_PercentAlcohol, "Degree Percentage", 119),
			(CusLineTariffDetail.Schema.BZ_TobaccoRetailPrice, "Retail Price", 95),
			(CusLineTariffDetail.Schema.ExciseValue, "Excise Value", 95)
		};
	}
}
