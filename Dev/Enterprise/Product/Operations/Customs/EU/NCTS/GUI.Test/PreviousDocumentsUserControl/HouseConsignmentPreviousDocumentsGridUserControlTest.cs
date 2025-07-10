using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class HouseConsignmentPreviousDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CommonPreviousDocumentCollection<CommonPreviousDocument>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CSI_Code, CSI_ReferenceNumber, CSI_ReferenceNumber2 },
				previousDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnsWidth()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code", 60, previousDocumentsGrid.GetColumnStyle(CSI_Code).Width);
				AssertEquals("CSI_ReferenceNumber", 200, previousDocumentsGrid.GetColumnStyle(CSI_ReferenceNumber).Width);
				AssertEquals("CSI_ReferenceNumber2", 200, previousDocumentsGrid.GetColumnStyle(CSI_ReferenceNumber2).Width);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentPreviousDocumentsGridUserControl();
			previousDocumentsGrid = userControl.PreviousDocumentsGrid;
		}
		HouseConsignmentPreviousDocumentsGridUserControl userControl;
		ZGrid previousDocumentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
