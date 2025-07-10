using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(EnableAJLandRJLRegistryDataType))]
	public class EnableAJLandRJLRegistryDataTypeTest : RegistryDataTypeTestCase<EnableAJLandRJLRegistryDataType>
	{
		protected override EnableAJLandRJLRegistryDataType GetNewDataType()
		{
			return new EnableAJLandRJLRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(false, new EnableAJLandRJLRegistryDataType().Serialise(false)),
				new ValidSampleAndBinaryValueInDB(true, new EnableAJLandRJLRegistryDataType().Serialise(true))
			};
		}

		public override void TestGetSetValidValues()
		{
			var array = GetValidSamples();
			for (int i = 0; i < array.Length; i++)
			{
				var validSampleAndBinaryValueInDB = array[i];
				var newRegistryItemWithDefaultDefaultValue = new BooleanRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, false);
				newRegistryItemWithDefaultDefaultValue.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, (bool)validSampleAndBinaryValueInDB.ValidSample);
				var valueWithoutFallback = newRegistryItemWithDefaultDefaultValue.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
				AssertValuesEqual("New value set.", validSampleAndBinaryValueInDB.ValidSample, valueWithoutFallback);
			}
		}

		public void TestValidateCore()
		{
			var registryItem = new BooleanRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, false);
			var dataType = new EnableAJLandRJLRegistryDataType();
			AccountingConfigurationRegistry.Instance.EnableAJLandRJLForChinaCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, true, Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));

			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL
			};
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting);
			AssertExceptionThrown(typeof(RegistryValidationException),
			"Please ensure that 'Allocation Option' has been set to 'GEN' in the registry 'Journal Entries Number Customization'.",
								() => dataType.Validate(registryItem, true, Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			testObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, ZDateTime.Now.Year);
			testObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			testObjectCreator.Factory.Save();
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL
			};
			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting);
		}
	}
}
