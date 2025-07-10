using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.ExitControlBase.Business.AutoCusExitConsignment.Schema;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ConsignmentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitConsignmentCollection<CusExitConsignment>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CXC_MovementReference, CXC_LocalReference, CXC_UniqueConsignmentReference }, consignmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumn_CXC_MovementReference()
		{
			CombineAssertions(() =>
			{
				var columnInfo = consignmentsGrid.GetColumnStyle(CXC_MovementReference);
				AssertEquals("Width", 120, columnInfo.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestColumn_CXC_LocalReference()
		{
			CombineAssertions(() =>
			{
				var columnInfo = consignmentsGrid.GetColumnStyle(CXC_LocalReference);
				AssertEquals("Width", 145, columnInfo.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestColumn_CXC_UniqueConsignmentReference()
		{
			CombineAssertions(() =>
			{
				var columnInfo = consignmentsGrid.GetColumnStyle(CXC_UniqueConsignmentReference);
				AssertEquals("Width", 220, columnInfo.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestCurrentDataItem()
		{
			var exitHeder = Factory.New<CusExitHeader>();
			exitHeder.CusExitConsignments.AddNew();
			userControl.SetDataBinding(exitHeder.CusExitConsignments, "");
			AssertType<CusExitConsignment>(userControl.CurrentDataItem);
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
