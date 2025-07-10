using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BiReportCredential))]
	sealed class BiReportCredentialTest : RegistryBusinessObjectTemplateTestCase<BiReportCredential>
	{
		public void TestValidateDomain()
		{
			var credential = new BiReportCredential();

			// No changes, validation doesn't run.
			credential.Domain = "";
			AssertNoNotifications(credential.DomainInfo);

			// Non-empty value entered. No error.
			credential.Domain = "ABCD";
			AssertNoNotifications(credential.DomainInfo);

			// Value changed to empty. Should have an error.
			credential.Domain = "";
			AssertHasErrorContaining(credential.DomainInfo, MandatoryValidation.MustBeEntered);

			// New non-empty value entered. No error.
			credential.Domain = "XYZ";
			AssertNoNotifications(credential.DomainInfo);
		}

		public void TestValidateUserName()
		{
			var credential = new BiReportCredential();

			// No changes, validation doesn't run.
			credential.UserName = "";
			AssertNoNotifications(credential.UserNameInfo);

			// Non-empty value entered. No error.
			credential.UserName = "1234";
			AssertNoNotifications(credential.UserNameInfo);

			// Value changed to empty. Should have an error.
			credential.UserName = "";
			AssertHasErrorContaining(credential.UserNameInfo, MandatoryValidation.MustBeEntered);

			// New non-empty value entered. No error.
			credential.UserName = "567";
			AssertNoNotifications(credential.UserNameInfo);
		}

		public void TestValidatePassword()
		{
			var credential = new BiReportCredential();

			// No changes, validation doesn't run.
			credential.Password = "";
			AssertNoNotifications(credential.PasswordInfo);

			// Non-empty value entered. No error.
			credential.Password = "12343";
			AssertNoErrorContaining(credential.PasswordInfo, MandatoryValidation.MustBeEntered);

			// Value changed to empty. Should have an error.
			credential.Password = "";
			AssertHasErrorContaining(credential.PasswordInfo, MandatoryValidation.MustBeEntered);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BiReportCredential GetBusinessObjectToClone()
		{
			return new BiReportCredential();
		}

		protected override BiReportCredential GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
