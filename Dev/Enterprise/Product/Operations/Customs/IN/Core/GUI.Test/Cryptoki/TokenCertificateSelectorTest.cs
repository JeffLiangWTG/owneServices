using System.Security.Cryptography;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing
{
	[TestedType(typeof(TokenCertificateSelector))]
	sealed class TokenCertificateSelectorTest : TestCaseWithFactory
	{
		public void TestChooseCertificate()
		{
			AssertExceptionThrown<CryptographicException>(
				"ChooseCertificate with unkown dll",
				"Cannot find PKCS#11 library.",
				() => new TokenCertificateSelector("InvalidLibrary.dll").ChooseCertificate());
		}
	}
}
