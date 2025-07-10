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

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class ReportAuthorizationsGridUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType() => AssertEquals(typeof(EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitReport>), userControl.BindingSource.DataSourceType);

	public void TestAvailableColumns()
	{
		var exitHeader = CreateCusExitHeaderForTest();

		using (var form = new ExitControlForm(exitHeader))
		{
			userControl.SetDataBinding(exitHeader, "");
			form.Show();

			AssertSequencesEqual("Columns",
				[CusAuthorizationUsage.Schema.AGC_Code, nameof(CusAuthorizationUsage.CustomsCode), nameof(CusAuthorizationUsage.EffectiveReferenceNumber), CusAuthorizationUsage.Schema.AGC_OH_Owner],
				authorizationsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}
	}

	public void TestColumn_AGC_Code()
	{
		userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
		var columnInfo = authorizationsGrid.GetColumnStyle(CusAuthorizationUsage.Schema.AGC_Code);
		AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
	}

	public void TestColumn_CustomsCode()
	{
		userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
		var columnInfo = authorizationsGrid.GetColumnStyle(nameof(CusAuthorizationUsage.CustomsCode));
		AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
	}

	public void TestColumn_EffectiveReferenceNumber()
	{
		userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
		var columnInfo = authorizationsGrid.GetColumnStyle(nameof(CusAuthorizationUsage.EffectiveReferenceNumber));
		AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
	}

	public void TestColumn_AGC_OH_Owner()
	{
		userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
		var columnInfo = authorizationsGrid.GetColumnStyle(CusAuthorizationUsage.Schema.AGC_OH_Owner);
		AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
	}

	public void TestColumnsAreInitializedUsingConfiguredExitControlLayoutProvider()
	{
		const string columnName = "AGC_Code";
		var mockGridColumnLayout = Mock.Of<IGridColumnLayout>(
			c => c.Columns == new[]
			{
					new ZTextBoxColumnStyleInfo(columnName, 80)
			});
		var mockAuthorizationsGridLayout = Mock.Of<IGridColumnLayoutProvider>(p => p.Layout == mockGridColumnLayout);
		var mockExitControlLayoutProvider = Mock.Of<IExitControlLayoutProvider>(
			p => p.AuthorizationGridColumnLayout == mockAuthorizationsGridLayout);

		var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Lithuania, new TestObjectHandle(mockExitControlLayoutProvider) }
			};

		using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Lithuania))
		{
			var exitHeader = CreateCusExitHeaderForTest();
			using (var control = new ReportItemsUserControl())
			{
				control.SetDataBinding(exitHeader, "");
				var authorizationsGrid = control.AuthorizationsGrid;
				AssertEquals("ColumnStyles Count", 1, authorizationsGrid.ColumnStyles.Count);

				var columnInfo = authorizationsGrid.GetColumnStyle(columnName);

				AssertNotNull("Column Style Info", columnInfo);
				CombineAssertions("Column Info", () =>
				{
					AssertEquals("Column Name", columnName, columnInfo.ColumnName);
					AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
				});
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new ReportAuthorizationsGridUserControl();
		authorizationsGrid = userControl.AuthorizationsGrid;
	}

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}

	ReportAuthorizationsGridUserControl userControl;
	ZGrid authorizationsGrid;

	CusExitHeader CreateCusExitHeaderForTest()
	{
		var exitHeader = Factory.New<CusExitHeader>();
		var exitConsignment = exitHeader.CusExitConsignments.AddNew();
		var exitConsignmentItem = exitConsignment.CusExitConsignmentItems.AddNew();

		var exitReport = exitHeader.CusExitReports.AddNew();
		var exitReportItem = exitReport.CusExitReportItems.AddNew();
		exitReportItem.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;
		return exitHeader;
	}
}
