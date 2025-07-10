using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Testing
{
	class SealNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var test = Factory.New<SealNumber>();
			test.Validation.ValidateCY_Code();
			AssertNoMessageErrors(test.CY_CodeInfo);
		}

		public void TestCheckCY_Data()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryInstructions.AddNew();
			var item1 = entry.Seals.AddNew();

			item1.CY_Data = string.Empty;
			AssertHasNotifications(item1.CY_DataInfo);
			item1.CY_Data = "123a";
			AssertNoMessageErrors(item1.CY_DataInfo);

			var item2 = entry.Seals.AddNew();
			item2.CY_Data = "123a";
			AssertHasMessageError(item2.CY_DataInfo, "Seal number with specified name already exists.");
		}
	}
}
