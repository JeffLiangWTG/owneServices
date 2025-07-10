using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class HouseConsignmentDifferencesPreviousDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber, CSI_ReferenceNumber2 },
				previousDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnsWidth()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_LineNo", 80, previousDocumentsGrid.GetColumnStyle(CSI_LineNo).Width);
				AssertEquals("CSI_Status", 80, previousDocumentsGrid.GetColumnStyle(CSI_Status).Width);
				AssertEquals("CSI_Code", 80, previousDocumentsGrid.GetColumnStyle(CSI_Code).Width);
				AssertEquals("CSI_ReferenceNumber", 200, previousDocumentsGrid.GetColumnStyle(CSI_ReferenceNumber).Width);
				AssertEquals("CSI_ReferenceNumber2", 200, previousDocumentsGrid.GetColumnStyle(CSI_ReferenceNumber2).Width);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentDifferencesPreviousDocumentsGridUserControl();
			previousDocumentsGrid = userControl.PreviousDocumentsGrid;
		}
		HouseConsignmentDifferencesPreviousDocumentsGridUserControl userControl;
		ZGrid previousDocumentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
