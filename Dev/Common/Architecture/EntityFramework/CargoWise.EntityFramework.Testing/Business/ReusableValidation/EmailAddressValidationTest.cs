using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class EmailAddressValidationTest : TestCaseWithDummyForValidationTesting
	{
		public void TestCheckEntered()
		{
			AssertEquals(false, Dummy.HasErrors);
			Dummy.Z0_Description = "rarar";
			EmailAddressValidation.ValidateEmailAddress(Dummy.Z0_DescriptionInfo);
			AssertEquals("random string", true, Dummy.HasErrors);

			Dummy.Z0_Description = "alex@edi.com.au";
			EmailAddressValidation.ValidateEmailAddress(Dummy.Z0_DescriptionInfo);
			AssertEquals("correct email", false, Dummy.HasErrors);

			Dummy.Z0_Description = "alex@edi,com.au";
			EmailAddressValidation.ValidateEmailAddress(Dummy.Z0_DescriptionInfo);
			AssertEquals("comman instead of dot", true, Dummy.HasErrors);

			Dummy.Z0_Description = "alex:edi.com.au";
			EmailAddressValidation.ValidateEmailAddress(Dummy.Z0_DescriptionInfo);
			AssertEquals("incorrect characters", true, Dummy.HasErrors);

			Dummy.Z0_Description = "alex@domain.xyz";
			EmailAddressValidation.ValidateEmailAddress(Dummy.Z0_DescriptionInfo);
			AssertEquals("still allow non-existent domain", false, Dummy.HasErrors);

			Dummy.Z0_Description = "alex@domain.xyz; alex@edi.com.au";
			EmailAddressValidation.ValidateEmailAddress(Dummy.Z0_DescriptionInfo);
			AssertEquals("multiple email addresses in Outlook style are not supported", true, Dummy.HasErrors);

			Dummy.Z0_Description = "";
			EmailAddressValidation.ValidateEmailAddress(Dummy.Z0_DescriptionInfo);
			AssertEquals("Allow empty email address", false, Dummy.HasErrors);
		}

		public void TestIsEmailAddressValidAndNotEmpty()
		{
			AssertEquals("Empty email address", false, EmailAddressValidation.IsEmailAddressValidAndNotEmpty(""));
			AssertEquals("Empty email address", false, EmailAddressValidation.IsEmailAddressValidAndNotEmpty("  "));
			AssertEquals("Invalid email address", false, EmailAddressValidation.IsEmailAddressValidAndNotEmpty("test"));
			AssertEquals("Valid email address", true, EmailAddressValidation.IsEmailAddressValidAndNotEmpty("test@test.com"));
			AssertEquals("Empty email address", false, EmailAddressValidation.IsEmailAddressValidAndNotEmpty("."));
			AssertEquals("Empty email address", false, EmailAddressValidation.IsEmailAddressValidAndNotEmpty("@"));
			AssertEquals("Empty email address", false, EmailAddressValidation.IsEmailAddressValidAndNotEmpty("test@c"));
			AssertEquals("Empty email address", false, EmailAddressValidation.IsEmailAddressValidAndNotEmpty("test@"));
		}

		public void TestValidateEmailAddressesAsString()
		{
			AssertEquals(false, Dummy.HasErrors);

			Dummy.Z0_Description = "rarar";
			EmailAddressValidation.ValidateEmailAddressesAsString(Dummy.Z0_DescriptionInfo, DummyBizoSchema.Z0_Description);
			Assert("random string", Dummy.HasErrors);

			Dummy.Z0_Description = "zys924@188.com";
			EmailAddressValidation.ValidateEmailAddressesAsString(Dummy.Z0_DescriptionInfo, DummyBizoSchema.Z0_Description);
			Assert("single email", !Dummy.HasErrors);

			Dummy.Z0_Description = "   zys924@188.com, zys870924@gmail.com ";
			EmailAddressValidation.ValidateEmailAddressesAsString(Dummy.Z0_DescriptionInfo, DummyBizoSchema.Z0_Description);
			Assert("multiple emails", !Dummy.HasErrors);

			Dummy.Z0_Description = "";
			EmailAddressValidation.ValidateEmailAddressesAsString(Dummy.Z0_DescriptionInfo, DummyBizoSchema.Z0_Description);
			Assert("Allow empty email address", !Dummy.HasErrors);
		}

		public void TestIsEmailAddressValid_ValidEmails()
		{
			var validEmails = new[]
			{
				"neo@domain.com",
				"neo123@domain.com",
				"neo@mail.domain.com",
				"neo-user@domain.com",
				"neo.user@domain.com",
				"neo@domain-2.com",
				"neo@domain123.com",
				"",
				"  "
			};

			CombineAssertions(() =>
			{
				foreach (var email in validEmails)
				{
					AssertEquals($"The email address: {email} should be valid!", true, EmailAddressValidation.IsEmailAddressValid(email));
				}
			});
		}

		public void TestIsEmailAddressValid_InvalidEmails()
		{
			var invalidEmails = new[]
			{
				"randomstring",
				"@domain.com",
				"user@",
				"neo@domain",
				"neo@domain@domain123.com",
				"neo..user@domain.com",
				"neo@domain?com",
				"neo@domain,com"
			};

			CombineAssertions(() =>
			{
				foreach (var email in invalidEmails)
				{
					AssertEquals($"The email address: {email} should be invalid!", false, EmailAddressValidation.IsEmailAddressValid(email));
				}
			});
		}
	}
}
