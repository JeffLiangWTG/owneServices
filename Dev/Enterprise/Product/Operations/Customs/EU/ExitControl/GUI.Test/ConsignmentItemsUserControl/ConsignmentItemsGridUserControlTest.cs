using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using static Enterprise.Customs.ExitControlBase.Business.AutoCusExitConsignmentItem.Schema;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
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

		public void TestAvailableColumns_NoExtraColumns()
		{
			userControl.SetDataBinding(Factory.New<CusExitConsignmentItem>(), "");
			AssertSequencesEqual("Columns", new[] { CCI_LineNumber, CCI_GrossMass, CCI_NetMass, CCI_UniqueConsignmentReference }, itemsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumn_CCI_LineNumber()
		{
			userControl.SetDataBinding(Factory.New<CusExitConsignmentItem>(), "");
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), itemsGrid.GetColumnStyle(CCI_LineNumber).Width);
		}

		public void TestColumn_CCI_GrossMass()
		{
			userControl.SetDataBinding(Factory.New<CusExitConsignmentItem>(), "");
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), itemsGrid.GetColumnStyle(CCI_GrossMass).Width);
		}

		public void TestColumn_CCI_NetMass()
		{
			userControl.SetDataBinding(Factory.New<CusExitConsignmentItem>(), "");
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), itemsGrid.GetColumnStyle(CCI_NetMass).Width);
		}

		public void TestColumn_CCI_UniqueConsignmentReference()
		{
			userControl.SetDataBinding(Factory.New<CusExitConsignmentItem>(), "");
			CombineAssertions(() =>
			{
				var columnInfo = itemsGrid.GetColumnStyle(CCI_UniqueConsignmentReference);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140), columnInfo.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestPackingDetailsGrid_ColumnsInitializedUsingConfiguredExitControlLayoutProvider()
		{
			const string columnName = "CCI_LineNumber";
			var mockGridColumnLayout = Mock.Of<IGridColumnLayout>(
				c => c.Columns == new[]
				{
					new ZTextBoxColumnStyleInfo(columnName, 120)
				});
			var mockConsignmentItemGridLayout = Mock.Of<IGridColumnLayoutProvider>(p => p.Layout == mockGridColumnLayout);
			var mockExitControlLayoutProvider = Mock.Of<IExitControlLayoutProvider>(
				p => p.ConsignmentItemsGridLayout == mockConsignmentItemGridLayout);

			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) },
				{ Core.Constants.CountryCodes.Italy, new TestObjectHandle(mockExitControlLayoutProvider) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var consignmentItem = Factory.New<CusExitConsignmentItem>();
				using (var control = new ConsignmentItemsGridUserControl())
				{
					control.SetDataBinding(consignmentItem, "");
					var grid = control.ItemsGrid;
					AssertEquals("ColumnStyles Count", 1, grid.ColumnStyles.Count);

					var columnInfo = grid.GetColumnStyle(columnName);

					AssertNotNull("Column Style Info", columnInfo);
					CombineAssertions("Column Info", () =>
					{
						AssertEquals("Column Name", columnName, columnInfo.ColumnName);
						AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), columnInfo.Width);
					});
				}
			}
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
