using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.StabilityChecker;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestDate]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0042:Deconstruct variable declaration", Justification = "Variable names are clearer as they are.")]
	sealed class CertificateCheckerTest : TestCaseWithFactory
	{
		static void AssertCertificateResult((StabilityResultLevel Result, string Message) result, string messagePrefix, StabilityResultLevel expectedStabilityResultLevel, string expectedMessage)
		{
			AssertStartsWith($"{messagePrefix} message", expectedMessage, result.Message);
			AssertEquals($"{messagePrefix} status", expectedStabilityResultLevel, result.Result);
		}

		public void TestCheckCompanyKey()
		{
			// Arrange
			var checker = new CertificateChecker(Factory);
			(StabilityResultLevel Result, string Message) CheckCompanyKey(bool allowNull)
			{
				var result = checker.CheckCompanyKey(out var message, allowNull);
				return (result, message);
			}

			// Act
			TestDateAttribute.Date = startDate.AddDays(-1);
			var certificateNotCurrentYetResult = CheckCompanyKey(allowNull: false);
			TestDateAttribute.Date = endDate.AddDays(-27);
			var certificateWillExpireSoonResult = CheckCompanyKey(allowNull: false);
			TestDateAttribute.Date = endDate.AddDays(1);
			var certificateExpiredResult = CheckCompanyKey(allowNull: false);
			TestDateAttribute.Date = endDate.AddDays(-60);
			var certificateHealthyDoNotAllowNullResult = CheckCompanyKey(allowNull: false);
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), branchPK: Guid.Empty, departmentPK: Guid.Empty, value: null);
			var certificateHealthyAllowNullResult = CheckCompanyKey(allowNull: true);
			var certificateType3IsNotLoaded = CheckCompanyKey(allowNull: false);
			var certData = certificatesHelper.GetEmbeddedFileData(CertificateManagerHelper.AUCryptCrt2004);
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), branchPK: Guid.Empty, departmentPK: Guid.Empty, value: certData);
			var certificateType3IsInvalid = CheckCompanyKey(allowNull: false);

			// Assert
			CombineAssertions(() =>
			{
				AssertCertificateResult(certificateNotCurrentYetResult, "Not current", StabilityResultLevel.Warning, $"CMR Company Key File (Type 3 Certificate) is not current yet, start date is {startDate.ToShortDateString()}");

				AssertCertificateResult(certificateWillExpireSoonResult, "Soon Expired", StabilityResultLevel.Warning, $"CMR Company Key File (Type 3 Certificate) will soon expire, expiry date is {endDate.ToShortDateString()}");

				AssertCertificateResult(certificateExpiredResult, "Expired", StabilityResultLevel.Critical, $"CMR Company Key File (Type 3 Certificate) has expired, expiry date is {endDate.ToShortDateString()}");

				AssertCertificateResult(certificateHealthyDoNotAllowNullResult, "Healthy", StabilityResultLevel.Healthy, string.Empty);

				AssertCertificateResult(certificateHealthyAllowNullResult, "Healthy", StabilityResultLevel.Healthy, string.Empty);

				AssertCertificateResult(certificateType3IsNotLoaded, "Type 3 Certificate Not Loaded", StabilityResultLevel.Critical, "No CMR Company Key File (Type 3 Certificate) is loaded in the registry");

				AssertCertificateResult(certificateType3IsInvalid, "Crypto Exception", StabilityResultLevel.Critical, "An invalid CMR Company Key File (Type 3 Certificate) is loaded in the registry, error: The supplied file is not a valid private key file. Error Number: 0");
			});
		}

		public void TestCheckCustomsKeyWithNewCert()
		{
			// Arrange
			using (CertificateManager certificateManager = new CertificateManager(Factory))
			{
				startDate = certificateManager.CustomsCertificate.ValidFromDate;
				endDate = certificateManager.CustomsCertificate.ValidToDate;
			}

			var checker = new CertificateChecker(Factory);
			(StabilityResultLevel Result, string Message) CheckCustomsKey(bool allowNull)
			{
				var result = checker.CheckCustomsKey(out var message, allowNull);
				return (result, message);
			}

			// Act
			TestDateAttribute.Date = startDate.AddDays(-1);
			var certificateNotCurrentYetResult = CheckCustomsKey(allowNull: false);
			TestDateAttribute.Date = endDate.AddDays(-27);
			var certificateWillExpireSoonResult = CheckCustomsKey(allowNull: false);
			TestDateAttribute.Date = endDate.AddDays(1);
			var certificateExpiredResult = CheckCustomsKey(allowNull: false);
			TestDateAttribute.Date = endDate.AddDays(-60);
			var certificateHealthyDoNotAllowNullResult = CheckCustomsKey(allowNull: false);
			certificatesHelper.RemoveCertificates();
			var certificateHealthyAllowNullResult = CheckCustomsKey(allowNull: true);
			var certificateAUCustomsIsNotLoaded = CheckCustomsKey(allowNull: false);

			// Assert
			CombineAssertions(() =>
			{
				AssertCertificateResult(certificateNotCurrentYetResult, "Not current", StabilityResultLevel.Warning, $"Australian Customs Key File (Certificate) is not current yet, start date is {startDate.ToShortDateString()}");

				AssertCertificateResult(certificateWillExpireSoonResult, "Soon Expired", StabilityResultLevel.Warning, $"The Australian Customs Key File (Certificate) will expire soon, the expiry date is {endDate.ToShortDateString()}");

				AssertCertificateResult(certificateExpiredResult, "Expired", StabilityResultLevel.Critical, $"Australian Customs Key File (Certificate) has expired, expiry date is {endDate.ToShortDateString()}");

				AssertCertificateResult(certificateHealthyDoNotAllowNullResult, "Healthy", StabilityResultLevel.Healthy, string.Empty);

				AssertCertificateResult(certificateHealthyAllowNullResult, "Healthy", StabilityResultLevel.Healthy, string.Empty);

				AssertCertificateResult(certificateAUCustomsIsNotLoaded, "Null Certificate", StabilityResultLevel.Critical, "No Australian Customs Key File (Certificate) is loaded in the registry");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			certificatesHelper = new CertificateManagerHelper(Factory);
			certificatesHelper.SetupValidCompanyCertificatesForTest(out startDate, out endDate);
			certificatesHelper.CreateCustomsCertificates();
			Factory.Save();
		}

		CertificateManagerHelper certificatesHelper;
		DateTime startDate;
		DateTime endDate;
	}
}
