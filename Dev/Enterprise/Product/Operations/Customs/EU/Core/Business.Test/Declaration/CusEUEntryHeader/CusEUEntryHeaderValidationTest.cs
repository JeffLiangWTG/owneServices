using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	internal class CusEUEntryHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEUH_CH()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var euEntryHeader = entryHeader.AddInfoChild;
			Factory.Save();

			euEntryHeader.Validation.ValidateEUH_CH();
			AssertNoErrors("Should not add error message.", euEntryHeader.EUH_CHInfo);

			var newEUEntryHeader = Factory.New<CusEUEntryHeader>();
			newEUEntryHeader.EUH_CH = entryHeader.PK;
			AssertHasError(newEUEntryHeader.EUH_CHInfo, "There is another record linked to the same Entry Header.");
		}
	}
}
