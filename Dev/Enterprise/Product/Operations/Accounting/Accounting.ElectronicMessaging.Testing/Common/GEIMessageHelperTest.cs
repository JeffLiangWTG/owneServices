using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	class GEIMessageHelperTest : TestCaseWithFactory
	{
		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestGetCertificateAsBase64String()
		{
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(GlbBranch.CurrentBranch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			{
				var credential = credentialCreator.CreateCertificateCredential();
				var expectedBase64String = Convert.ToBase64String(credential.GP_Certificate);
				AssertEquals("Certificate", expectedBase64String, credential.GetCertificateAsBase64String());
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestGetCertificateAsBase64String_EncodedTwice()
		{
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(GlbBranch.CurrentBranch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			{
				var credential = credentialCreator.CreateCertificateCredential(base64EncodedTwice: true);
				var decodedStr = System.Text.Encoding.UTF8.GetString(credential.GP_Certificate);  //expected string starts with "TUI...."
				var bytes = Convert.FromBase64String(decodedStr);
				var expectedBase64String = System.Text.Encoding.UTF8.GetString(bytes);  //expected string starts with "MII...."
				AssertEquals("Certificate", expectedBase64String, credential.GetCertificateAsBase64String(supportCertificateBase64EncodedTwice: true));
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestToISO8601StringWithZeroOffsetSymbol()
		{
			AssertEndsWith("Has Z at the end", "Z", ZDateTime.UtcNow.ToISO8601StringWithZeroOffsetSymbol());
		}
	}
}
