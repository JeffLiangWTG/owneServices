using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPackagePhase4ValidationBaseOnlyTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB5_UnitCount_CannotBeZero_Consolidated()
		{
			var bulkType = Factory.SetupBulkCusCode();
			const string errorMessage = MandatoryValidation.ValueCannotBeZero;

			CombineAssertions(() =>
			{
				nctsPackage.B5_UnitCount = 0;
				nctsPackage.B5_MarksAndNumbers = "mn1";
				nctsPackage.B5_UnitType = "AA";
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertHasMessageErrorContaining("B5_UnitType isn't bulk, one package", nctsPackage.B5_UnitCountInfo, errorMessage);

				var nctsPackage2 = goodsItem.Packages.AddNew();
				nctsPackage2.B5_UnitType = "AA";
				nctsPackage2.B5_MarksAndNumbers = "mn1";
				nctsPackage.B5_B5_ParentPackage = nctsPackage2.PK;

				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoMessageErrorContaining("B5_UnitType isn't bulk, consolidated package which has a Parent Pack with same Marks & Numbers", nctsPackage.B5_UnitCountInfo, errorMessage);

				nctsPackage2.B5_MarksAndNumbers = "mn2";
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertHasMessageErrorContaining("B5_UnitType isn't bulk, different Marks & Numbers", nctsPackage.B5_UnitCountInfo, errorMessage);

				nctsPackage.B5_UnitType = bulkType;
				nctsPackage.Validation.ValidateB5_UnitCount();
				AssertNoWarningContaining("B5_UnitType is bulk", nctsPackage.B5_UnitCountInfo, errorMessage);
			});
		}

		public void TestAllowZeroUnitForItemWithSameUnitTypeAndMarksAndNumbers()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 0;
			package1.B5_UnitType = "1A";
			package1.B5_MarksAndNumbers = "M-1234";
			AssertHasNotifications(package1.B5_UnitCountInfo);

			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_UnitCount = 10;
			package2.B5_UnitType = "1A";
			package2.B5_MarksAndNumbers = "M-1234";

			package1.Validation.ValidateB5_UnitCount();
			package2.Validation.ValidateB5_UnitCount();
			AssertNoNotifications(package1.B5_UnitCountInfo);
			AssertNoNotifications(package2.B5_UnitCountInfo);

			goodsItem1.Packages.RemoveFromRelationship(package2);
			package1.Validation.ValidateB5_UnitCount();
			AssertHasNotifications(package1.B5_UnitCountInfo);

			// Put the non-zero package in another item
			var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem2.Packages.Add(package2);

			package1.Validation.ValidateB5_UnitCount();
			package2.Validation.ValidateB5_UnitCount();
			AssertNoNotifications(package1.B5_UnitCountInfo);
			AssertNoNotifications(package2.B5_UnitCountInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovementHeader = nctsHeader.MovementHeader;
			goodsItem = departureMovementHeader.GoodsItems.AddNew();
			nctsPackage = goodsItem.Packages.AddNew();
		}
		NctsDepartureCargoDesc goodsItem;
		NctsPackage nctsPackage;
	}
}
