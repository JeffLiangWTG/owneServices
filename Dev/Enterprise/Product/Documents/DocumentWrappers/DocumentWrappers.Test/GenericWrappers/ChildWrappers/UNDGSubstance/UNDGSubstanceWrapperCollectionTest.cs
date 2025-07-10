using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(UNDGSubstanceWrapperCollection))]
	sealed class UNDGSubstanceWrapperCollectionTest : GenericWrapperCollectionTest<UNDGSubstanceWrapperCollection>
	{
		#region TestConstructors

		#region TestConstructor_WithPackLines

		public void TestConstructor_WithPackLines()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0000011";

			ForwardingShipment shipment = consol.Shipments.AddNew();

			ForwardingPackLine packline1 = shipment.OuterPackLines.AddNew();
			packline1.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3398", "b", "IMO").First().PK;
			packline1.Containers.Add(container);

			ForwardingPackLine packline2 = shipment.OuterPackLines.AddNew();
			packline2.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "a", "IMO").First().PK;
			packline2.Containers.Add(container);

			ForwardingPackLine packline3 = shipment.OuterPackLines.AddNew();
			packline3.Containers.Add(container);

			ContainerWrapper containerWrapper = new ContainerWrapperFromFreight(container, Factory);
			AssertEquals("wrapper.Count", 2, containerWrapper.UNDGSubstances.Count);
			UNDGSubstanceWrapper wrapper1 = containerWrapper.UNDGSubstances[0];
			AssertEquals("wrapper1.UNNumber", "3398", wrapper1.UNNumber);
			AssertEquals("wrapper1.IMOClass", "4.3", wrapper1.IMOClass);
			UNDGSubstanceWrapper wrapper2 = containerWrapper.UNDGSubstances[1];
			AssertEquals("wrapper2.UNNumber", "0014", wrapper2.UNNumber);
			AssertEquals("wrapper2.IMOClass", "1.4S", wrapper2.IMOClass);
		}

		#endregion

		#region TestConstructor_WithBasePackages

		public void TestConstructor_WithBasePackages()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "111";

			BasePackage package1 = declaration.Packages.AddNew();
			package1.CW_ContainerNoOrEquipmentNo = "111";
			package1.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3398", "b", "IMO").First().PK;

			BasePackage package2 = declaration.Packages.AddNew();
			package2.CW_ContainerNoOrEquipmentNo = "111";
			package2.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "a", "IMO").First().PK;

			BasePackage package3 = declaration.Packages.AddNew();
			package3.CW_ContainerNoOrEquipmentNo = "111";

			ContainerWrapper containerWrapper = new ContainerWrapperFromCustoms(container, Factory);
			AssertEquals("wrapper.Count", 2, containerWrapper.UNDGSubstances.Count);
			UNDGSubstanceWrapper wrapper1 = containerWrapper.UNDGSubstances[0];
			AssertEquals("wrapper1.UNNumber", "3398", wrapper1.UNNumber);
			AssertEquals("wrapper1.IMOClass", "4.3", wrapper1.IMOClass);
			UNDGSubstanceWrapper wrapper2 = containerWrapper.UNDGSubstances[1];
			AssertEquals("wrapper2.UNNumber", "0014", wrapper2.UNNumber);
			AssertEquals("wrapper2.IMOClass", "1.4S", wrapper2.IMOClass);
		}

		#endregion

		#region TestConstructor_WithDtbBooking

		public void TestConstructor_WithDtbBooking()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var packageJob = Factory.New<PkgPackageJob>();
			var container_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			var package_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Package);
			var package_Instruction2 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet);
			var package_Instruction2_NoUNDG = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet);
			var package_NoInstruction = packageJob.Packages.AddNew(Constants.PkgUnit.Bag);

			var undgDataItem1_Container_Instruction1 = CreateUNDGDataItem(container_Instruction1, "AAAA", "David1");
			var undgDataItem2_Container_Instruction1 = CreateUNDGDataItem(container_Instruction1, "BBBB", "David2");
			var undgDataItem_Package_Instruction1 = CreateUNDGDataItem(package_Instruction1, "CCCC", "David3");
			var undgDataItem_Package_Instruction2 = CreateUNDGDataItem(package_Instruction2, "DDDD", "David4");
			var undgDataItem_Package_NoInstruction = CreateUNDGDataItem(package_NoInstruction, "EEEE", "David5");

			var consolidation = helper.CreateConsolidation();
			var booking = helper.CreateBooking(consolidation);
			var instruction1 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);
			var instruction2 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);
			var instruction3 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);
			helper.CreatePackageDivot(instruction1, container_Instruction1, 1);
			helper.CreatePackageDivot(instruction1, package_Instruction1, 1);
			helper.CreatePackageDivot(instruction2, package_Instruction2, 1);
			helper.CreatePackageDivot(instruction2, package_Instruction2_NoUNDG, 1);

			var undgSubstanceWrapperCollection = new UNDGSubstanceWrapperCollection(booking, Factory);
			AssertEquals("Wrong number of UNDGs were created", 4, undgSubstanceWrapperCollection.Count);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItem1_Container_Instruction1);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItem2_Container_Instruction1);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItem_Package_Instruction1);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItem_Package_Instruction2);
		}

		public void TestConstructor_WithDtbBooking_UNDGOnChildPackage()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var packageJob = Factory.New<PkgPackageJob>();
			var container = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			var packageWithinContainer = container.Packages.AddNew(Constants.PkgUnit.Pallet);

			var undgDataItem_Package = CreateUNDGDataItem(packageWithinContainer, "AAAA", "Jason1");

			var consolidation = helper.CreateConsolidation();
			var booking = helper.CreateBooking(consolidation);
			var instruction1 = helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "", null);

			helper.CreatePackageDivot(instruction1, container, 1);
			var undgSubstanceWrapperCollection = new UNDGSubstanceWrapperCollection(booking, Factory);
			AssertEquals("Wrong number of UNDGs were created", 1, undgSubstanceWrapperCollection.Count);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItem_Package);
		}

		UNDGDataItem CreateUNDGDataItem(PkgPackage package, ZString undgSubstanceCode, string contactName = "BOB")
		{
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_Code = undgSubstanceCode;
			undgSubstance.DG_UNNO = undgSubstanceCode;

			var orgContact = Factory.New<OrgContact>();
			orgContact.OC_ContactName = contactName;

			var result = package.UNDGs.AddNew();
			result.DI_DG = undgSubstance.PK;
			result.DI_OC_DGContact = orgContact.PK;
			return result;
		}

		void AssertContainsUNDGFor(UNDGSubstanceWrapperCollection allUNDGWrappers, UNDGDataItem undgToFind)
		{
			foreach (UNDGSubstanceWrapper undgWrapper in allUNDGWrappers)
			{
				if (undgToFind.Substance != null && undgWrapper.UNNumber == undgToFind.Substance.DG_Code && undgWrapper.DGContact.FullName == undgToFind.DGContact.OC_ContactName)
				{
					return;
				}
			}
			Fail(string.Format("UNDG with substance code: {0} and contact name: {1} couldn't be found.", undgToFind.Substance?.DG_Code, undgToFind.DGContact.OC_ContactName));
		}

		#endregion

		#region TestConstructor_WithDtbBookingConsolidation

		public void TestConstructor_WithDtbBookingConsolidation()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var packageJobA = Factory.New<PkgPackageJob>();
			PkgPackage containerA_InstructionA1 = packageJobA.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage packageA_InstructionA1 = packageJobA.Packages.AddNew(Constants.PkgUnit.Package);
			PkgPackage packageA_InstructionA2 = packageJobA.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage packageA_InstructionA2_NoUNDG = packageJobA.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage packageA_NoInstruction = packageJobA.Packages.AddNew(Constants.PkgUnit.Bag);

			var packageJobB = Factory.New<PkgPackageJob>();
			PkgPackage containerB_InstructionB1 = packageJobB.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage packageB_InstructionB1 = packageJobB.Packages.AddNew(Constants.PkgUnit.Package);
			PkgPackage packageB_InstructionB2 = packageJobB.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage packageB_InstructionB2_NoUNDG = packageJobB.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage packageB_NoInstruction = packageJobB.Packages.AddNew(Constants.PkgUnit.Bag);

			UNDGDataItem undgDataItemA1_ContainerA_InstructionA1 = CreateUNDGDataItem(containerA_InstructionA1, "AAAA", "David1");
			UNDGDataItem undgDataItemA2_ContainerA_InstructionA1 = CreateUNDGDataItem(containerA_InstructionA1, "BBBB", "David2");
			UNDGDataItem undgDataItemA_PackageA_InstructionA1 = CreateUNDGDataItem(packageA_InstructionA1, "CCCC", "David3");
			UNDGDataItem undgDataItemA_PackageA_InstructionA2 = CreateUNDGDataItem(packageA_InstructionA2, "DDDD", "David4");
			UNDGDataItem undgDataItemA_PackageA_NoInstruction = CreateUNDGDataItem(packageA_NoInstruction, "EEEE", "David5");

			UNDGDataItem undgDataItemB1_ContainerB_InstructionB1 = CreateUNDGDataItem(containerB_InstructionB1, "AAAA", "David1");
			UNDGDataItem undgDataItemB2_ContainerB_InstructionB1 = CreateUNDGDataItem(containerB_InstructionB1, "BBBB", "David2");
			UNDGDataItem undgDataItemB_PackageB_InstructionB1 = CreateUNDGDataItem(packageB_InstructionB1, "CCCC", "David3");
			UNDGDataItem undgDataItemB_PackageB_InstructionB2 = CreateUNDGDataItem(packageB_InstructionB2, "DDDD", "David4");
			UNDGDataItem undgDataItemB_PackageB_NoInstruction = CreateUNDGDataItem(packageB_NoInstruction, "EEEE", "David5");

			var bookingConsolidation = helper.CreateConsolidation();
			var bookingA = helper.CreateBooking(bookingConsolidation);
			var bookingB = helper.CreateBooking(bookingConsolidation);
			DtbBookingInstruction instructionA1 = helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionA2 = helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionA3 = helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			helper.CreatePackageDivot(instructionA1, containerA_InstructionA1, 1);
			helper.CreatePackageDivot(instructionA1, packageA_InstructionA1, 1);
			helper.CreatePackageDivot(instructionA2, packageA_InstructionA2, 1);
			helper.CreatePackageDivot(instructionA2, packageA_InstructionA2_NoUNDG, 1);

			DtbBookingInstruction instructionB1 = helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionB2 = helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionB3 = helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			helper.CreatePackageDivot(instructionB1, containerB_InstructionB1, 1);
			helper.CreatePackageDivot(instructionB1, packageB_InstructionB1, 1);
			helper.CreatePackageDivot(instructionB2, packageB_InstructionB2, 1);
			helper.CreatePackageDivot(instructionB2, packageB_InstructionB2_NoUNDG, 1);

			var undgSubstanceWrapperCollection = new UNDGSubstanceWrapperCollection(bookingConsolidation, Factory);
			AssertEquals("Wrong number of UNDGs were created", 8, undgSubstanceWrapperCollection.Count);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItemA1_ContainerA_InstructionA1);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItemA2_ContainerA_InstructionA1);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItemA_PackageA_InstructionA1);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItemA_PackageA_InstructionA2);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItemB1_ContainerB_InstructionB1);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItemB2_ContainerB_InstructionB1);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItemB_PackageB_InstructionB1);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItemB_PackageB_InstructionB2);
		}

		#endregion

		#region TestConstructor_WithPkgPackage

		public void TestConstructor_WithPkgPackage()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var packageJob = Factory.New<PkgPackageJob>();
			var package = helper.CreatePackage("PKG");
			var undgDataItem1 = CreateUNDGDataItem(package, "ABCD");
			var undgDataItem2 = CreateUNDGDataItem(package, "1234");
			var undgSubstanceWrapperCollection = new UNDGSubstanceWrapperCollection(package, Factory);
			AssertEquals("Wrong number of UNDGs were created", 2, undgSubstanceWrapperCollection.Count);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItem1);
			AssertContainsUNDGFor(undgSubstanceWrapperCollection, undgDataItem2);
		}

		#endregion

		#region TestConstructor_UNDGSubstanceWrapperCollection

		public void TestConstructor_UNDGSubstanceWrapperCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1.PK, 1);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2.PK, 1);

			var undgSubstanceWrapperCollection = new UNDGSubstanceWrapperCollection(order, Factory);
			AssertEquals("Should have no UNDG", 0, undgSubstanceWrapperCollection.Count);

			var line1Undg1 = orderLine1.Product.Parent.UNDGs.AddNew();
			var line1Undg2 = orderLine1.Product.Parent.UNDGs.AddNew();
			undgSubstanceWrapperCollection = new UNDGSubstanceWrapperCollection(order, Factory);
			AssertContainsExactElementsInAnyOrder("Should have undgs from line 1", new[] { line1Undg1, line1Undg2 }, undgSubstanceWrapperCollection.Cast<UNDGSubstanceWrapper>().Select(w => w.WrappedObject));

			var line2Undg1 = orderLine1.Product.Parent.UNDGs.AddNew();
			var line2Undg2 = orderLine1.Product.Parent.UNDGs.AddNew();
			undgSubstanceWrapperCollection = new UNDGSubstanceWrapperCollection(order, Factory);
			AssertContainsExactElementsInAnyOrder("Should have undgs from line 1 and line 2", new[] { line1Undg1, line1Undg2, line2Undg1, line2Undg2 }, undgSubstanceWrapperCollection.Cast<UNDGSubstanceWrapper>().Select(w => w.WrappedObject));
		}

		#endregion

		#region TestConstructor_WhsDocket

		#region TestConstructor_WhsDocket_WeighVolume

		public void TestConstructor_WhsDocket_WeighVolume()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var part_NoUNDGs = helper.CreateProduct(data.Org1, "P3");
			var undgItem1 = helper.CreateUNDGDataItem(data.Part1, "2808", 0.1m, Constants.Weight.Kilograms, 0.01m, Constants.Volume.CubicMetres);
			var undgItem2 = helper.CreateUNDGDataItem(data.Part1, "3495", 0.2m, Constants.Weight.Kilograms, 0.02m, Constants.Volume.CubicMetres);
			var undgItem3 = helper.CreateUNDGDataItem(data.Part2, "2808", 0.15m, Constants.Weight.Kilograms, 0.015m, Constants.Volume.CubicMetres);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine3 = helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var orderLine4 = helper.CreateWhsOrderLine(order, part_NoUNDGs, 10m);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("Only 3 UNDG items should be shown on the document.", 3, orderWrapper.UNDGs.Count);
			AssertContainsUNDG(orderWrapper, undgItem1, 1m, Constants.Weight.Kilograms, 0.1m, Constants.Volume.CubicMetres);
			AssertContainsUNDG(orderWrapper, undgItem2, 2m, Constants.Weight.Kilograms, 0.2m, Constants.Volume.CubicMetres);
			AssertContainsUNDG(orderWrapper, undgItem3, 1.5m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);
		}

		#endregion

		#region TestConstructor_WhsDocket_WeighVolume_FallbackToProductGrossWeight

		public void TestConstructor_WhsDocket_WeighVolume_FallbackToProductGrossWeight()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			helper.SetProductWeightAndVolume(data.Part1, 1m, Constants.Weight.Kilograms, 0.1m, Constants.Volume.CubicMetres);
			var undgItem1 = helper.CreateUNDGDataItem(data.Part1, "2808", 0.1m, Constants.Weight.Kilograms, 0.01m, Constants.Volume.CubicMetres);
			var undgItem2 = helper.CreateUNDGDataItem(data.Part1, "3495", 0m, Constants.Weight.Grams, 0m, ""); // wrong UQs and no weight/volume specified

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);

			var orderWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("Only 2 UNDG items should be shown on the document.", 2, orderWrapper.UNDGs.Count);
			AssertContainsUNDG(orderWrapper, undgItem1, 1m, Constants.Weight.Kilograms, 0.1m, Constants.Volume.CubicMetres);
			AssertContainsUNDG(orderWrapper, undgItem2, 10m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres); // should be products weight/volume + UQs
		}

		#endregion

		#region AssertContainsUNDG

		void AssertContainsUNDG(WarehouseOrderWrapper orderWrapper, UNDGDataItem undgItem, ZDecimal expectedWeight, ZString expectedWeightUQ, ZDecimal expectedVolume, ZString expectedVolumeUQ)
		{
			var undgItemWrapper = orderWrapper.UNDGs.Cast<UNDGSubstanceWrapper>().Single(dg => dg.WrappedObject == undgItem);
			AssertEquals("Incorrect Weight", expectedWeight, undgItemWrapper.Weight.Value);
			AssertEquals("Incorrect Weight UQ", expectedWeightUQ, undgItemWrapper.Weight.Unit.Code);
			AssertEquals("Incorrect Volume", expectedVolume, undgItemWrapper.Volume.Value);
			AssertEquals("Incorrect Volume UQ", expectedVolumeUQ, undgItemWrapper.Volume.Unit.Code);
		}

		#endregion

		#endregion

		#endregion

		#region Properties

		#region TestUNNumbers

		public void TestUNNumbers()
		{
			var subs1 = Factory.NewWithValidTestData<UNDGSubstance>();
			subs1.DG_UNNO = "2478";
			subs1.DG_Variant = "c";
			var subs2 = Factory.NewWithValidTestData<UNDGSubstance>();
			subs2.DG_UNNO = "2357";

			var dg1 = Factory.New<UNDGDataItem>();
			dg1.DI_DG = subs1.PK;

			var dg2 = Factory.New<UNDGDataItem>();
			dg2.DI_DG = subs2.PK;

			var collection = new UNDGSubstanceWrapperCollection(Factory);
			AssertEquals("", collection.UNNumbers);

			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg2, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));

			AssertEquals("2478, 2357", collection.UNNumbers);
		}

		#endregion

		#region TestUNNumbersFallBackToIMOClasses

		public void TestUNNumbersFallBackToIMOClasses()
		{
			var dg1 = Factory.New<UNDGDataItem>();
			dg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2478", "c", "IMO").First().PK;
			dg1.DI_IMOClass = "1.1D";
			var dg2 = Factory.New<UNDGDataItem>();
			dg2.DI_IMOClass = "1.6N";
			var collection = new UNDGSubstanceWrapperCollection(Factory);
			AssertEquals("", collection.UNNumbers);

			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg2, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg2, Factory));

			AssertEquals("2478, 1.6N", collection.UNNumbersFallBackToIMOClasses);
		}

		#endregion

		#region TestUNDGContact

		public void TestUNDGContact()
		{
			OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Contact 1";

			OrgContact contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Contact 2";

			UNDGDataItem dg1 = Factory.New<UNDGDataItem>();
			dg1.DI_DG = Factory.NewWithValidTestData<UNDGSubstance>().PK;
			dg1.DI_OC_DGContact = contact1.PK;

			UNDGDataItem dg2 = Factory.New<UNDGDataItem>();
			dg2.DI_DG = Factory.NewWithValidTestData<UNDGSubstance>().PK;
			dg2.DI_OC_DGContact = contact2.PK;

			UNDGSubstanceWrapperCollection collection = new UNDGSubstanceWrapperCollection(Factory);
			AssertEquals("", collection.UNDGContact.FullName);

			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg2, Factory));

			AssertEquals("Contact 1", collection.UNDGContact.FullName);
		}

		#endregion

		#region TestFormattedTexts

		public void TestFormattedText()
		{
			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Code = "1234a";
			substance1.DG_PSN = "Shipper1";

			var reference1 = Factory.New<UNDGCountryReference>();
			reference1.DCR_HasFlashPointLower = false;
			reference1.DCR_HasFlashPointUpper = true;
			reference1.DCR_FlashPointUpperCentigrade = 13.5m;
			reference1.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference1.DCR_Type = "PSA";
			reference1.DCR_Code = "1S";
			substance1.UNDGCountryReferences.Add(reference1);

			var reference2 = Factory.New<UNDGCountryReference>();
			reference2.DCR_HasFlashPointLower = true;
			reference2.DCR_HasFlashPointUpper = true;
			reference2.DCR_FlashPointLowerCentigrade = 13.5m;
			reference2.DCR_FlashPointUpperCentigrade = 100.1m;
			reference2.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference2.DCR_Type = "PSA";
			reference2.DCR_Code = "2S";
			substance1.UNDGCountryReferences.Add(reference2);

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Code = "2345a";
			substance2.DG_PSN = "Shipper2";

			var reference3 = Factory.New<UNDGCountryReference>();
			reference3.DCR_HasFlashPointLower = false;
			reference3.DCR_HasFlashPointUpper = true;
			reference3.DCR_FlashPointUpperCentigrade = 13.5m;
			reference3.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference3.DCR_Type = "PSA";
			reference3.DCR_Code = "1S";
			substance2.UNDGCountryReferences.Add(reference3);

			var dg1 = Factory.New<UNDGDataItemForTest>();
			dg1.DI_DG = substance1.PK;
			dg1.LinkDefault(substance1);
			dg1.DI_IMOClass = "1.1D";
			dg1.DI_IsCombustible = true;
			dg1.DI_DGFlashPoint = 11m;

			var dg2 = Factory.New<UNDGDataItemForTest>();
			dg2.DI_DG = substance1.PK;
			dg2.LinkDefault(substance1);
			dg2.DI_IMOClass = "1.1D";
			dg2.DI_IsCombustible = true;
			dg2.DI_DGFlashPoint = 15m;

			var dg3 = Factory.New<UNDGDataItemForTest>();
			dg3.DI_DG = substance1.PK;
			dg3.LinkDefault(substance1);
			dg3.DI_IMOClass = "1.2D";
			dg3.DI_IsCombustible = true;
			dg3.DI_DGFlashPoint = 11m;

			var dg4 = Factory.New<UNDGDataItemForTest>();
			dg4.DI_DG = substance2.PK;
			dg4.LinkDefault(substance2);
			dg4.DI_IMOClass = "1.1D";
			dg4.DI_IsCombustible = true;
			dg4.DI_DGFlashPoint = 11m;

			var collection = new UNDGSubstanceWrapperCollection(Factory);
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));

			AssertEquals("1.1D PSA Group: 1S", collection.FormattedIMOClassAndPSAGroupWithLabel.ToString());
			AssertEquals("1.1D PSA Group: 1S (Shipper1)", collection.FormattedIMOClassAndPSAGroupWithLabelAndProperShippingName.ToString());

			collection = new UNDGSubstanceWrapperCollection(Factory);
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg2, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg2, Factory));

			AssertEquals("1.1D PSA Group: 1S, 1.1D PSA Group: 2S", collection.FormattedIMOClassAndPSAGroupWithLabel.ToString());
			AssertEquals("1.1D PSA Group: 1S (Shipper1), 1.1D PSA Group: 2S (Shipper1)", collection.FormattedIMOClassAndPSAGroupWithLabelAndProperShippingName.ToString());

			collection = new UNDGSubstanceWrapperCollection(Factory);
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg3, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg3, Factory));

			AssertEquals("1.1D PSA Group: 1S, 1.2D PSA Group: 1S", collection.FormattedIMOClassAndPSAGroupWithLabel.ToString());
			AssertEquals("1.1D PSA Group: 1S (Shipper1), 1.2D PSA Group: 1S (Shipper1)", collection.FormattedIMOClassAndPSAGroupWithLabelAndProperShippingName.ToString());

			collection = new UNDGSubstanceWrapperCollection(Factory);
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg4, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg4, Factory));

			AssertEquals("1.1D PSA Group: 1S", collection.FormattedIMOClassAndPSAGroupWithLabel.ToString());
			AssertEquals("1.1D PSA Group: 1S (Shipper1), 1.1D PSA Group: 1S (Shipper2)", collection.FormattedIMOClassAndPSAGroupWithLabelAndProperShippingName.ToString());

			collection = new UNDGSubstanceWrapperCollection(Factory);
			collection.Add(new UNDGSubstanceWrapper(dg1, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg2, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg3, Factory));
			collection.Add(new UNDGSubstanceWrapper(dg4, Factory));

			AssertEquals("1.1D PSA Group: 1S, 1.1D PSA Group: 2S, 1.2D PSA Group: 1S", collection.FormattedIMOClassAndPSAGroupWithLabel.ToString());
			AssertEquals("1.1D PSA Group: 1S (Shipper1), 1.1D PSA Group: 2S (Shipper1), 1.2D PSA Group: 1S (Shipper1), 1.1D PSA Group: 1S (Shipper2)", collection.FormattedIMOClassAndPSAGroupWithLabelAndProperShippingName.ToString());
		}

		internal class UNDGDataItemForTest : UNDGDataItem
		{
			public UNDGDataItemForTest(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public override bool IsPSAGroupApplicable => true;
		}

		#endregion

		#endregion

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new UNDGSubstanceWrapper(null, Factory);
		}

		protected override UNDGSubstanceWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new UNDGSubstanceWrapperCollection(Factory);
		}

		#endregion
	}
}
