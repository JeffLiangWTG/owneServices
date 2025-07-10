using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Accounting.Registry.Business.JournalEntriesNumberCustomisation;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JournalEntriesNumberCustomisationSetting))]
	public class JournalEntriesNumberCustomisationSettingTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValue()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			AssertEquals(journalEntriesNumberCustomisationSetting.NumberRule, AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL);
			AssertEquals(journalEntriesNumberCustomisationSetting.AllocationOption, AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON);
			AssertEquals(journalEntriesNumberCustomisationSetting.SequenceResetOption, AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.YEAR);
			AssertEquals(15, journalEntriesNumberCustomisationSetting.NumberSequenceCustomisations.Count);

			var journalEntriesNumberCustomisationCollection = journalEntriesNumberCustomisationSetting.NumberSequenceCustomisations.Cast<JournalEntriesNumberCustomisation>();
			var numberSequenceCustomisationElementNames = new string[]
			{
				TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits,
				TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits,
				TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsLetter,
				JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarMonthAs2Digits,
				JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarMonthAsLetter,
				JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits,
				JournalEntriesNumberCustomisationElementNames.CalendarYearAsLetter,
				TransactionNumberSequenceCustomisation.ElementNames.CustomElement1,
				TransactionNumberSequenceCustomisation.ElementNames.CustomElement2,
				JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesBranchCode,
				JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCode,
				JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesDepartmentCode,
				TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber,
				TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix,
				JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCodePrefix,
			};
			AssertContainsExactElementsInAnyOrder(numberSequenceCustomisationElementNames, journalEntriesNumberCustomisationCollection.Select(x => x.ElementName));

			var accountingYearAsDigits = journalEntriesNumberCustomisationCollection.First(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits);
			AssertEquals(4.ToString(), accountingYearAsDigits.Code);

			var calendarYearAsDigits = journalEntriesNumberCustomisationCollection.First(x => x.ElementName == JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits);
			AssertEquals(4.ToString(), calendarYearAsDigits.Code);

			var sequence = journalEntriesNumberCustomisationCollection.First(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
			AssertEquals(new ZByte(50), sequence.Order);
			AssertEquals(true, sequence.Include);
			AssertEquals(10, sequence.Length);
			AssertEquals(true, sequence.Fountain);
		}

		public void TestIsOptionGen()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals(false, journalEntriesNumberCustomisationSetting.IsOptionGen);

			journalEntriesNumberCustomisationSetting.AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN;
			AssertEquals(true, journalEntriesNumberCustomisationSetting.IsOptionGen);

			journalEntriesNumberCustomisationSetting.AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON;
			AssertEquals(false, journalEntriesNumberCustomisationSetting.IsOptionGen);

			journalEntriesNumberCustomisationSetting.AllocationOption = null;
			AssertEquals(false, journalEntriesNumberCustomisationSetting.IsOptionGen);

			journalEntriesNumberCustomisationSetting.AllocationOption = string.Empty;
			AssertEquals(false, journalEntriesNumberCustomisationSetting.IsOptionGen);

			journalEntriesNumberCustomisationSetting.AllocationOption = "XXX";
			AssertEquals(false, journalEntriesNumberCustomisationSetting.IsOptionGen);
		}

		#region Validation

		public void TestValidateNumberRule()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			journalEntriesNumberCustomisationSetting.NumberRule = string.Empty;
			journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
			AssertHasErrors("Please enter a Number Rule.", journalEntriesNumberCustomisationSetting.NumberRuleInfo);

			journalEntriesNumberCustomisationSetting.NumberRule = "XXX";
			journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
			AssertHasErrors("Please enter a valid Number Rule.", journalEntriesNumberCustomisationSetting.NumberRuleInfo);

			journalEntriesNumberCustomisationSetting.NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.GRP;
			journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
			AssertHasErrors("Group Code in registry 'Journal Entries Classification Group' should be set when Number Rule is GRP.", journalEntriesNumberCustomisationSetting.NumberRuleInfo);

			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode.Code = "TST";
			journalEntriesClassificationGroupCode.Description = "Description";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCodeCollection);

			var journalEntriesClassificationGroupCollection = new JournalEntriesClassificationGroupCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			var journalEntriesClassificationGroup = journalEntriesClassificationGroupCollection.AddNew();
			journalEntriesClassificationGroup.GroupCode = "TST";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCollection);
			journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
			AssertEquals(false, journalEntriesNumberCustomisationSetting.NumberRuleInfo.HasErrors());
		}

		[TestDate(2024, 10, 8)]
		public void TestValidateAllocationOption()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			journalEntriesNumberCustomisationSetting.AllocationOption = string.Empty;
			journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
			AssertHasErrors("Please enter a Allocation Option.", journalEntriesNumberCustomisationSetting.AllocationOptionInfo);

			journalEntriesNumberCustomisationSetting.AllocationOption = "XXX";
			journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
			AssertHasErrors("Please enter a valid Allocation Option.", journalEntriesNumberCustomisationSetting.AllocationOptionInfo);

			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			testObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			testObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2024);

			var registryItem = AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions;
			using (registryItem.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				journalEntriesNumberCustomisationSetting.AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN;
				journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
				AssertEquals(false, journalEntriesNumberCustomisationSetting.AllocationOptionInfo.HasErrors());

				journalEntriesNumberCustomisationSetting.AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON;
				journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
				AssertEquals(false, journalEntriesNumberCustomisationSetting.AllocationOptionInfo.HasErrors());
			}

			using (registryItem.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				journalEntriesNumberCustomisationSetting.AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN;
				journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
				AssertHasErrors("Please ensure that 'Generate and Store Journal Entries for Posted Accounting Transactions' has been enabled.", journalEntriesNumberCustomisationSetting.AllocationOptionInfo);

				journalEntriesNumberCustomisationSetting.AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON;
				journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
				AssertEquals(false, journalEntriesNumberCustomisationSetting.AllocationOptionInfo.HasErrors());
			}
		}

		[TestDate(2024, 10, 8)]
		public void TestValidateSequenceResetOption()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

			journalEntriesNumberCustomisationSetting.SequenceResetOption = string.Empty;
			journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
			AssertHasErrors("Please enter a Sequence Reset Option.", journalEntriesNumberCustomisationSetting.SequenceResetOptionInfo);

			journalEntriesNumberCustomisationSetting.SequenceResetOption = "XXX";
			journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
			AssertHasErrors("Please enter a valid Sequence Reset Option.", journalEntriesNumberCustomisationSetting.SequenceResetOptionInfo);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				journalEntriesNumberCustomisationSetting.SequenceResetOption = AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.MONTH;
				journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
				AssertEquals(false, journalEntriesNumberCustomisationSetting.SequenceResetOptionInfo.HasErrors());

				journalEntriesNumberCustomisationSetting.SequenceResetOption = AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.YEAR;
				journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
				AssertEquals(false, journalEntriesNumberCustomisationSetting.SequenceResetOptionInfo.HasErrors());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				journalEntriesNumberCustomisationSetting.SequenceResetOption = AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.MONTH;
				journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
				AssertHasErrors("'MTH' Sequence Reset Option can only be used in China login companies, please change the value back to 'YR'.", journalEntriesNumberCustomisationSetting.SequenceResetOptionInfo);

				journalEntriesNumberCustomisationSetting.SequenceResetOption = AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.YEAR;
				journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
				AssertEquals(false, journalEntriesNumberCustomisationSetting.SequenceResetOptionInfo.HasErrors());
			}
		}

		public void TestValidateTotalLength()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			journalEntriesNumberCustomisationSetting.NumberSequenceCustomisations.RemoveAndDeleteAll();
			var numberSequenceCustomisation = journalEntriesNumberCustomisationSetting.NumberSequenceCustomisations.AddNew();
			numberSequenceCustomisation.Include = true;
			numberSequenceCustomisation.Length = 41;
			journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
			Assert(journalEntriesNumberCustomisationSetting.RowErrors.Contains("Total length of all data elements must not exceed 40 characters"));

			numberSequenceCustomisation.Length = 40;
			journalEntriesNumberCustomisationSetting.RunPreSaveValidation();
			AssertEquals(false, journalEntriesNumberCustomisationSetting.HasRowErrors);
		}

		#endregion

		#region List

		public void TestNumberRuleList()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals(3, journalEntriesNumberCustomisationSetting.NumberRuleList.Count);

			var expectNumberRuleList = new CodeDescriptionPair[]
			{
				new CodeDescriptionPair(AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL, "One Number Sequence for All Transaction Types"),
				new CodeDescriptionPair(AccountingConstants.JournalEntriesNumberCustomisationNumberRule.GRP, "Separate Number Sequence Per Journal Entries Classification Group"),
				new CodeDescriptionPair(AccountingConstants.JournalEntriesNumberCustomisationNumberRule.TRN, "Separate Number Sequence Per Transaction Type"),
			};
			AssertContainsExactElementsInAnyOrder(expectNumberRuleList, journalEntriesNumberCustomisationSetting.NumberRuleList);
		}

		public void TestAllocationOptionList()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals(2, journalEntriesNumberCustomisationSetting.AllocationOptionList.Count);

			var expectAllocationOptionList = new CodeDescriptionPair[]
			{
				new CodeDescriptionPair(AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON, "No Allocation"),
				new CodeDescriptionPair(AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN, "Allocate Number when Journal Entries are Generated"),
			};
			AssertContainsExactElementsInAnyOrder(expectAllocationOptionList, journalEntriesNumberCustomisationSetting.AllocationOptionList);
		}

		public void TestSequenceResetOptionList()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals(2, journalEntriesNumberCustomisationSetting.SequenceResetOptionList.Count);

			var expectSequenceResetOptionList = new CodeDescriptionPair[]
			{
				new CodeDescriptionPair(AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.YEAR, "Reset from 1 on yearly basis"),
				new CodeDescriptionPair(AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.MONTH, "Reset from 1 on monthly basis"),
			};
			AssertContainsExactElementsInAnyOrder(expectSequenceResetOptionList, journalEntriesNumberCustomisationSetting.SequenceResetOptionList);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
