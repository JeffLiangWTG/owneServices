using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePackingSlipWrapper))]
	sealed class WarehousePackingSlipWrapperTest : WarehousePickableDocketWrapperTest
	{
		#region TestABN

		protected override void TestABNCore()
		{
			var client = Factory.New<OrgHeader>();
			var order = (WhsOrder)GetNewDocket();
			order.WD_OH_Client = client.PK;

			var code = Factory.New<OrgCusCode>();
			code.OK_RN_NKCodeCountry = "AU";
			code.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			code.OK_CustomsRegNo = "69 079 137 518";
			code.OK_OH = client.PK;

			var packingSlipWrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("69 079 137 518", packingSlipWrapper.ABN);
		}

		#endregion

		#region TestDocumentTitleCore

		protected override void TestDocumentTitleCore()
		{
			var whs = Factory.New<WhsWarehouse>();
			whs.WW_GB_RelatedCompanyBranch = Factory.New<GlbBranch>().PK;
			WarehouseDataRegistry.Instance.PackingSlipTitles.SetValue(Guid.Empty, whs.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, "Despatch Slip");

			var order = (WhsOrder)GetNewDocket();
			order.WD_WW_Whs = whs.PK;

			var packingSlipWrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("Despatch Slip", packingSlipWrapper.DocumentTitle);

			order.WD_WW_Whs = ZGuid.Empty;
			AssertEquals("Packing Slip", packingSlipWrapper.DocumentTitle);
		}

		#endregion

		#region TestJobClient

		protected override void TestJobClientCore()
		{
			var client = Factory.New<OrgHeader>();
			var order = (WhsOrder)GetNewDocket();
			order.WD_OH_Client = client.PK;

			var packingSlipWrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals(client.PK, packingSlipWrapper.JobClient.Organisation.PK);
		}

		#endregion

		#region TestSupplierBuyerLink

		protected override void TestSupplierBuyerLinkCore()
		{
			var client = Helper.CreateClient();
			var consignee = Helper.CreateClient();
			consignee.OH_RL_NKClosestPort = "AUSYD";
			var supplierBuyerLink = consignee.SupplierLinks.AddNew(client);
			supplierBuyerLink.OL_VendorID = "VendorID1234";

			var order = (WhsOrder)GetNewDocket();
			order.WD_OH_Client = client.PK;
			order.ConsigneePK = consignee.PK;
			var wrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("SupplierBuyerLink.VendorID", "VendorID1234", wrapper.SupplierBuyerLink.VendorID);
		}

		#endregion

		#region TestStatus

		protected override void TestStatusCore()
		{
			AssertEquals(true, WarehousePackingSlipWrapper.Status.IsEmpty);
		}

		#endregion

		#region TestVendorID_CheckForFallBack

		public void TestVendorID_CheckForFallBack()
		{
			var client = Helper.CreateClient();
			var consignee = Helper.CreateClient();
			consignee.OH_RL_NKClosestPort = "AUSYD";
			var supplierBuyerLink = consignee.SupplierLinks.AddNew(client);
			supplierBuyerLink.OL_VendorID = "VendorIDFromSupplierBuyerLink";

			var order = (WhsOrder)GetNewDocket();
			order.WD_OH_Client = client.PK;
			order.ConsigneePK = consignee.PK;
			var wrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("VendorID", "VendorIDFromSupplierBuyerLink", wrapper.VendorID);

			var vendorIDRef = order.References.AddNew();
			vendorIDRef.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.VendorIDCode;
			vendorIDRef.WX_Reference = "VendorIDFromReference";
			AssertEquals("VendorIDFromReference", wrapper.VendorID);
		}

		#endregion

		#region TestLoadNumber

		protected override void TestLoadNumberCore()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A");
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, jobID: "LOAD001");
			order.WD_WLO_PlannedLoad = load.PK;

			Factory.Save();

			var wrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("LOAD001", wrapper.LoadNumber);
		}

		public void TestLoadNumber_NoLoad()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A");
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);

			Factory.Save();

			var wrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("", wrapper.LoadNumber);
		}

		public void TestLoadNumber_MultipleLoad()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A");
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);

			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);

			var package1 = packingHelper.CreatePackage(order.PackageJob, 5, "UNT");
			var package2 = packingHelper.CreatePackage(order.PackageJob, 5, "UNT");

			var load1 = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, jobID: "LOAD001", startTime: DateTimeOffset.Now);
			var load2 = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, jobID: "LOAD002", startTime: DateTimeOffset.Now);
			var load3 = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, jobID: "LOAD003", startTime: DateTimeOffset.Now);
			var load4 = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, jobID: "LOAD004", startTime: DateTimeOffset.Now);

			order.WD_WLO_PlannedLoad = load1.PK;

			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load2);
			pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot1.WLP_GS_NKLoadingUser = "E";
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load3);

			Factory.Save();

			var wrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("LOAD001, LOAD002, LOAD003", wrapper.LoadNumber);
		}

		#endregion

		#region ExpectedDefaultFormatting

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return string.Format(@"
CarrierAccount :  is null
CarrierServiceLevel : 
Client : CLIENT\n#1
ClientRequestedBillToParty :  is null
CODAmount : 123.45
CODType : CHQ
ConfirmationInstructions : 
Consignee : 
ConsigneeAddress : 
Consignor : CLIENT\n#1
CubicSent : 50.000 M3
CustomerReference : CusRef#
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress :  is null
DropMode : HSL - Haulier Supplies Lift
DropOffAddress : 
FinalisedDate : 
Forwarder : 
FulfillRule : NON - None
GoodsBillToAddress : 
HandlingInstructions : 
IncoTerm : FOB
Insurance : 22.22
JobClient : CLIENT\n#1
PackagesSent : 5 UNT
PickingInstructions : 
PickMethod : 
PickNo : PN0000123
PickNumberReference : PN0000123
PickOption : AUT - Auto Pick
PickUpAddress : 
PrimaryBarcode : ExtRef#
Registry : (No Default Field Value Available on Registry)
RequiredDate : 25-Jun-10 00:00
SalesChannel :  is null
SecondaryReference : ExtRef#
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
Status : 
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
TransportReference : TransRef#
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

		#region TestPackingLines

		#region TestPackingLines_SortCore

		protected override void TestPackingLines_SortCore()
		{
			var order = (WhsOrder)GetNewDocket();
			order.WD_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;

			var line1 = CreateOrderLine(order, "C3", "DESC2", 1);
			var line2 = CreateOrderLine(order, "C1", "DESC3", 3);
			var line3 = CreateOrderLine(order, "C2", "DESC1", 2);

			var pick = Factory.New<WhsPick>();
			order.WD_WP = pick.PK;

			CreatePickableDocketLineAttribute(line1);
			CreatePickableDocketLineAttribute(line2);
			CreatePickableDocketLineAttribute(line3);

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductCode;
			AssertPackingLinesSortedOrder(order, "00003", "00001", "00002");

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductDescription;
			AssertPackingLinesSortedOrder(order, "00002", "00003", "00001");

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.LineNo;
			AssertPackingLinesSortedOrder(order, "00001", "00003", "00002");

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.SameAsOnGrid;
			AssertPackingLinesSortedOrder(order, "00001", "00002", "00003");

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = "DEF";
			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductCode);
			AssertPackingLinesSortedOrder(order, "00003", "00001", "00002");

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductDescription);
			AssertPackingLinesSortedOrder(order, "00002", "00003", "00001");

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.LineNo);
			AssertPackingLinesSortedOrder(order, "00001", "00003", "00002");

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.SameAsOnGrid);
			AssertPackingLinesSortedOrder(order, "00001", "00002", "00003");
		}

		#endregion

		#region TestPackingLines_RollingUpCore

		protected override void TestPackingLines_RollingUpCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var relation2 = data.Part2.RelatedOrganisations[0];
			relation2.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation2.OU_RollUpAttributesOnDocuments = true;

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var relation3 = part3.RelatedOrganisations[0];
			relation3.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation3.OU_RollUpAttributesOnDocuments = true;

			var part4 = Helper.CreateProduct(data.Org1, "P4");
			var relation4 = part4.RelatedOrganisations[0];
			relation4.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation4.OU_RollUpAttributesOnDocuments = false;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, part3, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, part4, true, useSerialNumber: false);

			var tomorrow = ZDate.Today.AddDays(1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, part3, 100m, null, tomorrow, tomorrow, "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, part4, 100m, null, tomorrow, tomorrow, "PA1", "PA2", "PA3", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);

			// Part1 - Not Attribute Neutral.
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1_1 = Helper.CreateWhsOrderLine(order, data.Part1, 4m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var orderLine1_2 = Helper.CreateWhsOrderLine(order, data.Part1, 4m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var orderLine1_3 = Helper.CreateWhsOrderLine(order, data.Part1, 2m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");

			// Part2 - Attribute Neutral and Roll Up.
			var orderLine2_1 = Helper.CreateWhsOrderLine(order, data.Part2, 4m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var orderLine2_2 = Helper.CreateWhsOrderLine(order, data.Part2, 4m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var orderLine2_3 = Helper.CreateWhsOrderLine(order, data.Part2, 2m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");

			// Part3 - Attribute Neutral and Roll Up. 
			var orderLine3_1 = Helper.CreateWhsOrderLine(order, part3, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");
			var orderLine3_2 = Helper.CreateWhsOrderLine(order, part3, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");

			// Part4 - Attribute Neutral and No Roll Up.
			var orderLine4_1 = Helper.CreateWhsOrderLine(order, part4, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");
			var orderLine4_2 = Helper.CreateWhsOrderLine(order, part4, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");
			Factory.Save();

			Helper.CreatePickNew(order);

			// Part1 Attribute Lines - with no Attribute Neutral on a Product and No Roll Up.
			orderLine1_1.ReleaseLines.RemoveAndDeleteAll();
			var attribLine1_1 = CreatePickableDocketLineAttribute(orderLine1_1, 1m, "", "", "", "", ZDate.Empty, ZDate.Empty);
			var attribLine1_2 = CreatePickableDocketLineAttribute(orderLine1_1, 1m, "", "", "", "", ZDate.Empty, ZDate.Empty);
			var attribLine1_3 = CreatePickableDocketLineAttribute(orderLine1_1, 1m, "PA1", "", "", "", ZDate.Empty, ZDate.Empty);
			var attribLine1_4 = CreatePickableDocketLineAttribute(orderLine1_1, 1m, "PA1", "PA2", "", "", ZDate.Empty, ZDate.Empty);

			orderLine1_2.ReleaseLines.RemoveAndDeleteAll();
			var attribLine1_5 = CreatePickableDocketLineAttribute(orderLine1_2, 1m, "PA1", "PA2", "PA3", "", ZDate.Empty, ZDate.Empty);
			var attribLine1_6 = CreatePickableDocketLineAttribute(orderLine1_2, 1m, "PA1", "PA2", "PA3", "", tomorrow, ZDate.Empty);
			var attribLine1_7 = CreatePickableDocketLineAttribute(orderLine1_2, 1m, "PA1", "PA2", "PA3", "", tomorrow, tomorrow);
			var attribLine1_8 = CreatePickableDocketLineAttribute(orderLine1_2, 1m, "PA1", "PA2", "PA3", "", tomorrow, tomorrow);

			// Part1 Attribute Lines - with no Attribute Neutral on a Product and No Roll Up. Different OrderLine but same Attributes shouldn't roll up.
			orderLine1_3.ReleaseLines.RemoveAndDeleteAll();
			var attribLine1_9 = CreatePickableDocketLineAttribute(orderLine1_3, 2m, "", "", "", "", ZDate.Empty, ZDate.Empty);

			// Part2 Attribute Lines - with an Attribute Neutral on a Product and Roll Up.
			orderLine2_1.ReleaseLines.RemoveAndDeleteAll();
			CreatePickableDocketLineAttribute(orderLine2_1, 1m, "", "", "", "", ZDate.Empty, ZDate.Empty);
			CreatePickableDocketLineAttribute(orderLine2_1, 1m, "", "", "", "", ZDate.Empty, ZDate.Empty);
			CreatePickableDocketLineAttribute(orderLine2_1, 1m, "PA1", "", "", "", ZDate.Empty, ZDate.Empty);
			CreatePickableDocketLineAttribute(orderLine2_1, 1m, "PA1", "PA2", "", "", ZDate.Empty, ZDate.Empty);

			orderLine2_2.ReleaseLines.RemoveAndDeleteAll();
			CreatePickableDocketLineAttribute(orderLine2_2, 1m, "PA1", "PA2", "PA3", "", ZDate.Empty, ZDate.Empty);
			CreatePickableDocketLineAttribute(orderLine2_2, 1m, "PA1", "PA2", "PA3", "", tomorrow, ZDate.Empty);
			CreatePickableDocketLineAttribute(orderLine2_2, 1m, "PA1", "PA2", "PA3", "", tomorrow, tomorrow);
			CreatePickableDocketLineAttribute(orderLine2_2, 1m, "PA1", "PA2", "PA3", "", tomorrow, tomorrow);

			// Part2 Attribute Lines - with an Attribute Neutral on a Product and Roll Up. Different OrderLine but same Attributes should be rolled up.
			orderLine2_3.ReleaseLines.RemoveAndDeleteAll();
			CreatePickableDocketLineAttribute(orderLine2_3, 2m, "", "", "", "", ZDate.Empty, ZDate.Empty);

			// Part3 Attribute Lines - with an Attribute Neutral on a Product and Roll Up.
			orderLine3_1.ReleaseLines.RemoveAndDeleteAll();
			CreatePickableDocketLineAttribute(orderLine3_1, 5m, "PA1", "PA2", "PA3", "", tomorrow, tomorrow);

			orderLine3_2.ReleaseLines.RemoveAndDeleteAll();
			CreatePickableDocketLineAttribute(orderLine3_2, 5m, "PA1", "PA2", "PA3", "", tomorrow, tomorrow);

			// Part4 Attribute Lines - with an Attribute Neutral on a Product and No Roll Up.
			orderLine4_1.ReleaseLines.RemoveAndDeleteAll();
			CreatePickableDocketLineAttribute(orderLine4_1, 5m, "PA1", "PA2", "PA3", "", tomorrow, tomorrow);

			orderLine4_2.ReleaseLines.RemoveAndDeleteAll();
			CreatePickableDocketLineAttribute(orderLine4_2, 5m, "PA1", "PA2", "PA3", "", tomorrow, tomorrow);

			var packingSlipWrapper = new WarehousePackingSlipWrapper(order, Factory);
			AssertEquals("22 Original attribute lines should be rolled up into 9 + 2 + 1 + 2 = 14 attribute lines.", 14, packingSlipWrapper.PackingLines.Count);
			AssertDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 1m, attribLine1_1);
			AssertDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 1m, attribLine1_2);
			AssertDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 1m, attribLine1_3);
			AssertDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 1m, attribLine1_4);
			AssertDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 1m, attribLine1_5);
			AssertDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 1m, attribLine1_6);
			AssertDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 1m, attribLine1_7);
			AssertDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 1m, attribLine1_8);
			AssertDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 2m, attribLine1_9);

			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 6m, orderLine2_1); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9
			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 6m, orderLine2_1); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9
			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 6m, orderLine2_1); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9
			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 6m, orderLine2_1); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9
			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 4m, orderLine2_2); // Should Be Rolled Up AttribLine2_5, 2_6, 2_7, 2_8
			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 4m, orderLine2_2); // Should Be Rolled Up AttribLine2_5, 2_6, 2_7, 2_8
			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 4m, orderLine2_2); // Should Be Rolled Up AttribLine2_5, 2_6, 2_7, 2_8
			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 4m, orderLine2_2); // Should Be Rolled Up AttribLine2_5, 2_6, 2_7, 2_8
			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 6m, orderLine2_3); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9

			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 10m, orderLine3_1); // Should Be Rolled Up AttribLine3_1, 3_2
			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 10m, orderLine3_2); // Should Be Rolled Up AttribLine3_1, 3_2

			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 5m, orderLine4_1);
			AssertRolledUpDocWhsPackingSlipLineExist(packingSlipWrapper.PackingLines, 5m, orderLine4_2);
		}

		void AssertDocWhsPackingSlipLineExist(WarehousePackingSlipLineWrapperCollection docWhsPackingSlipLineCollection, ZDecimal expectedQuantity, WhsReleaseLine expectedAttributes)
		{
			bool result = false;
			foreach (WarehousePackingSlipLineWrapper packingLine in docWhsPackingSlipLineCollection)
			{
				if (packingLine.PartAttribute1 == expectedAttributes.PartAttribute1 &&
					packingLine.PartAttribute2 == expectedAttributes.PartAttribute2 &&
					packingLine.PartAttribute3 == expectedAttributes.PartAttribute3 &&
					packingLine.ExpiryDate == expectedAttributes.ExpiryDate &&
					packingLine.PackingDate == expectedAttributes.PackingDate &&
					((WhsReleaseLine)packingLine.WrappedObject).Quantity == expectedQuantity)
				{
					result = true;
					break;
				}
			}
			Assert("DocWhsPackingSlipLine with set parameters couldn't be found", result);
		}

		void AssertRolledUpDocWhsPackingSlipLineExist(WarehousePackingSlipLineWrapperCollection docWhsPackingSlipLineCollection, ZDecimal expectedQuantity, WhsPickableDocketLine pickableDocketLine)
		{
			bool result = false;
			foreach (WarehousePackingSlipLineWrapper packingLine in docWhsPackingSlipLineCollection)
			{
				if (packingLine.PartAttribute1 == pickableDocketLine.WE_PartAttrib1 &&
					packingLine.PartAttribute2 == pickableDocketLine.WE_PartAttrib2 &&
					packingLine.PartAttribute3 == pickableDocketLine.WE_PartAttrib3 &&
					packingLine.ExpiryDate == pickableDocketLine.WE_ExpiryDate &&
					packingLine.PackingDate == pickableDocketLine.WE_PackingDate &&
					(ZDecimal)packingLine.UnitsMet.NativeValue == expectedQuantity)
				{
					result = true;
					break;
				}
			}
			Assert("DocWhsPackingSlipLine with set parameters couldn't be found", result);
		}

		#endregion

		#endregion

		#region TestWrapperMappingsEmpty_NotDependantToBizOProperties

		protected override void TestWrapperMappingsEmpty_NotDependantToBizOProperties(WarehouseJobGenericWrapper emptyWrapper)
		{
			AssertEquals("DocumentTitle", "Packing Slip", emptyWrapper.DocumentTitle);
			AssertEquals("JobNumberHeading", ZString.Empty, emptyWrapper.JobNumberHeading);
			AssertEquals("TotalExtendedLinePrice", 0m, emptyWrapper.TotalExtendedLinePrice.Amount);
		}

		#endregion

		#region TestWarehouseBOWrapper_FactoryCached

		protected override WhsDocket GetNewDocketInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsOrder>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WarehousePackingSlipWrapper((WhsOrder)bizO, factory);
		}

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsOrder>();
		}

		WarehousePackingSlipWrapper WarehousePackingSlipWrapper => (WarehousePackingSlipWrapper)Wrapper;

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var pick = Factory.NewWithValidTestData<WhsPick>();
			pick.WP_PickNo = "PN0000123";

			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WP = pick.PK;
			order.WD_WW_Whs = Warehouse.PK;
			order.WD_ShipperCODAmount = 100.00m;
			order.WD_LocalCartInsuranceCost = 200.00m;
			order.WD_CubicSent = 50m;
			order.WD_TotalCubicUnit = "M3";
			order.WD_WeightSent = 25m;
			order.WD_TotalWeightUnit = "KG";
			order.WD_PackagesSent = 5;
			order.WD_F3_NKTotalPackType = "UNT";
			order.WD_PalletsSent = 6;
			order.WD_UnitsSent = 7.5m;
			order.WD_TransportReference = "TransRef#";
			order.WD_INCO = "FOB";
			order.WD_ExternalReference = "ExtRef#";
			order.WD_CustomerReference = "CusRef#";
			order.WD_RequiredDate = new ZDateTimeOffset(2010, 6, 25);
			order.WD_BOLNo = "BOL123";
			order.WD_PickOption = "AUT";
			order.WD_DropMode = "HSL";
			order.WD_CODPayMethod = "CHQ";
			order.WD_ShipperCODAmount = 123.45m;
			order.WD_LocalCartInsuranceCost = 22.22m;
			order.WD_ShipperCODAmount = 100.00m;
			order.WD_LocalCartInsuranceCost = 200.00m;
			order.WD_CubicSent = 50m;
			order.WD_TotalCubicUnit = "M3";
			order.WD_WeightSent = 25m;
			order.WD_TotalWeightUnit = "KG";
			order.WD_PackagesSent = 5;
			order.WD_F3_NKTotalPackType = "UNT";
			order.WD_PalletsSent = 6;
			order.WD_UnitsSent = 7.5m;
			order.WD_INCO = "FOB";
			order.WD_RequiredDate = new ZDateTimeOffset(2010, 6, 25);
			order.WD_BOLNo = "BOL123";
			order.WD_WhsOrderFulfillmentRule = "NON";
			order.WD_PickOption = "AUT";
			order.WD_CODPayMethod = "CHQ";
			order.WD_ShipperCODAmount = 123.45m;
			order.WD_LocalCartInsuranceCost = 22.22m;

			var reference = order.References.AddNew();
			reference.WX_Reference = "VHN123";
			reference.WX_RefType = "VHN";

			return new WarehousePackingSlipWrapper(order, Factory);
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehousePackingSlipWrapper((WhsOrder)bizO, Factory);
		}

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return Factory.New<WhsOrder>();
		}

		#endregion
	}
}
