using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class ExportJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public override void TestMergeByForExport()
		{
			Assert("Export declaration for AU does not merge", true);
		}

		public void TestValidateJE_ContainerMode()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = "";
			AssertEquals(true, declaration.JE_ContainerModeInfo.HasMessageErrors());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_ContainerMode = Core.Constants.TransportModes.Air;
			AssertEquals(false, declaration.JE_ContainerModeInfo.HasMessageErrors());
		}

		public void TestBlankContainerModeForSeaShipment()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = "";
			AssertEquals(true, declaration.JE_ContainerModeInfo.HasMessageErrors());
		}

		public virtual void TestValidateJE_ExportDate()
		{
			declaration.JE_ExportDate = ZDateTime.Today;
			AssertEquals(false, declaration.JE_ExportDateInfo.HasMessageErrors());

			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertEquals(true, declaration.JE_ExportDateInfo.HasMessageErrors());

			declaration.JE_ExportDate = ZDateTime.Invalid;
			AssertEquals(true, declaration.JE_ExportDateInfo.HasErrors());
		}

		public void TestValidateFlightNo()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_ExportGoodsType = "OP";
			declaration.JE_VoyageFlightNo = "";
			Assert("No error expected", !declaration.JE_VoyageFlightNoInfo.HasNotifications());

			declaration.JE_ExportGoodsType = "SP";
			declaration.JE_VoyageFlightNo = "";
			Assert("Error expected, Flight No required", declaration.JE_VoyageFlightNoInfo.HasNotifications());

			declaration.JE_ExportGoodsType = "ST";
			declaration.JE_VoyageFlightNo = "";
			Assert("Error expected, Flight No required", declaration.JE_VoyageFlightNoInfo.HasNotifications());

			declaration.JE_VoyageFlightNo = "QF11";
			Assert("No Error expected", !declaration.JE_VoyageFlightNoInfo.HasNotifications());

			declaration.JE_VoyageFlightNo = "1AA11";
			Assert("Error expected", declaration.JE_VoyageFlightNoInfo.HasNotifications());

			declaration.JE_VoyageFlightNo = "AA11";
			Assert("No Error expected", !declaration.JE_VoyageFlightNoInfo.HasNotifications());
		}

		public void TestJE_OA_WarehouseAddress()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
			declaration.DepotDocAddress.E2_OA_Address = DepotAddress();
			AssertEquals("Warning should exist on Depot Address", true, declaration.DepotDocAddress.E2_OA_AddressInfo.HasWarnings());
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress();
			declaration.DepotDocAddress.Validation.ValidateE2_OA_Address();
			AssertEquals("Warning should not exist on Depot Address", false, declaration.DepotDocAddress.E2_OA_AddressInfo.HasWarnings());
			AssertEquals("Message Errors should exist on Bonded Warehouse", true, declaration.WarehouseDocAddress.E2_OA_AddressInfo.HasMessageErrors());
			declaration.WarehouseDocAddress.Address.Header.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "AP21P");
			declaration.WarehouseDocAddress.Validation.ValidateE2_OA_Address();
			AssertEquals("No notifications Expected on Bonded Warehouse", false, declaration.WarehouseDocAddress.E2_OA_AddressInfo.HasNotifications());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		}

		protected override JobDeclarationValidation GetNewValidationProvider(JobDeclaration jobDeclaration)
		{
			return new ExportJobDeclarationValidation(jobDeclaration);
		}

		ZGuid DepotAddress()
		{
			OrgHeader depot = Factory.New<OrgHeader>();
			depot.OH_FullName = "Test Depot";
			depot.MainAddress.OA_Address1 = "Test Address";
			depot.OH_RL_NKClosestPort = "AUSYD";
			return depot.MainAddress.PK;
		}

		ZGuid WarehouseAddress()
		{
			OrgHeader warehouse = Factory.New<OrgHeader>();
			warehouse.OH_FullName = "Test Warehouse";
			warehouse.MainAddress.OA_Address1 = "Test Address";
			warehouse.OH_RL_NKClosestPort = "AUSYD";

			return warehouse.MainAddress.PK;
		}

		#endregion
	}
}
