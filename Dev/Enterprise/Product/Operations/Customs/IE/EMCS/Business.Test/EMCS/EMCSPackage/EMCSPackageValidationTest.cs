using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	class EMCSPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB5_UnitCount_Mandatory()
		{
			CombineAssertions(() =>
			{
				package.B5_UnitType = "AE";
				package.Validation.ValidateB5_UnitCount();
				AssertHasMessageError(package.B5_UnitCountInfo, "No of Packages can't be 0.");

				package.B5_UnitType = "VQ";
				package.Validation.ValidateB5_UnitCount();
				AssertNoMessageError(package.B5_UnitCountInfo, "No of Packages can't be 0.");
			});
		}

		public void TestCheckB5_MarksAndNumbers_Mandatory()
		{
			CombineAssertions(() =>
			{
				package.B5_UnitType = "AE";
				package.Validation.ValidateB5_MarksAndNumbers();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(package.B5_MarksAndNumbersInfo);

				package.B5_UnitType = "VQ";
				package.Validation.ValidateB5_MarksAndNumbers();
				ValidationTestHelper.AssertFieldIsNotMandatory(package.B5_MarksAndNumbersInfo);
			});
		}

		void CreateCountableUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "EMCS Pack Types");

			var cusCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "AE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Countable, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCode.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Countable, RefCusCodeListAttributeTypes.Codes.Countable);

			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		protected override void SetUp()
		{
			CreateCountableUQ();
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			package = (EMCSPackage)declaration.EMCSPackages.AddNew();
		}
		EMCSPackage package;
		EMCSJobDeclaration declaration;
	}
}
