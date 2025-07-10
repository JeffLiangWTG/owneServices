using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class WarehousePickableDocketWrapperTest : WarehouseDocketWrapperTest
	{
		#region TestPickOption

		protected override void TestPickOptionCore()
		{
			var pickableDocket = GetNewDocket();
			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);

			AssertEquals("Pre-Condition", "AUT", pickableDocketWrapper.PickOption.Code);
			pickableDocket.WD_PickOption = "MAN";
			AssertEquals("MAN", pickableDocketWrapper.PickOption.Code);
		}

		#endregion

		#region WeightSent

		protected override void TestWeightSentCore()
		{
			var pickableDocket = GetNewDocket();
			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);

			AssertEquals("Pre-Condition", 0m, pickableDocketWrapper.WeightSent.Value);
			pickableDocket.WD_WeightSent = 67.9m;
			AssertEquals(67.9m, pickableDocketWrapper.WeightSent.Value);
		}

		#endregion

		#region TestPalletsSent

		protected override void TestPalletsSentCore()
		{
			var pickableDocket = GetNewDocket();
			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);

			AssertEquals("Pre-Condition", 0, (int)pickableDocketWrapper.PalletsSent);
			pickableDocket.WD_PalletsSent = 14;
			AssertEquals(14, (int)pickableDocketWrapper.PalletsSent);
		}

		#endregion

		#region TestPackagesSent

		protected override void TestPackagesSentCore()
		{
			var pickableDocket = GetNewDocket();
			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);

			AssertEquals("Pre-Condition", 0m, pickableDocketWrapper.PackagesSent.Value);
			AssertEquals("Pre-Condition", "", pickableDocketWrapper.PackagesSent.Unit.ToString());
			pickableDocket.WD_PackagesSent = 101;
			pickableDocket.WD_F3_NKTotalPackType = "BOX";
			AssertEquals(101, pickableDocketWrapper.PackagesSent.Value.ToZInt());
			AssertEquals("BOX - Box", pickableDocketWrapper.PackagesSent.Unit.ToString());
		}

		#endregion

		#region TestPickNo

		protected override void TestPickNoCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var wrapperWithoutPick = WarehouseJobGenericWrapper.New(order, Factory);
			AssertEquals("", wrapperWithoutPick.PickNo.Label);
			AssertEquals("", wrapperWithoutPick.PickNo.Value);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			var wrapperWithPick = WarehouseJobGenericWrapper.New(order, Factory);
			AssertEquals("Pick No", wrapperWithPick.PickNo.Label);
			AssertEquals(pick.WP_PickNo, wrapperWithPick.PickNo.Value);
		}

		#endregion

		#region TestCODAmount

		protected override void TestCODAmountCore()
		{
			var pickableDocket = GetNewDocket();
			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);

			AssertEquals("Pre-Condition", 0m, pickableDocketWrapper.CODAmount.Amount);
			pickableDocket.WD_ShipperCODAmount = 107.89m;
			AssertEquals(107.89m, pickableDocketWrapper.CODAmount.Amount);
		}

		#endregion

		#region TestInsurance

		protected override void TestInsuranceCore()
		{
			var pickableDocket = GetNewDocket();
			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);

			AssertEquals("Pre-Condition", 0m, pickableDocketWrapper.Insurance.Amount);
			pickableDocket.WD_LocalCartInsuranceCost = 34.89m;
			AssertEquals(34.89m, pickableDocketWrapper.Insurance.Amount);
		}

		#endregion

		#region TestCODType

		protected override void TestCODTypeCore()
		{
			var pickableDocket = GetNewDocket();
			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);

			pickableDocket.WD_CODPayMethod = "CHQ";
			AssertEquals("CHQ", pickableDocketWrapper.CODType.Code);
		}

		#endregion

		#region TestConsignee

		protected override void TestConsigneeCore()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "D12345";

			var pickableDocket = (WhsPickableDocket)GetNewDocket();
			pickableDocket.ConsigneePK = consignee.PK;

			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals(pickableDocket.ConsigneeDocAddress.PK, pickableDocketWrapper.Consignee.MainAddress.WrappedObjectPK);
			AssertEquals("D12345", pickableDocketWrapper.Consignee.CompanyCode);

			var consigneeAddress = Factory.New<OrgAddress>();
			consigneeAddress.OA_OH = consignee.PK;
			pickableDocket.ConsigneeAddressPK = consigneeAddress.PK;
			AssertEquals(pickableDocket.ConsigneeDocAddress.PK, pickableDocketWrapper.Consignee.MainAddress.WrappedObjectPK);
			AssertEquals("D12345", pickableDocketWrapper.Consignee.CompanyCode);
		}

		#endregion

		#region TestConsigneeAddress

		protected override void TestConsigneeAddressCore()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;

			org.OH_RL_NKClosestPort = "AU";
			org.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "AU";
			address.OA_RN_NKCountryCode = "AU";

			address.OA_OH = org.PK;
			var pickableDocket = (WhsPickableDocket)GetNewDocket();
			pickableDocket.ConsigneePK = org.PK;
			pickableDocket.ConsigneeAddressPK = address.PK;

			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("ConsigneeAddress is of type AddressWrapper", typeof(AddressWrapper), pickableDocketWrapper.ConsigneeAddress.GetType());

			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", pickableDocketWrapper.ConsigneeAddress.CompanyNameAndAddress);
			AssertEquals("AU", pickableDocketWrapper.ConsigneeAddress.Country.Code);

			pickableDocket.ConsigneeDocAddress.E2_AddressOverride = true;
			pickableDocket.ConsigneeDocAddress.E2_CompanyName = "Name Override";
			pickableDocket.ConsigneeDocAddress.E2_Address1 = "Address 1 Override";
			pickableDocket.ConsigneeDocAddress.E2_Address2 = "Address 2 Override";
			pickableDocket.ConsigneeDocAddress.E2_City = "City Override";
			pickableDocket.ConsigneeDocAddress.E2_Postcode = "Code";
			pickableDocket.ConsigneeDocAddress.E2_State = "State";
			pickableDocket.ConsigneeDocAddress.E2_RN_NKCountryCode = "US";

			var wrapperWithOverride = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("NAME OVERRIDE\nADDRESS 1 OVERRIDE\nADDRESS 2 OVERRIDE\nCITY OVERRIDE STATE CODE\nUNITED STATES", wrapperWithOverride.ConsigneeAddress.CompanyNameAndAddress);
			AssertEquals("US", wrapperWithOverride.ConsigneeAddress.Country.Code);
		}

		#endregion

		#region TestTransportBillToAddress

		protected override void TestTransportBillToAddressCore()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;

			org.OH_RL_NKClosestPort = "AU";
			org.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "AU";
			address.OA_RN_NKCountryCode = "AU";

			address.OA_OH = org.PK;

			var pickableDocket = GetNewDocket();
			pickableDocket.TransportBillToDocAddress.OrganisationPK = org.PK;
			pickableDocket.TransportBillToDocAddress.E2_OA_Address = address.PK;

			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("TransportBillToDocAddress is of type AddressWrapper", typeof(AddressWrapper), pickableDocketWrapper.TransportBillToAddress.GetType());

			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", pickableDocketWrapper.TransportBillToAddress.CompanyNameAndAddress);
			AssertEquals("AU", pickableDocketWrapper.TransportBillToAddress.Country.Code);

			pickableDocket.TransportBillToDocAddress.E2_AddressOverride = true;
			pickableDocket.TransportBillToDocAddress.E2_CompanyName = "Name Override";
			pickableDocket.TransportBillToDocAddress.E2_Address1 = "Address 1 Override";
			pickableDocket.TransportBillToDocAddress.E2_Address2 = "Address 2 Override";
			pickableDocket.TransportBillToDocAddress.E2_City = "City Override";
			pickableDocket.TransportBillToDocAddress.E2_Postcode = "Code";
			pickableDocket.TransportBillToDocAddress.E2_State = "State";
			pickableDocket.TransportBillToDocAddress.E2_RN_NKCountryCode = "US";

			var wrapperWithOverride = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("NAME OVERRIDE\nADDRESS 1 OVERRIDE\nADDRESS 2 OVERRIDE\nCITY OVERRIDE STATE CODE\nUNITED STATES", wrapperWithOverride.TransportBillToAddress.CompanyNameAndAddress);
			AssertEquals("US", wrapperWithOverride.TransportBillToAddress.Country.Code);
		}

		#endregion

		#region TestGoodsBillToAddress

		protected override void TestGoodsBillToAddressCore()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;

			org.OH_RL_NKClosestPort = "AU";
			org.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "AU";
			address.OA_RN_NKCountryCode = "AU";

			address.OA_OH = org.PK;
			var pickableDocket = GetNewDocket();
			pickableDocket.GoodsBillToPK = org.PK;
			pickableDocket.GoodsBillToAddressPK = address.PK;

			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("ConsigneeAddress is of type AddressWrapper", typeof(AddressWrapper), pickableDocketWrapper.GoodsBillToAddress.GetType());

			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", pickableDocketWrapper.GoodsBillToAddress.CompanyNameAndAddress);
			AssertEquals("AU", pickableDocketWrapper.GoodsBillToAddress.Country.Code);

			pickableDocket.GoodsBillToDocAddress.E2_AddressOverride = true;
			pickableDocket.GoodsBillToDocAddress.E2_CompanyName = "Name Override";
			pickableDocket.GoodsBillToDocAddress.E2_Address1 = "Address 1 Override";
			pickableDocket.GoodsBillToDocAddress.E2_Address2 = "Address 2 Override";
			pickableDocket.GoodsBillToDocAddress.E2_City = "City Override";
			pickableDocket.GoodsBillToDocAddress.E2_Postcode = "Code";
			pickableDocket.GoodsBillToDocAddress.E2_State = "State";
			pickableDocket.GoodsBillToDocAddress.E2_RN_NKCountryCode = "US";

			var wrapperWithOverride = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("NAME OVERRIDE\nADDRESS 1 OVERRIDE\nADDRESS 2 OVERRIDE\nCITY OVERRIDE STATE CODE\nUNITED STATES", wrapperWithOverride.GoodsBillToAddress.CompanyNameAndAddress);
			AssertEquals("US", wrapperWithOverride.GoodsBillToAddress.Country.Code);
		}

		#endregion

		#region JobNumberHeading

		protected override void TestJobNumberHeadingCore()
		{
			AssertEquals("Order Number", ((WarehousePickableDocketWrapper)Wrapper).JobNumberHeading);
		}

		#endregion

		#region TestHandlingInstructions

		protected override void TestHandlingInstructionsCore()
		{
			var pickableDocket = (WhsPickableDocket)GetNewDocket();
			pickableDocket.WD_HandlingInstructions = "Handle with care you guys!";

			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("Handle with care you guys!", pickableDocketWrapper.HandlingInstructions.Value);
			AssertEquals("Special Instructions", pickableDocketWrapper.HandlingInstructions.Label);
		}

		#endregion

		#region TestUnitsSent

		protected override void TestUnitsSentCore()
		{
			var pickableDocket = GetNewDocket();
			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);

			AssertEquals("Pre-Condition", 0m, pickableDocketWrapper.UnitsSent);
			pickableDocket.WD_UnitsSent = 109.785m;
			AssertEquals(109.785m, pickableDocketWrapper.UnitsSent);
		}

		#endregion

		#region TestCubicSent

		protected override void TestCubicSentCore()
		{
			var pickableDocket = GetNewDocket();
			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);

			AssertEquals("Pre-Condition", 0m, pickableDocketWrapper.CubicSent.Value);
			pickableDocket.WD_CubicSent = 10.345m;
			AssertEquals(10.345m, pickableDocketWrapper.CubicSent.Value);
		}

		#endregion

		#region TestFulfillRule

		protected override void TestFulfillRuleCore()
		{
			var pickableDocket = GetNewDocket();
			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);

			pickableDocket.WD_WhsOrderFulfillmentRule = "";
			AssertEquals("", pickableDocketWrapper.FulfillRule.Code);
			pickableDocket.WD_WhsOrderFulfillmentRule = "ALL";
			AssertEquals("ALL", pickableDocketWrapper.FulfillRule.Code);
		}

		#endregion

		#region TestIncoTerm

		protected override void TestIncoTermCore()
		{
			var pickableDocket = GetNewDocket();
			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);

			AssertEquals(ZString.Empty, pickableDocketWrapper.IncoTerm.Code);
			pickableDocket.WD_INCO = "PPD";
			AssertEquals("PPD", pickableDocketWrapper.IncoTerm.Code);
			AssertEquals("Prepaid", pickableDocketWrapper.IncoTerm.Description);
		}

		#endregion

		#region TestJobLines

		protected override void TestJobLinesCore()
		{
			AssertEquals(typeof(WarehousePackingSlipLineWrapperCollection), ((WarehousePickableDocketWrapper)Wrapper).JobLines.GetType());
		}

		#endregion

		#region TestPackingLines

		#region TestPackingLines

		protected override void TestPackingLinesCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var line1 = order.Lines[0];
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			line1.ReleaseLines[0].Quantity = 3m;
			line1.ReleaseLines[0].PartAttribute1 = "A";

			line1.ReleaseLines.AddNew().Quantity = 2m;
			line1.ReleaseLines[1].PartAttribute1 = "B";
			Factory.Save();

			var pickableDocketWrapper = WarehouseJobGenericWrapper.New(order, Factory);
			AssertEquals("Count should be 3", 3, pickableDocketWrapper.PackingLines.Count);
			Assert("Docket should not have any changes after we access PackingLines on the wrapper", !order.HasChanges);
		}

		#endregion

		#region TestBillOfLadingPackingLinesCore

		protected override void TestBillOfLadingPackingLinesCore()
		{
			var orderWrapper = GetWrapperSetupForBillOfLadingPackingLinesTests();
			AssertEquals("Count should be 1", 1, orderWrapper.BillOfLadingPackingLines.Count);
			AssertEquals(22m, ((WarehousePackingSlipLineWrapper)orderWrapper.BillOfLadingPackingLines[0]).GroupedLineUnitsMet);
		}

		#endregion

		#region TestBillOfLadingPackingLinesUSCore

		protected override void TestBillOfLadingPackingLinesUSCore()
		{
			var orderWrapper = GetWrapperSetupForBillOfLadingPackingLinesTests();
			AssertEquals("Count should be 1", 1, orderWrapper.BillOfLadingPackingLinesUS.Count);
			AssertEquals(22m, ((WarehousePackingSlipLineWrapper)orderWrapper.BillOfLadingPackingLinesUS[0]).GroupedLineUnitsMet);
		}

		#endregion

		#region TestPackageLabelsCore

		protected override void TestPackageLabelsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderWrapper = WarehouseJobGenericWrapper.New(order, Factory);
			AssertEquals(0, orderWrapper.PackageLabels.Count);

			var line1 = order.Lines.AddNew();
			var line2 = order.Lines.AddNew();
			line1.WE_PackQuantity = 2.1;
			line2.WE_PackQuantity = 3.8;
			AssertEquals(7, orderWrapper.PackageLabels.Count);

			var counter = 1;
			foreach (DocWhsLabel label in orderWrapper.PackageLabels)
			{
				AssertEquals(counter++, label.LabelNumber);
			}
		}

		#endregion

		#region TestPackageLabelsForBOMCore

		protected override void TestPackageLabelsForBOMCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts(saveFactoryForWarehouse: true);
			Helper.CreateProductUnit(data.BOM.Bike, "UNT", "PLT", 5m);

			// create some parts
			var laptop = Helper.CreateProduct(data.Org1, "Laptop");
			var tV = Helper.CreateProduct(data.Org1, "TV");

			var tvCable = Helper.CreateProduct(data.Org1, "Cable");
			var tvScreen = Helper.CreateProduct(data.Org1, "Screen");

			Helper.CreateProductBOM(tV, tvCable, 1, "UNT");
			Helper.CreateProductBOM(tV, tvScreen, 1, "UNT");

			// order the parts
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.BOM.Bike, 4m);
			orderLine1.WE_F3_NKPackType = "PLT";
			orderLine1.WE_PackQuantity = 0.8m;
			var orderLine2 = Helper.CreateWhsOrderLine(order, laptop, 2m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, tV, 3m);

			Factory.Save(); // All Products and Orders will be in the DB prior to printing documents (ediEnterprise Docs requirement)
			var workOrdersCountBefore = Factory.Load<WhsWorkOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.WorkOrder)).Length;

			AssertEquals(true, orderLine1.IsBOMProduct);
			AssertEquals(false, orderLine2.IsBOMProduct);
			AssertEquals(true, orderLine3.IsBOMProduct);

			var orderWrapper = WarehouseJobGenericWrapper.New(order, Factory);

			AssertEquals(6, orderWrapper.PackageLabels.Count);
			const int bikeLabels = 14;
			const int tvLabels = 3 * 3;
			const int laptopLabels = 2;
			AssertEquals(bikeLabels + tvLabels + laptopLabels, orderWrapper.PackageLabelsForBOM.Count);

			var counter = 0;
			var subLabelCounter = 0;
			var labelQuantity = new List<int>();
			var labelProduct = new List<String>();

			foreach (DocWhsPackageLabel label in orderWrapper.PackageLabels)
			{
				counter++;
				AssertEquals(counter, label.LabelNumber);
				foreach (DocWhsPackageLabel subLabel in orderWrapper.PackageLabelsForBOM)
				{
					if (label.LabelNumber == subLabel.LabelNumber)
					{
						subLabelCounter++;
					}
				}
				labelProduct.Add(label.ProductCode);
				labelQuantity.Add(subLabelCounter);

				subLabelCounter = 0;
			}

			for (var i = 0; i < counter; i++)
			{
				if (labelProduct[i].ToUpper() == "LAPTOP")
				{
					AssertEquals("Each Laptop should have 1 label.", 1, labelQuantity[i]);
				}
				else if (labelProduct[i].ToUpper() == "MOTORBIKE")
				{
					AssertEquals("Each Motorbike should have 14 labels.", 14, labelQuantity[i]);
				}
				else
				{
					AssertEquals("Each TV should have 3 labels.", 3, labelQuantity[i]);
				}
			}

			order.WD_ExternalReference = "Changed Value";
			AssertEquals("No errors", true, !order.HasErrors);
			Factory.Save();  // Virtual aka temp Work Order shouldn't save nor affect an Order save.
			var workOrdersCountAfter = Factory.Load<WhsWorkOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.WorkOrder)).Length;
			AssertEquals("Still no errors after creating a virtual work order", true, !order.HasErrors);
			AssertEquals("No new work orders", workOrdersCountBefore, workOrdersCountAfter);
		}

		#endregion

		#region TestPackingLines_Sort

		public void TestPackingLines_Sort()
		{
			TestPackingLines_SortCore();
		}

		protected virtual void TestPackingLines_SortCore()
		{
			Assert(true);
		}

		#endregion

		#region TestPackingLines_RollingUp

		public void TestPackingLines_RollingUp()
		{
			TestPackingLines_RollingUpCore();
		}

		protected virtual void TestPackingLines_RollingUpCore()
		{
			Assert(true);
		}

		#endregion

		#region AssertPackingLinesSortedOrder

		protected void AssertPackingLinesSortedOrder(WhsPickableDocket pickableDocket, ZString expectedLine1Position, ZString expectedLine2Position, ZString expectedLine3Position)
		{
			var pickableDocketWrapper = WarehouseJobGenericWrapper.New(pickableDocket, Factory);
			var sortedBy = pickableDocket.Client.MiscServ.OM_WhsPackingSlipOrderBy_List.GetDescriptionFromCode(pickableDocket.Client.MiscServ.OM_WhsPackingSlipOrderBy);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine1Position, pickableDocketWrapper.PackingLines[0].PositionAfterSorting);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine2Position, pickableDocketWrapper.PackingLines[1].PositionAfterSorting);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine3Position, pickableDocketWrapper.PackingLines[2].PositionAfterSorting);
		}

		#endregion

		#endregion

		#region TestRolledUpLinesForOrderCopy

		#region TestBOM

		public void TestOrderCopyForWithChildLines()
		{
			TestOrderCopyForWithChildLinesCore();
		}

		protected virtual void TestOrderCopyForWithChildLinesCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			orderLine2.WE_WE_ParentDocketLine = orderLine1.PK;
			var orderLine3 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			orderLine3.WE_WE_ParentDocketLine = orderLine1.PK;

			var warehousePickableDocketWrapper = WarehouseJobGenericWrapper.New(order1, Factory);
			AssertEquals("1 Orderline should be returned", 1, warehousePickableDocketWrapper.RolledUpLinesForOrderCopy.Count);
		}

		#endregion

		#region TestRolledUpLinesForOrderCopy

		protected override void TestRolledUpLinesForOrderCopyCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var today = ZDate.Today;
			var part3 = Helper.CreateProduct(data.Org1, "P3");

			var relation1 = data.Part1.RelatedOrganisations[0];
			relation1.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			var relation2 = data.Part2.RelatedOrganisations[0];
			relation2.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation2.OU_RollUpAttributesOnDocuments = true;

			var relation3 = part3.RelatedOrganisations[0];
			relation3.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation3.OU_RollUpAttributesOnDocuments = false;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			order.WD_OH_Client = data.Org1.PK;

			//attribute specified
			var line1_1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line1_2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line1_3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "", "");
			var line1_4 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "", "");
			var line1_5 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line1_6 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, today, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line1_7 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			line1_7.WE_SerialNumber = "SER1";
			var line1_8 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			line1_8.WE_SerialNumber = "SER1";
			var line1_9 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			line1_9.WE_LineComment = "COMMENT1";
			var line1_SER = Helper.CreateWhsOrderLine(order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			line1_SER.WE_SerialNumber = "SER1";

			// attribute neutral.
			var line2_1 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line2_2 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line2_3 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "", "");
			var line2_4 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "", "");
			var line2_5 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line2_6 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, today, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line2_7 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			line2_7.WE_SerialNumber = "SER1";
			var line2_8 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			line2_8.WE_SerialNumber = "SER1";
			var line2_9 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			line2_9.WE_LineComment = "COMMENT2";
			var line2_SER = Helper.CreateWhsOrderLine(order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			line2_SER.WE_SerialNumber = "SER1";

			//attribute neutral and no roll up
			var line3_1 = Helper.CreateWhsOrderLine(order, part3, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line3_2 = Helper.CreateWhsOrderLine(order, part3, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line3_3 = Helper.CreateWhsOrderLine(order, part3, 1m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "", "");
			var line3_4 = Helper.CreateWhsOrderLine(order, part3, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "", "");
			var line3_5 = Helper.CreateWhsOrderLine(order, part3, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line3_6 = Helper.CreateWhsOrderLine(order, part3, 1m, today, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line3_7 = Helper.CreateWhsOrderLine(order, part3, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			line3_7.WE_SerialNumber = "SER1";
			var line3_8 = Helper.CreateWhsOrderLine(order, part3, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			line3_8.WE_SerialNumber = "SER1";
			var line3_9 = Helper.CreateWhsOrderLine(order, part3, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			line3_9.WE_LineComment = "COMMENT3";
			var line3_SER = Helper.CreateWhsOrderLine(order, part3, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			line3_SER.WE_SerialNumber = "SER1";

			var warehousePickableDocketWrapper = WarehouseJobGenericWrapper.New(order, Factory);

			AssertEquals("30 Original Order lines should be rolled up into 10 + 8 + 10 = 28 Order lines", 28, warehousePickableDocketWrapper.RolledUpLinesForOrderCopy.Count);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line1_1);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line1_2);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line1_3);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line1_4);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line1_5);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line1_6);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line1_7);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line1_8);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line1_9);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line1_SER);

			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 2m, line2_1); // line21 and line22 are rolled up.
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 2m, line2_2); // line21 and line22 are rolled up.
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line2_3);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line2_4);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line2_5);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line2_6);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 2m, line2_7); // line27 and line28 are rolled up.
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 2m, line2_8); // line27 and line28 are rolled up.
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line2_9);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line2_SER);

			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line3_1);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line3_2);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line3_3);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line3_4);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line3_5);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line3_6);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line3_7);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line3_8);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line3_9);
			AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line3_SER);
		}

		void AssertDocWhsOrderLineExist(DocWhsOrderLineCollection docOrderLines, ZDecimal expectedQuantity, WhsPickableDocketLine expectedPickableDocketLine)
		{
			foreach (DocWhsPickableDocketLine docOrderLine in docOrderLines)
			{
				if (docOrderLine.ProductCode == expectedPickableDocketLine.SupplierPart.OP_PartNum &&
					docOrderLine.Units == expectedQuantity &&
					docOrderLine.LineComment == expectedPickableDocketLine.WE_LineComment &&
					docOrderLine.PartAttribute1 == expectedPickableDocketLine.WE_PartAttrib1 &&
					docOrderLine.PartAttribute2 == expectedPickableDocketLine.WE_PartAttrib2 &&
					docOrderLine.PartAttribute3 == expectedPickableDocketLine.WE_PartAttrib3 &&
					docOrderLine.TrackedSerialNumber == expectedPickableDocketLine.WE_SerialNumber &&
					docOrderLine.ExpiryDate == expectedPickableDocketLine.WE_ExpiryDate &&
					docOrderLine.PackingDate == expectedPickableDocketLine.WE_PackingDate)
				{
					return;
				}
			}
			Assert("The line you are expecting doesn't exist.", false);
		}

		public void TestRolledUpLinesForOrderCopyCoreWithWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var today = ZDateTime.Today;

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			var order = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);

			order.WD_OH_Client = data.Org1.PK;
			var line1_1 = Helper.CreateWhsWorkOrderLine(order, data.Part1, 1m);

			var warehousePickableDocketWrapper = WarehouseJobGenericWrapper.New(order, Factory);
			AssertNoExceptionThrown("A cast execption is thrown for a work order line!", () => AssertDocWhsOrderLineExist(warehousePickableDocketWrapper.RolledUpLinesForOrderCopy, 1m, line1_1));
		}

		#endregion

		#region TestRolledUpLinesForOrderCopy_Sorting

		public void TestRolledUpLinesForOrderCopy_Sorting()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("WHS");

			var order = Helper.CreateWhsOrder(client, whs);
			order.WD_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;

			var line1 = CreateOrderLine(order, "C3", "DESC2", 1);
			var line2 = CreateOrderLine(order, "C1", "DESC3", 3);
			var line3 = CreateOrderLine(order, "C2", "DESC1", 2);

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductCode;
			AssertRolledUpLinesForOrderCopySortedOrder(order, line2, line3, line1);

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductDescription;
			AssertRolledUpLinesForOrderCopySortedOrder(order, line3, line1, line2);

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.LineNo;
			AssertRolledUpLinesForOrderCopySortedOrder(order, line1, line3, line2);

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.SameAsOnGrid;
			AssertRolledUpLinesForOrderCopySortedOrder(order, line1, line2, line3);

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = "DEF";
			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductCode);
			AssertRolledUpLinesForOrderCopySortedOrder(order, line2, line3, line1);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductDescription);
			AssertRolledUpLinesForOrderCopySortedOrder(order, line3, line1, line2);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.LineNo);
			AssertRolledUpLinesForOrderCopySortedOrder(order, line1, line3, line2);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.SameAsOnGrid);
			AssertRolledUpLinesForOrderCopySortedOrder(order, line1, line2, line3);
		}

		protected WhsOrderLine CreateOrderLine(WhsOrder order, ZString partCode, ZString partDesc, ZShort lineNo)
		{
			var line = order.Lines.AddNew();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partCode;
			part.OP_Desc = partDesc;
			line.WE_OP = part.PK;
			line.WE_LineNo = lineNo;
			return line;
		}

		void AssertRolledUpLinesForOrderCopySortedOrder(WhsOrder order, WhsOrderLine expectedLine1, WhsOrderLine expectedLine2, WhsOrderLine expectedLine3)
		{
			var warehousePickableDocketWrapper = WarehouseJobGenericWrapper.New(order, Factory);
			var sortedBy = order.Client.MiscServ.OM_WhsPackingSlipOrderBy_List.GetDescriptionFromCode(order.Client.MiscServ.OM_WhsPackingSlipOrderBy);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine1, warehousePickableDocketWrapper.RolledUpLinesForOrderCopy[0].WrappedObject);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine2, warehousePickableDocketWrapper.RolledUpLinesForOrderCopy[1].WrappedObject);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine3, warehousePickableDocketWrapper.RolledUpLinesForOrderCopy[2].WrappedObject);
		}

		#endregion

		#endregion

		#region TestPalletizedInventory

		protected override void TestPalletizedInventoryCore()
		{
			Assert("Pickable dockets have no palletized inventory.", true);
		}

		protected override void TestPalletizedInventory_Setup(TestDataSimpleEnvironment data, out WhsDocket docket, out WhsInventoryView inventory)
		{
			// Pickable dockets have no palletized inventory.
			docket = null;
			inventory = null;
		}

		#endregion

		#region TestAdresses

		#region TestDropOffAddressCore

		protected override void TestDropOffAddressCore()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;

			SetAddress(org, address);

			var pickableDocket = GetNewDocket();
			pickableDocket.DropOffDocAddress.OrganisationPK = org.PK;
			pickableDocket.DropOffDocAddress.E2_OA_Address = address.PK;

			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("TransportBillToDocAddress is of type AddressWrapper", typeof(AddressWrapper), pickableDocketWrapper.DropOffAddress.GetType());

			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nUNITED STATES", pickableDocketWrapper.DropOffAddress.CompanyNameAndAddress);
			AssertEquals("US", pickableDocketWrapper.DropOffAddress.Country.Code);

			pickableDocket.DropOffDocAddress.E2_AddressOverride = true;
			pickableDocket.DropOffDocAddress.E2_CompanyName = "Name Override";
			pickableDocket.DropOffDocAddress.E2_Address1 = "Address 1 Override";
			pickableDocket.DropOffDocAddress.E2_Address2 = "Address 2 Override";
			pickableDocket.DropOffDocAddress.E2_City = "City Override";
			pickableDocket.DropOffDocAddress.E2_Postcode = "Code";
			pickableDocket.DropOffDocAddress.E2_State = "State";
			pickableDocket.DropOffDocAddress.E2_RN_NKCountryCode = "US";

			var wrapperWithOverride = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("NAME OVERRIDE\nADDRESS 1 OVERRIDE\nADDRESS 2 OVERRIDE\nCITY OVERRIDE STATE CODE\nUNITED STATES", wrapperWithOverride.DropOffAddress.CompanyNameAndAddress);
			AssertEquals("US", wrapperWithOverride.DropOffAddress.Country.Code);
		}

		#endregion

		#region TestPickUpAddressCore

		protected override void TestPickUpAddressCore()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;

			SetAddress(org, address);

			var pickableDocket = GetNewDocket();
			pickableDocket.PickUpDocAddress.OrganisationPK = org.PK;
			pickableDocket.PickUpDocAddress.E2_OA_Address = address.PK;

			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("TransportBillToDocAddress is of type AddressWrapper", typeof(AddressWrapper), pickableDocketWrapper.PickUpAddress.GetType());

			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nUNITED STATES", pickableDocketWrapper.PickUpAddress.CompanyNameAndAddress);
			AssertEquals("US", pickableDocketWrapper.PickUpAddress.Country.Code);

			pickableDocket.PickUpDocAddress.E2_AddressOverride = true;
			pickableDocket.PickUpDocAddress.E2_CompanyName = "Name Override";
			pickableDocket.PickUpDocAddress.E2_Address1 = "Address 1 Override";
			pickableDocket.PickUpDocAddress.E2_Address2 = "Address 2 Override";
			pickableDocket.PickUpDocAddress.E2_City = "City Override";
			pickableDocket.PickUpDocAddress.E2_Postcode = "Code";
			pickableDocket.PickUpDocAddress.E2_State = "State";
			pickableDocket.PickUpDocAddress.E2_RN_NKCountryCode = "US";

			var wrapperWithOverride = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("NAME OVERRIDE\nADDRESS 1 OVERRIDE\nADDRESS 2 OVERRIDE\nCITY OVERRIDE STATE CODE\nUNITED STATES", wrapperWithOverride.PickUpAddress.CompanyNameAndAddress);
			AssertEquals("US", wrapperWithOverride.PickUpAddress.Country.Code);
		}

		#endregion

		#region TestSupplierDocAddressCore

		protected override void TestSupplierDocAddressCore()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;

			SetAddress(org, address);

			var pickableDocket = GetNewDocket();
			pickableDocket.SupplierDocAddress.OrganisationPK = org.PK;
			pickableDocket.SupplierDocAddress.E2_OA_Address = address.PK;

			var pickableDocketWrapper = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("TransportBillToDocAddress is of type AddressWrapper", typeof(AddressWrapper), pickableDocketWrapper.SupplierDocAddress.GetType());

			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nUNITED STATES", pickableDocketWrapper.SupplierDocAddress.CompanyNameAndAddress);
			AssertEquals("US", pickableDocketWrapper.SupplierDocAddress.Country.Code);

			pickableDocket.SupplierDocAddress.E2_AddressOverride = true;
			pickableDocket.SupplierDocAddress.E2_CompanyName = "Name Override";
			pickableDocket.SupplierDocAddress.E2_Address1 = "Address 1 Override";
			pickableDocket.SupplierDocAddress.E2_Address2 = "Address 2 Override";
			pickableDocket.SupplierDocAddress.E2_City = "City Override";
			pickableDocket.SupplierDocAddress.E2_Postcode = "Code";
			pickableDocket.SupplierDocAddress.E2_State = "State";
			pickableDocket.SupplierDocAddress.E2_RN_NKCountryCode = "US";

			var wrapperWithOverride = GetNewWarehouseJobGenericWrapper(pickableDocket);
			AssertEquals("NAME OVERRIDE\nADDRESS 1 OVERRIDE\nADDRESS 2 OVERRIDE\nCITY OVERRIDE STATE CODE\nUNITED STATES", wrapperWithOverride.SupplierDocAddress.CompanyNameAndAddress);
			AssertEquals("US", wrapperWithOverride.SupplierDocAddress.Country.Code);
		}

		#endregion

		#region SetAddress

		void SetAddress(OrgHeader org, OrgAddress address)
		{
			org.OH_RL_NKClosestPort = "AU";
			org.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "US";
			address.OA_RN_NKCountryCode = "US";

			address.OA_OH = org.PK;
		}

		#endregion

		#endregion

		#region TestStagingAreaName

		protected override void TestStagingAreaNameCore()
		{
			var ddlLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var docket = GetNewDocket();
			var pickWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("Precondition", "", pickWrapper.StagingAreaName.Label);
			AssertEquals("Precondition", "", pickWrapper.StagingAreaName.Value);

			// Dock Door Location is only used by WhsOrders
			if (IsUsingDockDoorLocation)
			{
				var whs = docket.Warehouse ?? Helper.CreateWarehouse("WHS");
				var bRow = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 1);

				var locationB1 = bRow.Locations[0];
				locationB1.WLV_WLT_LocationType = ddlLocationType.PK;

				var pick = Factory.New<WhsPick>();
				pick.WP_WW_Whs = whs.PK;
				pick.WP_WL_DockDoor = locationB1.PK;
				docket.WD_WP = pick.PK;
				AssertEquals("When pick have a Dock Door Location should return label.", "Dock Door Location", pickWrapper.StagingAreaName.Label);
				AssertEquals("When pick have a Dock Door Location should return label.", "B-1", pickWrapper.StagingAreaName.Value);
			}
		}

		protected virtual bool IsUsingDockDoorLocation => true;

		#endregion

		#region TestWarehouseCCPCode

		protected override void TestWarehouseCCPCodeCore()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = Factory.New<OrgAddress>().PK;

			var docket = GetNewDocket();
			docket.WD_WW_Whs = warehouse.PK;

			var warehouseOrg = Factory.New<OrgHeader>();
			var code = warehouseOrg.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			code.OK_CustomsRegNo = "Z9876";
			docket.Warehouse.WarehouseAddress.OA_OH = warehouseOrg.PK;

			var wrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("Z9876", wrapper.WarehouseCCPCode);
		}

		#endregion

		#region TestClientCPC

		protected override void TestClientCPCCore()
		{
			var client = Factory.New<OrgHeader>();
			var docket = GetNewDocket();
			docket.WD_OH_Client = client.PK;

			var code = Factory.New<OrgCusCode>();
			code.OK_CodeType = OrgCusCode.CodeTypes.CustomsCPPermitCode;
			code.OK_CustomsRegNo = "1234A";
			code.OK_OH = client.PK;

			var wrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("1234A", wrapper.ClientCPC);
		}

		#endregion

		#region TestCP_IssueNo

		protected override void TestCP_IssueNoCore()
		{
			var client = Factory.New<OrgHeader>();

			var docket = GetNewDocket();
			docket.WD_OH_Client = client.PK;
			var code = Factory.New<OrgCusCode>();
			code.OK_CodeType = OrgCusCode.CodeTypes.CustomsCPPermitCode;
			code.OK_CustomsRegNo = "XYZ";
			code.OK_OH = client.PK;
			docket.WD_DocketID = "W00001001";

			var wrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("XYZ00001001", wrapper.CP_IssueNo);
		}

		#endregion

		#region TestClientGCR

		protected override void TestClientGCRCore()
		{
			var client = Factory.New<OrgHeader>();
			var docket = GetNewDocket();
			docket.WD_OH_Client = client.PK;

			var code = Factory.New<OrgCusCode>();
			code.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			code.OK_CustomsRegNo = "666";
			code.OK_OH = client.PK;

			var wrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("666", wrapper.ClientGCR);
		}

		#endregion

		#region TestACSEstCode

		protected override void TestACSEstCodeCore()
		{
			var consignee = Factory.New<OrgHeader>();
			var code = consignee.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			code.OK_CustomsRegNo = "A1234";

			var docket = (WhsPickableDocket)GetNewDocket();
			docket.ConsigneePK = consignee.PK;

			var wrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("A1234", wrapper.ACSEstCode);
		}

		#endregion

		#region TestATOEstCode

		protected override void TestATOEstCodeCore()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = Factory.New<OrgAddress>().PK;

			var docket = GetNewDocket();
			docket.WD_WW_Whs = warehouse.PK;

			var warehouseOrg = Factory.New<OrgHeader>();
			var code = warehouseOrg.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			code.OK_CustomsRegNo = "Z9876";
			docket.Warehouse.WarehouseAddress.OA_OH = warehouseOrg.PK;

			var wrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("Z9876", wrapper.ATOEstCode);
		}

		#endregion

		#region TestPrintPageWithContainerNumber

		protected override void TestPrintPageWithContainerNumberCore()
		{
			var docket = GetNewDocket();

			docket.Containers.AddNew().WC_ContainerNum = "CON1111";
			docket.Containers.AddNew().WC_ContainerNum = "CON2222";
			docket.Containers.AddNew().WC_ContainerNum = "CON3333";
			docket.Containers.AddNew().WC_ContainerNum = "CON4444";
			docket.Containers.AddNew().WC_ContainerNum = "CON5555";

			var wrapper = GetNewWarehouseJobGenericWrapper(docket);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)wrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)wrapper.PrintPageWithContainerNumber);
			docket.Containers.AddNew().WC_ContainerNum = "CON6666";
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)wrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be true", true, (bool)wrapper.PrintPageWithContainerNumber);
		}

		#endregion

		#region TestTotalExtendedLinePrice

		protected override void TestTotalExtendedLinePriceCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			AssertEquals(ZDecimal.Zero, WarehouseJobGenericWrapper.New(order, Factory).TotalExtendedLinePrice.Amount);

			line1.WE_ExtendedLinePrice = 10.00m;
			line2.WE_ExtendedLinePrice = 9.67m;
			Helper.CreatePickNew(order);
			AssertEquals(19.67m, WarehouseJobGenericWrapper.New(order, Factory).TotalExtendedLinePrice.Amount);
		}

		#endregion

		#region TestEmergencyContactMessageString

		protected override void TestEmergencyContactMessageStringCore()
		{
			var undg = Factory.NewWithValidTestData<UNDGSubstance>();

			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			data.Part1.UNDGs.AddNew().DI_DG = undg.PK;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);

			var wrapper1 = WarehouseJobGenericWrapper.New(order, Factory);
			AssertEquals("Y", wrapper1.PrintDGDetails);
			AssertEquals("Hazardous materials emergency contact number:\r\n ", wrapper1.EmergencyContactMessageString);

			var contact = order.Client.Contacts.AddNew();
			contact.OC_ContactName = "A";
			order.Client.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			contact.OC_HomePhone = "123";
			order.Client.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.HOM;

			AssertEquals("Hazardous materials emergency contact number:\r\nA 123", WarehouseJobGenericWrapper.New(order, Factory).EmergencyContactMessageString);
		}

		#endregion

		#region TestCurrencySymbol

		protected override void TestCurrencySymbolCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			line1.WE_ExtendedLinePrice = 10.00m;
			line1.WE_RX_NKUnitPriceCurrency = "AUD";
			line2.WE_ExtendedLinePrice = 9.67m;
			line2.WE_RX_NKUnitPriceCurrency = "AUD";
			Helper.CreatePickNew(order);

			AssertEquals("$", WarehouseJobGenericWrapper.New(order, Factory).CurrencySymbol);

			var line3 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			line3.WE_ExtendedLinePrice = 4.56m;
			line3.WE_RX_NKUnitPriceCurrency = "AFA";
			line3.ReleaseLines.AddNew();

			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.New(order, Factory).CurrencySymbol);
		}

		#endregion

		#region TestTotalExtendedLinePriceWithSymbol

		protected override void TestTotalExtendedLinePriceWithSymbolCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			line1.WE_ExtendedLinePrice = 10.00m;
			line1.WE_RX_NKUnitPriceCurrency = "AUD";
			line2.WE_ExtendedLinePrice = 9.67m;
			line2.WE_RX_NKUnitPriceCurrency = "AUD";

			Helper.CreatePickNew(order);

			AssertEquals("$19.67", WarehouseJobGenericWrapper.New(order, Factory).TotalExtendedLinePriceWithSymbol);
		}

		#endregion

		#region TestTotalTopLevelUnitsMet_TotalTopLevelUnitsOrdered

		protected override void TestTotalTopLevelUnitsMet_TotalTopLevelUnitsOrderedCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 12m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 38m);

			var pick = helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);

			var packingSlipWrapper1 = DocWhsPackingSlipLine.New(orderLine1.ReleaseLines[0], orderLine1, Factory);
			var line1UnitMet = packingSlipWrapper1.TopLevelUnitsMet;
			var line1UnitOrd = packingSlipWrapper1.TopLevelUnitsOrdered;

			var packingSlipWrapper2 = DocWhsPackingSlipLine.New(orderLine2.ReleaseLines[0], orderLine2, Factory);
			var line2UnitMet = packingSlipWrapper2.TopLevelUnitsMet;
			var line2UnitOrd = packingSlipWrapper2.TopLevelUnitsOrdered;

			var whsOrderWrapper1 = WarehouseJobGenericWrapper.New(order, Factory);
			AssertEquals("TotalTopLevelUnitsMet", line1UnitMet + line2UnitMet, whsOrderWrapper1.TotalTopLevelUnitsMet);
			AssertEquals("TotalTopLevelUnitsOrdered", line1UnitOrd + line2UnitOrd, whsOrderWrapper1.TotalTopLevelUnitsOrdered);
		}

		#endregion

		#region TestTotalGroupedLineUnitsMet

		protected override void TestTotalGroupedLineUnitsMetCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 22m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);

			orderLine1.ReleaseLines[0].Quantity = 9m;
			orderLine1.ReleaseLines[0].PartAttribute1 = "A";

			orderLine1.ReleaseLines.AddNew().Quantity = 1m;
			orderLine1.ReleaseLines[1].PartAttribute1 = "B";

			AssertEquals(10m, WarehouseJobGenericWrapper.New(order, Factory).TotalGroupedLineUnitsMet);
		}

		#endregion

		#region TestWrapperMappingsEmpty

		protected override void TestWrapperMappingsEmpty_NotDependantToBizOProperties(WarehouseJobGenericWrapper emptyWrapper)
		{
			AssertEquals("JobNumberHeading", "", emptyWrapper.JobNumberHeading);
			AssertEquals("TotalOuterPackagesWeight", WeightWrapper.Empty.ValueAndUnitCode, emptyWrapper.TotalOuterPackagesWeight.ValueAndUnitCode);
			AssertEquals("TotalOuterPackagesVolume", VolumeWrapper.Empty.ValueAndUnitCode, emptyWrapper.TotalOuterPackagesVolume.ValueAndUnitCode);
			AssertEquals("OuterPackagesContents", "", emptyWrapper.OuterPackagesContents);
		}

		#endregion

		#region Implementation

		#region CreatePickableDocketLine

		protected WhsPickableDocketLine CreatePickableDocketLine(WhsPickableDocket pickableDocket, ZString partCode, ZString partDesc, ZShort lineNo)
		{
			var line = pickableDocket.Lines.AddNew();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partCode;
			part.OP_Desc = partDesc;
			line.WE_OP = part.PK;
			line.WE_LineNo = lineNo;
			return line;
		}

		#endregion

		#region CreatePickableDocketLineAttribute

		protected WhsReleaseLine CreatePickableDocketLineAttribute(WhsPickableDocketLine pickableDocketLine)
		{
			return CreatePickableDocketLineAttribute(pickableDocketLine, 0m, "", "", "", "", ZDate.Empty, ZDate.Empty);
		}

		protected WhsReleaseLine CreatePickableDocketLineAttribute(WhsPickableDocketLine pickableDocketLine, ZDecimal units, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate expiryDate, ZDate packingDate)
		{
			var releaseLine = pickableDocketLine.ReleaseLines.AddNew(partAttrib1, partAttrib2, partAttrib3, serialNumber, expiryDate, packingDate);
			releaseLine.Quantity = units;
			return releaseLine;
		}

		#endregion

		#region GetWrapperSetupForBillOfLadingPackingLinesTests

		WarehouseJobGenericWrapper GetWrapperSetupForBillOfLadingPackingLinesTests()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 22m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 22m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Factory.Save();

			var pick = helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);

			orderLine1.ReleaseLines[0].Quantity = 10m;
			orderLine1.ReleaseLines[0].PartAttribute1 = "A";

			orderLine1.ReleaseLines.AddNew().Quantity = 12m;
			orderLine1.ReleaseLines[1].PartAttribute1 = "B";
			AssertEquals("Precondition", 2, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);

			return WarehouseJobGenericWrapper.New(order, Factory);
		}

		#endregion

		protected override WhsDocket GetNewDocket()
		{
			return Factory.NewWithValidTestData<WhsOrder>();
		}

		#region ExpectedFieldMap

		protected sealed override string ExpectedFieldMap
		{
			get
			{
				return @"
WarehouseJob
======================================================================
Name                                    Type
----------------------------------------------------------------------
ConsigneeAddress                        Address
DistributionCentreAddress               Address
DropOffAddress                          Address
GoodsBillToAddress                      Address
PickUpAddress                           Address
SupplierDocAddress                      Address
TransportBillToAddress                  Address
TransportCoAddress                      Address
CarrierServiceLevel                     CarrierServiceLevel
CODType                                 CodeAndDescription
CustomsStatus                           CodeAndDescription
DropMode                                CodeAndDescription
FulfillRule                             CodeAndDescription
IncoTerm                                CodeAndDescription
PickOption                              CodeAndDescription
SalesChannel                            CodeAndDescription
ServiceLevel                            CodeAndDescription
TransportationUnit                      Equipment
ConfirmationInstructions                LabelValuePair
CustomerReference                       LabelValuePair
FinalisedDate                           LabelValuePair
HandlingInstructions                    LabelValuePair
PickingInstructions                     LabelValuePair
PickMethod                              LabelValuePair
PickNo                                  LabelValuePair
PickNumberReference                     LabelValuePair
PrimaryBarcode                          LabelValuePair
RequiredDate                            LabelValuePair
SecondaryReference                      LabelValuePair
SOPCarrierServiceLevel                  LabelValuePair
SOPOrderNumber                          LabelValuePair
SOPRequiredDate                         LabelValuePair
SOPSpecialInstructions                  LabelValuePair
SOPStagingAreaName                      LabelValuePair
SOPTransportCompany                     LabelValuePair
SplitNumber                             LabelValuePair
StagingAreaName                         LabelValuePair
StagingLocationString                   LabelValuePair
Status                                  LabelValuePair
TotalLoadedPackages                     LabelValuePair
TotalLoadedUnits                        LabelValuePair
TransportReference                      LabelValuePair
VehicleReference                        LabelValuePair
WarehouseName                           LabelValuePair
WhoCreated                              LabelValuePair
WhoFinalised                            LabelValuePair
CODAmount                               Money
Insurance                               Money
TotalExtendedLinePrice                  Money
Client                                  Organisation
ClientRequestedBillToParty              Organisation
Consignee                               Organisation
Consignor                               Organisation
Forwarder                               Organisation
JobClient                               Organisation
Supplier                                Organisation
TransportCompany                        Organisation
CarrierAccount                          OrgCarrierAccount
Destination                             PlaceAndDate
SupplierBuyerLink                       SupplierBuyerLink
PackagesSent                            ValueAndUnit
CubicSent                               Volume
TotalLoadedVolume                       Volume
TotalOuterPackagesVolume                Volume
Warehouse                               WarehouseBO
TotalLoadedWeight                       Weight
TotalOuterPackagesWeight                Weight
WeightSent                              Weight
ABN                                     String
AccountCode                             String
ACSEstCode                              String
AllDCNsAreAuthorized                    Bool
AllowPartialLoading                     Bool
ArrivalDate                             DateTime
AssignedLoader                          String
ATOEstCode                              String
BarcodeText                             String
BarcodeTextForFont                      String
BOLNumber                               String
BookingDate                             DateTime
CartageAdviceClosingText                MultilingualString
CartageAdviceOpeningText                MultilingualString
CartageDropMode                         String
ClientCPC                               String
ClientGCR                               String
CompleteTime                            DateTime
ConsolidatedInvoiceRef                  String
ContainerNumberAndTypeLine              String
ContainerType                           String
CP_IssueNo                              String
CurrencyCode                            String
CurrencySymbol                          String
CustomAttribute1                        String
CustomAttribute2                        String
CustomAttribute3                        String
CustomAttribute4                        String
CustomAttribute5                        String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomDecimal3                          Decimal
CustomDecimal4                          Decimal
CustomDecimal5                          Decimal
CustomerReferenceBarcode                String
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomFlag3                             Bool
CustomFlag4                             Bool
CustomFlag5                             Bool
CutoffTime                              DateTime
DebtorCodeAndName                       String
DeliveryRoute                           String
DepartmentName                          String
DepartmentNumber                        String
DispatchDriverName                      String
DockDoor                                String
DocketStatus                            String
DocketSubType                           String
DocketType                              String
DocumentTitle                           String
EmergencyContactMessageString           String
EnableDangerousGoodsDetails             Bool
EnableExtendedLinePrice                 Bool
EventTypeCode                           String
ExpectedDispatchTime                    DateTime
FinalizedTime                           DateTime
FromDate                                DateTime
GateInTime                              DateTime
GateOutTime                             DateTime
HasDispatchDriverSignature              Bool
HasMultipleStockKeepingUnits            Bool
HasMultipleVolumeUnits                  Bool
HasMultipleWeightUnits                  Bool
HasNonPickedItems                       Bool
HasOversAndUnders                       Bool
HasReceiveDriverSignature               Bool
HasShortfallItems                       Bool
HouseBill                               String
HouseBillHeading                        String
InvoiceNumber                           String
IsAuthorisedToLeave                     Bool
IsAuthorizedForDispatch                 Bool
IsAwaitingForwardingChanges             Bool
IsCustomsTransaction                    Bool
IsFinalised                             Bool
IsReadyToStage                          Bool
IsSecure                                Bool
IsSplit                                 Bool
IsWorkOrder                             Bool
IsWorkOrderPick                         Bool
JobNumber                               String
JobNumberHeading                        String
JobType                                 String
LoadNumber                              String
MasterBill                              String
MasterBillHeading                       String
NextDischargePort                       String
OrderTypeCodeFirst2Characters           String
OrderTypeCodeLast4Characters            String
OtherReferences                         String
OuterPackagesContents                   String
PackingSlipTitle                        String
PalletsSent                             Short
PrimaryBarcodeText                      String
PrintDGDetails                          String
PrintPageWithContainerNumber            Bool
ProductLinesCount                       Int
ReceiveDriverName                       String
References                              String
ReferencesExtended                      String
ReportDescription                       MultilingualString
Seal                                    String
SecondaryHeading                        String
SecondaryNumber                         String
SelectedABCCategory                     String
SelectedArea                            MultilingualString
SelectedClient                          String
SelectedCommodityCode                   String
SelectedCycle                           String
SelectedLocation                        String
SelectedPickMethod                      String
SelectedRow                             String
SelectedStocktakeType                   String
SelectedSupplierPart                    String
SOPConsigneeAddressLabel                String
StartTime                               DateTime
StocktakeNumber                         String
SubTypeDesc                             String
ToDate                                  DateTime
TotalCubic                              Decimal
TotalExtendedLinePriceWithSymbol        String
TotalGroupedLineUnitsMet                Decimal
TotalInnerPackLines                     Short
TotalInners                             Short
TotalNumberOfLabels                     Int
TotalNumberOfPackageLabels              Int
TotalOverpacks                          Short
TotalPallets                            Short
TotalTopLevelUnitsMet                   Decimal
TotalTopLevelUnitsOrdered               Decimal
TotalUnits                              Decimal
TotalWeight                             Decimal
TransportMode                           String
TransportZone                           String
UnitsSent                               Decimal
UnloadCompleteTime                      DateTime
VehicleNumber                           String
VendorID                                String
WarehouseCartageCoordinatorName         String
WarehouseCartageCoordinatorPhone        String
WarehouseCCPCode                        String
WarehouseExpectedArrivalTime            DateTime
WarehouseNameAndAddress                 MultilingualString
WarehousePhoneAndFax                    String
WarehouseReference                      String
WorkOrderLevels10th                     String
WorkOrderLevels1st                      String
WorkOrderLevels2nd                      String
WorkOrderLevels3rd                      String
WorkOrderLevels4th                      String
WorkOrderLevels5th                      String
WorkOrderLevels6th                      String
WorkOrderLevels7th                      String
WorkOrderLevels8th                      String
WorkOrderLevels9th                      String

Containers                              Container Collection
LoadedPackages                          Package Collection
Packages                                Package Collection
UNDGs                                   UNDGSubstance Collection
JobLinesVariances                       WarehouseGroupedLinesForVariance Collection
DispatchLoadLists                       WarehouseJob Collection
DispatchTransportationUnits             WarehouseJob Collection
Jobs                                    WarehouseJob Collection
Orders                                  WarehouseJob Collection
ReceiveTransportationUnits              WarehouseJob Collection
BillOfLadingPackingLines                WarehouseJobLine Collection
BillOfLadingPackingLinesUS              WarehouseJobLine Collection
BOMStagingAreaParts                     WarehouseJobLine Collection
JobLines                                WarehouseJobLine Collection
PackingLines                            WarehouseJobLine Collection
PalletizedInventory                     WarehouseJobLine Collection
PickingLines                            WarehouseJobLine Collection
VarianceLines                           WarehouseJobLine Collection
WorkOrderLines                          WarehouseJobLine Collection
";
			}
		}

		#endregion

		#endregion
	}
}
