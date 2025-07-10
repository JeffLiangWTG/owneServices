using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.AE.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class VehicleUserControlTest : TestCaseWithFactory
{
	public void TestVehiclesGridColumnOrder()
	{
		using var userControl = new InvoiceLineUserControl();
		var controlGrid = (ZGrid)userControl.Controls.Find("VehiclesGrid", searchAllChildren: true).Single();
		var columnsOrder = controlGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
		var expectedColumnOrder = new[]
		{
			CusVehicle.Schema.CVH_VehicleIdentificationNumber,
			CusVehicle.Schema.CVH_BrandName,
			CusVehicle.Schema.CVH_ModelName,
			nameof(CusVehicle.Engine) + "+" + CusEngine.Schema.CEG_EngineNumber,
			nameof(CusVehicle.Engine) + "+" + CusEngine.Schema.CEG_CapacityCC,
			CusVehicle.Schema.CVH_Seats,
			CusVehicle.Schema.CVH_Payload,
			CusVehicle.Schema.CVH_PayloadUQ,
			CusVehicle.Schema.CVH_ModelYear,
			CusVehicle.Schema.CVH_Color,
			CusVehicle.Schema.CVH_IsUsed,
			CusVehicle.Schema.CVH_CarType,
			CusVehicle.Schema.CVH_DriveSide,
			CusVehicle.Schema.CVH_SpecificationStandard
		};
		AssertArrayEqualsByElements(expectedColumnOrder, columnsOrder);
	}

	public void TestVehiclesGridColumn_EngineCEG_CapacityCC()
	{
		using var userControl = new InvoiceLineUserControl();
		var controlGrid = (ZGrid)userControl.Controls.Find("VehiclesGrid", searchAllChildren: true).Single();
		var column = (ZCalcEditColumnStyleInfo)controlGrid.ColumnStyles.Cast<ZGridColumnInfo>()
			.First(x => x.ColumnName == nameof(CusVehicle.Engine) + "+" + CusEngine.Schema.CEG_CapacityCC);

		AssertEquals("Max value", 9999m, column.MaxValue);
		AssertEquals("Decimal places", 2, column.Decimals);
	}

	public void TestVehiclesGridColumn_CVH_Payload()
	{
		using var userControl = new InvoiceLineUserControl();
		var controlGrid = (ZGrid)userControl.Controls.Find("VehiclesGrid", searchAllChildren: true).Single();
		var column = (ZCalcEditColumnStyleInfo)controlGrid.ColumnStyles.Cast<ZGridColumnInfo>()
			.First(x => x.ColumnName == CusVehicle.Schema.CVH_Payload);

		AssertEquals("Max value", 9999m, column.MaxValue);
		AssertEquals("Decimal places", 2, column.Decimals);
	}
}
