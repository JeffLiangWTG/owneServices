using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(GlbLoginPassword))]
sealed class GlbLoginPasswordTest : MasterFiles.Business.Testing.GlbExternalPasswordTest<GlbLoginPassword>
{
	public void TestSetDefaultValues()
	{
		AssertEquals("INC", GlbExternalPassword.GP_PasswordType);
	}

	public void TestCaptions()
	{
		var password = GlbExternalPassword;
		CombineAssertions(() =>
		{
			AssertCaption(password.GP_UserIDInfo, caption: "ICEGATE Login Id");
			AssertCaption(password.CurrentDecryptedPasswordInfo, caption: "Password");
			AssertCaption(password.GP_MailBoxIDInfo, caption: "ICEGATE Email Id");
			AssertCaption(password.AutoGenerateEmailIdInfo, caption: "ICEGATE Email Id");
			AssertCaption(password.NeedCopyOfEmailsInfo, caption: "Need Copy of Emails?", mediumCaption: "Need Copy?", shortCaption: "Need Copy?", fullDescription: "The email ID mentioned here will receive a copy of each outgoing/incoming CW1 email specified under the ICEGATE email ID.");
		});
	}

	public void TestMaxLengths()
	{
		var password = GlbExternalPassword;
		CombineAssertions(() =>
		{
			AssertEquals("Max length for GP_UserID", GlbLoginPassword.Schema.LoginIDMaxLength, password.GP_UserIDInfo.MaxLength);
			AssertEquals("Max length for CurrentDecryptedPassword", GlbLoginPassword.Schema.PasswordMaxLength, password.CurrentDecryptedPasswordInfo.MaxLength);
			AssertEquals("Max length for GP_MailBoxID", GlbLoginPassword.Schema.EmailIDMaxLength, password.GP_MailBoxIDInfo.MaxLength);
		});
	}

	public void TestCopyToMailBox()
	{
		CombineAssertions(() =>
		{
			var password = GlbExternalPassword;
			AssertEquals("Before Main address", ZString.Empty, password.CopyToMailBox);

			var email = Staff.EmailAddresses.AddNew();
			email.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;
			email.GSE_EmailAddress = "user@wtg.in";
			AssertEquals("After Main address and dont need copy", ZString.Empty, password.CopyToMailBox);

			password.NeedCopyOfEmails = true;
			AssertEquals("After Main address and Need copy", "user@wtg.in", password.CopyToMailBox);
		});
	}

	public void TestNeedCopyOfEmails()
	{
		CombineAssertions(() =>
		{
			var password = GlbExternalPassword;
			password.SetSystemDefinedValue(GlbLoginPassword.Schema.NeedCopyOfEmails, ZBool.True);
			AssertEquals("Set GenAddOnColumn", ZBool.True, password.NeedCopyOfEmails);
			password.NeedCopyOfEmails = ZBool.False;
			AssertEquals("Set NeedCopyOfEmails", ZBool.False, password.GetSystemDefinedValue<ZBool>(GlbLoginPassword.Schema.NeedCopyOfEmails));
		});
	}

	public void TestAutoGenerateEmailId()
	{
		var password = GlbExternalPassword;
		var pm1 = Factory.NewWithValidTestData<GlbStaff>();
		pm1.GS_Code = "PM1";
		pm1.GS_EmailAddress = "pm1@test.com";

		var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
		var company = Factory.New<GlbCompany>();
		registrationKey.EnterpriseCodeForTest = "ENT";
		company.GC_Code = "CMP";
		registrationKey.ServerCodeForTest = "SVR";
		password.GP_GS = pm1.PK;
		password.GP_GC = company.PK;
		CombineAssertions(() =>
		{
			password.AutoGenerateEmailId = ZBool.True;
			AssertEquals("Should generate mailbox id after check", $"ENTCMPSVR_PM1@ic.wisegrid.net", password.GP_MailBoxID);
			password.AutoGenerateEmailId = ZBool.False;
			AssertEquals("Should clear mailbox id after ucheck", ZString.Empty, password.GP_MailBoxID);
		});
	}

	public void TestGP_MailBoxID_Readonly()
	{
		Assert(GlbExternalPassword.GP_MailBoxIDInfo.ReadOnly);
	}

	void AssertCaption(ZPropertyInfo propInfo, string caption, string mediumCaption = "", string shortCaption = "", string fullDescription = "")
	{
		var name = propInfo.Name;
		var resData = DataBoundResourceStrings.GetDataForProperty(propInfo);
		AssertEquals($"{name} - Caption", caption, resData.Caption);
		AssertEquals($"{name} - MediumCaption", mediumCaption, resData.MediumCaption);
		AssertEquals($"{name} - ShortCaption", shortCaption, resData.ShortCaption);
		AssertEquals($"{name} - FullDescription", fullDescription, resData.FullDescription);
	}
}
