using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(GlbExternalPasswordCUSValidation))]
	sealed class GlbExternalPasswordCUSValidationTest : GlbExternalPasswordValidationTest<GlbExternalPasswordCUS, GlbExternalPasswordCUSValidation>
	{
		public void TestCheckGP_PasswordType()
		{
			CombineAssertions(() =>
			{
				var sub = GlbExternalPassword;
				sub.GP_PasswordType = ZString.Empty;
				AssertHasErrorContaining(sub.GP_PasswordTypeInfo, MandatoryValidation.MustBeEntered);

				sub.GP_PasswordType = "AAA";
				AssertNoErrorContaining(sub.GP_PasswordTypeInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining(sub.GP_PasswordTypeInfo, ListValidation.InvalidCodeError);

				sub.GP_PasswordType = JPPasswordType.Codes.CUS;
				AssertNoErrors(sub.GP_PasswordTypeInfo);
			});
		}

		public void TestCheckGP_Transport()
		{
			CombineAssertions(() =>
			{
				var sub = GlbExternalPassword;
				var validation = sub.Validation;

				sub.GP_Transport = ZString.Empty;
				validation.ValidateGP_Transport();
				AssertHasErrorContaining(sub.GP_TransportInfo, MandatoryValidation.MustBeEntered);

				sub.GP_Transport = "AAA";
				AssertNoErrorContaining(sub.GP_TransportInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining(sub.GP_TransportInfo, ListValidation.InvalidCodeError);

				sub.GP_Transport = UserCodeSpecificTransportModeList.Codes.SEA;
				AssertNoErrors(sub.GP_TransportInfo);
			});
		}

		public void TestCheckGP_MailboxID()
		{
			CombineAssertions(() =>
			{
				var sub = GlbExternalPassword;
				sub.GP_MailBoxID = ZString.Empty;
				AssertHasErrorContaining(sub.GP_MailBoxIDInfo, MandatoryValidation.MustBeEntered);

				sub.GP_MailBoxID = "AAA";
				AssertNoErrorContaining(sub.GP_MailBoxIDInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining(sub.GP_MailBoxIDInfo, "must be exactly 5 characters long");

				sub.GP_MailBoxID = "12345";
				AssertNoErrors(sub.GP_MailBoxIDInfo);
			});
		}

		public void TestCheckGP_UserID()
		{
			CombineAssertions(() =>
			{
				var sub = GlbExternalPassword;
				sub.GP_UserID = ZString.Empty;
				AssertHasErrorContaining(sub.GP_UserIDInfo, MandatoryValidation.MustBeEntered);

				sub.GP_UserID = "AAAA";
				AssertNoErrorContaining(sub.GP_UserIDInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining(sub.GP_UserIDInfo, "must be exactly 3 characters long");

				sub.GP_UserID = "AAA";
				AssertNoErrorContaining(sub.GP_UserIDInfo, "must be exactly 3 characters long");
				AssertHasErrorContaining(sub.GP_UserIDInfo, "must end with a digit");

				sub.GP_UserID = "AA1";
				AssertNoErrors(sub.GP_UserIDInfo);
			});
		}

		public void TestCheckCurrentDecryptedPassword()
		{
			CombineAssertions(() =>
			{
				var sub = GlbExternalPassword;
				sub.CurrentDecryptedPassword = ZString.Empty;
				AssertHasErrorContaining(sub.CurrentDecryptedPasswordInfo, MandatoryValidation.MustBeEntered);

				sub.CurrentDecryptedPassword = "AAA";
				AssertNoErrorContaining(sub.CurrentDecryptedPasswordInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining(sub.CurrentDecryptedPasswordInfo, "must be exactly 8 characters long");

				sub.CurrentDecryptedPassword = "12345678";
				AssertNoErrors(sub.CurrentDecryptedPasswordInfo);
			});
		}

		public void TestValidateDuplicateConstraint()
		{
			var pw1 = Factory.New<GlbExternalPasswordCUS>();
			pw1.GP_GS = Factory.New<GlbStaff>().PK;
			pw1.GP_MailBoxID = "12345";
			pw1.GP_UserID = "123";

			var pw2 = Factory.New<GlbExternalPasswordCUS>();
			pw2.GP_GS = Factory.New<GlbStaff>().PK;
			pw2.GP_UserID = "123";
			pw2.GP_MailBoxID = "12345";

			CombineAssertions(() =>
			{
				pw2.RunPreSaveValidation();
				AssertHasRowErrorContaining(pw2, "already exists");

				pw2.GP_MailBoxID = "ABCDE";
				pw2.RunPreSaveValidation();
				AssertNoRowErrors(pw2);
			});
		}

		protected override GlbExternalPasswordCUS CreateNewGlbExternalPassword() => Factory.New<GlbExternalPasswordCUS>();
	}
}
