using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(CryptokiExternalPassword))]
sealed class CryptokiExternalPasswordTest : MasterFiles.Business.Testing.GlbExternalPasswordTest<CryptokiExternalPassword>
{
	public void TestSetDefaultValues()
	{
		AssertEquals("GP_GC", ZGuid.Empty, GlbExternalPassword.GP_GC);
		AssertEquals("GP_PasswordType", PasswordTypesList.Codes.ITX, GlbExternalPassword.GP_PasswordType);
	}

	public void TestCertificateAuthority()
	{
		AssertEquals("GP_CertificateAuthority Initial Value", "", GlbExternalPassword.GP_CertificateAuthority);
		GlbExternalPassword.GP_CertificateAuthority = "SomeCa";
		AssertEquals("SomeCa", GlbExternalPassword.GP_CertificateAuthority);
	}

	public void TestChipset()
	{
		AssertEquals("GP_Name Initial Value", "", GlbExternalPassword.GP_Name);
		GlbExternalPassword.GP_Name = ChipsetList.Codes.Bit4id;
		AssertEquals(ChipsetList.Codes.Bit4id, GlbExternalPassword.GP_Name);
	}

	[ExpectNoExceptions]
	public void TestCertificateSerialNumber()
	{
		AssertEquals("GP_CertificateSerialNumber Initial Value", "", GlbExternalPassword.GP_CertificateSerialNumber);
		GlbExternalPassword.GP_CertificateSerialNumber = "297DF0DFF2A742BDAC51AB78B6F86620";
		AssertEquals("297DF0DFF2A742BDAC51AB78B6F86620", GlbExternalPassword.GP_CertificateSerialNumber);

		AssertEquals(32, GlbExternalPassword.GP_CertificateSerialNumberInfo.MaxLength);
		NUnit.Framework.Assert.That(delegate { GlbExternalPassword.GP_CertificateSerialNumber = "123456789012345678901234567890123"; }, CustomConstraints.InnermostExceptionThrown(typeof(MaxLengthExceededException)));
		ErrorReporter.Clear();
	}

	public void TestCertificateSerialNumberReadOnly()
	{
		AssertEquals("GP_CertificateSerialNumber ReadOnly", true, GlbExternalPassword.GP_CertificateSerialNumberInfo.ReadOnly);
	}

	public void TestTokenPinStore()
	{
		var tokenPinStore = GlbExternalPassword.TokenPinStore;
		AssertNotNull("TokenPinStore", tokenPinStore);
		AssertSame("TokenPinStore Cached", tokenPinStore, GlbExternalPassword.TokenPinStore);

		tokenPinStore.SetPin("123");
		Factory.Save();

		var extPasswordInAnotherFactory = new BusinessObjectFactory().Load<CryptokiExternalPassword>(GlbExternalPassword.PK);
		AssertEquals("Pin is not persisted", "", extPasswordInAnotherFactory.TokenPinStore.GetPin());
	}

	public void TestLookupsType()
	{
		AssertType<CryptokiExternalPasswordLookups>(GlbExternalPassword.Lookups);
	}

	public void TestValidationType()
	{
		AssertType<CryptokiExternalPasswordValidation>(GlbExternalPassword.Validation);
	}

	public void TestPopulateCertificateRelatedFields()
	{
		AssertExceptionThrown<ArgumentNullException>("When cryptokiCertificate is null", () => GlbExternalPassword.PopulateCertificateRelatedFields(cryptokiCertificate: null));

		GlbExternalPassword.PopulateCertificateRelatedFields(new CryptokiCertificate()
		{
			SerialNumber = "SerialNumber",
			NotBefore = new ZDateTime(2022, 01, 01),
			NotAfter = new ZDateTime(2022, 12, 31),
		});
		CombineAssertions(() =>
		{
			AssertEquals("GP_CertificateSerialNumber", "SerialNumber", GlbExternalPassword.GP_CertificateSerialNumber);
			AssertEquals("GP_IssueDate", new ZDateTime(2022, 01, 01), GlbExternalPassword.GP_IssueDate);
			AssertEquals("GP_ExpiryDate", new ZDateTime(2022, 12, 31), GlbExternalPassword.GP_ExpiryDate);
		});
	}

	public void TestSetGP_NameClearCertificateRelatedFields()
	{
		GlbExternalPassword.GP_CertificateSerialNumber = "SerialNumber";
		GlbExternalPassword.GP_IssueDate = new ZDateTime(2022, 01, 01);
		GlbExternalPassword.GP_ExpiryDate = new ZDateTime(2022, 12, 31);

		GlbExternalPassword.GP_Name = "ABC";
		CombineAssertions(() =>
		{
			AssertEquals("GP_CertificateSerialNumber", "", GlbExternalPassword.GP_CertificateSerialNumber);
			AssertEquals("GP_IssueDate", ZDateTime.Empty, GlbExternalPassword.GP_IssueDate);
			AssertEquals("GP_ExpiryDate", ZDateTime.Empty, GlbExternalPassword.GP_ExpiryDate);
		});
	}

	public void TestShouldSendCredential()
	{
		var cryptokiExternalPassword = Factory.New<CryptokiExternalPasswordForTest>();
		CombineAssertions(() =>
		{
			AssertEquals("ShouldSendCredential", false, cryptokiExternalPassword.ShouldSendCredentialExposed());
			AssertEquals("ShouldSendDeleteCredential", false, cryptokiExternalPassword.ShouldSendDeleteCredentialExposed());
		});
	}
}

class CryptokiExternalPasswordForTest : CryptokiExternalPassword
{
	public CryptokiExternalPasswordForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public bool ShouldSendCredentialExposed() => ShouldSendCredential();

	public bool ShouldSendDeleteCredentialExposed() => ShouldSendDeleteCredential();
}
