using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using static Enterprise.Customs.ExitControlBase.Business.AutoCusExitConsignmentPackage.Schema;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	class ConsignmentItemPackingDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitConsignmentPivotCollection<CusExitConsignmentPivot>), userControl.BindingSource.DataSourceType);
		}

		public void TestPackingDetailsGroupBox()
		{
			var packingDetailsGroupBox = userControl.PackingDetailsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, packingDetailsGroupBox.Dock);
				AssertEquals("Caption", "Packing Details", packingDetailsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Contains PackingDetailsGrid", true, packingDetailsGroupBox.Controls.Contains(packingDetailsGrid));
			});
		}

		public void TestAvailableColumns_NoExtraColumns()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			AssertSequencesEqual("Columns", new[] { SequenceColumnName, QuantityColumnName, PackageTypeColumnName, MarksAndNumbersColumnName, CusExitConsignmentPivot.Schema.CNP_CXN_Container },
					packingDetailsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumn_CXP_Sequence()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50), packingDetailsGrid.GetColumnStyle(SequenceColumnName).Width);
		}

		public void TestColumn_CXP_Quantity()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), packingDetailsGrid.GetColumnStyle(QuantityColumnName).Width);
		}

		public void TestColumn_CXP_PackageType()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60), packingDetailsGrid.GetColumnStyle(PackageTypeColumnName).Width);
		}

		public void TestColumn_CXP_MarksAndNumbers()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), packingDetailsGrid.GetColumnStyle(MarksAndNumbersColumnName).Width);
		}

		public void TestPackingDetailsGrid_ColumnsInitializedUsingConfiguredExitControlLayoutProvider()
		{
			const string columnName = "Package+CXP_MarksAndNumbersStatus";
			var mockGridColumnLayout = Mock.Of<IGridColumnLayout>(
				c => c.Columns == new[]
				{
					new ZDropEditColumnStyleInfo(columnName, 120)
				});
			var mockConsignmentItemPackagingDetailsGridLayout = Mock.Of<IGridColumnLayoutProvider>(p => p.Layout == mockGridColumnLayout);
			var mockExitControlLayoutProvider = Mock.Of<IExitControlLayoutProvider>(
				p => p.ConsignmentItemPackingDetailsGridLayout == mockConsignmentItemPackagingDetailsGridLayout);

			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) },
				{ Core.Constants.CountryCodes.Italy, new TestObjectHandle(mockExitControlLayoutProvider) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var exitHeader = CreateCusExitHeaderForTest();
				using (var control = new ConsignmentItemPackingDetailsUserControl())
				{
					control.SetDataBinding(exitHeader, "");
					var packagingDetailsGrid = control.PackingDetailsGrid;
					AssertEquals("ColumnStyles Count", 1, packagingDetailsGrid.ColumnStyles.Count);

					var columnInfo = packagingDetailsGrid.GetColumnStyle(columnName);

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
			userControl = new ConsignmentItemPackingDetailsUserControl();
			packingDetailsGrid = userControl.PackingDetailsGrid;
		}
		ConsignmentItemPackingDetailsUserControl userControl;
		ZGrid packingDetailsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		const string Package = nameof(CusExitConsignmentPivot.Package);

		const string SequenceColumnName = Package + "+" + CXP_Sequence;

		const string QuantityColumnName = Package + "+" + CXP_Quantity;

		const string PackageTypeColumnName = Package + "+" + CXP_PackageType;

		const string MarksAndNumbersColumnName = Package + "+" + CXP_MarksAndNumbers;

		CusExitHeader CreateCusExitHeaderForTest()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var exitConsignment = exitHeader.CusExitConsignments.AddNew();
			var exitConsignmentItem = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
			exitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var exitConsignmentItem2 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem2.CusExitConsignmentPackagePivots.AddNew();

			return exitHeader;
		}
	}
}
