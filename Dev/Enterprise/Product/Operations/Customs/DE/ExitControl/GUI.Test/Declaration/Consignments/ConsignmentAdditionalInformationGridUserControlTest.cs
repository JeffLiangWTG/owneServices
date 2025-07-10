using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.ExitControl.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.DE.ExitControl.GUI.Testing
{
	sealed class ConsignmentAdditionalInformationGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlAdditionalInfoCollection), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new string[] { CSI_Code }, additionalInformationGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumn_CSI_Code()
		{
			var columnInfo = additionalInformationGrid.GetColumnStyle(CSI_Code);
			AssertEquals(80, columnInfo.Width);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ConsignmentAdditionalInformationGridUserControl();
			additionalInformationGrid = userControl.AdditionalInformationGrid;
		}
		ConsignmentAdditionalInformationGridUserControl userControl;
		ZGrid additionalInformationGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
