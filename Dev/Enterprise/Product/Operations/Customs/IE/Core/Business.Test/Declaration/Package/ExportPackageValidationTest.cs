using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExportPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCW_MarksAndNos_TransitionPeriodAES30()
		{
			var transitionPeriodMessage = "Shipping marks of packages can have up to 42 alpha numeric characters.";
			var message = "Shipping marks of packages can have up to 512 alpha numeric characters.";

			var declaration = Factory.New<JobDeclaration>();
			var package = (Package)declaration.Packages.AddNew();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				package.CW_MarksAndNos = new ZString('A', 43);
				AssertHasMessageError("TransitionPeriodAES30, Export, 43 characters", package.CW_MarksAndNosInfo, transitionPeriodMessage);
				package.CW_MarksAndNos = new ZString('A', 42);
				AssertNoMessageError("42 characters", package.CW_MarksAndNosInfo, transitionPeriodMessage);
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				package.CW_MarksAndNos = new ZString('A', 43);
				AssertNoMessageError("TransitionPeriodAES30, Import, 43 characters", package.CW_MarksAndNosInfo, transitionPeriodMessage);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				package.CW_MarksAndNos = new ZString('A', 512);
				AssertNoMessageError("Not TransitionPeriodAES30, Export, 512 characters", package.CW_MarksAndNosInfo, message);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				package.CW_MarksAndNos = new ZString('A', 513);
				AssertHasMessageError("Not TransitionPeriodAES30, Export, 513 characters", package.CW_MarksAndNosInfo, message);
			}
		}

		public void TestCheckCW_PackQty()
		{
			TestDataHelper.SetUpPackageTypes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			var package = (Package)declaration.Packages.AddNew();

			package.CW_PackType = "VG";
			package.CW_PackQty = 1;
			AssertHasMessageErrorContaining("CW_PackQty should be empty for BULK packages.", package.CW_PackQtyInfo, MandatoryValidation.DoNotEntered);

			package.CW_PackQty = 0;
			AssertNoMessageErrorContaining("CW_PackQty should be empty for BULK packages(validation passes).", package.CW_PackQtyInfo, MandatoryValidation.DoNotEntered);

			package.CW_PackType = "NE";
			package.Validation.ValidateCW_PackQty();
			AssertNoMessageErrors("CW_PackQty optional for BREAKBULK packages.", package.CW_PackQtyInfo);
			package.CW_PackQty = 1;
			AssertNoMessageErrors("CW_PackQty optional for BREAKBULK packages.", package.CW_PackQtyInfo);

			package.CW_PackType = "1A";
			package.CW_PackQty = 0;
			AssertNoMessageErrors("CW_PackQty optional for normal packages.", package.CW_PackQtyInfo);
			package.CW_PackQty = 1;
			AssertNoMessageErrors("CW_PackQty optional for normal packages.", package.CW_PackQtyInfo);
		}
	}
}
