using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.ExitControl.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.ExitControlBase.Business.AutoCusExitConsignment.Schema;

namespace Enterprise.Customs.DE.ExitControl.GUI.Testing
{
	sealed class ConsignmentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitConsignmentCollection<CusExitConsignment>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CXC_MovementReference, CXC_LocalReference, CXC_Status, nameof(CusExitConsignment.StatusDescription), CXC_UniqueConsignmentReference, CXC_ReferenceNumber },
					consignmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
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

		public void TestColumn_CXC_Status()
		{
			CombineAssertions(() =>
			{
				var columnInfo = consignmentsGrid.GetColumnStyle(CXC_Status);
				AssertEquals("Width", 50, columnInfo.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestColumn_StatusDescription()
		{
			var columnInfo = consignmentsGrid.GetColumnStyle(nameof(CusExitConsignment.StatusDescription));
			AssertEquals("Width", 440, columnInfo.Width);
			AssertEquals("CharacterCasing", CharacterCasing.Normal, columnInfo.CharacterCasing);
		}

		public void TestColumn_CXC_UniqueConsignmentReference()
		{
			var columnInfo = consignmentsGrid.GetColumnStyle(CXC_UniqueConsignmentReference);
			AssertEquals(220, columnInfo.Width);
		}

		public void TestCXC_ReferenceNumber()
		{
			var columnInfo = consignmentsGrid.GetColumnStyle(CXC_ReferenceNumber);
			AssertEquals(320, columnInfo.Width);
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
