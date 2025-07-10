using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class HouseConsignmentDifferencesSupportingDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsSupportingDocumentCollection<NctsSupportingDocument>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber, CSI_ReferenceNumber2 },
				supportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnsWidth()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code", 80, supportingDocumentsGrid.GetColumnStyle(CSI_Code).Width);
				AssertEquals("CSI_ReferenceNumber", 200, supportingDocumentsGrid.GetColumnStyle(CSI_ReferenceNumber).Width);
				AssertEquals("CSI_ReferenceNumber2", 200, supportingDocumentsGrid.GetColumnStyle(CSI_ReferenceNumber2).Width);
				AssertEquals("CSI_LineNo", 80, supportingDocumentsGrid.GetColumnStyle(CSI_LineNo).Width);
				AssertEquals("CSI_Status", 80, supportingDocumentsGrid.GetColumnStyle(CSI_Status).Width);
			});
		}

		public void TestCSI_StatusType()
		{
			AssertType<ZDropEditColumnStyleInfo>("CSI_Status is type ZDropEditColumnStyleInfo", supportingDocumentsGrid.GetColumnStyle(CSI_Status));
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentDifferencesSupportingDocumentGridUserControl();
			supportingDocumentsGrid = userControl.SupportingDocumentsGrid;
		}
		HouseConsignmentDifferencesSupportingDocumentGridUserControl userControl;
		ZGrid supportingDocumentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
