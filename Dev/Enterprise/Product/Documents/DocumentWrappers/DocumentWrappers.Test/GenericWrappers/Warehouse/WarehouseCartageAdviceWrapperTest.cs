using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseCartageAdviceWrapper))]
	sealed class WarehouseCartageAdviceWrapperTest : WarehouseOrderWrapperTest
	{
		#region Test Properties

		#region TestPrintDGDetails

		protected override void TestPrintDGDetailsCore()
		{
			AssertEquals("Precondition: Empty", "", CartageAdviceWrapper.PrintDGDetails);

			var line1 = Order.Lines.AddNew();

			var part = Factory.New<OrgSupplierPart>();
			part.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			line1.WE_OP = part.PK;
			var releaseLine1 = line1.ReleaseLines.AddNew();
			var pick = Factory.New<WhsPick>();
			Order.WD_WP = pick.PK;

			var whsOrderWrapper1 = GetNewWarehouseJobGenericWrapper(Order);
			AssertEquals("Y", whsOrderWrapper1.PrintDGDetails);
		}

		#endregion

		#region TestWarehouseCartageCoordinatorName

		protected override void TestWarehouseCartageCoordinatorNameCore()
		{
			var order = GetNewDocket();
			order.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			order.Warehouse.WW_WarehouseName = "Warehouse One";

			var whsOrg = Factory.New<OrgHeader>();
			var address = whsOrg.MainAddress;
			address.OA_OH = whsOrg.PK;
			order.Warehouse.WW_OA_WarehouseAddress = address.PK;

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_FullName = "Person 1";
			glbStaff.GS_Code = "AAA";

			var assignedStaff = whsOrg.StaffAssignments.AddNew();
			assignedStaff.O8_GS_NKPersonResponsible = glbStaff.GS_Code;
			assignedStaff.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;

			var cartageAdviceWrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("Person 1", cartageAdviceWrapper.WarehouseCartageCoordinatorName);
		}

		#endregion

		#region TestWarehouseCartageCoordinatorPhone

		protected override void TestWarehouseCartageCoordinatorPhoneCore()
		{
			var order = GetNewDocket();
			order.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			order.Warehouse.WW_WarehouseName = "Warehouse One";

			var whsOrg = Factory.New<OrgHeader>();
			var address = whsOrg.MainAddress;
			address.OA_OH = whsOrg.PK;
			order.Warehouse.WW_OA_WarehouseAddress = address.PK;

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			glbStaff.GS_FullName = "Person 1";
			glbStaff.GS_Code = "BBB";
			glbStaff.GS_WorkPhone = "02 5555 9999";

			var assignedStaff = whsOrg.StaffAssignments.AddNew();
			assignedStaff.O8_GS_NKPersonResponsible = glbStaff.GS_Code;
			assignedStaff.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;

			var cartageAdviceWrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("+61 2 5555 9999", cartageAdviceWrapper.WarehouseCartageCoordinatorPhone);
		}

		#endregion

		#region TestCartageAdviceOpeningText

		protected override void TestCartageAdviceOpeningTextCore()
		{
			AssertEquals("Precondition: Empty", "", CartageAdviceWrapper.CartageAdviceOpeningText);

			DocumentsDataRegistry.Instance.WarehouseCartageAdviceOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Warehouse Cartage Advice Opening Text");

			AssertEquals("Warehouse Cartage Advice Opening Text", CartageAdviceWrapper.CartageAdviceOpeningText);
		}

		#endregion

		#region TestCartageAdviceClosingText

		protected override void TestCartageAdviceClosingTextCore()
		{
			AssertEquals("Precondition: Empty", "", CartageAdviceWrapper.CartageAdviceClosingText);

			DocumentsDataRegistry.Instance.WarehouseCartageAdviceClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Warehouse Cartage Advice Closing Text");

			AssertEquals("Warehouse Cartage Advice Closing Text", CartageAdviceWrapper.CartageAdviceClosingText);
		}

		#endregion

		#region TestContainerNumberAndTypeLine

		protected override void TestContainerNumberAndTypeLineCore()
		{
			var order = GetNewDocket();
			var cartageAdviceWrapper = GetNewWarehouseJobGenericWrapper(order);

			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition: Empty", "", cartageAdviceWrapper.ContainerNumberAndTypeLine);

			var ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "20FR";
			var ref3 = Factory.New<RefContainer>();
			ref3.RC_Code = "40FR";
			var ref4 = Factory.New<RefContainer>();
			ref4.RC_Code = "30FR";

			var container1 = order.Containers.AddNew();
			container1.WC_ContainerNum = "CONTAINER1";
			container1.WC_RC = ref1.PK;

			order.Containers.AddNew().WC_ContainerNum = "CONTAINER2";

			var container3 = order.Containers.AddNew();
			container3.WC_ContainerNum = "CONTAINER3";
			container3.WC_RC = ref3.PK;

			var container4 = order.Containers.AddNew();
			container4.WC_ContainerNum = "CONTAINER4";
			container4.WC_RC = ref4.PK;

			order.Containers.AddNew().WC_ContainerNum = "CONTAINER5";
			AssertEquals("Cartage Advice Container Number And Type Line", "CONTAINER1 (20FR), CONTAINER2, CONTAINER3 (40FR), CONTAINER4 (30FR), CONTAINER5", cartageAdviceWrapper.ContainerNumberAndTypeLine);

			order.Containers.AddNew().WC_ContainerNum = "CONTAINER6";
			AssertEquals("Cartage Advice Container Number And Type Line", "CONTAINER1 (20FR), CONTAINER2, CONTAINER3 (40FR), CONTAINER4 (30FR), CONTAINER5 ...", cartageAdviceWrapper.ContainerNumberAndTypeLine);

			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Separate Container Details list", "See attached Container Details list.", cartageAdviceWrapper.ContainerNumberAndTypeLine);
		}

		#endregion

		#region TestCartageDropMode

		protected override void TestCartageDropModeCore()
		{
			var order = (WhsOrder)GetNewDocket();
			var cartageAdviceWrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("Precondition: Empty", "", cartageAdviceWrapper.CartageDropMode);

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "AU";
			consignee.OH_FullName = "My Organisation Name";

			var orgAddress = consignee.Addresses[0];
			orgAddress.OA_LCLEquipmentNeeded = Core.Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			orgAddress.OA_FCLEquipmentNeeded = Core.Constants.FCLEquipmentNeeded.WaitForUnpack;

			order.ConsigneePK = consignee.PK;
			order.WD_DropMode = "";
			AssertEquals("Should have loaded Cartage Drop Mode for LCL freight", "HUL - Hand Unload/Load by Premise", cartageAdviceWrapper.CartageDropMode);

			order.Containers.AddNew().WC_ContainerNum = "C0001";
			AssertEquals("Should have loaded Cartage Drop Mode for FCL freight", "WUP - Wait for Pack/Unpack", cartageAdviceWrapper.CartageDropMode);

			order.WD_DropMode = Core.Constants.EquipmentNeeded.Ask;
			AssertEquals("Should be the order drop mode", "ASK - Ask Client", cartageAdviceWrapper.CartageDropMode);
		}

		#endregion

		#endregion

		#region Implementation

		#region CartageAdviceWrapper

		WarehouseCartageAdviceWrapper CartageAdviceWrapper
		{
			get { return cartageAdviceWrapper ?? (cartageAdviceWrapper = (WarehouseCartageAdviceWrapper)Wrapper); }
		}

		WarehouseCartageAdviceWrapper cartageAdviceWrapper;

		#endregion

		#region GetNewWarehouseJobGenericWrapper

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehouseCartageAdviceWrapper((WhsOrder)bizO, Factory);
		}

		#endregion

		#endregion

		#region Overriden Abstract Members

		public override void TestWrapperMappingsEmpty()
		{
			var emptyWrapper = new WarehouseCartageAdviceWrapper(null, Factory);
			AssertEquals("PickNo", ZString.Empty, emptyWrapper.PickNo.Value);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return CartageAdviceWrapper;
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return string.Format(@"
CarrierAccount :  is null
CarrierServiceLevel : 
Client : 
ClientRequestedBillToParty :  is null
CODAmount : 
CODType : 
ConfirmationInstructions : 
Consignee : 
ConsigneeAddress : 
Consignor : 
CubicSent : 
CustomerReference : 
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress : 
DropMode : 
DropOffAddress : 
FinalisedDate : 
Forwarder : 
FulfillRule : 
GoodsBillToAddress : 
HandlingInstructions : 
IncoTerm : 
Insurance : 
JobClient : 
PackagesSent : 
PickingInstructions : 
PickMethod : 
PickNo : 
PickNumberReference : 
PickOption : AUT - Auto Pick
PickUpAddress : 
PrimaryBarcode : 
Registry : (No Default Field Value Available on Registry)
RequiredDate : 
SalesChannel :  is null
SecondaryReference : 
ServiceLevel : 
SOPCarrierServiceLevel : 
SOPOrderNumber : 
SOPRequiredDate : 
SOPSpecialInstructions : 
SOPStagingAreaName : 
SOPTransportCompany : 
SplitNumber : 
StagingAreaName : 
StagingLocationString : 
Status : New Entry (Unsaved)
Supplier : 
SupplierBuyerLink : (No Default Field Value Available on SupplierBuyerLink)
SupplierDocAddress : 
TotalExtendedLinePrice : 
TotalLoadedPackages : 
TotalLoadedUnits : 
TotalLoadedVolume : 
TotalLoadedWeight : 
TotalOuterPackagesVolume : 
TotalOuterPackagesWeight : 
TransportationUnit :  is null
TransportBillToAddress : 
TransportCoAddress : 
TransportCompany : 
TransportReference : 
VehicleReference : 
Warehouse : 
WarehouseName : 
WeightSent : 
WhoCreated : {0}
WhoFinalised :
", Env.CurrentUser.Initials);
			}
		}

		#endregion
	}
}
