using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	public abstract class DocWhsPickableDocketTest<T, TWrapper> : DocWhsDocketTest<T, TWrapper>
			where T : WhsPickableDocket
			where TWrapper : DocWhsPickableDocket
	{
		#region Related Business Objects

		#region Collections

		#region TestPackingLines

		#region TestPackingLines

		public void TestPackingLines()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			PickableDocket.WD_OH_Client = orgHeader.PK;
			PickableDocket.WD_WW_Whs = Helper.CreateWarehouse("W1", "A").PK;
			PickableDocket.WD_RequiredDate = ZDateTimeOffset.Today;
			PickableDocket.ConsigneeNameOrPK = Helper.CreateClient("CNE").PK.ToString();
			Factory.Save();

			var line1 = CreatePickableDocketLine(PickableDocket, "P1", "D1", 1);
			var line2 = CreatePickableDocketLine(PickableDocket, "P2", "D2", 2);
			line1.WE_TransactionQuantity = 10m;
			line2.WE_TransactionQuantity = 10m;

			var isOrder = PickableDocket is WhsOrder;
			if (isOrder)
			{
				Helper.CreateProductClientRelationShip(orgHeader, line1.SupplierPart);
				Helper.CreateProductClientRelationShip(orgHeader, line2.SupplierPart);

				Helper.SetClientAllAttributeType(PickableDocket.Client, true);
				Helper.SetProductAttributeUse(PickableDocket.Client, line1.SupplierPart, AttributeNumber.One, true, true);

				Helper.CreateWhsReceiveWithInventory(PickableDocket.Client, PickableDocket.Warehouse, "ReceiveForPackingLineTest1", line1.SupplierPart, 10m);
				Helper.CreateWhsReceiveWithInventory(PickableDocket.Client, PickableDocket.Warehouse, "ReceiveForPackingLineTest2", line2.SupplierPart, 10m);
				Factory.Save();
			}
			else
			{
				var receive = Helper.CreateWhsReceive(orgHeader, PickableDocket.Warehouse);

				var bomProduct1 = line1.SupplierPart;
				var componentProduct1 = Helper.CreateProduct(orgHeader, "C1");
				Helper.CreateProductBOM(bomProduct1, componentProduct1);
				Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 10m, PickableDocket.Warehouse.DefaultLocation);

				var bomProduct2 = line2.SupplierPart;
				var componentProduct2 = Helper.CreateProduct(orgHeader, "C2");
				Helper.CreateProductBOM(bomProduct2, componentProduct2);
				Helper.CreateWhsReceiveInventoryLine(receive, componentProduct2, 10m, PickableDocket.Warehouse.DefaultLocation);

				receive.FinaliseDocket();
				Factory.Save();
				AssertEquals("Precondition", true, receive.IsFinalised);
			}

			Helper.CreatePickNew(PickableDocket);

			if (isOrder)
			{
				line1.ReleaseLines.RemoveAndDeleteAll();
				CreateReleaseLine(line1, 5m, "PA1", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
				CreateReleaseLine(line1, 5m, "PA2", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			}
			Factory.Save();

			AssertEquals(isOrder ? 3 : 2, PickableDocketWrapper.PackingLines.Count);
			Assert("Docket should not have any changes after we access PackingLines on the wrapper", !PickableDocket.HasChanges);
		}

		#endregion

		#region TestPackingLinesBOM

		public void TestPackingLinesBOM()
		{
			TestPackingLinesBOMCore();
		}

		protected virtual void TestPackingLinesBOMCore()
		{
			CreatePickableDocketLine(PickableDocket, "P1", "D1", 1);
			var line2 = CreatePickableDocketLine(PickableDocket, "P2", "D2", 2);
			var line3 = CreatePickableDocketLine(PickableDocket, "P3", "D3", 3);
			line3.WE_WE_ParentDocketLine = line2.PK;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			PickableDocket.WD_OH_Client = orgHeader.PK;
			Factory.Save();
			AssertEquals("Count should be 3, should not consider child lines", 3, PickableDocketWrapper.PackingLines.Count);
		}

		#endregion

		#region TestPackingLines_Sort

		public void TestPackingLines_Sort()
		{
			TestPackingLines_SortCore();
		}

		protected abstract void TestPackingLines_SortCore();

		#endregion

		#region TestPackingLines_RollingUp

		public void TestPackingLines_RollingUp()
		{
			TestPackingLines_RollingUpCore();
		}

		protected abstract void TestPackingLines_RollingUpCore();

		#endregion

		#region CreatePickableDocketLine

		protected WhsPickableDocketLine CreatePickableDocketLine(WhsPickableDocket pickableDocket, ZString partCode, ZString partDesc, ZShort lineNo)
		{
			WhsPickableDocketLine line = pickableDocket.Lines.AddNew();
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partCode;
			part.OP_Desc = partDesc;
			line.WE_OP = part.PK;
			line.WE_LineNo = lineNo;
			return line;
		}

		#endregion

		#region CreatePickableDocketLineAttribute

		protected WhsReleaseLine CreateReleaseLine(WhsPickableDocketLine pickableDocketLine)
		{
			return CreateReleaseLine(pickableDocketLine, 0m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
		}

		protected WhsReleaseLine CreateReleaseLine(WhsPickableDocketLine pickableDocketLine, ZDecimal units, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDateTime expiryDate, ZDateTime packingDate)
		{
			var releaseLine = pickableDocketLine.ReleaseLines.AddNew(partAttrib1, partAttrib2, partAttrib3, serialNumber, expiryDate.Date, packingDate.Date);
			releaseLine.Quantity = units;
			return releaseLine;
		}

		#endregion

		#region AssertPackingLinesSortedOrder

		protected void AssertPackingLinesSortedOrder(ZString expectedLine1Position, ZString expectedLine2Position, ZString expectedLine3Position)
		{
			DocWhsPickableDocket pickableDocketWrapper = CreateWhsDocketWrapper(PickableDocket);
			ZString sortedBy = PickableDocket.Client.MiscServ.OM_WhsPackingSlipOrderBy_List.GetDescriptionFromCode(PickableDocket.Client.MiscServ.OM_WhsPackingSlipOrderBy);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine1Position, pickableDocketWrapper.PackingLines[0].PositionAfterSorting);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine2Position, pickableDocketWrapper.PackingLines[1].PositionAfterSorting);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine3Position, pickableDocketWrapper.PackingLines[2].PositionAfterSorting);
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region Properties

		#region Consignee

		protected override void TestConsigneeCore(T docket, TWrapper wrapper)
		{
			base.TestConsigneeCore(docket, wrapper);

			var cnee = Factory.New<OrgHeader>();
			cnee.OH_FullName = "CONSIGNEE";
			docket.ConsigneePK = cnee.PK;
			AssertEquals("CONSIGNEE", wrapper.Consignee.Name);
		}

		protected override void TestConsigneeAddressCore(T docket, TWrapper wrapper)
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
			address.OA_RL_NKRelatedPortCode = "US";
			address.OA_RN_NKCountryCode = "AU";

			address.OA_OH = org.PK;
			docket.ConsigneePK = org.PK;
			docket.ConsigneeAddressPK = address.PK;

			AssertEquals("ConsigneeAddress is of type DocDocAddress", typeof(DocDocAddress), wrapper.ConsigneeAddress.GetType());

			AssertEquals("My Organisation Name", wrapper.ConsigneeAddress.CompanyName);
			AssertEquals("My Address 1", wrapper.ConsigneeAddress.Address1);
			AssertEquals("My Address 2", wrapper.ConsigneeAddress.Address2);
			AssertEquals("My City", wrapper.ConsigneeAddress.City);
			AssertEquals("My Code", wrapper.ConsigneeAddress.PostCode);
			AssertEquals("My State", wrapper.ConsigneeAddress.State);
			AssertEquals("AU", wrapper.ConsigneeAddress.Country.Code);

			docket.ConsigneeDocAddress.E2_AddressOverride = true;
			docket.ConsigneeDocAddress.E2_CompanyName = "Name Override";
			docket.ConsigneeDocAddress.E2_Address1 = "Address 1 Override";
			docket.ConsigneeDocAddress.E2_Address2 = "Address 2 Override";
			docket.ConsigneeDocAddress.E2_City = "City Override";
			docket.ConsigneeDocAddress.E2_Postcode = "Code";
			docket.ConsigneeDocAddress.E2_State = "State";
			docket.ConsigneeDocAddress.E2_RN_NKCountryCode = "US";

			AssertEquals("Name Override", wrapper.ConsigneeAddress.CompanyName);
			AssertEquals("Address 1 Override", wrapper.ConsigneeAddress.Address1);
			AssertEquals("Address 2 Override", wrapper.ConsigneeAddress.Address2);
			AssertEquals("City Override", wrapper.ConsigneeAddress.City);
			AssertEquals("Code", wrapper.ConsigneeAddress.PostCode);
			AssertEquals("State", wrapper.ConsigneeAddress.State);
			AssertEquals("US", wrapper.ConsigneeAddress.Country.Code);
		}

		#endregion

		#region TestConsigneeName

		protected override void TestConsigneeNameCore(T docket, TWrapper wrapper)
		{
			base.TestConsigneeNameCore(docket, wrapper);

			docket.ConsigneeDocAddress.E2_AddressOverride = false;
			docket.ConsigneePK = ZGuid.Empty;
			AssertEquals(ZString.Empty, wrapper.ConsigneeName);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "TEST NAME1";
			docket.ConsigneePK = consignee.PK;
			AssertEquals("TEST NAME1", wrapper.ConsigneeName);

			docket.ConsigneeDocAddress.E2_AddressOverride = true;
			docket.ConsigneeDocAddress.E2_CompanyName = "TEST NAME2";
			AssertEquals("TEST NAME2", wrapper.ConsigneeName);
		}

		#endregion

		#region TestDestinationPort

		protected override void TestDestinationPortCore(T docket, TWrapper wrapper)
		{
			base.TestDestinationPortCore(docket, wrapper);

			docket.ConsigneeAddressPK = ZGuid.Empty;
			AssertEquals(ZString.Empty, wrapper.DestinationPort);

			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			var port = Factory.New<RefUNLOCO>();
			var country = Factory.New<RefCountry>();
			port.RL_Code = "TEST";
			port.RL_RN_NKCountryCode = country.Code;

			docket.ConsigneePK = org.PK;
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = port.RL_Code;
			docket.ConsigneeDocAddress.E2_AddressOverride = false;

			docket.ConsigneeAddressPK = address.PK;
			AssertEquals("TEST", wrapper.DestinationPort);

			docket.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals(ZString.Empty, wrapper.DestinationPort);
		}

		#endregion

		#region TestDockDoorLocation

		public void TestDockDoorLocation()
		{
			TestDockDoorLocationCore();
		}

		protected abstract void TestDockDoorLocationCore();

		#endregion

		#region TestLoadedPackagesTotals

		public void TestLoadedPackagesTotals_LoadedPackages()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 70m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);
			order.WD_TotalWeightUnit = Constants.Weight.Grams;
			order.WD_TotalCubicUnit = Constants.Volume.CubicMetres;

			var pick = Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);
			Factory.Save();

			var orderWrapper = DocWhsOrder.New(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport.CommonLoadVolumeUQ = Constants.Volume.Litre;

			AssertEquals("Order - TotalLoadedPackages", "2", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "10", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 10m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedWeight", Constants.Weight.Kilograms, orderWrapper.TotalLoadedWeight.Unit.Code);
			AssertEquals("Order - TotalLoadedVolume", 10m, orderWrapper.TotalLoadedVolume.Value);
			AssertEquals("Order - TotalLoadedVolume", Constants.Volume.Litre, orderWrapper.TotalLoadedVolume.Unit.Code);
		}

		public void TestLoadedPackagesTotals_NoLoadedPackages()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 70m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);

			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: false);
			PreparePkgPackagePivot(package2, load, isLoaded: false);

			Factory.Save();

			var orderWrapper = DocWhsOrder.New(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport.CommonLoadVolumeUQ = Constants.Volume.Litre;

			AssertEquals("Order - TotalLoadedPackages", "0", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "0", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 0m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 0m, orderWrapper.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_MixedLoad()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 20m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order1, part, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O2", part, 5m);
			Helper.CreateWhsOrderLine(order2, part, 5m);

			order1.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order1.WD_TotalCubicUnit = Constants.Volume.Litre;

			order2.WD_TotalWeightUnit = Constants.Weight.Grams;
			order2.WD_TotalCubicUnit = Constants.Volume.CubicCentimeters;

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var packingHelper = new PackingTestHelper(Factory);

			var pick1Lines = pick1.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order1.PackageJob.PK, "", 5, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order1.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pick1Lines[0]);
			packingHelper.CreatePackageDivot(package2, pick1Lines[1]);

			var pick2Lines = pick2.GetAllPickLines().ToArray();
			var package3 = (PkgPackage)packingHelper.CreatePackage(order2.PackageJob.PK, "", 5, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package4 = (PkgPackage)packingHelper.CreatePackage(order2.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package3, pick2Lines[0]);
			packingHelper.CreatePackageDivot(package4, pick2Lines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: false);

			PreparePkgPackagePivot(package3, load, isLoaded: false);
			PreparePkgPackagePivot(package4, load, isLoaded: true);

			Factory.Save();

			var orderWrapper1 = DocWhsOrder.New(order1, Factory);
			((ILoadingSupport)orderWrapper1).LoadPK = load.PK;
			var loadingSupport1 = (ILoadingSupport)orderWrapper1;
			loadingSupport1.LoadPK = load.PK;
			loadingSupport1.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport1.CommonLoadVolumeUQ = Constants.Volume.Litre;

			AssertEquals("Order1 - TotalLoadedPackages", "1", orderWrapper1.TotalLoadedPackages.Value);
			AssertEquals("Order1 - TotalLoadedUnits", "5", orderWrapper1.TotalLoadedUnits.Value);
			AssertEquals("Order1 - TotalLoadedWeight", 5m, orderWrapper1.TotalLoadedWeight.Value);
			AssertEquals("Order1 - TotalLoadedVolume", 5m, orderWrapper1.TotalLoadedVolume.Value);

			var orderWrapper2 = DocWhsOrder.New(order2, Factory);
			var loadingSupport2 = (ILoadingSupport)orderWrapper2;
			loadingSupport2.LoadPK = load.PK;
			loadingSupport2.CommonLoadWeightUQ = Constants.Weight.Grams;
			loadingSupport2.CommonLoadVolumeUQ = Constants.Volume.CubicCentimeters;

			AssertEquals("Order2 - TotalLoadedPackages", "1", orderWrapper2.TotalLoadedPackages.Value);
			AssertEquals("Order2 - TotalLoadedUnits", "5.000", orderWrapper2.TotalLoadedUnits.Value);
			AssertEquals("Order2 - TotalLoadedWeight", 5000m, orderWrapper2.TotalLoadedWeight.Value);
			AssertEquals("Order2 - TotalLoadedVolume", 5000m, orderWrapper2.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_InvalidUQs()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 20m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);

			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);

			Factory.Save();

			var orderWrapper = DocWhsOrder.New(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadWeightUQ = "AB";
			loadingSupport.CommonLoadVolumeUQ = "CD";

			AssertEquals("Order - TotalLoadedPackages", "2", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "10", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 0m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 0m, orderWrapper.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_MultipleDivots()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 20m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(org, whs, "O1");
			Helper.CreateWhsOrderLine(order, part, 3m);
			Helper.CreateWhsOrderLine(order, part, 5m);
			Helper.CreateWhsOrderLine(order, part, 7m);
			Helper.CreateWhsOrderLine(order, part, 5m);

			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 3, "UNT", 3m, Constants.Volume.Litre, 3000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			var package3 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 7, "UNT", 7m, Constants.Volume.Litre, 7000m, Constants.Weight.Grams);
			var package4 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);
			packingHelper.CreatePackageDivot(package3, pickLines[2]);
			packingHelper.CreatePackageDivot(package4, pickLines[3]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);
			PreparePkgPackagePivot(package3, load, isLoaded: true);
			PreparePkgPackagePivot(package4, load, isLoaded: true);

			Factory.Save();

			var orderWrapper = DocWhsOrder.New(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport.CommonLoadVolumeUQ = Constants.Volume.Litre;
			AssertEquals("Order - TotalLoadedPackages", "4", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "20", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 20m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 20m, orderWrapper.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_LoadedPackagesOnMultipleLoads()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 70m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);
			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			var package3 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 1m, Constants.Volume.Litre, 1m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);
			packingHelper.CreatePackageDivot(package3, pickLines[2]);

			var load1 = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load1, isLoaded: true);
			PreparePkgPackagePivot(package2, load1, isLoaded: true);
			var load2 = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package3, load2, isLoaded: true);
			Factory.Save();

			var orderWrapper1 = DocWhsOrder.New(order, Factory);
			var loadingSupport1 = (ILoadingSupport)orderWrapper1;
			loadingSupport1.LoadPK = load1.PK;
			loadingSupport1.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport1.CommonLoadVolumeUQ = Constants.Volume.Litre;

			AssertEquals("Order - TotalLoadedPackages", "2", orderWrapper1.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "10", orderWrapper1.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 10m, orderWrapper1.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 10m, orderWrapper1.TotalLoadedVolume.Value);

			var orderWrapper2 = DocWhsOrder.New(order, Factory);
			var loadingSupport2 = (ILoadingSupport)orderWrapper2;
			loadingSupport2.LoadPK = load2.PK;
			loadingSupport2.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport2.CommonLoadVolumeUQ = Constants.Volume.Litre;
			AssertEquals("Order - TotalLoadedPackages", "1", orderWrapper2.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "5", orderWrapper2.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 1m, orderWrapper2.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 1m, orderWrapper2.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_LoadedPackages_NoLoadPK()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 70m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);
			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);
			Factory.Save();

			var orderWrapper = DocWhsOrder.New(order, Factory);
			AssertEquals("Order - TotalLoadedPackages", "0", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "0", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 0m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 0m, orderWrapper.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_NoCommonUnit_NoWeightUQ()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 20m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);

			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);

			Factory.Save();

			var orderWrapper = DocWhsOrder.New(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadVolumeUQ = Constants.Volume.Litre;

			AssertEquals("Order - TotalLoadedPackages", "2", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "10", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 0m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 10m, orderWrapper.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_NoCommonUnit_NoVolumeUQ()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 20m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);

			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);

			Factory.Save();

			var orderWrapper = DocWhsOrder.New(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadWeightUQ = Constants.Weight.Kilograms;

			AssertEquals("Order - TotalLoadedPackages", "2", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "10", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 10m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 0m, orderWrapper.TotalLoadedVolume.Value);
		}

		void PreparePkgPackagePivot(PkgPackage package, WhsLoad load, bool isLoaded)
		{
			var pivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			if (isLoaded)
			{
				pivot.WLP_LoadedTime = ZDateTimeOffset.Now;
				pivot.WLP_GS_NKLoadingUser = "E";
			}
		}

		#endregion

		#endregion

		#region Implementation

		#region Helper

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			PickableDocket = Docket;
			PickableDocketWrapper = DocketWrapper;
		}

		protected T PickableDocket;
		protected TWrapper PickableDocketWrapper;

		#endregion
	}
}
