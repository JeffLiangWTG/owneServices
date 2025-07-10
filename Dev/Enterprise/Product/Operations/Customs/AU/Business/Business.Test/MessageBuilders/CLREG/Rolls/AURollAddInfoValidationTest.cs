using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AURollAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZA_Roll()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper = new OrgHeaderWrapper(org);
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.ZA_IsExDocsUser = true;
			var roll = Factory.New<Roll>();
			dataProvider.Rolls.Add(roll);
			Factory.Save();

			roll.AddInfoValidation.ValidateZA_Roll();
			AssertHasMessageErrorContaining(roll.ZA_RollInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(dataProvider.ZA_IsExDocsUserInfo, AUCLREGInfoProviderAddInfoValidation.ExDocsUser);

			roll.ZA_Roll = "!";
			AssertHasMessageError(roll.ZA_RollInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(roll.ZA_RollInfo, MandatoryValidation.YouHaveNotEntered);

			roll = Factory.New<Roll>();
			roll.ZA_Roll = CMRClientRolls.Codes.Exporter;
			dataProvider.Rolls.Add(roll);
			AssertNoMessageError(roll.ZA_RollInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(dataProvider.ZA_IsExDocsUserInfo, AUCLREGInfoProviderAddInfoValidation.ExDocsUser);
		}
	}
}
