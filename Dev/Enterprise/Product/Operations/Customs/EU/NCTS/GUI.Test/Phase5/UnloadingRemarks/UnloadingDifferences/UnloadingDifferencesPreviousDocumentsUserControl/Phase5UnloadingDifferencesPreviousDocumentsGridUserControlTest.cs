using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5UnloadingDifferencesPreviousDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(Business.CommonPreviousDocumentCollection<Business.CommonPreviousDocument>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CSI_ItemNumber, CSI_Status, CSI_Code, CSI_ReferenceNumber, CSI_ReferenceNumber2 },
				previousDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestUnloadingDifferencesPreviousDocumentsGrid()
		{
			var unloadingDifferencesPreviousDocumentsGrid = userControl.UnloadingDifferencesPreviousDocumentsGrid;
			CombineAssertions(() =>
			{
				AssertType<ZGrid>("Type", unloadingDifferencesPreviousDocumentsGrid);
				AssertEquals("BindTo", ".", unloadingDifferencesPreviousDocumentsGrid.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5UnloadingDifferencesPreviousDocumentsGridUserControl();
			previousDocumentsGrid = userControl.UnloadingDifferencesPreviousDocumentsGrid;
		}
		Phase5UnloadingDifferencesPreviousDocumentsGridUserControl userControl;
		ZGrid previousDocumentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
