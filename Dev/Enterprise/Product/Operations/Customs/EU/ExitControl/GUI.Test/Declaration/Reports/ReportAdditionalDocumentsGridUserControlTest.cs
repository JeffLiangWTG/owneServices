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
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ReportAdditionalDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(IAdditionalInfoCollection<AdditionalInfo>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns_NoExtraColumns()
		{
			var exitHeader = CreateCusExitHeaderForTest();

			using (var form = new ExitControlForm(exitHeader))
			{
				userControl.SetDataBinding(exitHeader, "");
				form.Show();

				AssertSequencesEqual("Columns", new string[] { CSI_SubType, CSI_Code, CSI_ReferenceNumber }, additionalDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		public void TestColumn_CSI_SubType()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			var columnInfo = additionalDocumentsGrid.GetColumnStyle(CSI_SubType);
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		public void TestColumn_CSI_Code()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			var columnInfo = additionalDocumentsGrid.GetColumnStyle(CSI_Code);
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		public void TestColumn_CSI_ReferenceNumber()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			var columnInfo = additionalDocumentsGrid.GetColumnStyle(CSI_ReferenceNumber);
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), columnInfo.Width);
		}

		public void TestAdditionalDocumentsGrid_ColumnsInitializedUsingConfiguredExitControlLayoutProvider()
		{
			const string columnName = "CSI_ReferenceNumber";
			var mockGridColumnLayout = Mock.Of<IGridColumnLayout>(
				c => c.Columns == new[]
				{
					new ZTextBoxColumnStyleInfo(columnName, 150)
				});
			var mockAdditionalDocumentsGridLayout = Mock.Of<IGridColumnLayoutProvider>(p => p.Layout == mockGridColumnLayout);
			var mockExitControlLayoutProvider = Mock.Of<IExitControlLayoutProvider>(
				p => p.ReportAdditionalDocumentsGridLayout == mockAdditionalDocumentsGridLayout);

			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) },
				{ Core.Constants.CountryCodes.Italy, new TestObjectHandle(mockExitControlLayoutProvider) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var exitHeader = CreateCusExitHeaderForTest();
				using (var control = new ReportAdditionalDocumentsGridUserControl())
				{
					control.SetDataBinding(exitHeader, "");
					var additionalInfGrid = control.AdditionalDocumentsGrid;
					AssertEquals("ColumnStyles Count", 1, additionalInfGrid.ColumnStyles.Count);

					var columnInfo = additionalInfGrid.GetColumnStyle(columnName);

					AssertNotNull("Column Style Info", columnInfo);
					CombineAssertions("Column Info", () =>
					{
						AssertEquals("Column Name", columnName, columnInfo.ColumnName);
						AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150), columnInfo.Width);
					});
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ReportAdditionalDocumentsGridUserControl();
			additionalDocumentsGrid = userControl.AdditionalDocumentsGrid;
		}

		ReportAdditionalDocumentsGridUserControl userControl;
		ZGrid additionalDocumentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		CusExitHeader CreateCusExitHeaderForTest()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var exitConsignment = exitHeader.CusExitConsignments.AddNew();
			var exitConsignmentItem = exitConsignment.CusExitConsignmentItems.AddNew();
			var packagePivot = exitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();

			var exitReport = exitHeader.CusExitReports.AddNew();
			var exitReportItem = exitReport.CusExitReportItems.AddNew();
			exitReportItem.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;
			exitReportItem.ERI_CXP_Package = packagePivot.Package.PK;
			exitReportItem.AdditionalInfos.AddNew();
			return exitHeader;
		}
	}
}
