using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.FormalEntry.Testing
{
	[TestedType(typeof(DocContainerAndPackageInfoCollection))]
	sealed class DocContainerAndPackageInfoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocContainerAndPackageInfoCollection>
	{
		protected override DocContainerAndPackageInfoCollection GetCollectionToTest()
		{
			return new DocContainerAndPackageInfoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocContainerAndPackageInfo(Factory, "", "", "", "", "0");
		}

		public void TestHouseBillWithNoPackagesDoesntGetIncluded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONTNUMBER";
			container.CO_FCL_LCL_AIR = "FCL";

			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = "MYHOUSE1";
			PackingGroup packingGroup1 = houseBill1.PackingGroups.AddNew();
			Package package1 = packingGroup1.Packages.AddNew();
			package1.CW_PackQty = 0;

			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "MYHOUSE2";
			PackingGroup packingGroup2 = houseBill2.PackingGroups.AddNew();
			packingGroup2.CR_CO_Container = container.PK;
			Package package2 = packingGroup2.Packages.AddNew();
			package2.CW_PackQty = 40;

			Bill houseBill3 = declaration.Bills.AddNew();
			houseBill3.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill3.CU_HouseBill = "MYHOUSE3";
			PackingGroup packingGroup3 = houseBill2.PackingGroups.AddNew();
			packingGroup3.CR_CO_Container = container.PK;
			Package package3 = packingGroup3.Packages.AddNew();
			package3.CW_PackQty = 0;

			DocContainerAndPackageInfoCollection packInfos = new DocContainerAndPackageInfoCollection(declaration);

			packInfos.Load();

			AssertEquals(2, packInfos.Count);

			DocContainerAndPackageInfo packInfo1 = packInfos[0];
			AssertEquals("PackInfo1.BillNumber", "MYHOUSE2", packInfo1.BillNumber);
			AssertEquals("PackInfo1.ContainerNumber", "CONTNUMBER", packInfo1.ContainerNumber);
			AssertEquals("PackInfo1.ContainerStatus", "FCL", packInfo1.ContainerStatus);
			AssertEquals("PackInfo1.NumberOfPackages", "40", packInfo1.PackagesAndType);

			DocContainerAndPackageInfo packInfo2 = packInfos[1];
			AssertEquals("PackInfo2.BillNumber", "MYHOUSE2", packInfo2.BillNumber);
			AssertEquals("PackInfo2.ContainerNumber", "CONTNUMBER", packInfo2.ContainerNumber);
			AssertEquals("PackInfo2.ContainerStatus", "FCL", packInfo2.ContainerStatus);
			AssertEquals("PackInfo2.NumberOfPackages", "0", packInfo2.PackagesAndType);
		}

		public void TestManyLinesOfPackagesWithTypes()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONTNUMBER";
			container.CO_FCL_LCL_AIR = "FCL";

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "MYHOUSE1";
			PackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			Package package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 30;

			houseBill = declaration.Bills.AddNew();
			houseBill.CU_HouseBill = "MYHOUSE2";
			packingGroup = houseBill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;
			package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 10;
			package.CW_PackType = Core.Constants.PkgUnit.Bag;
			package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 30;
			package.CW_PackType = Core.Constants.PkgUnit.Box;
			package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 60;
			package.CW_PackType = Core.Constants.PkgUnit.Tube;

			DocContainerAndPackageInfoCollection packInfos = new DocContainerAndPackageInfoCollection(declaration);

			packInfos.Load();

			AssertEquals(2, packInfos.Count);

			DocContainerAndPackageInfo packInfo1 = packInfos[0];
			AssertEquals("MYHOUSE1", packInfo1.BillNumber);
			AssertEquals("", packInfo1.ContainerNumber);
			AssertEquals("", packInfo1.ContainerStatus);
			AssertEquals("30", packInfo1.PackagesAndType.ToString());

			DocContainerAndPackageInfo packInfo2 = packInfos[1];
			AssertEquals("MYHOUSE2", packInfo2.BillNumber);
			AssertEquals("CONTNUMBER", packInfo2.ContainerNumber);
			AssertEquals("FCL", packInfo2.ContainerStatus);
			AssertEquals("10 BAG, 30 BOX, 60 TUB", packInfo2.PackagesAndType);
		}

		public void TestInstantiationAndEverything()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONTNUMBER";
			container.CO_FCL_LCL_AIR = "FCL";

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "MYHOUSE1";
			PackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			Package package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 30;

			houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "MYHOUSE2";
			packingGroup = houseBill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;
			package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 10;
			package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 30;

			DocContainerAndPackageInfoCollection packInfos = new DocContainerAndPackageInfoCollection(declaration);

			packInfos.Load();

			AssertEquals(2, packInfos.Count);

			DocContainerAndPackageInfo packInfo1 = packInfos[0];
			AssertEquals("MYHOUSE1", packInfo1.BillNumber);
			AssertEquals("", packInfo1.ContainerNumber);
			AssertEquals("", packInfo1.ContainerStatus);
			AssertEquals("30", packInfo1.PackagesAndType);

			DocContainerAndPackageInfo packInfo2 = packInfos[1];
			AssertEquals("MYHOUSE2", packInfo2.BillNumber);
			AssertEquals("CONTNUMBER", packInfo2.ContainerNumber);
			AssertEquals("FCL", packInfo2.ContainerStatus);
			AssertEquals("10, 30", packInfo2.PackagesAndType);
		}
	}
}
