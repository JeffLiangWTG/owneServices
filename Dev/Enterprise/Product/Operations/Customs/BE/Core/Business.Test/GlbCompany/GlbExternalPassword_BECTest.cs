using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(GlbExternalPassword_BEC))]
sealed class GlbExternalPassword_BECTest : GlbExternalPasswordTest<GlbExternalPassword_BEC>
{
	protected override GlbExternalPassword_BEC CreateNewGlbExternalPassword(BusinessObjectFactory factory) => factory.New<GlbExternalPassword_BEC>();

	public void TestSetDefaultValues()
	{
		var password = Factory.New<GlbExternalPassword_BEC>();
		CombineAssertions(() =>
		{
			AssertEquals(PasswordTypesList.Codes.BEC, password.GP_PasswordType);
			AssertNotEquals(ZGuid.Empty, password.GP_GC);
		});
	}

	public void TestGetMessageAttrDictionary()
	{
		var password = Factory.New<GlbExternalPassword_BEC>();
		password.GP_UserID = "user";
		password.CurrentDecryptedPassword = "password";
		var expected = new Dictionary<string, string>
		{
			{ xTMessaging.Shared.Constants.xTMsgAttributes.Oauth2ClientID, "user" },
			{ xTMessaging.Shared.Constants.xTMsgAttributes.Oauth2ClientSecret, "password" },
		};
		AssertContainsExactElementsInAnyOrder(expected, password.GetMessageAttrDictionary());
	}
}
