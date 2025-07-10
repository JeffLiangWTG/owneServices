using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(GlbMauExternalPassword))]
sealed class GlbMauExternalPasswordTest : GlbExternalPasswordTest<GlbMauExternalPassword>
{
	public override void TestSetDefaultValues()
	{
		base.TestSetDefaultValues();

		AssertEquals("GP_GS", ZGuid.Empty, GlbExternalPassword.GP_GS);
	}

	public void TestLookups()
	{
		AssertType<GlbMauExternalPasswordLookups>(nameof(GlbExternalPassword.Lookups), GlbExternalPassword.Lookups);
	}

	public void TestValidation()
	{
		AssertType<GlbMauExternalPasswordValidation>(nameof(GlbExternalPassword.Validation), GlbExternalPassword.Validation);
	}

	public void TestShouldSendCredential()
	{
		var glbMauExternalPassword = Factory.New<GlbMauExternalPassword_ITForTest>();
		AssertEquals("ShouldSendCredential", false, glbMauExternalPassword.ShouldSendCredentialExposed());
	}

	public void TestGetMessageAttrDictionary()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var mauExternalPassword = companyWrapper.PasswordCollection.AddNew();
		mauExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		mauExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		AssertEquals("GlbMauExternalPassword must implement IxTMessageAttributeProvider", true, mauExternalPassword is IxTMessageAttributeProvider);
		var xtMessageAttributeProvider = mauExternalPassword as IxTMessageAttributeProvider;

		var messageAttrDictionary = xtMessageAttributeProvider.GetMessageAttrDictionary();
		AssertNotNull(messageAttrDictionary);

		messageAttrDictionary.TryGetValue("cw1.key", out var key);
		AssertEquals("key", X509Certificate2TestHelper.ValidCertificate_KeyPEM, key);

		messageAttrDictionary.TryGetValue("cw1.certificate", out var certificate);
		AssertEquals("certificate", X509Certificate2TestHelper.ValidCertificate_CertPEM, certificate);
	}

	public void TestGP_UserIDResourceStringData()
	{
		var mauExternalPassword = CreateNewGlbExternalPassword(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(mauExternalPassword.GP_UserIDInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Internal Code", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Int. Code", resourceStringData.ShortCaption);
		});
	}

	public void TestGP_UserIDMaxLength()
	{
		var mauExternalPassword = CreateNewGlbExternalPassword(Factory);
		AssertEquals("GP_UserID MaxLength", 20, mauExternalPassword.GP_UserIDInfo.MaxLength);
	}

	public void TestGP_NameResourceStringData()
	{
		var mauExternalPassword = CreateNewGlbExternalPassword(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(mauExternalPassword.GP_NameInfo);
		AssertEquals("Caption", "Declarant", resourceStringData.Caption);
	}

	public void TestGP_MailBoxIDResourceStringData()
	{
		var mauExternalPassword = CreateNewGlbExternalPassword(Factory);
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(mauExternalPassword.GP_MailBoxIDInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Authorized User", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Auth. User", resourceStringData.ShortCaption);
		});
	}

	public void TestGP_MailBoxIDMaxLength()
	{
		var mauExternalPassword = CreateNewGlbExternalPassword(Factory);
		AssertEquals("GP_MailBoxID MaxLength", 20, mauExternalPassword.GP_MailBoxIDInfo.MaxLength);
	}

	public void TestDeclarantTaxNumber()
	{
		var mauExternalPassword = CreateNewGlbExternalPassword(Factory);
		var iMauExternalPassword = (IGlbMauExternalPassword)mauExternalPassword;

		CombineAssertions(() =>
		{
			AssertEquals("DeclarantTaxNumber", "", iMauExternalPassword.DeclarantTaxNumber);

			mauExternalPassword.GP_MailBoxID = "22222222222";
			AssertEquals("DeclarantTaxNumber", "22222222222", iMauExternalPassword.DeclarantTaxNumber);

			mauExternalPassword.GP_MailBoxID = "11111111111-001";
			AssertEquals("DeclarantTaxNumber", "11111111111", iMauExternalPassword.DeclarantTaxNumber);
		});
	}

	protected override GlbMauExternalPassword CreateNewGlbExternalPassword(BusinessObjectFactory factory)
	{
		var externalPassword = base.CreateNewGlbExternalPassword(factory);
		externalPassword.GP_GC = Company.PK;
		externalPassword.GP_GS = ZGuid.Empty;
		return externalPassword;
	}

	GlbCompany Company
	{
		get
		{
			if (company == null)
			{
				company = Factory.New<GlbCompany>();
				company.GC_Code = "CC";
			}
			return company;
		}
	}
	GlbCompany company;

	protected override ZString PasswordType => "ITM";
}

class GlbMauExternalPassword_ITForTest : GlbMauExternalPassword
{
	public GlbMauExternalPassword_ITForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public bool ShouldSendCredentialExposed() => ShouldSendCredential();
}
