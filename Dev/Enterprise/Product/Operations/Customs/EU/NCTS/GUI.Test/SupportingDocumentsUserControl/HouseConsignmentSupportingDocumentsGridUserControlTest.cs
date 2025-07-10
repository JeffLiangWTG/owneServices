using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class HouseConsignmentSupportingDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsSupportingDocumentCollection<NctsSupportingDocument>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CSI_Code, CSI_ReferenceNumber, CSI_ItemNumber, CSI_ReferenceNumber2 },
				supportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnsWidth()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code", 80, supportingDocumentsGrid.GetColumnStyle(CSI_Code).Width);
				AssertEquals("CSI_ReferenceNumber", 200, supportingDocumentsGrid.GetColumnStyle(CSI_ReferenceNumber).Width);
				AssertEquals("CSI_ItemNumber", 80, supportingDocumentsGrid.GetColumnStyle(CSI_ItemNumber).Width);
				AssertEquals("CSI_ReferenceNumber2", 200, supportingDocumentsGrid.GetColumnStyle(CSI_ReferenceNumber2).Width);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			userControl = new HouseConsignmentSupportingDocumentsGridUserControl();
			supportingDocumentsGrid = userControl.SupportingDocumentsGrid;
			userControl.SetDataBinding(nctsHeader, "");
		}
		HouseConsignmentSupportingDocumentsGridUserControl userControl;
		ZGrid supportingDocumentsGrid;
		NctsHeader nctsHeader;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
