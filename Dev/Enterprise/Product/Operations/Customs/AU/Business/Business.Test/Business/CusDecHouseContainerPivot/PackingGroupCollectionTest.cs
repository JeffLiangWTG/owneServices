using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(PackingGroupCollection))]
	sealed class PackingGroupCollectionTest : BaseDeclarationLevelPackingGroupCollectionTest<JobDeclaration>
	{
		public void TestDefaultPacksWhenHouseBillCreatesNewRecord()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TotalNoOfPacks = 123;
			declaration.JE_HouseBill = "House Bill";
			AssertEquals("House Bill Count", 1, declaration.Bills.Count);
			AssertEquals("House Bill Packs Count", 1, declaration.PackingGroups.Count);
			AssertEquals("Packs have defaulted", 123, declaration.PackingGroups[0].TotalNumberOfPackages);

			PackingGroup newPack = declaration.PackingGroups.AddNew();
			AssertEquals("Packs not defaulted", 0, newPack.TotalNumberOfPackages);
		}

		public void TestDefaultPacksWhenContainerCreatesNewRecord()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_HouseBill = "HB1";
			declaration.JE_TotalNoOfPacks = 123;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "Number";
			AssertEquals("Container is hooked", container, declaration.PackingGroups[0].Container);
		}

		public void TestNotAddingNewPackRecord()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2004, 01, 01);
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "Num";
			AssertEquals("Container Count", 1, declaration.CusContainers.Count);
			AssertEquals("House Bill Packs Count", 0, declaration.PackingGroups.Count);
		}

		public void TestDefaultPacks()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_HouseBill = "HB1";
			declaration.JE_TotalNoOfPacks = 123;
			PackingGroup newPack = declaration.PackingGroups[0];
			AssertEquals("Packs defaulted", 123, newPack.TotalNumberOfPackages);
		}

		public void TestLoadCollectionFromContainers()
		{
			AssertEquals("No elements in collection", 0, testCollection.Count);

			SetupPackage();
			testCollection.Load();
			AssertEquals("One element in the collection", 1, testCollection.Count);
		}

		public void TestLoadCollectionFromHouseBills()
		{
			AssertEquals("No elements in collection", 0, testCollection.Count);

			testDec.CusContainers.RemoveAndDeleteAll();
			PackingGroup packGroup = Factory.New<PackingGroup>();
			packGroup.CR_CU_HouseBill = houseBill.PK;
			testCollection.Load();
			AssertEquals("One element in the collection", 1, testCollection.Count);
		}

		public void TestTotalNumberOfPackages()
		{
			SetupPackage();
			package.CW_PackQty = 100;
			testCollection.Load();
			AssertEquals("Total Number of Packages", 100, testCollection.TotalNumberOfPackages);

			var package2 = testDec.Packages.AddNew();
			package2.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			package2.CW_HouseBill = houseBill.CU_BillUniqueCode;
			package2.CW_PackQty = 200;
			testCollection.Load();
			AssertEquals("Total Number of Packages is sum of packQty", 300, testCollection.TotalNumberOfPackages);

			package2.CW_PackQty = int.MaxValue;
			testCollection.Load();
			AssertEquals("Total Number of Packages is maxInt (does not overflow)", int.MaxValue, testCollection.TotalNumberOfPackages);
		}

		public void TestTotalWarehouseNumberOfPackages()
		{
			SetupPackage();
			package.CW_InBondPackQty = 100;
			testCollection.Load();

			AssertEquals("WarehouseNumberOfPackages", 100, testCollection.TotalWarehouseNumberOfPackages);
		}

		public void TestTotalPackingUnitCount()
		{
			SetupPackage();
			package.CW_OuterPacks = 100;
			testCollection.Load();

			AssertEquals("PackingUnitCount", 100, testCollection.TotalPackingUnitCount);
		}

		#region Set Up

		protected override void SetUp()
		{
			base.SetUp();
			CreateDeclaration();
			testCollection = testDec.PackingGroups;
		}

		void CreateDeclaration()
		{
			testDec = Factory.New<JobDeclaration>();

			houseBill = testDec.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "House Bill";

			container = testDec.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONTAINERNUM";
		}

		void SetupPackage()
		{
			package = testDec.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
		}

		JobDeclaration testDec;
		Bill houseBill;
		CusContainer container;
		PackingGroupCollection testCollection;
		Package package;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PackingGroupCollection(Factory.New<JobDeclaration>());
		}

		#endregion

	}
}
