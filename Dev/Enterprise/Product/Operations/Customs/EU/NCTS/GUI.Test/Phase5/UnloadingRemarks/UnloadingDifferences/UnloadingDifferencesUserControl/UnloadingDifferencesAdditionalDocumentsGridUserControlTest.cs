using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class UnloadingDifferencesAdditionalDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(INctsAdditionalInfoCollection<NctsAdditionalInfo>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[]
				{
					nameof(NctsAdditionalInfo.CSI_ItemNumber),
					nameof(NctsAdditionalInfo.CSI_Status),
					nameof(NctsAdditionalInfo.CSI_SubType),
					nameof(NctsAdditionalInfo.CSI_Code),
					nameof(NctsAdditionalInfo.CSI_ReferenceNumber),
					nameof(NctsAdditionalInfo.CSI_Description)
				},
				additionalDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			userControl = new UnloadingDifferencesAdditionalDocumentsGridUserControl();
			additionalDocumentsGrid = userControl.AdditionalDocumentsGrid;
			userControl.SetDataBinding(nctsHeader, "ArrivalMovementHeader.AdditionalDocuments");
		}
		UnloadingDifferencesAdditionalDocumentsGridUserControl userControl;
		ZGrid additionalDocumentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
