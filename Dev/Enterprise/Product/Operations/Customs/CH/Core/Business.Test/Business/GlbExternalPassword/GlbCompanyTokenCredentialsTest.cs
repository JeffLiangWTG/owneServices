using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.MessagingConstants;
using CHPasswordStatusList = Enterprise.Customs.CH.Business.UniversalReferenceConstants.PasswordStatusList;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(GlbCompanyTokenCredentials))]
class GlbCompanyTokenCredentialsTest : GlbExternalPasswordTest<GlbCompanyTokenCredentials>
{
	public void TestReadOnly() => CombineAssertions(() =>
	{
		GlbExternalPassword.GP_PasswordStatus = ZString.Empty;
		AssertReadOnly(false);

		GlbExternalPassword.GP_PasswordStatus = CHPasswordStatusList.Suspended;
		AssertReadOnly(true);

		void AssertReadOnly(bool expectedReadOnly)
		{
			AssertEquals($"GP_PasswordStatus={GlbExternalPassword.GP_PasswordStatus} GP_CertificateTextInfo.ReadOnly", expectedReadOnly, GlbExternalPassword.GP_CertificateTextInfo.ReadOnly);
			AssertEquals($"GP_PasswordStatus={GlbExternalPassword.GP_PasswordStatus} RefreshTokenInfo.ReadOnly", expectedReadOnly, GlbExternalPassword.RefreshTokenTextInfo.ReadOnly);
			AssertEquals($"GP_PasswordStatus={GlbExternalPassword.GP_PasswordStatus} GP_UserIDInfo.ReadOnly", expectedReadOnly, GlbExternalPassword.GP_UserIDInfo.ReadOnly);
			AssertEquals($"GP_PasswordStatus={GlbExternalPassword.GP_PasswordStatus} CurrentDecryptedPasswordInfo.ReadOnly", expectedReadOnly, GlbExternalPassword.CurrentDecryptedPasswordInfo.ReadOnly);
		}
	});

	public void TestIsSuspended() => CombineAssertions(() =>
	{
		GlbExternalPassword.GP_PasswordStatus = ZString.Empty;
		AssertEquals($"GP_PasswordStatus={GlbExternalPassword.GP_PasswordStatus}", false, GlbExternalPassword.IsSuspended);

		GlbExternalPassword.GP_PasswordStatus = CHPasswordStatusList.Suspended;
		AssertEquals($"GP_PasswordStatus={GlbExternalPassword.GP_PasswordStatus}", true, GlbExternalPassword.IsSuspended);

		GlbExternalPassword.GP_PasswordStatus = CHPasswordStatusList.Received;
		AssertEquals($"GP_PasswordStatus={GlbExternalPassword.GP_PasswordStatus}", false, GlbExternalPassword.IsSuspended);

		GlbExternalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
		AssertEquals($"GP_PasswordStatus={GlbExternalPassword.GP_PasswordStatus}", false, GlbExternalPassword.IsSuspended);
	});

	public void TestGP_CertificateText()
	{
		AssertEquals("MaxLength", 8000, GlbExternalPassword.GP_CertificateTextInfo.MaxLength);
	}

	[TestDate]
	public void TestGP_ExpireDate()
	{
		CombineAssertions(() =>
			{
				AssertEquals("Before OnSaving GP_ExpiryDate should be empty", ZDateTime.Empty, GlbExternalPassword.GP_ExpiryDate);

				GlbExternalPassword.GP_Certificate = new ZBlob(new byte[] { 1, 2, 3 });
				Factory.Save();
				AssertEquals("After OnSaving GP_ExpiryDate should be now + 1 hour when GP_Certificate changed to not empty", ZDateTime.UtcNow.AddHours(1), GlbExternalPassword.GP_ExpiryDate);

				TestDateAttribute.AddHours(2);

				var existingExpiryDate = GlbExternalPassword.GP_ExpiryDate;
				GlbExternalPassword.GP_Certificate = new ZBlob(new byte[] { 1, 2, 3 });
				GlbExternalPassword.GP_UserID = "123";
				Factory.Save();
				AssertEquals("After OnSaving GP_ExpiryDate should be same when GP_Certificate not changed", existingExpiryDate, GlbExternalPassword.GP_ExpiryDate);

				GlbExternalPassword.GP_Certificate = new ZBlob(new byte[] { 2, 3, 4 });
				Factory.Save();
				AssertEquals("After OnSaving GP_ExpiryDate should be now + 1 hour when GP_Certificate changed to not empty", ZDateTime.UtcNow.AddHours(1), GlbExternalPassword.GP_ExpiryDate);

				GlbExternalPassword.GP_Certificate = new ZBlob(new byte[] { 3, 4, 5 });
				GlbExternalPassword.ShouldUpdateExpiryDateOnSaving = false;
				existingExpiryDate = GlbExternalPassword.GP_ExpiryDate;
				Factory.Save();
				AssertEquals("When ShouldUpdateExpiryDateOnSaving is false, GP_ExpiryDate shouldn't change", existingExpiryDate, GlbExternalPassword.GP_ExpiryDate);

				GlbExternalPassword.GP_Certificate = ZBlob.Empty;
				GlbExternalPassword.ShouldUpdateExpiryDateOnSaving = true;
				Factory.Save();
				AssertEquals("After OnSaving GP_ExpiryDate should be empty when GP_Certificate changed to empty", ZDateTime.Empty, GlbExternalPassword.GP_ExpiryDate);
			});
	}

	public void TestGP_UserID()
	{
		AssertEquals("MaxLength", 256, GlbExternalPassword.GP_UserIDInfo.MaxLength);
	}

	public void TestGP_ExpiryDate()
	{
		Assert("ReadOnly", GlbExternalPassword.GP_ExpiryDateInfo.ReadOnly);
	}

	public void TestShouldUpdateExpiryDateOnSavingDefaultValue()
	{
		Assert("ShouldUpdateExpiryDateOnSaving default value", GlbExternalPassword.ShouldUpdateExpiryDateOnSaving);
	}

	public void TesValidation()
	{
		AssertType<GlbCompanyTokenCredentialsValidation>(GlbExternalPassword.Validation);
	}

	public void TestSetDefaultValues()
	{
		AssertEquals(PasswordTypesList.Codes.CHT, GlbExternalPassword.GP_PasswordType);
	}

	public void TestGetMessageAttrDictionary()
	{
		var company = CreateCompany(accessToken: "access");
		var tokenCredentials = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).TokenCredentials;
		using (DisposableEnvironment.ForCompany(company.GC_Code))
		{
			var expected = new Dictionary<string, string>
				{
					{ CustomMsgAttributes.AccessToken, "access" },
				};
			AssertContainsExactElementsInAnyOrder(expected, tokenCredentials.GetMessageAttrDictionary());
		}
	}

	public void TestTokenRefresh()
	{
		var company = CreateCompany(accessToken: "access");
		var tokenCredentials = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).TokenCredentials;

		AssertNullOrEmpty(tokenCredentials.RefreshTokenText);
		Factory.Save();

		var refreshToken = GetRefreshToken(company.PK);
		refreshToken.GP_Certificate = ZBlob.FromAscii("refresh token");
		AssertEquals("Refresh Token from GlbexternalPassword CHR", refreshToken.GP_Certificate.ToAscii(), tokenCredentials.RefreshTokenText);
	}

	GlbCompany CreateCompany(string accessToken)
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_Code = "CHC";
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;

		var branch = Factory.NewWithValidTestData<GlbBranch>();
		branch.GB_Code = "CHB";
		branch.GB_GC = company.PK;
		branch.GB_RL_NKHomePort = "AUSYD";

		if (accessToken != null)
		{
			var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			companyWrapper.TokenCredentialsEnabled = true;
			var token = companyWrapper.TokenCredentials;
			token.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			token.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			token.GP_CertificateText = accessToken;
		}

		Factory.Save();
		return company;
	}

	GlbExternalPassword GetRefreshToken(ZGuid companyPk)
	{
		ZQuery zQuery = new ZQuery(GlbExternalPasswordSchema.GP_GC, companyPk);
		zQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.CHR);
		zQuery.OrderBy = AutoGlbExternalPassword.Schema.GP_SystemCreateTimeUtc;
		return base.Factory.LoadTop1<GlbExternalPassword>(zQuery);
	}
}
