namespace CargoWise.EntityFramework.Testing
{
	sealed class SkypeIdValidationTest : TestCaseWithDummyForValidationTesting
	{
		public void TestValidateSkypeId()
		{
			var validation = new SkypeIdValidation();

			Dummy.Z0_Description = "a.n,d-r_ew";
			validation.ValidateSkypeId(Dummy.Z0_DescriptionInfo);
			AssertNoErrors("Should be no errors when valid Skype name", Dummy.Z0_DescriptionInfo);

			Dummy.Z0_Description = "andrew.luong@wisetechglobal.com";
			validation.ValidateSkypeId(Dummy.Z0_DescriptionInfo);
			AssertNoErrors("Should be no errors when valid email address", Dummy.Z0_DescriptionInfo);

			Dummy.Z0_Description = ".n,d-r_ew";
			validation.ValidateSkypeId(Dummy.Z0_DescriptionInfo);
			AssertHasError("Skype name must start with a letter", Dummy.Z0_DescriptionInfo, "Not a valid Skype name nor email address.");

			Dummy.Z0_Description = "andrew,luong@wisetechglobal,com";
			validation.ValidateSkypeId(Dummy.Z0_DescriptionInfo);
			AssertHasError(Dummy.Z0_DescriptionInfo, "Not a valid Skype name nor email address.");
		}
	}
}
