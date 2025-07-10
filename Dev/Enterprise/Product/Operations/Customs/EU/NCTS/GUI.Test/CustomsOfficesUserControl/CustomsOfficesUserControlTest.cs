using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class CustomsOfficesUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
		}

		public void TestCustomsOfficesGrid()
		{
			var customsOfficesGrid = control.CustomsOfficesGrid;
			CombineAssertions(() =>
			{
				AssertSequencesEqual("Sequences", new[] { Customs.Business.AutoCusCodeData.Schema.CY_Order, Customs.Business.AutoCusCodeData.Schema.CY_Code, Customs.Business.AutoCusCodeData.Schema.CY_Data, EuOfficeCode.Schema.CY_OfficeDescription, Customs.Business.AutoCusCodeData.Schema.CY_Date }
					, customsOfficesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));

				AssertEquals("CY_Order", 60, customsOfficesGrid.GetColumnStyle(Customs.Business.AutoCusCodeData.Schema.CY_Order).Width);
				AssertEquals("CY_Code", 59, customsOfficesGrid.GetColumnStyle(Customs.Business.AutoCusCodeData.Schema.CY_Code).Width);
				AssertEquals("CY_Data", 70, customsOfficesGrid.GetColumnStyle(Customs.Business.AutoCusCodeData.Schema.CY_Data).Width);
				AssertEquals("CY_OfficeDescription", 200, customsOfficesGrid.GetColumnStyle(EuOfficeCode.Schema.CY_OfficeDescription).Width);
				AssertEquals("CY_Date", 95, customsOfficesGrid.GetColumnStyle(Customs.Business.AutoCusCodeData.Schema.CY_Date).Width);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CustomsOfficesUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		CustomsOfficesUserControl control;
	}
}
