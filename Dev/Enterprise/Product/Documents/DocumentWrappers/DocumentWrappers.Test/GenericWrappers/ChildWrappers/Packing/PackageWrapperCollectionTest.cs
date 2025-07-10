using System.ComponentModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageWrapperCollection))]
	internal class PackageWrapperCollectionTest : GenericWrapperCollectionTest<PackageWrapperCollection>
	{
		public void TestLoadFromContainer()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			PackLine packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_ActualWeight = 123;
			packLine1.JL_JC = container.PK;
			PackLine packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 456;
			packLine2.JL_JC = container.PK;
			PackLine packLine3 = Factory.New<PackLine>();
			packLine3.JL_JC = container.PK;

			PackageWrapperCollection collection = new PackageWrapperCollection(container, Factory);
			AssertEquals("Collection should only include PackLines attached to a shipment", 2, collection.Count);
			AssertEquals("collection[0].Weight.Value", new ZDecimal(123), collection[0].Weight.Value);
			AssertEquals("collection[1].Weight.Value", new ZDecimal(456), collection[1].Weight.Value);
		}

		public void TestIncludingDuplicatesFromConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_UniqueConsignRef = "S00009999";
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			ForwardingShipment subShipment = consol.Shipments.AddNew();
			subShipment.JS_UniqueConsignRef = "S00007777";
			subShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			masterShipment.CoLoadShipments.Add(subShipment);

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT11111111";

			PackLine packLine1 = subShipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;
			packLine1.JL_Description = "this is packline 1";

			masterShipment.OuterPackLines.Load();

			PackageWrapperCollection collection = new PackageWrapperCollection(consol, true, false, Factory);
			AssertEquals("collection.Count should be 2, PackLines attached to a subshipment should also be included on the master", 2, collection.Count);
			AssertEquals("collection[0].ContainerNo", "CONT11111111", collection[0].ContainerNo);
			AssertEquals("collection[0].Description", "this is packline 1", collection[0].Description);
			AssertEquals("collection[1].ContainerNo", "CONT11111111", collection[1].ContainerNo);
			AssertEquals("collection[1].Description", "this is packline 1", collection[1].Description);
			AssertContainsExactElementsInAnyOrder(".Parent.JobNumbers", new ZString[] { "S00007777", "S00009999" }, collection.Select(item => ((PackageWrapper)item).Parent.JobNumber));
		}

		public void TestIncludeMasterShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			var masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_UniqueConsignRef = "S00009999";
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var subShipment1 = consol.Shipments.AddNew();
			subShipment1.JS_UniqueConsignRef = "S00008888";
			subShipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			var subShipment2 = consol.Shipments.AddNew();
			subShipment2.JS_UniqueConsignRef = "S00007777";
			subShipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			masterShipment.CoLoadShipments.Add(subShipment2);
			masterShipment.CoLoadShipments.Add(subShipment1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S000066666";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var subPackLine1 = subShipment1.OuterPackLines.AddNew();
			var subPackLine2 = subShipment2.OuterPackLines.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();

			masterShipment.OuterPackLines.Load();

			var collection = new PackageWrapperCollection(consol, false, true, Factory);
			AssertContainsExactElementsInAnyOrder(".Parent.JobNumbers", new ZString[] { "S000066666", "S00009999" }, collection.Select(item => ((PackageWrapper)item).Parent.JobNumber).Distinct());
		}

		public void TestSetShipmentWithMasterAndSubs()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			CommonShipment subShipment1 = masterShipment.CoLoadShipments.AddNew();
			CommonShipment subShipment2 = masterShipment.CoLoadShipments.AddNew();

			masterShipment.JS_UniqueConsignRef = "masterShipment";
			subShipment1.JS_UniqueConsignRef = "subShipment1";
			subShipment2.JS_UniqueConsignRef = "subShipment2";

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			PackLine subPackLine1 = subShipment1.OuterPackLines.AddNew();
			PackLine subPackLine2 = subShipment2.OuterPackLines.AddNew();

			CommonConsol consol = Factory.New<CommonConsol>();

			consol.Shipments.Add(masterShipment);
			consol.Shipments.Add(subShipment1);
			consol.Shipments.Add(subShipment2);

			masterShipment.OuterPackLines.Load();
			PackageWrapperCollection col = new PackageWrapperCollection(consol, Factory);
			AssertEquals(2, col.Count);
		}

		public void TestShipmentsAreSortedByHAWB()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment1 = Factory.New<CommonShipment>();
			CommonShipment shipment2 = Factory.New<CommonShipment>();
			CommonShipment shipment3 = Factory.New<CommonShipment>();

			PackLine line1 = shipment1.OuterPackLines.AddNew();
			PackLine line2 = shipment2.OuterPackLines.AddNew();
			PackLine line3 = shipment3.OuterPackLines.AddNew();

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.Shipments.Add(shipment3);

			shipment2.JS_HouseBill = "A";
			shipment3.JS_HouseBill = "B";
			shipment1.JS_HouseBill = "C";

			PackageWrapperCollection col = new PackageWrapperCollection(consol, Factory);

			AssertEquals("A", col[0].HouseBill);
			AssertEquals("B", col[1].HouseBill);
			AssertEquals("C", col[2].HouseBill);
		}

		public void TestPackagesSummaryDangerousOnly()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine package1 = shipment.OuterPackLines.AddNew();
			ForwardingPackLine package2 = shipment.OuterPackLines.AddNew();
			ForwardingPackLine package3 = shipment.OuterPackLines.AddNew();
			ForwardingPackLine package4 = shipment.OuterPackLines.AddNew();

			PackageWrapperCollection collection = new PackageWrapperCollection(shipment, Factory);

			AssertEquals("Should be empty string", "", collection.PackagesSummaryDangerousOnly);

			package1.JL_RH_NKCommodityCode = "GEN";
			package2.JL_RH_NKCommodityCode = "HAZ";
			package3.JL_RH_NKCommodityCode = "";
			package4.JL_RH_NKCommodityCode = "SEED";

			package2.UNDGs.TryGetOrCreate("2478a", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			package3.UNDGs.TryGetOrCreate("3208b", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			package4.UNDGs.TryGetOrCreate("1051", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);

			package3.UNDGs[0].DI_MPMarinePollutant = "T";

			AssertEquals("Should be loaded with specific Hazardous Goods in specific format",
					"HAZ (HAZARDOUS GOODS) - UN2478, ISOCYANATES, FLAMMABLE, TOXIC, N.O.S., class 3 (6.1), PG II\r\n" +
					"UN3208, METALLIC SUBSTANCE, WATER-REACTIVE, N.O.S., class 4.3, PG II, MARINE POLLUTANT\r\n" +
					"SEED (SEEDS) - UN1051, HYDROGEN CYANIDE, STABILIZED, class 6.1 (3), PG I, (-18.0C c.c.), MARINE POLLUTANT",
				collection.PackagesSummaryDangerousOnly);
		}

		public void TestPackagesSummaryAll()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine package1 = shipment.OuterPackLines.AddNew();
			ForwardingPackLine package2 = shipment.OuterPackLines.AddNew();
			ForwardingPackLine package3 = shipment.OuterPackLines.AddNew();
			ForwardingPackLine package4 = shipment.OuterPackLines.AddNew();
			ForwardingPackLine package5 = shipment.OuterPackLines.AddNew();
			ForwardingPackLine package6 = shipment.OuterPackLines.AddNew();
			ForwardingPackLine package7 = shipment.OuterPackLines.AddNew();

			package1.JL_RH_NKCommodityCode = "EFRT";
			package2.JL_RH_NKCommodityCode = "SEED";
			package3.JL_RH_NKCommodityCode = "ZINC";
			package4.JL_RH_NKCommodityCode = "HAZ";
			package5.JL_RH_NKCommodityCode = "SEED";
			package6.JL_RH_NKCommodityCode = "SEED";
			package7.JL_RH_NKCommodityCode = "";

			package4.UNDGs.TryGetOrCreate("2478a", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			package6.UNDGs.TryGetOrCreate("3208b", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);

			package6.UNDGs[0].DI_MPMarinePollutant = "T";

			package1.JL_HarmonisedCode = "HAR000";
			package3.JL_HarmonisedCode = "HAR123";
			package6.JL_HarmonisedCode = "HAR456";

			PackageWrapperCollection collection = new PackageWrapperCollection(shipment, Factory);
			AssertEquals("Should be loaded with specific Goods in specific format",
					"HAZ (HAZARDOUS GOODS) - UN2478, ISOCYANATES, FLAMMABLE, TOXIC, N.O.S., class 3 (6.1), PG II\r\n" +
					"SEED (SEEDS) - HAR456 - UN3208, METALLIC SUBSTANCE, WATER-REACTIVE, N.O.S., class 4.3, PG II, MARINE POLLUTANT\r\n" +
					"EFRT (FRUIT) - HAR000\r\n" +
					"SEED (SEEDS)\r\n" +
					"ZINC - HAR123",
				collection.PackagesSummaryAll);
		}

		#region TestLoadFromPkgPackages

		public void TestLoadFromPkgPackages()
		{
			LoadFromPkgPackagesCore(true);
		}

		public void TestLoadFromPkgPackages_IgnorePackageID()
		{
			LoadFromPkgPackagesCore(false);
		}

		protected virtual void LoadFromPkgPackagesCore(bool checkPackageID)
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			Factory.Save();

			var package1 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "789", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "", packageJob.Packages);

			var collection = new PackageWrapperCollection(new[] { packageJob }, PackageWrapperCollection.PackLevel.All, PackageWrapperCollection.PackSelection.All, Factory, checkPackageID);
			AssertContainsExactElementsInAnyOrder("collection", checkPackageID ? new[] { package1, package3 } : new[] { package1, package2, package3, package4 }, collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		#endregion

		#region TestWhsOrderLoading

		public void TestLoadFromWhsOrderDoesNotAddPackagesWithEmptyIDs()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			Factory.Save();

			var package1 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "789", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "", packageJob.Packages);
			var packageJobs = new PkgPackageJob[] { packageJob };
			var collection = new PackageWrapperCollection(packageJobs, PackageWrapperCollection.PackLevel.Second, PackageWrapperCollection.PackSelection.All, Factory);

			AssertContainsExactElementsInAnyOrder("collection",
				new PkgPackage[] { package1 }, collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			package3.KP_PackageID = "";

			var package5 = Helper.CreatePackage("KEG", "456", package3.Packages);

			collection = new PackageWrapperCollection(packageJobs, PackageWrapperCollection.PackLevel.All, PackageWrapperCollection.PackSelection.All, Factory);

			AssertContainsExactElementsInAnyOrder("collection",
				new PkgPackage[] { package1, package5 }, collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		public void TestLoadFromWhsOrderWithBasicLabels()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			Factory.Save();

			var package1 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "456", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "789", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "123", packageJob.Packages);

			var collection = new PackageWrapperCollection(new PkgPackageJob[] { packageJob }, PackageWrapperCollection.PackLevel.Second, PackageWrapperCollection.PackSelection.All, Factory);

			AssertContainsExactElementsInAnyOrder("collection",
				new PkgPackage[] { package1, package2, package4 }, collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		public void TestLoadFromWhsOrderWithBasicLabelsForAllLevels()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			Factory.Save();

			var package1 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "456", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "789", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "123", packageJob.Packages);

			var collection = new PackageWrapperCollection(new PkgPackageJob[] { packageJob }, PackageWrapperCollection.PackLevel.All, PackageWrapperCollection.PackSelection.All, Factory);

			AssertContainsExactElementsInAnyOrder("collection",
				new PkgPackage[] { package1, package2, package3, package4 }, collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		#endregion

		#region TestWhsPickLoading

		public void TestLoadFromWhsPickDoesNotAddPackagesWithEmptyIDs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);

			Factory.Save();

			var order1package1 = Helper.CreatePackage("KEG", "123", packageJob1.Packages);
			var order1package2 = Helper.CreatePackage("KEG", "456", order1package1.Packages);
			var order1package3 = Helper.CreatePackage("KEG", "", order1package2.Packages);
			var order1package4 = Helper.CreatePackage("KEG", "123", packageJob1.Packages);

			var order2package1 = Helper.CreatePackage("KEG", "123", packageJob2.Packages);
			var order2package2 = Helper.CreatePackage("KEG", "", order2package1.Packages);
			var order2package3 = Helper.CreatePackage("KEG", "789", order2package2.Packages);
			var order2package4 = Helper.CreatePackage("KEG", "", packageJob2.Packages);
			var packageJobs = pick.Orders.Cast<WhsOrder>().Select(order => order.PackageJob);
			var collection = new PackageWrapperCollection(packageJobs, PackageWrapperCollection.PackLevel.Second, PackageWrapperCollection.PackSelection.All, Factory);

			AssertContainsExactElementsInAnyOrder("collection",
				new PkgPackage[] { order1package1, order1package2, order1package4, order2package1 },
				collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			collection = new PackageWrapperCollection(packageJobs, PackageWrapperCollection.PackLevel.All, PackageWrapperCollection.PackSelection.All, Factory);

			AssertContainsExactElementsInAnyOrder("collection",
				new PkgPackage[] { order1package1, order1package2, order1package4, order2package1, order2package3 },
				collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		public void TestLoadFromWhsPickWithBasicLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);

			Factory.Save();

			var order1package1 = Helper.CreatePackage("KEG", "123", packageJob1.Packages);
			var order1package2 = Helper.CreatePackage("KEG", "456", order1package1.Packages);
			var order1package3 = Helper.CreatePackage("KEG", "789", order1package2.Packages);
			var order1package4 = Helper.CreatePackage("KEG", "123", packageJob1.Packages);

			var order2package1 = Helper.CreatePackage("KEG", "123", packageJob2.Packages);
			var order2package2 = Helper.CreatePackage("KEG", "456", order2package1.Packages);
			var order2package3 = Helper.CreatePackage("KEG", "789", order2package2.Packages);
			var order2package4 = Helper.CreatePackage("KEG", "123", packageJob2.Packages);

			var collection = new PackageWrapperCollection(pick.Orders.Cast<WhsOrder>().Select(order => order.PackageJob),
				PackageWrapperCollection.PackLevel.Second, PackageWrapperCollection.PackSelection.All, Factory);

			AssertContainsExactElementsInAnyOrder("collection",
				new PkgPackage[] { order1package1, order1package2, order1package4, order2package1, order2package2, order2package4 },
				collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		public void TestLoadFromWhsPickWithBasicLabelForAllLevels()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);

			Factory.Save();

			var order1package1 = Helper.CreatePackage("KEG", "123", packageJob1.Packages);
			var order1package2 = Helper.CreatePackage("KEG", "456", order1package1.Packages);
			var order1package3 = Helper.CreatePackage("KEG", "789", order1package2.Packages);
			var order1package4 = Helper.CreatePackage("KEG", "123", packageJob1.Packages);

			var order2package1 = Helper.CreatePackage("KEG", "123", packageJob2.Packages);
			var order2package2 = Helper.CreatePackage("KEG", "456", order2package1.Packages);
			var order2package3 = Helper.CreatePackage("KEG", "789", order2package2.Packages);
			var order2package4 = Helper.CreatePackage("KEG", "123", packageJob2.Packages);

			var collection = new PackageWrapperCollection(pick.Orders.Cast<WhsOrder>().Select(order => order.PackageJob),
				PackageWrapperCollection.PackLevel.All, PackageWrapperCollection.PackSelection.All, Factory);

			AssertContainsExactElementsInAnyOrder("collection",
				new PkgPackage[] { order1package1, order1package2, order1package3, order1package4, order2package1, order2package2, order2package3, order2package4 },
				collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		#endregion

		#region TestPkgPackageJobLoading

		public void TestLoadFromPackageJobDoesNotAddPackagesWithEmptyIDs()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			Factory.Save();

			var package1 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "789", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "", packageJob.Packages);
			packageJob.Selected.UpdateSelectedPackages(new[] { package1, package4 });

			var collection1 = new PackageWrapperCollection(new PkgPackageJob[] { packageJob }, PackageWrapperCollection.PackLevel.First, PackageWrapperCollection.PackSelection.Selected, Factory);
			AssertContainsExactElementsInAnyOrder("collection", new PkgPackage[] { package1 }, collection1.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			var collection2 = new PackageWrapperCollection(new PkgPackageJob[] { packageJob }, PackageWrapperCollection.PackLevel.All, PackageWrapperCollection.PackSelection.Selected, Factory);
			AssertContainsExactElementsInAnyOrder("collection", new PkgPackage[] { package1, package3 }, collection2.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		public void TestLoadFromPackageJobWithPrintSelected()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			Factory.Save();

			var package1 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "456", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "789", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			packageJob.Selected.UpdateSelectedPackages(new[] { package1, package4 });

			var collection = new PackageWrapperCollection(new PkgPackageJob[] { packageJob }, PackageWrapperCollection.PackLevel.First, PackageWrapperCollection.PackSelection.Selected, Factory);
			AssertContainsExactElementsInAnyOrder("collection", new PkgPackage[] { package1, package4 }, collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		public void TestLoadFromPackageJobWithPrintSelectedAndChildren()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			Factory.Save();

			var package1 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "456", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "789", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			packageJob.Selected.UpdateSelectedPackages(new[] { package1, package4 });

			var collection = new PackageWrapperCollection(new PkgPackageJob[] { packageJob }, PackageWrapperCollection.PackLevel.All, PackageWrapperCollection.PackSelection.Selected, Factory);
			AssertContainsExactElementsInAnyOrder("collection", new PkgPackage[] { package1, package2, package3, package4 }, collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		#endregion

		public void TestLoadFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine package1 = shipment.OuterPackLines.AddNew();
			package1.JL_PackageCount = 12;
			package1.JL_F3_NKPackType = package1.JL_F3_NKPackType_List[0].F3_Code;

			ForwardingPackLine package2 = shipment.OuterPackLines.AddNew();
			package2.JL_PackageCount = 23;
			package2.JL_F3_NKPackType = package2.JL_F3_NKPackType_List[1].F3_Code;

			ForwardingPackLine package3 = shipment.OuterPackLines.AddNew();
			package3.JL_PackageCount = 45;
			package3.JL_F3_NKPackType = package3.JL_F3_NKPackType_List[2].F3_Code;

			PackageWrapperCollection collection = new PackageWrapperCollection(shipment, Factory);

			AssertEquals("collection.Count", 3, collection.Count);
		}

		public void TestPickUpDeliveryConfirmation()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine2.JL_PackageCount = 20;
			packLine3.JL_PackageCount = 30;

			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot1 = packLine1.ConfirmDivots[0];
			CommonConfirmDivot divot2 = packLine2.ConfirmDivots[0];
			CommonConfirmDivot divot3 = packLine3.ConfirmDivots[0];
			divot1.J8_PackagesDelivered = 6;
			divot2.J8_PackagesDelivered = 13;
			divot3.J8_PackagesDelivered = 0;

			PackageWrapperCollection collection = new PackageWrapperCollection(confirm, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
		}

		public void TestLoadFromConsignment()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment = helper.CreateConsignment();
			var package1 = consignment.PackageJob.Packages.AddNew();
			var package2 = consignment.PackageJob.Packages.AddNew();
			var package3 = consignment.PackageJob.Packages.AddNew();

			var collection = new PackageWrapperCollection(consignment, Factory);
			AssertEquals("collection.Count", 3, collection.Count);
		}

		public void TestLoadFromCartage()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			CommonBookedCtgMove move2 = cartage.LooseBookedMoves.AddNew();
			move1.EW_BookedPackCount = 12;
			move2.EW_BookedPackCount = 23;

			PackageWrapperCollection collection = new PackageWrapperCollection(cartage, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
		}

		public void TestLoadFromCFSShipment()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();

			CFSPackLine package1 = shipment.OuterPackLines.AddNew();
			package1.JL_PackageCount = 12;
			package1.JL_F3_NKPackType = package1.JL_F3_NKPackType_List[0].F3_Code;

			CFSPackLine package2 = shipment.OuterPackLines.AddNew();
			package2.JL_PackageCount = 23;
			package2.JL_F3_NKPackType = package2.JL_F3_NKPackType_List[1].F3_Code;

			CFSPackLine package3 = shipment.OuterPackLines.AddNew();
			package3.JL_PackageCount = 45;
			package3.JL_F3_NKPackType = package3.JL_F3_NKPackType_List[2].F3_Code;

			PackageWrapperCollection collection = new PackageWrapperCollection(shipment, Factory);

			AssertEquals("collection.Count", 3, collection.Count);
		}

		public void TestLoadFromAgencyShipment()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentPackLine package1 = shipment.OuterPackLines.AddNew();
			package1.JL_PackageCount = 12;
			package1.JL_F3_NKPackType = package1.JL_F3_NKPackType_List[0].F3_Code;

			AgencyShipmentPackLine package2 = shipment.OuterPackLines.AddNew();
			package2.JL_PackageCount = 23;
			package2.JL_F3_NKPackType = package2.JL_F3_NKPackType_List[1].F3_Code;

			AgencyShipmentPackLine package3 = shipment.OuterPackLines.AddNew();
			package3.JL_PackageCount = 45;
			package3.JL_F3_NKPackType = package3.JL_F3_NKPackType_List[2].F3_Code;

			PackageWrapperCollection collection = new PackageWrapperCollection(shipment, Factory);

			AssertEquals("collection.Count", 3, collection.Count);
		}

		public void TestLoadFromDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_HouseBill = "HOUSECAT";
			declaration.JE_MasterBill = "MASTERCAT";
			Bill houseBill = declaration.PrimaryHouseBill;
			AssertNotNull("Precondition: declaration.PrimaryBill should not be null", houseBill.CU_HouseBill);

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000026";

			BasePackingGroup packingGroup1 = (declaration.PackingGroups.Count > 0) ? declaration.PackingGroups[0] : declaration.PackingGroups.AddNew();
			packingGroup1.CR_CU_HouseBill = houseBill.PK;
			packingGroup1.CR_CO_Container = container.PK;

			BasePackingGroup packingGroup2 = declaration.PackingGroups.AddNew();
			packingGroup2.CR_CU_HouseBill = houseBill.PK;

			BasePackage package1 = (declaration.Packages.Count > 0) ? declaration.Packages[0] : declaration.Packages.AddNew();
			package1.CW_CR_HouseContainer = packingGroup1.PK;
			package1.CW_PackQty = 23;
			package1.CW_PackType = "PK";

			BasePackage package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = packingGroup1.PK;
			package2.CW_PackQty = 24;
			package2.CW_PackType = "PK";

			BasePackage package3 = declaration.Packages.AddNew();
			package3.CW_CR_HouseContainer = packingGroup2.PK;
			package3.CW_PackQty = 25;
			package3.CW_PackType = "PK";

			PackageWrapperCollection collection = new PackageWrapperCollection(declaration, Factory);

			AssertEquals("collection.Count", 3, collection.Count);
		}

		#region TestLoadFromDtbBooking

		public void TestLoadFromDtbBooking()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var packageJob = Factory.New<PkgPackageJob>();
			PkgPackage package1_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Box);
			PkgPackage package2_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Package);
			PkgPackage package3_Instruction2 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage package4_NoInstruction = packageJob.Packages.AddNew(Constants.PkgUnit.Bag);

			// way to determine which package is which and that qty is used from packages rather that from Divots.
			package1_Instruction1.KP_PackageQty = 1;
			package2_Instruction1.KP_PackageQty = 2;
			package3_Instruction2.KP_PackageQty = 3;
			package4_NoInstruction.KP_PackageQty = 4;

			var booking = helper.CreateBooking();
			DtbBookingInstruction instruction1 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instruction2 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instruction3 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstructionPkgDivot instruction1_PkgDivot1 = helper.CreatePackageDivot(instruction1, package1_Instruction1, 1);
			DtbBookingInstructionPkgDivot instruction1_PkgDivot2 = helper.CreatePackageDivot(instruction1, package2_Instruction1, 1);
			DtbBookingInstructionPkgDivot instruction2_PkgDivot1 = helper.CreatePackageDivot(instruction2, package3_Instruction2, 1);

			var packageWrapperCollection = new PackageWrapperCollection(booking, Factory);
			AssertEquals("Wrong number of Packages were created", 3, packageWrapperCollection.Count);
			AssertContainsPackageFor(packageWrapperCollection, package1_Instruction1);
			AssertContainsPackageFor(packageWrapperCollection, package2_Instruction1);
			AssertContainsPackageFor(packageWrapperCollection, package3_Instruction2);
		}

		void AssertContainsPackageFor(PackageWrapperCollection packageWrapperCollection, PkgPackage expectedPackage)
		{
			foreach (PackageWrapper packageWrapper in packageWrapperCollection)
			{
				if (packageWrapper.Packages.Value == expectedPackage.KP_PackageQty)
				{
					return;
				}
			}
			Fail(string.Format("No Packages could be found for package with Qty '{0}'", expectedPackage.KP_PackageQty));
		}

		#endregion

		#region TestLoadFromDtbBookingConsolidation

		public void TestLoadFromDtbBookingConsolidation()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var packageJobA = Factory.New<PkgPackageJob>();
			PkgPackage packageA1_InstructionA1 = packageJobA.Packages.AddNew(Constants.PkgUnit.Box);
			PkgPackage packageA2_InstructionA1 = packageJobA.Packages.AddNew(Constants.PkgUnit.Package);
			PkgPackage packageA3_InstructionA2 = packageJobA.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage packageA4_NoInstruction = packageJobA.Packages.AddNew(Constants.PkgUnit.Bag);

			var packageJobB = Factory.New<PkgPackageJob>();
			PkgPackage packageB1_InstructionB1 = packageJobB.Packages.AddNew(Constants.PkgUnit.Box);
			PkgPackage packageB2_InstructionB1 = packageJobB.Packages.AddNew(Constants.PkgUnit.Package);
			PkgPackage packageB3_InstructionB2 = packageJobB.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage packageB4_NoInstruction = packageJobB.Packages.AddNew(Constants.PkgUnit.Bag);

			// way to determine which package is which and that qty is used from packages rather that from Divots.
			packageA1_InstructionA1.KP_PackageQty = 1;
			packageA2_InstructionA1.KP_PackageQty = 2;
			packageA3_InstructionA2.KP_PackageQty = 3;
			packageA4_NoInstruction.KP_PackageQty = 4;
			packageB1_InstructionB1.KP_PackageQty = 5;
			packageB2_InstructionB1.KP_PackageQty = 6;
			packageB3_InstructionB2.KP_PackageQty = 7;
			packageB4_NoInstruction.KP_PackageQty = 8;

			var bookingConsolidation = helper.CreateConsolidation();
			var bookingA = helper.CreateBooking(bookingConsolidation);
			var bookingB = helper.CreateBooking(bookingConsolidation);

			DtbBookingInstruction instructionA1 = helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionA2 = helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionA3 = helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstructionPkgDivot instructionA1_PkgDivotA1 = helper.CreatePackageDivot(instructionA1, packageA1_InstructionA1, 1);
			DtbBookingInstructionPkgDivot instructionA1_PkgDivotA2 = helper.CreatePackageDivot(instructionA1, packageA2_InstructionA1, 1);
			DtbBookingInstructionPkgDivot instructionA2_PkgDivotA1 = helper.CreatePackageDivot(instructionA2, packageA3_InstructionA2, 1);

			DtbBookingInstruction instructionB1 = helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionB2 = helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionB3 = helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstructionPkgDivot instructionB1_PkgDivotB1 = helper.CreatePackageDivot(instructionB1, packageB1_InstructionB1, 1);
			DtbBookingInstructionPkgDivot instructionB1_PkgDivotB2 = helper.CreatePackageDivot(instructionB1, packageB2_InstructionB1, 1);
			DtbBookingInstructionPkgDivot instructionB2_PkgDivotB1 = helper.CreatePackageDivot(instructionB2, packageB3_InstructionB2, 1);

			var packageWrapperCollection = new PackageWrapperCollection(bookingConsolidation, Factory);
			AssertEquals("Wrong number of Packages were created", 6, packageWrapperCollection.Count);
			AssertContainsPackageFor(packageWrapperCollection, packageA1_InstructionA1);
			AssertContainsPackageFor(packageWrapperCollection, packageA2_InstructionA1);
			AssertContainsPackageFor(packageWrapperCollection, packageA3_InstructionA2);
			AssertContainsPackageFor(packageWrapperCollection, packageB1_InstructionB1);
			AssertContainsPackageFor(packageWrapperCollection, packageB2_InstructionB1);
			AssertContainsPackageFor(packageWrapperCollection, packageB3_InstructionB2);
		}

		#endregion

		#region TestLoadFromPkgPackageJob

		public void TestLoadFromPkgPackageJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Factory.Save();

			var pick = helper.CreatePickNew(order);

			// So we have
			// PackageJob
			//     1x Container
			//         2x Part1 (PackableItem)
			//         2x Box
			//         4x Crate
			//             3x Part1 (PackableItem)
			//             9x Part2 (PackableItem)
			//     5x Pallet
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			// Level 1
			var container = packageJob.Packages.AddNew(Constants.PkgUnit.Container, 1);
			var pallet = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, 5);

			// Level 2
			var crate = container.Packages.AddNew(Constants.PkgUnit.Crate, 4); // adding crate first, but should be sorted to show box first in result.
			var box = container.Packages.AddNew(Constants.PkgUnit.Box, 2);
			var packedItem1 = container.Pack_ForTesting(orderLine1.ReleaseLines[0], 2);

			// Level 3
			var packedItem2 = crate.Pack_ForTesting(orderLine1.ReleaseLines[0], 3);
			var packedItem3 = crate.Pack_ForTesting(orderLine2.ReleaseLines[0], 5);
			var packedItem4 = crate.Pack_ForTesting(orderLine2.ReleaseLines[0], 4);
			AssertEquals("PackedItems should be same - Divots should be grouped together.", packedItem3, packedItem4);

			// To avoid intermittent test failures.
			packageJob.Packages.ApplySort(PkgPackageSchema.KP_F3_NKPackType.Name, ListSortDirection.Ascending);
			packageJob.Packages[0].Packages.ApplySort(PkgPackageSchema.KP_F3_NKPackType.Name, ListSortDirection.Ascending);
			packageJob.Packages[0].Packages[1].PackedItemDivots.ApplySort(PkgPackageItemDivotSchema.Constants.KI_PackedQty, ListSortDirection.Ascending);

			var packagesWrapper = new PackageWrapperCollection(order.PackageJob, Factory);
			AssertEquals(5, packagesWrapper.Count);
			AssertDisplayOrderAndIndent(packagesWrapper[0], "001", "", container, packedItem1);
			AssertDisplayOrderAndIndent(packagesWrapper[1], "002", new string(' ', 6), box, null);
			AssertDisplayOrderAndIndent(packagesWrapper[2], "003", new string(' ', 6), crate, packedItem2);
			AssertDisplayOrderAndIndent(packagesWrapper[3], "003", new string(' ', 6), crate, packedItem3);
			AssertDisplayOrderAndIndent(packagesWrapper[4], "004", "", pallet, null);
		}

		void AssertDisplayOrderAndIndent(PackageWrapper packageWrapper, ZString expectedDisplayOrder, ZString expectedIndent, PkgPackage expectedPackage, params PkgPackageItemDivotsWrapper[] expectedPackedItems)
		{
			string message = string.Format("Package wrapper ({0}) {1}", packageWrapper.Packages.ValueAndUnitCode, packageWrapper.RefNumber);
			AssertEquals(message + " has a wrong display order.", expectedDisplayOrder, packageWrapper.DisplayOrder);
			AssertEquals(message + " has a wrong indent.", expectedIndent, packageWrapper.Indent);
			AssertEquals(message + " wraps wrong package.", expectedPackage.PK, packageWrapper.WrappedObjectPK);
			if (expectedPackedItems == null || expectedPackedItems.Length == 0)
			{
				AssertNull("No PackableItemParent should be wrapped.", packageWrapper.PackedItem.WrappedObject);
				AssertEquals("No Packed Items should be wrapped.", 0, packageWrapper.PackedItem.PackedItems.Count());
			}
			else
			{
				var packableItemParent = expectedPackedItems.First().PackableItemParent;
				AssertEquals(message + " wraps wrong packed item.", packableItemParent, packageWrapper.PackedItem.WrappedObject);
				AssertContainsExactElementsInAnyOrder(message + " wraps wrong packed item.", expectedPackedItems, packageWrapper.PackedItem.PackedItems);
			}
		}

		#endregion

		public void TestTotalWeightFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine package1 = shipment.OuterPackLines.AddNew();
			package1.JL_ActualWeight = 12.56m;
			package1.JL_ActualWeightUQ = "KG";

			ForwardingPackLine package2 = shipment.OuterPackLines.AddNew();
			package2.JL_ActualWeight = 23.89m;
			package2.JL_ActualWeightUQ = "G";

			ForwardingPackLine package3 = shipment.OuterPackLines.AddNew();
			package3.JL_ActualWeight = 45.34m;
			package3.JL_ActualWeightUQ = "LT";

			var collection = (IBODocDataProviderCollection)(new PackageWrapperCollection(shipment, Factory));
			AssertEquals("Total Weight", "29.506 KG", collection.Total("Weight", "3", null));
		}

		public void TestTotalVolumeFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine package1 = shipment.OuterPackLines.AddNew();
			package1.JL_ActualVolume = 29.45m;
			package1.JL_ActualVolumeUQ = "TE";

			ForwardingPackLine package2 = shipment.OuterPackLines.AddNew();
			package2.JL_ActualVolume = 34.90m;
			package2.JL_ActualVolumeUQ = "M3";

			ForwardingPackLine package3 = shipment.OuterPackLines.AddNew();
			package3.JL_ActualVolume = 80.12m;
			package3.JL_ActualVolumeUQ = "D3";

			var collection = (IBODocDataProviderCollection)(new PackageWrapperCollection(shipment, Factory));
			AssertEquals("Total Volume", "38.73 M3", collection.Total("Volume", "2", null));
		}

		public void TestTotalPackagesFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine package1 = shipment.OuterPackLines.AddNew();
			package1.JL_PackageCount = 42;
			package1.JL_F3_NKPackType = "SHT";

			ForwardingPackLine package2 = shipment.OuterPackLines.AddNew();
			package2.JL_PackageCount = 89;
			package2.JL_F3_NKPackType = "BOT";

			ForwardingPackLine package3 = shipment.OuterPackLines.AddNew();
			package3.JL_PackageCount = 8;
			package3.JL_F3_NKPackType = "CYL";

			var collection = (IBODocDataProviderCollection)new PackageWrapperCollection(shipment, Factory);
			AssertEquals("Total Packs", "139 PKG", collection.Total("Packages", "0", null));
		}

		public void TestTotalPackagesFromDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_HouseBill = "HOUSECAT";
			declaration.JE_MasterBill = "MASTERCAT";
			Bill houseBill = declaration.PrimaryHouseBill;
			AssertNotNull("Precondition: declaration.PrimaryBill should not be null", houseBill.CU_HouseBill);

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000026";

			BasePackingGroup packingGroup1 = declaration.PackingGroups.AddNew();
			packingGroup1.CR_CU_HouseBill = houseBill.PK;
			packingGroup1.CR_CO_Container = container.PK;

			BasePackingGroup packingGroup2 = declaration.PackingGroups.AddNew();
			packingGroup2.CR_CU_HouseBill = houseBill.PK;

			BasePackage package1 = declaration.Packages.AddNew();
			package1.CW_CR_HouseContainer = packingGroup1.PK;
			package1.CW_PackQty = 23;
			package1.CW_PackType = "BX";

			BasePackage package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = packingGroup1.PK;
			package2.CW_PackQty = 24;
			package2.CW_PackType = "BM";

			BasePackage package3 = declaration.Packages.AddNew();
			package3.CW_CR_HouseContainer = packingGroup2.PK;
			package3.CW_PackQty = 25;
			package3.CW_PackType = "PO";

			var collection = (IBODocDataProviderCollection)new PackageWrapperCollection(declaration, Factory);
			AssertEquals("Total Packs", "72 PKG", collection.Total("Packages", "0", null));
		}

		public void TestOutturnTotalPackagesFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine package1 = shipment.OuterPackLines.AddNew();
			package1.JL_Outturn = 58;
			package1.JL_F3_NKPackType = "SHT";

			ForwardingPackLine package2 = shipment.OuterPackLines.AddNew();
			package2.JL_Outturn = 34;
			package2.JL_F3_NKPackType = "BOT";

			ForwardingPackLine package3 = shipment.OuterPackLines.AddNew();
			package3.JL_Outturn = 99;
			package3.JL_F3_NKPackType = "CYL";

			var collection = (IBODocDataProviderCollection)new PackageWrapperCollection(shipment, Factory);
			AssertEquals("Total Outturn Packs", "191 PKG", collection.Total("OutturnedPackages", "0", null));
		}

		public void TestOutturnTotalWeightFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine package1 = shipment.OuterPackLines.AddNew();
			package1.JL_OutturnedWeight = 58.34m;
			package1.JL_ActualWeightUQ = "KG";

			ForwardingPackLine package2 = shipment.OuterPackLines.AddNew();
			package2.JL_OutturnedWeight = 34;
			package2.JL_ActualWeightUQ = "KG";

			ForwardingPackLine package3 = shipment.OuterPackLines.AddNew();
			package3.JL_OutturnedWeight = 99.26;
			package3.JL_ActualWeightUQ = "KG";

			var collection = (IBODocDataProviderCollection)new PackageWrapperCollection(shipment, Factory);
			AssertEquals("Total Outturn Weight", "191.60 KG", collection.Total("OutturnedWeight", "2", null));
		}

		public void TestOutturnTotalVolumeFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine package1 = shipment.OuterPackLines.AddNew();
			package1.JL_OutturnedVolume = 130.5m;
			package1.JL_ActualVolumeUQ = "L";

			ForwardingPackLine package2 = shipment.OuterPackLines.AddNew();
			package2.JL_OutturnedVolume = 34000;
			package2.JL_ActualVolumeUQ = "L";

			ForwardingPackLine package3 = shipment.OuterPackLines.AddNew();
			package3.JL_OutturnedVolume = 250.50;
			package3.JL_ActualVolumeUQ = "L";

			var collection = (IBODocDataProviderCollection)new PackageWrapperCollection(shipment, Factory);
			AssertEquals("Total Outturn Volume", "34381.00 L", collection.Total("OutturnedVolume", "2", null));
		}

		public void TestPillagedTotalFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine package1 = shipment.OuterPackLines.AddNew();
			package1.JL_Pillaged = 38;
			package1.JL_F3_NKPackType = "SHT";

			ForwardingPackLine package2 = shipment.OuterPackLines.AddNew();
			package2.JL_Pillaged = 39;
			package2.JL_F3_NKPackType = "BOT";

			ForwardingPackLine package3 = shipment.OuterPackLines.AddNew();
			package3.JL_Pillaged = 40;
			package3.JL_F3_NKPackType = "CYL";

			var collection = (IBODocDataProviderCollection)new PackageWrapperCollection(shipment, Factory);
			AssertEquals("Total Pillaged Packs", "117 PKG", collection.Total("PillagedPackages", "0", null));
		}

		public void TestDamagedTotalFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine package1 = shipment.OuterPackLines.AddNew();
			package1.JL_Damaged = 25;
			package1.JL_F3_NKPackType = "LOK";

			ForwardingPackLine package2 = shipment.OuterPackLines.AddNew();
			package2.JL_Damaged = 89;
			package2.JL_F3_NKPackType = "LOK";

			ForwardingPackLine package3 = shipment.OuterPackLines.AddNew();
			package3.JL_Damaged = 149;
			package3.JL_F3_NKPackType = "LOK";

			var collection = (IBODocDataProviderCollection)new PackageWrapperCollection(shipment, Factory);
			AssertEquals("Total Damaged Packs", "263 LOK", collection.Total("DamagedPackages", "0", null));
		}

		public void TestPackagesSummaryDangerousOnlyPSAGroup()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HAWB000001";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_RL_NKOrigin = "HKHKG";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_RH_NKCommodityCode = "GEN";

			var collection = new PackageWrapperCollection(shipment, Factory);

			AssertEquals("Should be empty string", "", collection.PackagesSummaryDangerousOnly);

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "2222a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "G";

			var reference1 = Factory.New<UNDGCountryReference>();
			reference1.DCR_HasFlashPointLower = false;
			reference1.DCR_HasFlashPointUpper = true;
			reference1.DCR_FlashPointUpperCentigrade = 13.5m;
			reference1.DCR_RN_NKCountry = Constants.CountryCodes.Singapore;
			reference1.DCR_Type = "PSA";
			reference1.DCR_Code = "1S";
			substance.UNDGCountryReferences.Add(reference1);

			var reference2 = Factory.New<UNDGCountryReference>();
			reference2.DCR_HasFlashPointLower = true;
			reference2.DCR_HasFlashPointUpper = true;
			reference2.DCR_FlashPointLowerCentigrade = 13.5m;
			reference2.DCR_FlashPointUpperCentigrade = 100.1m;
			reference2.DCR_RN_NKCountry = Constants.CountryCodes.Singapore;
			reference2.DCR_Type = "PSA";
			reference2.DCR_Code = "2S";
			substance.UNDGCountryReferences.Add(reference2);

			var undg1 = packLine.UNDGs.AddNew();
			undg1.DI_DG = substance.PK;
			undg1.DI_IsCombustible = true;
			undg1.DI_DGFlashPoint = 5;

			var undg2 = packLine.UNDGs.AddNew();
			undg2.DI_DG = substance.PK;
			undg2.DI_IsCombustible = true;
			undg2.DI_DGFlashPoint = 20;

			AssertEquals("Separate display of different PSA groups",
				"GEN (General) - UN2222, (5C c.c.), PSA Group: 1S\r\n" +
				"GEN (General) - UN2222, (20C c.c.), PSA Group: 2S",
				collection.PackagesSummaryDangerousOnly);
		}

		#region Implementation

		#region Helper

		internal WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion

		protected override PackageWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new PackageWrapperCollection((BaseJobDeclaration)null, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new PackageWrapperFromCustomsPackage(null, Factory);
		}

		#endregion
	}
}
