using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.Registry
{
	class AutoJRJRegistryStatusHelperTest : TestCaseWithFactory
	{
		public void TestIsAutoJRJEnabled()
		{
			AssertIsAutoJRJEnabled(true, "TAX");
			AssertIsAutoJRJEnabled(true, "YES");
			AssertIsAutoJRJEnabled(false, "NON");

			void AssertIsAutoJRJEnabled(bool expected, string autoJRJValue)
			{
				using (AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, autoJRJValue))
				{
					AssertEquals(expected, AutoJRJRegistryStatusHelper.IsAutoJRJEnabled());
				}
			}
		}

		public void TestIsAutoJRJEnabledWithCompany()
		{
			AssertIsAutoJRJEnabled(true, "TAX");
			AssertIsAutoJRJEnabled(true, "YES");
			AssertIsAutoJRJEnabled(false, "NON");

			void AssertIsAutoJRJEnabled(bool expectedResult, string registryValue)
			{
				var companyPK = TestObjectCreator.NonCurrentCompany.PK.ToGuid();
				using (AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, registryValue))
				{
					AssertEquals(false, AutoJRJRegistryStatusHelper.IsAutoJRJEnabled());
					AssertEquals(expectedResult, AutoJRJRegistryStatusHelper.IsAutoJRJEnabled(companyPK));
				}
			}
		}

		public void TestIsAutoJRJWithTaxRegistrationNumberEnabled()
		{
			AssertIsAutoJRJWithTaxRegistrationNumberEnabled(true, "TAX");
			AssertIsAutoJRJWithTaxRegistrationNumberEnabled(false, "YES");
			AssertIsAutoJRJWithTaxRegistrationNumberEnabled(false, "NON");

			void AssertIsAutoJRJWithTaxRegistrationNumberEnabled(bool expectedResult, string autoJRJValue)
			{
				using (AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, autoJRJValue))
				{
					AssertEquals(expectedResult, AutoJRJRegistryStatusHelper.IsAutoJRJWithTaxRegistrationNumberEnabled());
				}
			}
		}

		public void TestSetAutoJRJEnabled_ForTestOnly()
		{
			AssertSetAutoJRJCore("NON", "YES", (x) => AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(x));
		}

		public void TestSetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly()
		{
			AssertSetAutoJRJCore("NON", "TAX", (x) => AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly(x));
		}

		public void TestSetAutoJRJDisabled_ForTestOnly()
		{
			AssertSetAutoJRJCore("YES", "NON", (x) => AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly(x));
		}

		void AssertSetAutoJRJCore(string initialRegistryValue, string expectedRegistryValue, Func<Guid?, IDisposable> setAutoJRJFunc)
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, initialRegistryValue);
			AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetValue(companyPK, Guid.Empty, Guid.Empty, initialRegistryValue);

			using (setAutoJRJFunc(null))
			{
				AssertEquals(expectedRegistryValue, AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				AssertEquals(initialRegistryValue, AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
			}

			AssertEquals("Should reset the registry value once disposed", initialRegistryValue, AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			using (setAutoJRJFunc(companyPK))
			{
				AssertEquals(initialRegistryValue, AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				AssertEquals(expectedRegistryValue, AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
			}

			AssertEquals("Should reset the registry value once disposed", initialRegistryValue, AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
