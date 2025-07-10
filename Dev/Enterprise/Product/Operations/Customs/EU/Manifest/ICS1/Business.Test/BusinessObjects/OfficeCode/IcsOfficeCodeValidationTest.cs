namespace Enterprise.Customs.EU.Manifest.Business
{
	using CargoWise.EntityFramework.Testing;

	public class IcsOfficeCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOnlyOneOfficeOfThisTypeExists()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var office1 = header.EUCustomsOffices.AddNew(OfficeCodes_ICS.Codes.OfficeOfActualEntryDiversion);
			office1.Validation.ValidateCY_Code();
			AssertNoError(office1.CY_CodeInfo, "Only one office of type OOA is allowed");

			var office2 = header.EUCustomsOffices.AddNew(OfficeCodes_ICS.Codes.OfficeOfActualEntryDiversion);
			AssertHasError(office2.CY_CodeInfo, "Only one office of type OOA is allowed");
			office1.Validation.ValidateCY_Code();
			AssertHasError(office1.CY_CodeInfo, "Only one office of type OOA is allowed");

			office2.CY_Code = OfficeCodes_ICS.Codes.OfficeOfFirstEntry;
			AssertNoError(office2.CY_CodeInfo, "Only one office of type OOA is allowed");
			office1.Validation.ValidateCY_Code();
			AssertNoError(office1.CY_CodeInfo, "Only one office of type OOA is allowed");
		}
	}
}
