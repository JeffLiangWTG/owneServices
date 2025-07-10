using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class ArrivalGoodsItemAdditionalDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(Business.INctsAdditionalInfoCollection<Business.NctsAdditionalInfo>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CSI_LineNo, CSI_Status, CSI_SubType, CSI_Code, CSI_ReferenceNumber, CSI_Description },
				additionalDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnsWidth()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_LineNo", 80, additionalDocumentsGrid.GetColumnStyle(CSI_LineNo).Width);
				AssertEquals("CSI_Status", 80, additionalDocumentsGrid.GetColumnStyle(CSI_Status).Width);
				AssertEquals("CSI_SubType", 80, additionalDocumentsGrid.GetColumnStyle(CSI_SubType).Width);
				AssertEquals("CSI_Code", 80, additionalDocumentsGrid.GetColumnStyle(CSI_Code).Width);
				AssertEquals("CSI_ReferenceNumber", 150, additionalDocumentsGrid.GetColumnStyle(CSI_ReferenceNumber).Width);
				AssertEquals("CSI_Description", 300, additionalDocumentsGrid.GetColumnStyle(CSI_Description).Width);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ArrivalGoodsItemAdditionalDocumentsGridUserControl();
			additionalDocumentsGrid = userControl.AdditionalDocumentsGrid;
		}
		ArrivalGoodsItemAdditionalDocumentsGridUserControl userControl;
		ZGrid additionalDocumentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
