using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonPackagingWrapperTest : WrapperHelperTest<NCTS5CommonPackagingWrapper>
	{
		public void TestSequenceNumber()
		{
			var wrapper = new NCTS5CommonPackagingWrapper(Factory, ZString.Empty, ZString.Empty, ZLong.Zero, 2);
			AssertEquals("Expected filled SequenceNumber", "2", wrapper.SequenceNumber);
		}

		public void TestNumberOfPackages()
		{
			var wrapper = new NCTS5CommonPackagingWrapper(Factory, ZString.Empty, ZString.Empty, 9, ZShort.Zero);
			AssertEquals("Expected filled NumberOfPackages", "9", wrapper.NumberOfPackages);
		}

		public void TestNumberOfPackages0WithTypeBulk()
		{
			var wrapper = new NCTS5CommonPackagingWrapper(Factory, "VG", ZString.Empty, 0, ZShort.Zero);
			AssertEquals("Expected empty NumberOfPackages with type bulk", string.Empty, wrapper.NumberOfPackages);
		}

		public void TestNumberOfPackages0WithoutTypeBulk()
		{
			var wrapper = new NCTS5CommonPackagingWrapper(Factory, "CT", ZString.Empty, 0, ZShort.Zero);
			AssertEquals("Expected filled NumberOfPackages without type bulk", "0", wrapper.NumberOfPackages);
		}

		public void TestNumberOfPackagesWithFlagshouldNotDeclarePacksQtyTrue()
		{
			var wrapper = new NCTS5CommonPackagingWrapper(Factory, ZString.Empty, ZString.Empty, 9, ZShort.Zero, shouldNotDeclarePacksQty: true);
			AssertEquals("Expected empty NumberOfPackages when shouldNotDeclarePacksQty is true", string.Empty, wrapper.NumberOfPackages);
		}

		public void TestGetPackagesListDeparture()
		{
			goodsItem.IsVehicles = false;

			var package1 = goodsItem.Packages.AddNew();
			package1.B5_MarksAndNumbers = "marks";
			package1.B5_UnitType = "CT";
			package1.B5_UnitCount = 9;

			var package2 = goodsItem.Packages.AddNew();
			package2.B5_MarksAndNumbers = "marks2";
			package2.B5_UnitType = "NE";
			package2.B5_UnitCount = 4;

			var package3 = goodsItem.Packages.AddNew();
			package3.B5_MarksAndNumbers = "bulk gas marks";
			package3.B5_UnitType = "VG";
			package3.B5_UnitCount = 2;

			package1.B5_SequenceNumber = 5;
			package2.B5_SequenceNumber = 7;
			package3.B5_SequenceNumber = 3;

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: package1.B5_SequenceNumber", (ZShort)5, package1.B5_SequenceNumber);
				AssertEquals("Prereq: package2.B5_SequenceNumber", (ZShort)7, package2.B5_SequenceNumber);
				AssertEquals("Prereq: package3.B5_SequenceNumber", (ZShort)3, package3.B5_SequenceNumber);

				var packages = NCTS5CommonPackagingWrapper.GetPackagesListDeparture(goodsItem).ToList();
				AssertEquals("Expected filled packages", 3, packages.Count);

				AssertEquals("For first package expected filled PackageType", "VG", packages[0].PackageType);
				AssertEquals("For first package expected filled Marks", "bulk gas marks", packages[0].Marks);
				AssertEquals("For first package expected empty NumberOfPackages", ZString.Empty, packages[0].NumberOfPackages);
				AssertEquals("For first package expected filled SequenceNumber", "3", packages[0].SequenceNumber);

				AssertEquals("For second package expected filled PackageType", "CT", packages[1].PackageType);
				AssertEquals("For second package expected filled Marks", "marks", packages[1].Marks);
				AssertEquals("For second package expected filled NumberOfPackages", "9", packages[1].NumberOfPackages);
				AssertEquals("For second package expected filled SequenceNumber", "5", packages[1].SequenceNumber);

				AssertEquals("For third package expected filled PackageType", "NE", packages[2].PackageType);
				AssertEquals("For third package expected filled Marks", "marks2", packages[2].Marks);
				AssertEquals("For third package expected filled NumberOfPackages", "4", packages[2].NumberOfPackages);
				AssertEquals("For third package expected filled SequenceNumber", "7", packages[2].SequenceNumber);
			});
		}

		public void TestGetPackagesListDepartureWhenVehicles()
		{
			goodsItem.IsVehicles = true;

			var vehicle1 = goodsItem.Packages.AddNew();
			vehicle1.B5_PackageID = "VINCODE1";
			vehicle1.B5_Brand = "BRAND1";
			vehicle1.B5_Model = "MODEL1";

			var vehicle2 = goodsItem.Packages.AddNew();
			vehicle2.B5_PackageID = ZString.Empty;
			vehicle2.B5_Brand = "BRAND2";
			vehicle2.B5_Model = "MODEL2";

			var vehicle3 = goodsItem.Packages.AddNew();
			vehicle3.B5_PackageID = "VINCODE3";
			vehicle3.B5_Brand = ZString.Empty;
			vehicle3.B5_Model = "MODEL3";

			var vehicle4 = goodsItem.Packages.AddNew();
			vehicle4.B5_PackageID = "VINCODE4";
			vehicle4.B5_Brand = "BRAND4";
			vehicle4.B5_Model = ZString.Empty;

			vehicle1.B5_SequenceNumber = 6;
			vehicle2.B5_SequenceNumber = 5;
			vehicle3.B5_SequenceNumber = 8;
			vehicle4.B5_SequenceNumber = 9;

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: vehicle1.B5_SequenceNumber", (ZShort)6, vehicle1.B5_SequenceNumber);
				AssertEquals("Prereq: vehicle2.B5_SequenceNumber", (ZShort)5, vehicle2.B5_SequenceNumber);
				AssertEquals("Prereq: vehicle3.B5_SequenceNumber", (ZShort)8, vehicle3.B5_SequenceNumber);
				AssertEquals("Prereq: vehicle4.B5_SequenceNumber", (ZShort)9, vehicle4.B5_SequenceNumber);

				var packages = NCTS5CommonPackagingWrapper.GetPackagesListDeparture(goodsItem).ToList();
				AssertEquals("Expected filled packages", 4, packages.Count);

				AssertEquals("For first package expected filled PackageType", "FR", packages[0].PackageType);
				AssertEquals("For first package expected filled Marks", ":BRAND2:MODEL2", packages[0].Marks);
				AssertEquals("For first package expected filled NumberOfPackages", "1", packages[0].NumberOfPackages);
				AssertEquals("For first package expected filled SequenceNumber", "5", packages[0].SequenceNumber);

				AssertEquals("For second package expected filled PackageType", "FR", packages[1].PackageType);
				AssertEquals("For second package expected filled Marks", "VINCODE1:BRAND1:MODEL1", packages[1].Marks);
				AssertEquals("For second package expected filled NumberOfPackages", "1", packages[1].NumberOfPackages);
				AssertEquals("For second package expected filled SequenceNumber", "6", packages[1].SequenceNumber);

				AssertEquals("For third package expected filled PackageType", "FR", packages[2].PackageType);
				AssertEquals("For third package expected filled Marks", "VINCODE3::MODEL3", packages[2].Marks);
				AssertEquals("For third package expected filled NumberOfPackages", "1", packages[2].NumberOfPackages);
				AssertEquals("For third package expected filled SequenceNumber", "8", packages[2].SequenceNumber);

				AssertEquals("For fourth package expected filled PackageType", "FR", packages[3].PackageType);
				AssertEquals("For fourth package expected filled Marks", "VINCODE4:BRAND4:", packages[3].Marks);
				AssertEquals("For fourth package expected filled NumberOfPackages", "1", packages[3].NumberOfPackages);
				AssertEquals("For fourth package expected filled SequenceNumber", "9", packages[3].SequenceNumber);
			});
		}

		public void TestGetPackagesListDepartureWhenSamePackageTypeAndMarks()
		{
			goodsItem.IsVehicles = false;

			var package1 = goodsItem.Packages.AddNew();
			package1.B5_MarksAndNumbers = "marks";
			package1.B5_UnitType = "CT";
			package1.B5_UnitCount = 9;

			var package2 = goodsItem.Packages.AddNew();
			package2.B5_MarksAndNumbers = "marks";
			package2.B5_UnitType = "CT";
			package2.B5_UnitCount = 15;

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: package1.B5_SequenceNumber", (ZShort)1, package1.B5_SequenceNumber);
				AssertEquals("Prereq: package2.B5_SequenceNumber", (ZShort)2, package2.B5_SequenceNumber);

				var packages = NCTS5CommonPackagingWrapper.GetPackagesListDeparture(goodsItem).ToList();
				AssertEquals("Expected filled 2 packages even if they have the same marks and type", 2, packages.Count);

				AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
				AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
				AssertEquals("For first package expected filled NumberOfPackages", "9", packages[0].NumberOfPackages);
				AssertEquals("For first package expected filled SequenceNumber", "1", packages[0].SequenceNumber);

				AssertEquals("For second package expected filled PackageType", "CT", packages[1].PackageType);
				AssertEquals("For second package expected filled Marks", "marks", packages[1].Marks);
				AssertEquals("For second package expected filled NumberOfPackages", "15", packages[1].NumberOfPackages);
				AssertEquals("For second package expected filled SequenceNumber", "2", packages[1].SequenceNumber);
			});
		}

		public void TestGetPackagesListArrival()
		{
			var goodsItem = SetUpForArrival();

			var package1 = goodsItem.Packages.AddNew();
			package1.B5_TypeOfDifference = "DIF";
			package1.PackDifference.B5_MarksAndNumbers = "marks";
			package1.PackDifference.B5_UnitType = "CT";
			package1.PackDifference.B5_UnitCount = 9;

			var package2 = goodsItem.Packages.AddNew();
			package2.B5_MarksAndNumbers = "marks2";
			package2.B5_UnitType = "NE";
			package2.B5_UnitCount = 4;
			package2.B5_TypeOfDifference = "NEW";

			var package3 = goodsItem.Packages.AddNew();
			package3.B5_MarksAndNumbers = "bulk gas marks";
			package3.B5_UnitType = "VG";
			package3.B5_UnitCount = 2;
			package3.B5_TypeOfDifference = "NEW";

			var package4 = goodsItem.Packages.AddNew();
			package4.B5_MarksAndNumbers = "marks4";
			package4.B5_UnitType = "BX";
			package4.B5_UnitCount = 9;
			package4.B5_TypeOfDifference = "MIS";

			var vehicle1 = goodsItem.Packages.AddNew();
			vehicle1.B5_TypeOfDifference = "DIF";
			vehicle1.PackDifference.B5_UnitType = "FR";
			vehicle1.PackDifference.B5_PackageID = "VINCODE1";
			vehicle1.PackDifference.B5_Brand = "BRAND1";
			vehicle1.PackDifference.B5_Model = "MODEL1";

			var vehicle2 = goodsItem.Packages.AddNew();
			vehicle2.B5_UnitType = "FR";
			vehicle2.B5_PackageID = "VINCODE2";
			vehicle2.B5_Brand = "BRAND2";
			vehicle2.B5_Model = "MODEL2";
			vehicle2.B5_TypeOfDifference = "MIS";

			var vehicle3 = goodsItem.Packages.AddNew();
			vehicle3.B5_UnitType = "FR";
			vehicle3.B5_PackageID = "VINCODE3";
			vehicle3.B5_Brand = "BRAND3";
			vehicle3.B5_Model = "MODEL3";
			vehicle3.B5_TypeOfDifference = "NEW";

			var vehicle4 = goodsItem.Packages.AddNew();
			vehicle4.B5_UnitType = "FR";
			vehicle4.B5_PackageID = ZString.Empty;
			vehicle4.B5_Brand = "BRAND4";
			vehicle4.B5_Model = "MODEL4";
			vehicle4.B5_TypeOfDifference = "NEW";

			var vehicle5 = goodsItem.Packages.AddNew();
			vehicle5.B5_UnitType = "FR";
			vehicle5.B5_PackageID = "VINCODE5";
			vehicle5.B5_Brand = ZString.Empty;
			vehicle5.B5_Model = "MODEL5";
			vehicle5.B5_TypeOfDifference = "NEW";

			var vehicle6 = goodsItem.Packages.AddNew();
			vehicle6.B5_UnitType = "FR";
			vehicle6.B5_PackageID = "VINCODE6";
			vehicle6.B5_Brand = "BRAND6";
			vehicle6.B5_Model = ZString.Empty;
			vehicle6.B5_TypeOfDifference = "NEW";

			package1.B5_SequenceNumber = 5;
			package2.B5_SequenceNumber = 7;
			package3.B5_SequenceNumber = 3;
			package4.B5_SequenceNumber = 4;
			vehicle1.B5_SequenceNumber = 6;
			vehicle2.B5_SequenceNumber = 2;
			vehicle3.B5_SequenceNumber = 1;
			vehicle4.B5_SequenceNumber = 8;
			vehicle5.B5_SequenceNumber = 9;
			vehicle6.B5_SequenceNumber = 10;

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: package1.B5_SequenceNumber", (ZShort)5, package1.B5_SequenceNumber);
				AssertEquals("Prereq: package2.B5_SequenceNumber", (ZShort)7, package2.B5_SequenceNumber);
				AssertEquals("Prereq: package3.B5_SequenceNumber", (ZShort)3, package3.B5_SequenceNumber);
				AssertEquals("Prereq: package4.B5_SequenceNumber", (ZShort)4, package4.B5_SequenceNumber);
				AssertEquals("Prereq: vehicle1.B5_SequenceNumber", (ZShort)6, vehicle1.B5_SequenceNumber);
				AssertEquals("Prereq: vehicle2.B5_SequenceNumber", (ZShort)2, vehicle2.B5_SequenceNumber);
				AssertEquals("Prereq: vehicle3.B5_SequenceNumber", (ZShort)1, vehicle3.B5_SequenceNumber);
				AssertEquals("Prereq: vehicle4.B5_SequenceNumber", (ZShort)8, vehicle4.B5_SequenceNumber);
				AssertEquals("Prereq: vehicle5.B5_SequenceNumber", (ZShort)9, vehicle5.B5_SequenceNumber);
				AssertEquals("Prereq: vehicle6.B5_SequenceNumber", (ZShort)10, vehicle6.B5_SequenceNumber);

				var packages = NCTS5CommonPackagingWrapper.GetPackagesListArrival(goodsItem).ToList();
				AssertEquals("Expected filled packages", 10, packages.Count);

				AssertEquals("For first package expected filled PackageType", "FR", packages[0].PackageType);
				AssertEquals("For first package expected filled Marks", "VINCODE3:BRAND3:MODEL3", packages[0].Marks);
				AssertEquals("For first package expected filled NumberOfPackages", "1", packages[0].NumberOfPackages);
				AssertEquals("For first package expected filled SequenceNumber", "1", packages[0].SequenceNumber);

				AssertEquals("For second package expected empty PackageType", ZString.Empty, packages[1].PackageType);
				AssertEquals("For second package expected empty Marks", ZString.Empty, packages[1].Marks);
				AssertEquals("For second package expected empty NumberOfPackages", ZString.Empty, packages[1].NumberOfPackages);
				AssertEquals("For second package expected filled SequenceNumber", "2", packages[1].SequenceNumber);

				AssertEquals("For third package expected filled PackageType", "VG", packages[2].PackageType);
				AssertEquals("For third package expected filled Marks", "bulk gas marks", packages[2].Marks);
				AssertEquals("For third package expected empty NumberOfPackages", ZString.Empty, packages[2].NumberOfPackages);
				AssertEquals("For third package expected filled SequenceNumber", "3", packages[2].SequenceNumber);

				AssertEquals("For fourth package expected empty PackageType", ZString.Empty, packages[3].PackageType);
				AssertEquals("For fourth package expected empty Marks", ZString.Empty, packages[3].Marks);
				AssertEquals("For fourth package expected empty NumberOfPackages", ZString.Empty, packages[3].NumberOfPackages);
				AssertEquals("For fourth package expected filled SequenceNumber", "4", packages[3].SequenceNumber);

				AssertEquals("For fifth package expected filled PackageType", "CT", packages[4].PackageType);
				AssertEquals("For fifth package expected filled Marks", "marks", packages[4].Marks);
				AssertEquals("For fifth package expected filled NumberOfPackages", "9", packages[4].NumberOfPackages);
				AssertEquals("For fifth package expected filled SequenceNumber", "5", packages[4].SequenceNumber);

				AssertEquals("For sixth package expected filled PackageType", "FR", packages[5].PackageType);
				AssertEquals("For sixth package expected filled Marks", "VINCODE1:BRAND1:MODEL1", packages[5].Marks);
				AssertEquals("For sixth package expected filled NumberOfPackages", "1", packages[5].NumberOfPackages);
				AssertEquals("For sixth package expected filled SequenceNumber", "6", packages[5].SequenceNumber);

				AssertEquals("For seventh package expected filled PackageType", "NE", packages[6].PackageType);
				AssertEquals("For seventh package expected filled Marks", "marks2", packages[6].Marks);
				AssertEquals("For seventh package expected filled NumberOfPackages", "4", packages[6].NumberOfPackages);
				AssertEquals("For seventh package expected filled SequenceNumber", "7", packages[6].SequenceNumber);

				AssertEquals("For eight package expected filled PackageType", "FR", packages[7].PackageType);
				AssertEquals("For eight package expected filled Marks", ":BRAND4:MODEL4", packages[7].Marks);
				AssertEquals("For eight package expected filled NumberOfPackages", "1", packages[7].NumberOfPackages);
				AssertEquals("For eight package expected filled SequenceNumber", "8", packages[7].SequenceNumber);

				AssertEquals("For nineth package expected filled PackageType", "FR", packages[8].PackageType);
				AssertEquals("For nineth package expected filled Marks", "VINCODE5::MODEL5", packages[8].Marks);
				AssertEquals("For nineth package expected filled NumberOfPackages", "1", packages[8].NumberOfPackages);
				AssertEquals("For nineth package expected filled SequenceNumber", "9", packages[8].SequenceNumber);

				AssertEquals("For tenth package expected filled PackageType", "FR", packages[9].PackageType);
				AssertEquals("For tenth package expected filled Marks", "VINCODE6:BRAND6:", packages[9].Marks);
				AssertEquals("For tenth package expected filled NumberOfPackages", "1", packages[9].NumberOfPackages);
				AssertEquals("For tenth package expected filled SequenceNumber", "10", packages[9].SequenceNumber);
			});
		}

		public void TestGetPackagesListArrivalWhenSamePackageTypeAndMarks()
		{
			var goodsItem = SetUpForArrival();

			var package1 = goodsItem.Packages.AddNew();
			package1.B5_MarksAndNumbers = "marks";
			package1.B5_UnitType = "CT";
			package1.B5_UnitCount = 9;
			package1.B5_TypeOfDifference = "NEW";

			var package2 = goodsItem.Packages.AddNew();
			package2.B5_MarksAndNumbers = "marks";
			package2.B5_UnitType = "CT";
			package2.B5_UnitCount = 15;
			package2.B5_TypeOfDifference = "NEW";

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: package1.B5_SequenceNumber", (ZShort)1, package1.B5_SequenceNumber);
				AssertEquals("Prereq: package2.B5_SequenceNumber", (ZShort)2, package2.B5_SequenceNumber);

				var packages = NCTS5CommonPackagingWrapper.GetPackagesListArrival(goodsItem).ToList();
				AssertEquals("Expected filled 2 packages even if they have the same marks and type", 2, packages.Count);

				AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
				AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
				AssertEquals("For first package expected filled NumberOfPackages", "9", packages[0].NumberOfPackages);
				AssertEquals("For first package expected filled SequenceNumber", "1", packages[0].SequenceNumber);

				AssertEquals("For second package expected filled PackageType", "CT", packages[1].PackageType);
				AssertEquals("For second package expected filled Marks", "marks", packages[1].Marks);
				AssertEquals("For second package expected filled NumberOfPackages", "15", packages[1].NumberOfPackages);
				AssertEquals("For second package expected filled SequenceNumber", "2", packages[1].SequenceNumber);
			});
		}

		NctsArrivalCargoDesc SetUpForArrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			return bill.ArrivalGoodsItems.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Factory.SetBulkTypeHelper();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			goodsItem = bill.GoodsItems.AddNew();
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;

		protected override NCTS5CommonPackagingWrapper GetProvider() => new NCTS5CommonPackagingWrapper(Factory, InternalPackage1.Type, InternalPackage1.Marks, InternalPackage1.NumberOfPackages, 1);
	}
}
