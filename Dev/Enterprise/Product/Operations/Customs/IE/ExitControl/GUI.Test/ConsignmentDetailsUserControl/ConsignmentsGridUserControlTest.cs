using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.ExitControlBase.Business.AutoCusExitConsignment.Schema;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	sealed class ConsignmentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.CusExitConsignmentCollection<Business.CusExitConsignment>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CXC_MovementReference, CXC_UniqueConsignmentReference },
					consignmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumn_CXC_MovementReference()
		{
			CombineAssertions(() =>
			{
				var columnInfo = consignmentsGrid.GetColumnStyle(CXC_MovementReference);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), columnInfo.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestColumn_CXC_UniqueConsignmentReference()
		{
			CombineAssertions(() =>
			{
				var columnInfo = consignmentsGrid.GetColumnStyle(CXC_UniqueConsignmentReference);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220), columnInfo.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ConsignmentsGridUserControl();
			consignmentsGrid = userControl.ConsignmentsGrid;
		}
		ConsignmentsGridUserControl userControl;
		ZGrid consignmentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
