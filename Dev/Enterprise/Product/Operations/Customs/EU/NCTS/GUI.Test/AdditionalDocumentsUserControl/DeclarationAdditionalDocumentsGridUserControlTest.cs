using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class DeclarationAdditionalDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), userControl.BindingSource.DataSourceType);
			AssertEquals(nameof(NctsHeader.AdditionalDocuments), additionalDocumentsGrid.GetBindingMember());
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CSI_SubType, CSI_Code, CSI_ReferenceNumber, CSI_Description },
				additionalDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnsWidth()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_SubType", 80, additionalDocumentsGrid.GetColumnStyle(CSI_SubType).Width);
				AssertEquals("CSI_Code", 60, additionalDocumentsGrid.GetColumnStyle(CSI_Code).Width);
				AssertEquals("CSI_ReferenceNumber", 150, additionalDocumentsGrid.GetColumnStyle(CSI_ReferenceNumber).Width);
				AssertEquals("CSI_Description", 300, additionalDocumentsGrid.GetColumnStyle(CSI_Description).Width);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			userControl = new DeclarationAdditionalDocumentsGridUserControl();
			additionalDocumentsGrid = userControl.AdditionalDocumentsGrid;
			userControl.SetDataBinding(nctsHeader, "");
		}
		DeclarationAdditionalDocumentsGridUserControl userControl;
		ZGrid additionalDocumentsGrid;
		NctsHeader nctsHeader;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
