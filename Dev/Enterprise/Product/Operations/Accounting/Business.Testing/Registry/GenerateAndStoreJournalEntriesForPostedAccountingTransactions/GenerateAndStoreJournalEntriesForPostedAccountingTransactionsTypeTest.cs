using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GenerateAndStoreJournalEntriesForPostedAccountingTransactionsType))]
	class GenerateAndStoreJournalEntriesForPostedAccountingTransactionsTypeTest : RegistryDataTypeTestCase<GenerateAndStoreJournalEntriesForPostedAccountingTransactionsType>
	{
		[ExpectNoExceptions]
		public override void TestValuesAreEqual()
		{
			var dataType = GetNewDataType();
			var samples = GetValidSamples();
			var grouped = samples
				.Zip(samples, (a, b) => Tuple.Create(a.ValidSample, b.ValidSample))
				.ToList();

			var nonNull = grouped.Where(tuple => tuple.Item1 != null).Take(2).ToList().First();
			AssertNotNull(nonNull);
			Assert("Same items should be same", dataType.ValuesAreEqual(nonNull.Item1, nonNull.Item2));
		}

		[TestDate(2023, 2, 2)]
		public override void TestGetSetValidValues()
		{
			var dataType = GetNewDataType();
			var item = new GenerateAndStoreJournalEntriesForPostedAccountingTransactionsItemImpl(
						"GenerateAndStoreJournalEntriesForPostedAccountingTransactions",
						(NoResString)"",
						(NoResString)"Category",
						(NoResString)"Caption",
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false,
						dataType);
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			testObjectCreator.DeleteAllPeriodsForCurrentCompany();
			testObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			testObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2023);
			factory.Save();

			var calculator = new AccountingPeriodCalculator(factory);
			var periodManagement = calculator.GetFirstPeriodManagementFromDate(ZDateTime.UtcNow.ToDateTime(), GlbCompany.CurrentCompany.PK.ToGuid());

			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, periodManagement.AM_StartDate.ToDateTime());
			AssertNoExceptionThrown(() => dataType.Validate(item, true, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestValidate_CanNotBeDisabled()
		{
			var dataType = GetNewDataType();
			var item = new GenerateAndStoreJournalEntriesForPostedAccountingTransactionsItemImpl(
						"GenerateAndStoreJournalEntriesForPostedAccountingTransactions",
						(NoResString)"",
						(NoResString)"Category",
						(NoResString)"Caption",
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false,
						dataType);
			AssertExceptionThrown(typeof(RegistryValidationException), "This registry can't be disabled.", () => dataType.Validate(item, false, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		[TestDate(2023, 2, 2)]
		public void TestValidate_ShouldSetPeriodBeforeEnableRegistry()
		{
			var dataType = GetNewDataType();
			var item = new GenerateAndStoreJournalEntriesForPostedAccountingTransactionsItemImpl(
						"GenerateAndStoreJournalEntriesForPostedAccountingTransactions",
						(NoResString)"",
						(NoResString)"Category",
						(NoResString)"Caption",
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false,
						dataType);
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			testObjectCreator.DeleteAllPeriodsForCurrentCompany();

			AssertExceptionThrown(typeof(RegistryValidationException), "The Accounting Year must be configured under Manage > General Ledger > Period Management before this registry can be set to \"Yes\"", () => dataType.Validate(item, true, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		protected override GenerateAndStoreJournalEntriesForPostedAccountingTransactionsType GetNewDataType()
		{
			return new GenerateAndStoreJournalEntriesForPostedAccountingTransactionsType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[] { new ValidSampleAndBinaryValueInDB(true, GetNewDataType().Serialise(true)), new ValidSampleAndBinaryValueInDB(true, GetNewDataType().Serialise(true)) };
		}
	}
}
