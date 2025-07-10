using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class UnloadingDifferencesSupportingDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsSupportingDocumentCollection<NctsSupportingDocument>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[]
				{
					nameof(NctsSupportingDocument.CSI_ItemNumber),
					nameof(NctsSupportingDocument.CSI_Status),
					nameof(NctsSupportingDocument.CSI_Code),
					nameof(NctsSupportingDocument.CSI_ReferenceNumber),
					nameof(NctsSupportingDocument.CSI_ReferenceNumber2)
				},
				supportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			userControl = new UnloadingDifferencesSupportingDocumentsGridUserControl();
			supportingDocumentsGrid = userControl.SupportingDocumentsGrid;
			userControl.SetDataBinding(nctsHeader, "ArrivalMovementHeader.SupportingDocuments");
		}
		UnloadingDifferencesSupportingDocumentsGridUserControl userControl;
		ZGrid supportingDocumentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
