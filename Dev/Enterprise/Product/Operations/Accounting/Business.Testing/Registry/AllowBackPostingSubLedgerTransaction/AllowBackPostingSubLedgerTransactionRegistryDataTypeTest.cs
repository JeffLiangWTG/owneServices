using System;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(AllowBackPostingSubLedgerTransactionRegistryDataType))]
	class AllowBackPostingSubLedgerTransactionRegistryDataTypeTest : BooleanRegistryDataTypeTest
	{
		protected override BooleanRegistryDataType GetNewDataType()
		{
			return new AllowBackPostingSubLedgerTransactionRegistryDataType();
		}

		public void TestBackDateInvoicesConfigurationValidation()
		{
			AllowBackPostingSubLedgerTransactionRegistryDataType testDataType = (AllowBackPostingSubLedgerTransactionRegistryDataType)GetNewDataType();

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new BackDateInvoicesConfiguration() { OverridePostDate = true, DefaultPostDateFromInvoiceDate = true });
			AssertBackDateInvoicesConfigurationValidation(testDataType, true);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new BackDateInvoicesConfiguration() { OverridePostDate = true, DefaultPostDateFromInvoiceDate = false });
			AssertBackDateInvoicesConfigurationValidation(testDataType, true);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new BackDateInvoicesConfiguration() { OverridePostDate = false, DefaultPostDateFromInvoiceDate = true });
			AssertBackDateInvoicesConfigurationValidation(testDataType, true);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new BackDateInvoicesConfiguration() { OverridePostDate = false, DefaultPostDateFromInvoiceDate = false });
			AssertBackDateInvoicesConfigurationValidation(testDataType, false);
		}

		void AssertBackDateInvoicesConfigurationValidation(AllowBackPostingSubLedgerTransactionRegistryDataType registryDataType, bool shouldValidationFails)
		{
			bool validationFailed = false;
			try
			{
				registryDataType.Validate(null, true, Guid.Empty, Guid.Empty, Guid.Empty);
			}
			catch (RegistryValidationException)
			{
				validationFailed = true;
			}
			AssertEquals("Validation of BackDateInvoicesConfiguration for True value should never fails.", false, validationFailed);

			try
			{
				registryDataType.Validate(null, false, Guid.Empty, Guid.Empty, Guid.Empty);
			}
			catch (RegistryValidationException)
			{
				validationFailed = true;
			}
			AssertEquals("Validation for BackDateInvoicesConfiguration for False value works incorrectly.", shouldValidationFails, validationFailed);
		}
	}
}
