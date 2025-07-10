using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsPackagePhase4ValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckB5_UnitCount_Mandatory()
	{
		var bulkType = Factory.SetupBulkCusCode();
		var unpackType = Factory.SetupUnpackCusCode();

		AssertNoMessageErrorContaining("Is valid when UnitType = Unpack and UnitCount > 0", package.B5_UnitCountInfo, MandatoryValidation.ValueCannotBeZero);
		AssertNoMessageErrorContaining("Check no errors when UnitType isUnpack and UnitCount = 0", package.B5_UnitCountInfo, ValidationCaptions.NctsPackage.UnitCountMustBeEmpty);
		AssertNoMessageErrorContaining("Check no errors when UnitType >= 0", package.B5_UnitCountInfo, MandatoryValidation.ValueCannotBeNegative);

		package.B5_UnitCount = -10;
		package.Validation.ValidateB5_UnitCount();
		AssertHasMessageErrorContaining("UnitCount must be positive", package.B5_UnitCountInfo, MandatoryValidation.ValueCannotBeNegative);

		package.B5_UnitCount = 0;
		package.B5_UnitType = unpackType;
		package.Validation.ValidateB5_UnitCount();
		AssertHasMessageErrorContaining("When UnitType = Unpack, UnitCount cannot be 0", package.B5_UnitCountInfo, MandatoryValidation.ValueCannotBeZero);
		package.B5_UnitCount = 10;
		AssertNoMessageErrorContaining("Is valid when UnitType = Unpack and UnitCount > 0", package.B5_UnitCountInfo, MandatoryValidation.ValueCannotBeZero);

		package.B5_UnitType = bulkType;
		package.Validation.ValidateB5_UnitCount();
		AssertHasMessageErrorContaining("When UnitType isBulk, Unit count must be empty", package.B5_UnitCountInfo, ValidationCaptions.NctsPackage.UnitCountMustBeEmpty);
		package.B5_UnitCount = 0;
		package.Validation.ValidateB5_UnitCount();
		AssertNoMessageErrorContaining("Check no errors when UnitType isUnpack and UnitCount = 0", package.B5_UnitCountInfo, ValidationCaptions.NctsPackage.UnitCountMustBeEmpty);
	}

	public void TestCheckB5_MarksAndNumbers_Mandatory()
	{
		var bulkType = Factory.SetupBulkCusCode();
		var unpackType = Factory.SetupUnpackCusCode();

		package.B5_UnitType = unpackType;
		package.Validation.ValidateB5_MarksAndNumbers();
		AssertNoMessageErrorContaining("When UnitType isUnpacked, Marks and Numbers is optional", package.B5_MarksAndNumbersInfo, "You have not entered");

		package.B5_UnitType = bulkType;
		AssertNoMessageErrorContaining("When UnitType isBulk, Marks and Numbers is optional", package.B5_MarksAndNumbersInfo, "You have not entered");

		package.B5_UnitType = "CT";
		package.Validation.ValidateB5_MarksAndNumbers();
		AssertHasMessageErrorContaining("When UnitType isNotBulk and isNotUnpacked, Marks and Numbers is required", package.B5_MarksAndNumbersInfo, "You have not entered");
		package.B5_MarksAndNumbers = "123456";
		package.Validation.ValidateB5_MarksAndNumbers();
		AssertNoMessageErrorContaining("When UnitType isNotBulk and isNotUnpacked, Marks and Numbers is required", package.B5_MarksAndNumbersInfo, "You have not entered");
	}

	public void TestCheckB5_UnitType_Mandatory()
	{
		package.Validation.ValidateB5_UnitType();
		AssertHasMessageErrorContaining("Unit type is required", package.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);
		package.B5_UnitType = "PAC";
		AssertNoMessageErrorContaining("Unit type is valid", package.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestNumberOfPackagesLineValidation()
	{
		var expectedError = "Only 1 line of Packages is allowed in the ET message";

		package.Validation.ValidateAll();
		AssertNoRowErrorContaining(package, expectedError);

		var package2 = nctsDepartureCargoDesc.Packages.AddNew();

		package.Validation.ValidateAll();
		package2.Validation.ValidateAll();
		CombineAssertions("When goods items contains more than one package", () =>
		{
			AssertHasRowErrorContaining(package, expectedError);
			AssertHasRowErrorContaining(package2, expectedError);
		});

		nctsDepartureCargoDesc.Packages.RemoveFromRelationship(package2);
		package.Validation.ValidateAll();
		AssertNoRowErrorContaining(package, expectedError);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsDepartureMovementHeader = nctsHeader.MovementHeader;
		nctsDepartureCargoDesc = nctsDepartureMovementHeader.GoodsItems.AddNew();
		package = (NctsPackage)nctsDepartureCargoDesc.Packages.AddNew();
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader nctsDepartureMovementHeader;
	NctsDepartureCargoDesc nctsDepartureCargoDesc;
	NctsPackage package;
}
