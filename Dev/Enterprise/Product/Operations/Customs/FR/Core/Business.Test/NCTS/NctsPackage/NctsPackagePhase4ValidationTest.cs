using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	sealed class NctsPackagePhase4ValidationTest : TestCaseWithFactory
	{
		public void TestCheckB5_UnitCount_min_Zero()
		{
			package.B5_UnitType = "AA";
			CombineAssertions(() =>
			{
				package.B5_UnitCount = -1;
				AssertHasErrorContaining("can't be negative", package.B5_UnitCountInfo, MandatoryValidation.ValueCannotBeNegative);

				package.B5_UnitCount = 0;
				AssertNoMessageErrors("Count 0", package.B5_UnitCountInfo);
			});
		}

		public void TestCheckB5_UnitType_isMandatory()
		{
			var packType = EU.NCTS.Business.Testing.NctsPackageTestHelper.SetupUnpackCusCode(Factory);

			package.B5_UnitType = packType;
			AssertNoMessageErrorContaining(package.B5_UnitTypeInfo, ListValidation.InvalidCodeMessageError);

			package.B5_UnitType = "~Z";
			AssertHasMessageErrorContaining(package.B5_UnitTypeInfo, ListValidation.InvalidCodeMessageError);

			package.B5_UnitType = "";
			AssertHasMessageErrorContaining(package.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsDepartureMovementHeader = nctsHeader.MovementHeader;
			nctsDepartureCargoDesc = nctsDepartureMovementHeader.GoodsItems.AddNew();
			package = nctsDepartureCargoDesc.Packages.AddNew();
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader nctsDepartureMovementHeader;
		NctsDepartureCargoDesc nctsDepartureCargoDesc;
		NctsPackage package;
	}
}
