using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class AlternativeEvidenceUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalDocumentGroupBox()
		{
			var reportItemAdditionalInfosGroupBox = userControl.AdditionalDocumentGroupBox;
			AssertEquals("Caption", "Additional Document", reportItemAdditionalInfosGroupBox.CaptionResourceString.Caption);
		}

		public void TestAdditionalDocumentsGrid()
		{
			AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, userControl.AdditionalDocumentsGrid.Dock);
		}

		public void TestAdditionalDocumentAvailableColumns()
		{
			AssertSequencesEqual("Columns", new string[] { "CSI_SubType", "CSI_Code", "CSI_ReferenceNumber" }, userControl.AdditionalDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(IAlternativeEvidenceCollection<AlternativeEvidence>), userControl.BindingSource.DataSourceType);
		}

		public void TestAlternativeEvidenceAvailableColumns()
		{
			AssertSequencesEqual("Columns", new string[] { "CY_Code" }, alternativeEvidencesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestAlternativeEvidence_CY_Code()
		{
			var columnInfo = alternativeEvidencesGrid.GetColumnStyle("CY_Code");
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new AlternativeEvidenceUserControl();
			alternativeEvidencesGrid = userControl.AlternativeEvidencesGrid;
		}
		AlternativeEvidenceUserControl userControl;
		ZGrid alternativeEvidencesGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
