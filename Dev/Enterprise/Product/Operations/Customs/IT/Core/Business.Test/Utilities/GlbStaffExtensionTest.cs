using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class GlbStaffExtensionTest : TestCaseWithFactory
{
	public void TestGetItalianRegistrationNumber()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected", () => (null as GlbStaff).GetItalianRegistrationNumber());

		var staff = Factory.New<GlbStaff>();
		AssertEquals("Italian Registration Number", "", staff.GetItalianRegistrationNumber());

		var codCertificate = staff.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "00891230153";
		AssertEquals("Italian Registration Number", "00891230153", staff.GetItalianRegistrationNumber());
	}

	public void TestGetItalianRegistrationNumberCertificate()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected", () => (null as GlbStaff).GetItalianRegistrationNumberCertificate());

		var staff = Factory.New<GlbStaff>();
		AssertNull("Italian Registration Number Certificate", staff.GetItalianRegistrationNumberCertificate());

		var codCertificate = staff.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "00891230153";

		var result = staff.GetItalianRegistrationNumberCertificate();
		AssertNotNull("Italian Registration Number Certificate", result);
		CombineAssertions(() =>
		{
			AssertEquals("Italian Registration Number Certificate PK", codCertificate.PK, result.PK);
			AssertEquals("Italian Registration Number", "00891230153", codCertificate.XZ_RefNumber);
		});
	}

	public void TestGetSignerFiscalCode()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected", () => (null as GlbStaff).GetSignerFiscalCode());

		var staff = Factory.New<GlbStaff>();
		AssertEquals("Signer Fiscal Code", "", staff.GetSignerFiscalCode());

		var codCertificate = staff.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "00891230153";
		AssertEquals("Signer Fiscal Code", "00891230153", staff.GetSignerFiscalCode());

		var staffWrapper = GlbStaffWrapper.Get(staff);
		var automaticSignature = staffWrapper.AutomaticSignaturePasswordCollection.AddNew();

		automaticSignature.IsConfigurationActive = true;
		automaticSignature.GP_Name = "12345678901";
		AssertEquals("Signer Fiscal Code", "12345678901", staff.GetSignerFiscalCode());

		automaticSignature.IsConfigurationActive = false;
		AssertEquals("Signer Fiscal Code", "00891230153", staff.GetSignerFiscalCode());
	}

	public void TestHasValidAutomaticSignaturePassword()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected", () => (null as GlbStaff).HasValidAutomaticSignaturePassword());

		var staff = Factory.New<GlbStaff>();
		AssertEquals("Has Valid Automatic Signature Password", false, staff.HasValidAutomaticSignaturePassword());

		var staffWrapper = GlbStaffWrapper.Get(staff);
		var automaticSignature = staffWrapper.AutomaticSignaturePasswordCollection.AddNew();

		automaticSignature.IsConfigurationActive = true;
		AssertEquals("Has Valid Automatic Signature Password", true, staff.HasValidAutomaticSignaturePassword());

		automaticSignature.IsConfigurationActive = false;
		AssertEquals("Has Valid Automatic Signature Password", false, staff.HasValidAutomaticSignaturePassword());
	}
}
