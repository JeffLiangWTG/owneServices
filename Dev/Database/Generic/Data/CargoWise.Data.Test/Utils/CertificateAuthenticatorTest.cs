using System.IO;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class CertificateAuthenticatorTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestVerifyWithCertificate()
		{
			using (((ICurrentDbControl)authenticator.Connection).UseDatabase(Db.SqlMasterDb))
			{
				DropCertificate();
				InstallDummyCertificate();
			}
			Assert("Certificate exists. Verification should pass.", authenticator.Verify());
		}

		public void TestVerifyNoCertificate()
		{
			using (((ICurrentDbControl)authenticator.Connection).UseDatabase(Db.SqlMasterDb))
			{
				DropCertificate();
			}
			Assert("Certificate does not exist. Verification should fail.", !authenticator.Verify());
		}

		#region Implementation

		readonly CertificateAuthenticator authenticator = new CertificateAuthenticatorForTest();

		void InstallDummyCertificate()
		{
			var query = string.Format(@"
CREATE CERTIFICATE WtgServer
	FROM FILE = '{0}'
	WITH PRIVATE KEY (FILE = '{1}',
	DECRYPTION BY PASSWORD = '74isQS3Y990m3d9nuy7E3NEA62tssK470w7F0S6z',
	ENCRYPTION BY PASSWORD = 'kU9f4Zku2832s6R31479Gz5zekll8F350N10i7xR');",
Path.Combine(BaseSourcePath, @"Database\Generic\Data\CargoWise.Data\Utils\Testing\dummyCertificate.cer"),
Path.Combine(BaseSourcePath, @"Database\Generic\Data\CargoWise.Data\Utils\Testing\privateKey.pvk"));

			authenticator.Connection.ExecuteNonQuery(query);
		}

		void DropCertificate()
		{
			var query = @"
IF EXISTS (SELECT 1 FROM sys.certificates WHERE name = 'WtgServer')
	DROP CERTIFICATE WtgServer"
;
			authenticator.Connection.ExecuteNonQuery(query);
		}

		class CertificateAuthenticatorForTest : CertificateAuthenticator
		{
			protected override string CertificateKey => "0x3082020830820175A003020102021063F030B6B44DAF8546C21ADC996BEA08300906052B0E03021D0500301C311A3018060355040313114357315345525645524C4F434154494F4E3020170D3135303730333034313934365A180F39393938313233313134303030305A301C311A3018060355040313114357315345525645524C4F434154494F4E30819F300D06092A864886F70D010101050003818D0030818902818100AAB1EFC4A2FED7D3DB4C5625C62DD73E4FBC71B82EFAB31A3FB320D25FD14B5BA7DE505860C1EB028F5BD1321115A4798B73FFF194CE2E13C59DDB4CB5889E61975529A0FD376C95232838192AA15B1F4D6D108B78507C1DBE6CDE729C0368D6599C7438F20C783D0A0D003F46EF7A8387A99F6509C4BA4FF583D976CF11A8650203010001A351304F304D0603551D01044630448010AEF0401CA4847BB9A9D7F7DFB1281233A11E301C311A3018060355040313114357315345525645524C4F434154494F4E821063F030B6B44DAF8546C21ADC996BEA08300906052B0E03021D05000381810037CC28A1230766BDB1E4C333AC5AC75DC16F4EFCBEFF046B90F7D8312D1522EF2992AAFE5BE3BBBD71232DC87633A05CD5559BA12DE4593A2AA8A4CF6702D7776AE91A8662B5BC12F689C55BF2072262F18A1E080B0D5B10DD2C9A8E9FDB5DDCC97FAC247A85EEB3396F84AA3E7C5AF90523EC58D3B4F301F14EAD22B1E046D6";
		}

		#endregion
	}
}
