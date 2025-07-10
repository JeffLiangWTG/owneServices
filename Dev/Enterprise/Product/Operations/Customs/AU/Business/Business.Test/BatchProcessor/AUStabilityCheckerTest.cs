using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.StabilityChecker;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestDate]
	class AUStabilityCheckerTest : TestCaseWithFactory
	{
		public void TestCompanyCert()
		{
			TestDateAttribute.Date = startDate.AddDays(-1);
			var result = checker.Check().FirstOrDefault(x => x.Description.Contains("Company Key File"));
			AssertEquals("Not current message", "Eagle Datamation International - CMR Company Key File (Type 3 Certificate) is not current yet, start date is " + startDate.ToShortDateString(), result.Description);
			AssertEquals("Not current status", StabilityResultLevel.Warning, result.StabilityLevel);

			TestDateAttribute.Date = endDate.AddDays(-27);
			result = checker.Check().FirstOrDefault(x => x.Description.Contains("Company Key File"));
			AssertEquals("Soon expires message", "Eagle Datamation International - CMR Company Key File (Type 3 Certificate) will soon expire, expiry date is " + endDate.ToShortDateString(), result.Description);
			AssertEquals("Soon expires status", StabilityResultLevel.Warning, result.StabilityLevel);
			TestDateAttribute.Date = endDate.AddDays(1);
			result = checker.Check().FirstOrDefault(x => x.Description.Contains("Company Key File"));
			AssertEquals("Expired message", "Eagle Datamation International - CMR Company Key File (Type 3 Certificate) has expired, expiry date is " + endDate.ToShortDateString(), result.Description);
			AssertEquals("Expired status", StabilityResultLevel.Critical, result.StabilityLevel);

			TestDateAttribute.Date = endDate.AddDays(-32);
			result = checker.Check().FirstOrDefault(x => x.Description.Contains("Company Key File"));
			AssertEquals("No errors", ZString.Empty, result?.Description ?? ZString.Empty);
		}

		public void TestMultipleCompaniesCert()
		{
			var refCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			var auCompany2 = Factory.New<GlbCompany>();
			auCompany2.GC_RN_NKCountryCode = refCountry.Code;
			auCompany2.GC_Code = "TC2";
			auCompany2.GC_Name = "Test Company 2";
			var auCo2Branch = auCompany2.Branches.AddNew();
			auCo2Branch.GB_Code = "BR1";
			Factory.Save();
			certificatesHelper.SetupValidCompanyCertificatesForTest(out startDate, out endDate, auCompany2.PK.ToGuid());

			TestDateAttribute.Date = endDate.AddDays(1);
			var results = checker.Check().Where(x => x.Description.Contains("Company Key File")).ToArray();
			var result1 = results.FirstOrDefault(x => x.Description.Contains("Eagle Datamation International"));
			AssertEquals("Has error for EDI", "Eagle Datamation International - CMR Company Key File (Type 3 Certificate) has expired, expiry date is " + endDate.ToShortDateString(), result1.Description);
			var result2 = results.FirstOrDefault(x => x.Description.Contains("Test Company 2"));
			AssertEquals("Has error for TC2", "Test Company 2 - CMR Company Key File (Type 3 Certificate) has expired, expiry date is " + endDate.ToShortDateString(), result2.Description);
			AssertEquals(2, results.Length);
		}

		public void TestCompanyCertCryptoException()
		{
			var certData = certificatesHelper.GetEmbeddedFileData(CertificateManagerHelper.AUCryptCrt2004);
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, certData);
			var result = checker.Check();
			AssertEquals("result.Length", 1, result.Length);
			AssertEquals("Status", StabilityResultLevel.Critical, result[0].StabilityLevel);
			AssertEquals("Crypto Exception message", "Eagle Datamation International - An invalid CMR Company Key File (Type 3 Certificate) is loaded in the registry, error: The supplied file is not a valid private key file. Error Number: 0", result[0].Description);
		}

		public void TestCustomsCert()
		{
			// Arrange
			StabilityResult GetCustomsCertificate() => checker.Check().FirstOrDefault(result => result.Description.Contains("Customs Key File"));

			// Act
			TestDateAttribute.Date = new DateTime(2021, 08, 22, 12, 12, 12);
			var stabilityResultNotCurrent = GetCustomsCertificate();

			TestDateAttribute.Date = new DateTime(2023, 08, 25, 12, 12, 12);
			var stabilityResultHasExpired = GetCustomsCertificate();

			TestDateAttribute.Date = new DateTime(2023, 08, 01, 12, 12, 12);
			var stabilityResultExpiresSoon = GetCustomsCertificate();

			TestDateAttribute.Date = new DateTime(2021, 08, 24, 12, 12, 12);
			var stabilityResultDescriptionNotEmpty = GetCustomsCertificate();

			// Assert
			CombineAssertions(() =>
			{
				// Certificate is not yet current.
				AssertEquals("Stability description, when Certificate is not current, does not match expected message.", $"Eagle Datamation International - Australian Customs Key File (Certificate) is not current yet, start date is {new DateTime(2021, 08, 23).ToShortDateString()}", stabilityResultNotCurrent.Description);
				AssertEquals("Stability result was not Warning, as expected.", StabilityResultLevel.Warning, stabilityResultNotCurrent.StabilityLevel);

				// Certificate has expired.
				AssertEquals("Expired message", $"Eagle Datamation International - Australian Customs Key File (Certificate) has expired, expiry date is {new DateTime(2023, 08, 24).ToShortDateString()}", stabilityResultHasExpired.Description);
				AssertEquals("Expired result was not Critical.", StabilityResultLevel.Critical, stabilityResultHasExpired.StabilityLevel);

				// Certificate is expiring soon.
				AssertStartsWith("Expired message does not match expected message.", $"Eagle Datamation International - The Australian Customs Key File (Certificate) will expire soon, the expiry date is {new DateTime(2023, 08, 24).ToShortDateString()}", stabilityResultExpiresSoon.Description);
				AssertEquals("Expired result was not Warning.", StabilityResultLevel.Warning, stabilityResultExpiresSoon.StabilityLevel);

				// Certificate Description holds no errors.
				AssertEquals("Stability description should be empty and free of errors.", ZString.Empty, stabilityResultDescriptionNotEmpty?.Description ?? ZString.Empty);
			});
		}

		public void TestCustomsCertIssuer()
		{
			certificatesHelper.RemoveCertificates();
			certificatesHelper.CreateCustomsCertificates2021();
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2021, 08, 24, 12, 12, 12);
			var result = checker.Check().FirstOrDefault(x => x.Description.Contains("Customs Key File"));
			AssertEquals("Expired message", "Eagle Datamation International - Australian Customs Key File (Certificate) Issuer Name is invalid, should be 'DigiCert Gatekeeper Device Issuing CA' but is 'Gatekeeper TYPE 3 CA'", result.Description);
			AssertEquals("Expired status", StabilityResultLevel.Critical, result.StabilityLevel);
		}

		public void TestIsUserRelatedNotification()
		{
			TestDateAttribute.Date = new DateTime(2003, 4, 2, 12, 12, 12);
			var results = checker.Check();

			foreach (var result in results)
			{
				Assert(result.IsUserRelatedNotification);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			certificatesHelper = new CertificateManagerHelper(Factory);
			certificatesHelper.SetupValidCompanyCertificatesForTest(out startDate, out endDate);
			certificatesHelper.CreateCustomsCertificates2023();
			Factory.Save();
			checker = new AUStabilityChecker();
		}

		CertificateManagerHelper certificatesHelper;
		protected AUStabilityChecker checker;
		protected DateTime startDate;
		protected DateTime endDate;
	}
}
