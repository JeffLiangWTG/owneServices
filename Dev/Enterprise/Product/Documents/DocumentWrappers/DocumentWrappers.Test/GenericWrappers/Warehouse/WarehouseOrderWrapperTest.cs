using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseOrderWrapper))]
	public class WarehouseOrderWrapperTest : WarehousePickableDocketWrapperTest
	{
		#region Properties

		#region TestAddresses
		#region TestPickUpAddress

		protected override void TestPickUpAddressCore()
		{
			var docket = GetNewDocket();
			docket.PickUpAddressPK = AddressPKWithDetailsFilled();

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", docketWrapper.PickUpAddress.CompanyNameAndAddress);
		}

		#endregion

		#region TestDropOffAddress

		protected override void TestDropOffAddressCore()
		{
			var docket = GetNewDocket();
			docket.DropOffAddressPK = AddressPKWithDetailsFilled();

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", docketWrapper.DropOffAddress.CompanyNameAndAddress);
		}

		#endregion

		#region TestSupplierDocumentaryAddress

		protected override void TestSupplierDocAddressCore()
		{
			var docket = GetNewDocket();
			docket.SupplierDocAddress.E2_OA_Address = AddressPKWithDetailsFilled();

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", docketWrapper.SupplierDocAddress.CompanyNameAndAddress);
		}

		#endregion

		#region TestGoodsBillToAddressCore

		protected override void TestGoodsBillToAddressCore()
		{
			var docket = GetNewDocket();
			docket.GoodsBillToAddressPK = AddressPKWithDetailsFilled();

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", docketWrapper.GoodsBillToAddress.CompanyNameAndAddress);
		}

		#endregion

		#region TestTransportBillToAddressCore

		protected override void TestTransportBillToAddressCore()
		{
			var docket = GetNewDocket();
			docket.TransportBillToDocAddress.E2_OA_Address = AddressPKWithDetailsFilled();

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", docketWrapper.TransportBillToAddress.CompanyNameAndAddress);
		}

		#endregion

		#region TestDistributionCentreAddressCore

		protected override void TestDistributionCentreAddressCore()
		{
			var docket = GetNewDocket();
			docket.DistributionCentreDocAddress.E2_OA_Address = AddressPKWithDetailsFilled();

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", docketWrapper.DistributionCentreAddress.CompanyNameAndAddress);
		}

		#endregion

		#region TestAddress

		ZGuid AddressPKWithDetailsFilled()
		{
			var org = Helper.CreateClient("Org1");
			var address = org.MainAddress;
			org.OH_RL_NKClosestPort = "AU";
			org.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "US";
			address.OA_RN_NKCountryCode = "AU";
			return address.PK;
		}

		#endregion

		#endregion

		#region ReferenceTypes

		#region TestDepartmentName

		protected override void TestDepartmentNameCore()
		{
			AssertReferenceCode(WarehouseAdditionalReferenceTypes.Codes.DepartmentNameCode, o => o.DepartmentName);
		}

		#endregion

		#region TestDepartmentNumber

		protected override void TestDepartmentNumberCore()
		{
			AssertReferenceCode(WarehouseAdditionalReferenceTypes.Codes.DepartmentNumberCode, o => o.DepartmentNumber);
		}

		#endregion

		#region TestOrderTypeCodeFirst2Characters

		protected override void TestOrderTypeCodeFirst2CharactersCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			var reference = order.References.AddNew();
			reference.WX_RefType = "XXX";
			reference.WX_Reference = "YYY";
			AssertEquals("", orderWrapper.OrderTypeCodeFirst2Characters);

			reference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.OrderTypeCode;
			reference.WX_Reference = "";
			AssertEquals("", orderWrapper.OrderTypeCodeFirst2Characters);

			reference.WX_Reference = "Z";
			AssertEquals("Z", orderWrapper.OrderTypeCodeFirst2Characters);

			reference.WX_Reference = "ZXY";
			AssertEquals("ZX", orderWrapper.OrderTypeCodeFirst2Characters);
		}

		#endregion

		#region TestOrderTypeCodeLast4Characters

		protected override void TestOrderTypeCodeLast4CharactersCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			var reference = order.References.AddNew();
			reference.WX_RefType = "XXX";
			reference.WX_Reference = "YYY";
			AssertEquals("", orderWrapper.OrderTypeCodeLast4Characters);

			reference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.OrderTypeCode;
			reference.WX_Reference = "";
			AssertEquals("", orderWrapper.OrderTypeCodeLast4Characters);

			reference.WX_Reference = "Z";
			AssertEquals("", orderWrapper.OrderTypeCodeLast4Characters);

			reference.WX_Reference = "ZXY";
			AssertEquals("Y", orderWrapper.OrderTypeCodeLast4Characters);

			reference.WX_Reference = "ZXYABC";
			AssertEquals("YABC", orderWrapper.OrderTypeCodeLast4Characters);

			reference.WX_Reference = "ZXYABC1234";
			AssertEquals("YABC", orderWrapper.OrderTypeCodeLast4Characters);
		}

		#endregion

		#region TestEventTypeCode

		protected override void TestEventTypeCodeCore()
		{
			AssertReferenceCode(WarehouseAdditionalReferenceTypes.Codes.EventTypeCode, o => o.EventTypeCode);
		}

		#endregion

		void AssertReferenceCode(string referenceType, Func<WarehouseOrderWrapper, ZString> referenceProperty)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			var reference = order.References.AddNew();
			reference.WX_RefType = "XXX";
			reference.WX_Reference = "YYY";
			AssertEquals("", referenceProperty(orderWrapper));

			reference.WX_RefType = referenceType;
			reference.WX_Reference = "";
			AssertEquals("", referenceProperty(orderWrapper));

			reference.WX_Reference = "ZZZ";
			AssertEquals("ZZZ", referenceProperty(orderWrapper));
		}

		#endregion

		#region TestJobType

		protected override void TestJobTypeCore()
		{
			var order = GetNewDocket();
			var orderWrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("DOM", orderWrapper.JobType);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "NZCHC";

			order.WD_WW_Whs = warehouse.PK;
			AssertEquals("", orderWrapper.JobType);
		}

		#endregion

		#region SecondaryReference

		protected override void TestSecondaryReferenceCore()
		{
			var order = GetNewDocket();
			order.WD_ExternalReference = "OrderReference1";

			var orderWrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("Order Number", orderWrapper.SecondaryReference.Label);
			AssertEquals("OrderReference1", orderWrapper.SecondaryReference.Value);
		}

		#endregion

		#region TestHandlingInstructions

		protected override void TestHandlingInstructionsCore()
		{
			var order = (WhsOrder)GetNewDocket();
			var orderWrapper = GetNewWarehouseJobGenericWrapper(order);

			AssertEquals("Special Instructions", orderWrapper.HandlingInstructions.Label);
			AssertEquals("Precondition: Empty", "", orderWrapper.HandlingInstructions.Value);

			order.WD_HandlingInstructions = "Instructions so special they need a special name";

			AssertEquals("Special instructions", "Instructions so special they need a special name", orderWrapper.HandlingInstructions.Value);
		}

		#endregion

		#region TestPackingSlipTitle

		protected override void TestPackingSlipTitleCore()
		{
			var whs = Factory.New<WhsWarehouse>();
			whs.WW_GB_RelatedCompanyBranch = Factory.New<GlbBranch>().PK;
			WarehouseDataRegistry.Instance.PackingSlipTitles.SetValue(Guid.Empty, whs.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, "Despatch Slip");

			var order = (WhsOrder)GetNewDocket();
			order.WD_WW_Whs = whs.PK;

			var packingSlipWrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("Despatch Slip", packingSlipWrapper.PackingSlipTitle);

			order.WD_WW_Whs = ZGuid.Empty;
			AssertEquals("Packing Slip", packingSlipWrapper.PackingSlipTitle);
		}

		#endregion

		#region TestPickingInstructions

		protected override void TestPickingInstructionsCore()
		{
			var order = GetNewDocket();
			order.WD_ExternalReference = "OrderReference1";

			var orderWrapper = GetNewWarehouseJobGenericWrapper(order);
			StmNote pickingInstructionNote = order.Notes.AddNew();
			pickingInstructionNote.ST_Description = PredefinedNoteTypes.Instance.PickingInstructions.Description;
			pickingInstructionNote.ST_NoteText = "Pick note";
			AssertEquals("Document Wrapper PickingInstruction is incorrect", "Order OrderReference1 - Pick note", orderWrapper.PickingInstructions.Value);
			AssertEquals("Document Wrapper PickingInstruction Label is incorrect", "Picking Inst.", orderWrapper.PickingInstructions.Label);
		}

		#endregion

		#region Totals

		#region TestTotalOuterPackagesWeight

		public void TestTotalOuterPackagesWeight()
		{
			var order = Factory.New<WhsOrder>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = order.PK;
			packageJob.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			var pallet = CreatePackage(packageJob.Packages, 1, "PLT", 500m, "KG", 4m, "M3");
			CreatePackage(packageJob.Packages, 2, "BOX", 0.2m, "T", 1000m, "D3");
			CreatePackage(pallet.Packages, 5, "BAG", 250m, "KG", 2.4m, "M3");

			var wrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("700.00 KG", wrapper.TotalOuterPackagesWeight.ValueAndUnitCode);
		}

		#endregion

		#region TestTotalOuterPackagesVolume

		public void TestTotalOuterPackagesVolume()
		{
			var order = Factory.New<WhsOrder>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = order.PK;
			packageJob.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			var pallet = CreatePackage(packageJob.Packages, 1, "PLT", 500m, "KG", 4m, "M3");
			CreatePackage(packageJob.Packages, 2, "BOX", 0.2m, "T", 1000m, "D3");
			CreatePackage(pallet.Packages, 5, "BAG", 250m, "KG", 2.4m, "M3");

			var wrapper = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("5.000 M3", wrapper.TotalOuterPackagesVolume.ValueAndUnitCode);
		}

		#endregion

		#region TestOuterPackagesContents

		public void TestOuterPackagesContents()
		{
			var order = Factory.New<WhsOrder>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = order.PK;
			packageJob.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			var pallet = CreatePackage(packageJob.Packages, 1, Constants.PkgUnit.Pallet, 500m, "KG", 4m, "M3");
			CreatePackage(packageJob.Packages, 2, Constants.PkgUnit.Box, 0.2m, "T", 1000m, "D3");
			CreatePackage(packageJob.Packages, 3, Constants.PkgUnit.Crate, 0.2m, "T", 1000m, "D3");
			CreatePackage(packageJob.Packages, 4, Constants.PkgUnit.Unit, 0.2m, "T", 1000m, "D3");
			CreatePackage(packageJob.Packages, 5, Constants.PkgUnit.Package, 0.2m, "T", 1000m, "D3");
			CreatePackage(packageJob.Packages, 6, Constants.PkgUnit.Piece, 0.2m, "T", 1000m, "D3");
			CreatePackage(packageJob.Packages, 7, Constants.PkgUnit.Roll, 0.2m, "T", 1000m, "D3");
			CreatePackage(packageJob.Packages, 8, Constants.PkgUnit.Sheet, 0.2m, "T", 1000m, "D3");
			CreatePackage(packageJob.Packages, 9, Constants.PkgUnit.Tube, 0.2m, "T", 1000m, "D3");
			CreatePackage(packageJob.Packages, 10, Constants.PkgUnit.Basket, 0.2m, "T", 1000m, "D3");

			// Level 2
			CreatePackage(pallet.Packages, 5, Constants.PkgUnit.Bag, 250m, "KG", 2.4m, "M3");

			var wrapper1 = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("Only outer packages should be displayed.", "2x BOX, 10x BSK, 3x CRT, 6x PCE, 5x PKG, 1x PLT, 7x RLL, 8x SHT, 9x TUB, 4x UNT", wrapper1.OuterPackagesContents);

			CreatePackage(packageJob.Packages, 12, "BOT", 0.2m, "T", 1000m, "D3");
			var wrapper2 = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("Only 5 different outer packages should be displayed.", "12x BOT, 2x BOX, 10x BSK, 3x CRT, 6x PCE, 5x PKG, 1x PLT, 7x RLL, 8x SHT, 9x TUB...", wrapper2.OuterPackagesContents);
		}

		#endregion

		#region CreatePackage

		PkgPackage CreatePackage(PkgPackageCollection packageCollection, ZInt packs, ZString packType, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ)
		{
			var package = packageCollection.AddNew();
			package.KP_PackageQty = packs;
			package.KP_F3_NKPackType = packType;
			using (new SemaphoreManager(package.SuspendAddWeightToParentPackageSemaphore))
			{
				package.KP_Weight = weight;
				package.KP_WeightUQ = weightUQ;
				package.KP_Volume = volume;
				package.KP_VolumeUQ = volumeUQ;
			}
			return package;
		}

		#endregion

		#endregion

		#region TestOrderCarrierServiceLevel

		public void TestOrderCarrierServiceLevel()
		{
			var transportCo = Helper.CreateClient();
			var sampleCarrierServiceLevel = transportCo.MiscServ.CarrierServiceLevels.ToArray<OrgCarrierServiceLevel>().FirstOrDefault();
			if (sampleCarrierServiceLevel == null)
			{
				Fail("Test data error: transportCo have no carrier service level settings.");
			}

			var order = (WhsOrder)GetNewDocket();
			order.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			order.WD_PL_NKCarrierServiceLevel = "STD";

			AssertNotNull(order.CarrierServiceLevel);
			AssertEquals("WD_PL_NKCarrierServiceLevel field should be set to 'STD' in Order.", "STD", order.WD_PL_NKCarrierServiceLevel);
			AssertEquals("PL_Code field should be set to 'STD' in sampleCarrierServiceLevel.", "STD", sampleCarrierServiceLevel.PL_Code);
			AssertEquals("WD_PL_NKCarrierServiceLevel and PL_Code should be same valued in this case.", order.WD_PL_NKCarrierServiceLevel, sampleCarrierServiceLevel.PL_Code);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);

			AssertNotNull(orderWrapper.CarrierServiceLevel.WrappedObject);
			AssertEquals("OrderWrapper should be initiated with sampleCarrierServiceLevel.", sampleCarrierServiceLevel, orderWrapper.CarrierServiceLevel.WrappedObject);
		}

		public void TestOrderCarrierServiceLevel_Fallback()
		{
			var transportCo = Helper.CreateClient();
			var order = (WhsOrder)GetNewDocket();
			order.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			order.WD_PL_NKCarrierServiceLevel = "XYZ";

			AssertNull(order.CarrierServiceLevel);
			AssertEquals("WD_PL_NKCarrierServiceLevel field should be set to 'XYZ' in Order.", "XYZ", order.WD_PL_NKCarrierServiceLevel);
			AssertEquals("There should be no same carrier service level settings in transport company.", null, transportCo.MiscServ.CarrierServiceLevels.ToArray<OrgCarrierServiceLevel>()
				.FirstOrDefault(s => s.PL_Code.EqualsIgnoringCase(order.WD_PL_NKCarrierServiceLevel)));

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);

			AssertEquals("OrderWrapper should NOT have CarrierServiceLevel property with an actual OrgCarrierServiceLevel BO.", null, orderWrapper.CarrierServiceLevel.WrappedObject);
			AssertEquals("OrderWrapper should be initiated with WD_PL_NKCarrierServiceLevel field from Order for Code property.", "XYZ", orderWrapper.CarrierServiceLevel.Code);
			AssertEquals("OrderWrapper should be initiated with WD_PL_NKCarrierServiceLevel field from Order for Code property.", order.WD_PL_NKCarrierServiceLevel, orderWrapper.CarrierServiceLevel.Code);
		}

		#endregion

		#region TestCarrierAccount

		public void TestCarrierAccount()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var warehouse = data.Whs1;

			var order = Helper.CreateWhsOrder(client, warehouse);
			order.TransportCoPK = client.PK;
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertNull(orderWrapper.CarrierAccount);

			var orgCarrierAccount = Factory.New<OrgCarrierAccount>();
			orgCarrierAccount.OAN_OH_Carrier = client.PK;
			orgCarrierAccount.OAN_AccountNumber = "1234567";

			var accountAssociation = Factory.New<OrgWhsClientAccountAssociation>();
			accountAssociation.OWC_WW_Warehouse = warehouse.PK;
			accountAssociation.OWC_OH_Client = client.PK;
			accountAssociation.OWC_OAN_CarrierAccount = orgCarrierAccount.PK;
			Factory.Save();

			orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("1234567", orderWrapper.CarrierAccount.AccountNumber);
		}

		public void TestCarrierAccount_SalesChannel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var warehouse = data.Whs1;

			var order = Helper.CreateWhsOrder(client, warehouse);
			order.TransportCoPK = client.PK;
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertNull(orderWrapper.CarrierAccount);

			var orgCarrierAccount = Factory.New<OrgCarrierAccount>();
			orgCarrierAccount.OAN_OH_Carrier = client.PK;
			orgCarrierAccount.OAN_AccountNumber = "1234567";

			var salesChannel = Factory.New<WhsSalesChannel>();
			salesChannel.WSH_Code = "DAR";
			salesChannel.WSH_Description = "DART through";
			order.WD_WSH_SalesChannel = salesChannel.PK;

			var accountAssociation = Factory.New<OrgWhsClientAccountAssociation>();
			accountAssociation.OWC_OH_Client = client.PK;
			accountAssociation.OWC_OAN_CarrierAccount = orgCarrierAccount.PK;
			accountAssociation.OWC_WSH_SalesChannel = salesChannel.PK;
			Factory.Save();

			orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("1234567", orderWrapper.CarrierAccount.AccountNumber);
		}

		#endregion

		#region TestSalesChannel

		public void TestSalesChannel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var warehouse = data.Whs1;

			var order = Helper.CreateWhsOrder(client, warehouse);
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertNull(nameof(orderWrapper.SalesChannel), orderWrapper.SalesChannel);

			var salesChannel = Factory.New<WhsSalesChannel>();
			salesChannel.WSH_Code = "DAR";
			salesChannel.WSH_Description = "DART through";

			order.WD_WSH_SalesChannel = salesChannel.PK;
			Factory.Save();

			orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals(nameof(orderWrapper.SalesChannel.Code), "DAR", orderWrapper.SalesChannel.Code);
			AssertEquals(nameof(orderWrapper.SalesChannel.Description), "DART through", orderWrapper.SalesChannel.Description);
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

		#region TransportZone

		public void TestGetTransportZone_WithoutTransportZoneSet()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1500");
			var order = CreateOrder(consigneeDocAddress, transportCo);

			var query = new ZQuery();
			query.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, transportCo.PK);
			query.AddToFilter(RateTransportProviderSchema.TP_IsActive, true);

			var provider = Factory.LoadTop1<RateTransportProvider>(query);
			AssertNull("Precondition: No domestic sets", provider);
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasTransportZoneSetButInactive()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1500");
			var order = CreateOrder(consigneeDocAddress, transportCo);

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			prov.TP_IsActive = false;

			var query = new ZQuery();
			query.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, transportCo.PK);
			query.AddToFilter(RateTransportProviderSchema.TP_IsActive, true);
			var provider = Factory.LoadTop1<RateTransportProvider>(query);
			Factory.Save();

			AssertNull("Precondition: No domestic sets", provider);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasTransportZoneSetWithoutZones()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1500");
			var order = CreateOrder(consigneeDocAddress, transportCo);

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			Factory.Save();

			AssertEquals("Precondition: No Zones", 0, prov.Zones.Count);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasTransportZoneSetButZoneInactive()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1500");
			var order = CreateOrder(consigneeDocAddress, transportCo);

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			var zone = prov.Zones.AddNew();
			zone.TZ_ZoneName = "V0";
			zone.TZ_IsActive = false;
			Factory.Save();

			AssertEquals("Precondition: Zone is inactive", false, zone.TZ_IsActive);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasTransportZoneSetWithoutZoneDefinition()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1500");
			var order = CreateOrder(consigneeDocAddress, transportCo);

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			var zone = prov.Zones.AddNew();
			zone.TZ_ZoneName = "V0";
			Factory.Save();

			AssertEquals("Precondition: Transport Zone has no ZoneItem", 0, zone.Items.Count);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasTransportZoneSetButConsigneeWithoutPostCode()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("");
			var order = CreateOrder(consigneeDocAddress, transportCo);

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			TransportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");
			Factory.Save();

			AssertEquals("Precondition: Consignee address has no post code", "", consigneeDocAddress.E2_Postcode);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasTransportZoneSetAndConsigneeButPostCodeOutOfRange()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("2000");
			var order = CreateOrder(consigneeDocAddress, transportCo);

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			TransportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasTransportZoneSetAndConsigneeButPostCodeIsInactive()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("ABCD");
			var order = CreateOrder(consigneeDocAddress, transportCo);

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			var zone = TransportHelper.CreateZone("V0", prov);
			var cityTown = TransportHelper.CreateCityTown("TEST", "AU");
			cityTown.R9_IsActive = false;
			TransportHelper.AddCityToZone(zone, cityTown);
			Factory.Save();

			AssertEquals("Precondition: City in ZoneDefination is inactive", false, cityTown.R9_IsActive);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetAndConsigneeButHasNoTransportCo()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200");
			var order = CreateOrder(consigneeDocAddress, null);

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			TransportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");
			Factory.Save();

			AssertNull("Precondition: Order has no TransportCo", order.TransportCo);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetAndConsignee_NotOverride()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200");

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			TransportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");
			Factory.Save();

			var order = CreateOrder(consigneeDocAddress, transportCo);
			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should not be empty", "V0", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetAndConsignee_Override()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200", true);

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			TransportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");
			Factory.Save();

			var order = CreateOrder(consigneeDocAddress, transportCo);
			AssertEquals("Precondition: Consignee is override", true, consigneeDocAddress.E2_AddressOverride);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should not be empty", "V0", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetWithMultiZonesAndConsignee()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1800");

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			TransportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");
			TransportHelper.CreateZoneWithPostCodes("V1", prov, "1500", "2000");
			Factory.Save();

			var order = CreateOrder(consigneeDocAddress, transportCo);
			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should not be empty", "V1", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectMultiTransportZoneSetsAndConsignee_DifferentCountries()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200");

			var auProv = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			TransportHelper.CreateZoneWithPostCodes("A Zone in AU", auProv, "1000", "1500");

			var usProv = TransportHelper.CreateZoneRateProvider("US", transportCo);
			TransportHelper.CreateZoneWithPostCodes("A Zone in US", usProv, "1000", "1500");
			Factory.Save();

			var order = CreateOrder(consigneeDocAddress, transportCo);
			AssertEquals("Precondition: ", true, auProv.CountryCode == consigneeDocAddress.Country.Code);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should not be empty", "A Zone in AU", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetAndConsigneeButCountryCodeIsEmpty_NotOverride()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200", false, false);
			var order = CreateOrder(consigneeDocAddress, transportCo);

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			TransportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");
			Factory.Save();

			AssertEquals("Precondition: CountryCode is empty", "", consigneeDocAddress.E2_RN_NKCountryCode);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("CountryCode should be empty", "", orderWrapper.Consignee.MainAddress.Country.Code);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetAndConsigneeButCountryCodeIsEmpty_Override()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200", true, false);

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			TransportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");
			Factory.Save();

			var order = CreateOrder(consigneeDocAddress, transportCo);
			AssertEquals("Precondition: Consignee is override", true, consigneeDocAddress.E2_AddressOverride);
			AssertEquals("Precondition: CountryCode is empty", "", consigneeDocAddress.E2_RN_NKCountryCode);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);

			AssertEquals("CountryCode should be empty", "", orderWrapper.Consignee.MainAddress.Country.Code);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetButZoneHubLocationInactive()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200");
			var order = CreateOrder(consigneeDocAddress, transportCo, "SYDNEY");

			var inactiveCity = TransportHelper.CreateCityTown("SYDNEY", "AU");
			inactiveCity.R9_IsActive = false;

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo, inactiveCity);
			TransportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");
			Factory.Save();

			AssertEquals("Precondition: City is inactive", false, inactiveCity.R9_IsActive);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should be empty", "", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetsWithDifferentZoneHubLocationsAndConsignee()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200", city: "BROKILON");
			consigneeDocAddress.E2_RN_NKCountryCode = "AU";

			var city0 = TransportHelper.CreateCityTown("TEST", "US", stateCode: "LA");
			var city1 = TransportHelper.CreateCityTown("BROKILON", "AU", stateCode: consigneeDocAddress.StateCode);

			var prov0 = TransportHelper.CreateZoneRateProvider("US", transportCo, city0);
			prov0.TP_RN_NKCountry = "US";
			var zone0 = TransportHelper.CreateZoneWithPostCodes("V0", prov0, "1000", "1500");

			var prov1 = TransportHelper.CreateZoneRateProvider("AU", transportCo, city1);
			prov1.TP_RN_NKCountry = "AU";
			var zone1 = TransportHelper.CreateZoneWithPostCodes("V1", prov1, "1000", "1500");

			Factory.Save();

			var order = CreateOrder(consigneeDocAddress, transportCo, "BROKILON");
			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should not be empty", "V1", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetsWithDifferentZoneHubLocationsAndConsignee_FallbackCanMatchZoneHubLocation()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200", city: "BROKILON");
			var countryCode = consigneeDocAddress.Country.Code;

			var city = TransportHelper.CreateCityTown("BROKILON", countryCode, stateCode: consigneeDocAddress.StateCode);

			var prov0 = TransportHelper.CreateZoneRateProvider(countryCode, transportCo, city);
			prov0.TP_RN_NKCountry = countryCode;
			TransportHelper.CreateZoneWithPostCodes("V0", prov0, "1000", "1500");

			var prov1 = TransportHelper.CreateZoneRateProvider(countryCode, transportCo);
			TransportHelper.CreateZoneWithPostCodes("V1", prov1, "1000", "1500");
			Factory.Save();

			var order = CreateOrder(consigneeDocAddress, transportCo, "BROKILON");
			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should not be empty", "V0", orderWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetsWithDifferentZoneHubLocationsAndConsignee_FallbackCannotMatchZoneHubLocation()
		{
			var transportCo = Helper.CreateClient("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200");

			var city = TransportHelper.CreateCityTown("ABCD", "AU");

			var prov = TransportHelper.CreateZoneRateProvider("AU", transportCo, city);
			TransportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			var prov2 = TransportHelper.CreateZoneRateProvider("AU", transportCo);
			TransportHelper.CreateZoneWithPostCodes("V1", prov2, "1000", "1500");
			Factory.Save();

			var order = CreateOrder(consigneeDocAddress, transportCo, "SYDNEY");
			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("TransportZone should not be empty", "V1", orderWrapper.TransportZone);
		}

		WhsOrder CreateOrder(JobDocAddress consigneeDocAddress, OrgHeader transportCo, string warehouseCity = "")
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			if (transportCo != null)
			{
				order.TransportCoPK = transportCo.PK;
			}

			if (!string.IsNullOrEmpty(warehouseCity))
			{
				data.Whs1.WarehouseAddress.City = warehouseCity;
			}

			if (consigneeDocAddress.OrganisationPK.IsEmpty)
			{
				order.ConsigneeDocAddress.E2_AddressOverride = consigneeDocAddress.E2_AddressOverride;
				order.ConsigneeDocAddress.E2_RN_NKCountryCode = consigneeDocAddress.E2_RN_NKCountryCode;
				order.ConsigneeDocAddress.City = consigneeDocAddress.E2_City;
				order.ConsigneeDocAddress.E2_Postcode = consigneeDocAddress.E2_Postcode;
			}
			else
			{
				order.ConsigneePK = consigneeDocAddress.OrganisationPK;
			}

			return order;
		}

		JobDocAddress CreateConsigneeDocAddress(string postCode, bool isOverride = false, bool hasCountry = true, string city = "ADELAIDE")
		{
			var consigneeDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			if (isOverride)
			{
				consigneeDocAddress.E2_AddressOverride = true;
				consigneeDocAddress.E2_RN_NKCountryCode = hasCountry ? "AU" : "";
				consigneeDocAddress.E2_Postcode = postCode;
			}
			else
			{
				var consignee = TransportHelper.CreateOrganisation("CONSIGNEE");
				var consigneeAddress = TransportHelper.AddAddressToOrganisation(consignee, "Address1", OrgAddressType.Office);
				consigneeAddress.OA_PostCode = postCode;
				consigneeAddress.OA_City = city;
				consigneeAddress.OA_RL_NKRelatedPortCode = hasCountry ? "AUSYD" : "";
				consigneeAddress.OA_RN_NKCountryCode = hasCountry ? "AU" : "";
				consigneeDocAddress.E2_OA_Address = consigneeAddress.PK;
			}

			return consigneeDocAddress;
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

		#region TestIsAuthorisedToLeave

		protected override void TestIsAuthorisedToLeaveCore()
		{
			var wrapper = GetNewWarehouseJobGenericWrapper(null);
			AssertEquals("When BO is null should return false", false, wrapper.IsAuthorisedToLeave);

			var order = (WhsOrder)GetNewDocket();
			var wrapper2 = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("IsAuthorisedToLeave", false, wrapper2.IsAuthorisedToLeave);

			order.WD_IsAuthorisedToLeave = true;
			AssertEquals("IsAuthorisedToLeave", true, wrapper2.IsAuthorisedToLeave);
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

		#endregion

		#region Order Lines

		#region TestOrderLines

		protected override void TestOrderLinesCore()
		{
			var order = GetNewDocket();
			var orderWrapper1 = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("Pre-Condition: no lines", 0, orderWrapper1.OrderLines.Count);

			order.Lines.AddNew();
			order.Lines.AddNew();

			var orderWrapper2 = GetNewWarehouseJobGenericWrapper(order);
			AssertEquals("2 lines", 2, orderWrapper2.OrderLines.Count);
		}

		#endregion

		#region TestLinesSort

		public void TestOrderLinesSort()
		{
			var order = (WhsOrder)GetNewDocket();
			order.WD_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;

			var line1 = CreateOrderLine(order, "C3", "DESC2", 1);
			var line2 = CreateOrderLine(order, "C1", "DESC3", 3);
			var line3 = CreateOrderLine(order, "C2", "DESC1", 2);

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductCode;
			AssertLinesSortedOrder(order, line2, line3, line1);

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductDescription;
			AssertLinesSortedOrder(order, line3, line1, line2);

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.LineNo;
			AssertLinesSortedOrder(order, line1, line3, line2);

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.SameAsOnGrid;
			AssertLinesSortedOrder(order, line1, line2, line3);

			order.Client.MiscServ.OM_WhsPackingSlipOrderBy = "DEF";
			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductCode);
			AssertLinesSortedOrder(order, line2, line3, line1);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductDescription);
			AssertLinesSortedOrder(order, line3, line1, line2);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.LineNo);
			AssertLinesSortedOrder(order, line1, line3, line2);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.SameAsOnGrid);
			AssertLinesSortedOrder(order, line1, line2, line3);
		}

		void AssertLinesSortedOrder(WhsOrder order, WhsOrderLine expectedLine1, WhsOrderLine expectedLine2, WhsOrderLine expectedLine3)
		{
			var orderWrapper = GetNewWarehouseJobGenericWrapper(order);
			var sortedBy = order.Client.MiscServ.OM_WhsPackingSlipOrderBy_List.GetDescriptionFromCode(order.Client.MiscServ.OM_WhsPackingSlipOrderBy);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine1, orderWrapper.OrderLines[0].WrappedObject);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine2, orderWrapper.OrderLines[1].WrappedObject);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine3, orderWrapper.OrderLines[2].WrappedObject);
		}

		#endregion

		#endregion

		#region Test JobChargeLines

		protected override void TestJobChargeLinesCore()
		{
			var order = (WhsOrder)GetNewDocket();
			AssertNull("The initial JobHeader of Order should be null", order.JobHeader);
			var wrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("Job charge line collection on Order Wrapper should be empty when JobHeader is null", 0, wrapper.JobChargeLines.Count);

			var newOrder = (WhsOrder)GetNewDocket();
			var newWrapper = new WarehouseOrderWrapper(newOrder, Factory);
			AssertNotSame("Wrappers for different orders do not share the same cached value of JobChargeLines", wrapper.JobChargeLines, newWrapper.JobChargeLines);

			Factory.NewJobWithValidTestDataForTesting<Job>().JH_ParentID = order.PK;
			var orderJob = (Job)order.JobHeader;
			AssertNotNull("After the Job is created, the JobHeader of Order should not be null", orderJob);
			AssertEquals("Charge collection on Order object should be empty", 0, orderJob.Charges.Count);
			var wrapperWithJob = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("Job charge line collection on Order Wrapper should be empty when JobHeader's Charge collection is empty", 0, wrapperWithJob.JobChargeLines.Count);

			orderJob.Charges.AddNew();
			orderJob.Charges.AddNew();
			orderJob.Charges.AddNew();
			AssertEquals("Charge collection on Order object should have count of 3", 3, orderJob.Charges.Count);
			// No need to create a new Wrapper as a cached value of JobChargeLines was reset on JobCharge changes
			AssertNotSame("Wrappers of same order have different collections cached on Factory level", wrapper.JobChargeLines, wrapperWithJob.JobChargeLines);
			AssertEquals("Job charge line collection on Order Wrapper should have count of 3", 3, wrapperWithJob.JobChargeLines.Count);

			JobChargeCollectionTest.AssertNoJobChargesContainedInJobChargeCollection(Factory);
		}

		#endregion

		#region TestDockDoor

		protected override void TestDockDoorCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var wrapperWithoutPick = WarehouseJobGenericWrapper.New(order, Factory);
			AssertEquals("Empty wrapper DockDoor is blank.", String.Empty, wrapperWithoutPick.DockDoor);

			Helper.CreatePickNew(order);
			Factory.Save();

			var wrapperWithPick = WarehouseJobGenericWrapper.New(order, Factory);
			AssertEquals("Wrapper with order and pick has correct DockDoor.", "DOCKDOOR", wrapperWithPick.DockDoor);
		}

		#endregion

		#region Overriden Abstract Members

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return OrderWrapper;
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

		#region TestWarehouseBOWrapper_FactoryCached

		protected override WhsDocket GetNewDocketInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsOrder>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WarehouseOrderWrapper((WhsOrder)bizO, factory);
		}

		#endregion

		#region Implementation

		#region TransportHelper

		TransportBookingTestHelper TransportHelper => transportHelper ?? (transportHelper = new TransportBookingTestHelper(Factory));
		TransportBookingTestHelper transportHelper;

		#endregion

		#region OrderWrapper

		WarehouseOrderWrapper OrderWrapper => orderWrapper ?? (orderWrapper = (WarehouseOrderWrapper)Wrapper);
		WarehouseOrderWrapper orderWrapper;

		#endregion

		#region Order

		public WhsOrder Order => order ?? (order = (WhsOrder)Docket);
		WhsOrder order;

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsOrder>();
		}

		#endregion

		#region GetNewWarehouseJobGenericWrapper

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehouseOrderWrapper((WhsOrder)bizO, Factory);
		}

		#endregion

		#endregion
	}
}
