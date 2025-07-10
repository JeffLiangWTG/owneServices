using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCusInBondContainerPackageCollection<NctsCusInBondContainerPackageGenPivot, NctsPackage, NctsCusInBondContainer>))]
	class NctsCusInBondContainerPackageCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestLoad()
		{
			(var header, _, var package) = SetupData();
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			var pivot1 = Factory.New<NctsCusInBondContainerPackageGenPivot>();
			pivot1.XX_RelationType = "$#";
			pivot1.XX_Relation1TableCode = CusInvPackSchema.Constants.Prefix;
			pivot1.XX_Relation2TableCode = CusInBondContainerSchema.Constants.Prefix;
			pivot1.XX_Relation1ID = package.PK;
			pivot1.XX_Relation2ID = container1.PK;
			var pivot2 = Factory.New<NctsCusInBondContainerPackageGenPivot>();
			pivot2.XX_RelationType = Core.Constants.GenPivotTypes.CusNctsContainer;
			pivot2.XX_Relation1TableCode = "!@";
			pivot2.XX_Relation2TableCode = CusInBondContainerSchema.Constants.Prefix;
			pivot2.XX_Relation1ID = package.PK;
			pivot2.XX_Relation2ID = container1.PK;
			var pivot3 = Factory.New<NctsCusInBondContainerPackageGenPivot>();
			pivot3.XX_RelationType = Core.Constants.GenPivotTypes.CusNctsContainer;
			pivot3.XX_Relation1TableCode = CusInvPackSchema.Constants.Prefix;
			pivot3.XX_Relation2TableCode = "@#";
			pivot3.XX_Relation1ID = package.PK;
			pivot3.XX_Relation2ID = container1.PK;
			var pivot4 = Factory.New<NctsCusInBondContainerPackageGenPivot>();
			pivot4.XX_RelationType = Core.Constants.GenPivotTypes.CusNctsContainer;
			pivot4.XX_Relation1TableCode = CusInvPackSchema.Constants.Prefix;
			pivot4.XX_Relation2TableCode = CusInBondContainerSchema.Constants.Prefix;
			pivot4.XX_Relation1ID = ZGuid.NewZGuid();
			pivot4.XX_Relation2ID = container1.PK;
			var pivot5 = Factory.New<NctsCusInBondContainerPackageGenPivot>();
			pivot5.XX_RelationType = Core.Constants.GenPivotTypes.CusNctsContainer;
			pivot5.XX_Relation1TableCode = CusInvPackSchema.Constants.Prefix;
			pivot5.XX_Relation2TableCode = CusInBondContainerSchema.Constants.Prefix;
			pivot5.XX_Relation1ID = package.PK;
			pivot5.XX_Relation2ID = container2.PK;
			var pivot6 = Factory.New<NctsCusInBondContainerPackageGenPivot>();
			pivot6.XX_RelationType = Core.Constants.GenPivotTypes.CusNctsContainer;
			pivot6.XX_Relation1TableCode = CusInvPackSchema.Constants.Prefix;
			pivot6.XX_Relation2TableCode = CusInBondContainerSchema.Constants.Prefix;
			pivot6.XX_Relation1ID = package.PK;
			pivot6.XX_Relation2ID = container1.PK;
			var collection = CreateNewCollection(package);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { pivot5, pivot6 }, collection.Cast<GenPivot>());
		}

		public void TestContainers()
		{
			(var header, _, var package) = SetupData();
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			var container3 = header.DepartureHeaderContainers.AddNew();
			container3.BC_ContainerNum = "CONTAINER3";
			var container4 = header.DepartureHeaderContainers.AddNew();
			container4.BC_ContainerNum = "CONTAINER4";
			var collection = CreateNewCollection(package);
			collection.AddPivotFor(container1);
			collection.AddPivotFor(container3);
			collection.AddPivotFor(container4);
			AssertContainsExactElementsInAnyOrder(new[] { container1, container3, container4 }, collection.Containers);
		}

		public void TestContains()
		{
			(var header, _, var package) = SetupData();
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			var container3 = header.DepartureHeaderContainers.AddNew();
			container3.BC_ContainerNum = "CONTAINER3";
			var collection = CreateNewCollection(package);
			collection.AddPivotFor(container1);
			var pivot2 = collection.AddPivotFor(container2);
			var pivot3 = collection.AddPivotFor(container3);
			AssertSame("collection.GetRelatedPivot(container2)", pivot2, collection.GetRelatedPivot(container2));
			AssertSame("collection.GetRelatedPivot(container3)", pivot3, collection.GetRelatedPivot(container3));
		}

		public void TestAddPivotFor()
		{
			(var header, _, var package) = SetupData();
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			var collection = CreateNewCollection(package);
			var pivot1 = collection.AddPivotFor(container1);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertSame("collection[0]", pivot1, collection[0]);
			AssertEquals("pivot1.XX_RelationType", Core.Constants.GenPivotTypes.CusNctsContainer, pivot1.XX_RelationType);
			AssertEquals("pivot1.XX_Relation1TableCode", CusInvPackSchema.Constants.Prefix, pivot1.XX_Relation1TableCode);
			AssertEquals("pivot1.XX_Relation2TableCode", CusInBondContainerSchema.Constants.Prefix, pivot1.XX_Relation2TableCode);
			AssertEquals("pivot1.XX_Relation1ID", package.PK, pivot1.XX_Relation1ID);
			AssertEquals("pivot1.XX_Relation2ID", container1.PK, pivot1.XX_Relation2ID);

			AssertSame("Should match", pivot1, collection.AddPivotFor(container1));
			AssertEquals("collection.Count", 1, collection.Count);
			AssertEquals("Should create new", false, object.ReferenceEquals(pivot1, collection.AddPivotFor(container2)));
			AssertEquals("collection.Count", 2, collection.Count);
		}

		public void TestDeletePivotFor()
		{
			(var header, _, var package) = SetupData();
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			var container3 = header.DepartureHeaderContainers.AddNew();
			container3.BC_ContainerNum = "CONTAINER3";
			var collection = CreateNewCollection(package);
			var pivot1 = collection.AddPivotFor(container1);
			var pivot2 = collection.AddPivotFor(container2);
			AssertEquals("collection.Count", 2, collection.Count);
			collection.DeletePivotFor(container2);
			AssertEquals("pivot2.IsDeleted", true, pivot2.IsDeleted);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertSame("collection[0]", pivot1, collection[0]);
			collection.DeletePivotFor(container3);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertSame("collection[0]", pivot1, collection[0]);
		}

		public void TestGetRelatedPivot()
		{
			(var header, _, var package) = SetupData();
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			var container3 = header.DepartureHeaderContainers.AddNew();
			container3.BC_ContainerNum = "CONTAINER3";
			var collection = CreateNewCollection(package);
			var pivot1 = collection.AddPivotFor(container1);
			var pivot2 = collection.AddPivotFor(container2);
			AssertSame("collection.GetRelatedPivot(container2)", pivot2, collection.GetRelatedPivot(container2));
			AssertSame("collection.GetRelatedPivot(container1)", pivot1, collection.GetRelatedPivot(container1));
			AssertNull("collection.GetRelatedPivot(container3)", collection.GetRelatedPivot(container3));
		}

		public void TestAllowNew()
		{
			var collection = CreateNewCollection(SetupData().package);
			AssertEquals("AllowNew", false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = CreateNewCollection(SetupData().package);
			AssertEquals("AllowRemove", false, collection.AllowRemove);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => CreateNewCollection(SetupData().package);

		NctsCusInBondContainerPackageCollection<NctsCusInBondContainerPackageGenPivot, NctsPackage, NctsCusInBondContainer> CreateNewCollection(NctsPackage package) => new NctsCusInBondContainerPackageCollection<NctsCusInBondContainerPackageGenPivot, NctsPackage, NctsCusInBondContainer>(package);

		(NctsHeader header, NctsDepartureCargoDesc goodsItem, NctsPackage package) SetupData()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();
			return (header, goodsItem, package);
		}
	}
}
