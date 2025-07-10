using CargoWise.DataProtection;
using CargoWise.DataProtection.TestFramework;
using Enterprise.Dat.Implementation;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Enterprise.Dat.Adapter.Test
{
	class TestExecutionEnvironmentTest : TestCase
	{
		public void TestDatMachinePasswordsAreAsExpected()
		{
			var factory = ProtectedDataService.GlobalServiceProvider.GetRequiredService<IProtectedDataServiceFactory>();

			var enterprisePds = factory.CreateEnterpriseService("localhost");
			var systemPds = factory.CreateSystemService("localhost", DatConfiguration.OdysseyTestDatabase);

			AssertExpectedPassword<SysAdminCredentials>(DataProtectionTestBed.Current, enterprisePds, TestEnvironmentWellKnownSecret.TestServerSysAdmin, shouldBeDefault: true);
			AssertExpectedPassword<OdysseyAdminCredentials>(DataProtectionTestBed.Current, enterprisePds, TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin, shouldBeDefault: true);
			AssertExpectedPassword<CargoWiseReaderLoginCredentials>(DataProtectionTestBed.Current, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatCargoWiseReaderLogin, shouldBeDefault: true);
			AssertExpectedPassword<CargoWiseWriterLoginCredentials>(DataProtectionTestBed.Current, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatCargoWiseWriterLogin, shouldBeDefault: true);
			AssertExpectedPassword<RestrictedReaderLoginCredentials>(DataProtectionTestBed.Current, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatRestrictedReaderLogin, shouldBeDefault: true);
			AssertExpectedPassword<RestrictedWriterLoginCredentials>(DataProtectionTestBed.Current, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatRestrictedWriterLogin, shouldBeDefault: true);
			AssertExpectedPassword<UnrestrictedWriterLoginCredentials>(DataProtectionTestBed.Current, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatUnrestrictedWriterLogin, shouldBeDefault: true);
		}

		void AssertExpectedPassword<TCredentials>(DataProtectionTestBed testBed, IProtectedDataService pds, TestEnvironmentWellKnownSecret wellknownSecret, bool shouldBeDefault) where TCredentials : DBCredentials
		{
			var defaultValue = testBed.GetDefaultCredentialsFor<TCredentials>(pds);
			var expectedValue = testBed.GetExpectedCredentials(wellknownSecret);
			Assert(shouldBeDefault ^ (expectedValue.Password != defaultValue.Password));
		}

		[DeveloperOnlyTest]
		public void TestDevMachinePasswordsAreAsExpected()
		{
			var factory = ProtectedDataService.GlobalServiceProvider.GetRequiredService<IProtectedDataServiceFactory>();

			var enterprisePds = factory.CreateEnterpriseService("localhost");
			var systemPds = factory.CreateSystemService("localhost", DatConfiguration.OdysseyTestDatabase);

			var testBed = new DataProtectionTestBed("ExistsNot.Yodish"); // On a dev machine, the file doesn't exist.

			AssertExpectedPassword<SysAdminCredentials>(testBed, enterprisePds, TestEnvironmentWellKnownSecret.TestServerSysAdmin, shouldBeDefault: true);
			AssertExpectedPassword<OdysseyAdminCredentials>(testBed, enterprisePds, TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin, shouldBeDefault: true);
			AssertExpectedPassword<CargoWiseReaderLoginCredentials>(testBed, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatCargoWiseReaderLogin, shouldBeDefault: true);
			AssertExpectedPassword<CargoWiseWriterLoginCredentials>(testBed, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatCargoWiseWriterLogin, shouldBeDefault: true);
			AssertExpectedPassword<RestrictedReaderLoginCredentials>(testBed, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatRestrictedReaderLogin, shouldBeDefault: true);
			AssertExpectedPassword<RestrictedWriterLoginCredentials>(testBed, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatRestrictedWriterLogin, shouldBeDefault: true);
			AssertExpectedPassword<UnrestrictedWriterLoginCredentials>(testBed, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatRestrictedWriterLogin, shouldBeDefault: true);
		}

		[DeveloperOnlyTest]
		public void TestCorrectLoginsCreatedForOdysseyGL()
		{
			var factory = ProtectedDataService.GlobalServiceProvider.GetRequiredService<IProtectedDataServiceFactory>();

			var enterprisePds = factory.CreateEnterpriseService("localhost");
			var systemPds = factory.CreateSystemService("localhost", DatConfiguration.OdysseyTestDatabase);

			var testBed = new DataProtectionTestBed("ExistsNot.Yodish"); // On a dev machine, the file doesn't exist.

			AssertExpectedPassword<SysAdminCredentials>(testBed, enterprisePds, TestEnvironmentWellKnownSecret.TestServerSysAdmin, shouldBeDefault: true);
			AssertExpectedPassword<OdysseyAdminCredentials>(testBed, enterprisePds, TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin, shouldBeDefault: true);
			AssertExpectedPassword<CargoWiseReaderLoginCredentials>(testBed, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatCargoWiseReaderLogin, shouldBeDefault: true);
			AssertExpectedPassword<CargoWiseWriterLoginCredentials>(testBed, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatCargoWiseWriterLogin, shouldBeDefault: true);
			AssertExpectedPassword<RestrictedReaderLoginCredentials>(testBed, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatRestrictedReaderLogin, shouldBeDefault: true);
			AssertExpectedPassword<RestrictedWriterLoginCredentials>(testBed, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatRestrictedWriterLogin, shouldBeDefault: true);
			AssertExpectedPassword<UnrestrictedWriterLoginCredentials>(testBed, systemPds, TestEnvironmentWellKnownSecret.OdysseyDatRestrictedWriterLogin, shouldBeDefault: true);
		}
	}
}
