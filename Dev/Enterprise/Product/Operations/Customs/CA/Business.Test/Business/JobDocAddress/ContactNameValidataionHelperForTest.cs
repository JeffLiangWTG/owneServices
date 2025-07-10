using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	public static class ContactNameValidataionHelperForTest
	{
		public static void AssertE2_Contact(JobDocAddress docAddress)
		{
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Contact = "赵四!|";
			TestCaseWithFactory.AssertHasMessageErrorContaining("should not has invalid characters", docAddress.E2_ContactInfo, "The following characters are not allowed in the contact name:");
			TestCaseWithFactory.AssertHasMessageErrorContaining("should not has invalid characters", docAddress.E2_ContactInfo, "The following characters are not allowed in the contact name: '赵','四','!','|'");
			docAddress.E2_Contact = "abc@";
			TestCaseWithFactory.AssertNoMessageErrorContaining("No messsage error", docAddress.E2_ContactInfo, "The following characters are not allowed in the contact name:");
		}
	}
}
