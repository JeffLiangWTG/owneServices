using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.ExitControl.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.ExitControlBase.Business.AutoCusExitConsignmentItem.Schema;

namespace Enterprise.Customs.DE.ExitControl.GUI.Testing
{
	sealed class ConsignmentItemsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitConsignmentItemCollection<CusExitConsignmentItem>), userControl.BindingSource.DataSourceType);
		}

		public void TestItemsGroupBox()
		{
			var itemsGroupBox = userControl.ItemsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, itemsGroupBox.Dock);
				AssertEquals("Caption", "Items", itemsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Contains ItemsGrid", true, itemsGroupBox.Controls.Contains(itemsGrid));
			});
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CCI_LineNumber, CCI_GrossMass, CCI_NetMass, CCI_UniqueConsignmentReference, CCI_ReferenceNumber },
					itemsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumn_CCI_LineNumber()
		{
			AssertEquals(60, itemsGrid.GetColumnStyle(CCI_LineNumber).Width);
		}

		public void TestColumn_CCI_GrossMass()
		{
			AssertEquals(100, itemsGrid.GetColumnStyle(CCI_GrossMass).Width);
		}

		public void TestColumn_CCI_NetMass()
		{
			AssertEquals(100, itemsGrid.GetColumnStyle(CCI_NetMass).Width);
		}

		public void TestColumn_CCI_UniqueConsignmentReference()
		{
			var columnInfo = itemsGrid.GetColumnStyle(CCI_UniqueConsignmentReference);
			AssertEquals(140, columnInfo.Width);
		}

		public void TestCCI_ReferenceNumber()
		{
			var columnInfo = itemsGrid.GetColumnStyle(CCI_ReferenceNumber);
			AssertEquals(140, columnInfo.Width);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ConsignmentItemsGridUserControl();
			itemsGrid = userControl.ItemsGrid;
		}
		ConsignmentItemsGridUserControl userControl;
		ZGrid itemsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
