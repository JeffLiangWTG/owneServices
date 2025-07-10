using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JournalEntriesNumberCustomisationSettingRegistryItem))]
	public class JournalEntriesNumberCustomisationSettingRegistryItemTest : StronglyTypedRegistryItemTestCase<JournalEntriesNumberCustomisationSetting>
	{
		protected override StronglyTypedRegistryItem<JournalEntriesNumberCustomisationSetting, JournalEntriesNumberCustomisationSetting> GetNewRegistryItem()
		{
			return new JournalEntriesNumberCustomisationSettingRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		[TestDate(2024, 7, 21)]
		public void TestShouldSetStartDate_WhenAllocationOptionIsGEN()
		{
			var factory = new BusinessObjectFactory();
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var testPeriodSettings = new NewYearPeriodSettings { StartDate =  new ZDateTime(2024, 7, 1) };
			var testManager = new PeriodManager(factory);
			testManager.CreatePeriodData(testPeriodSettings, factory);
			factory.Save();

			var testObjectCreator = new TestObjectCreator(factory);
			testObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions
				.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var setting = new JournalEntriesNumberCustomisationSetting();
			setting.AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN;

			var item = GetNewRegistryItem();
			item.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);
			AssertEquals("AllocateJournalEntriesNumberStartDate is wrong.", new DateTime(2024, 7, 1), AccountingConfigurationRegistry.Instance.AllocateJournalEntriesNumberStartDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}
	}

	[TestedType(typeof(JournalEntriesNumberCustomisationSettingRegistryDataType))]
	public class JournalEntriesNumberCustomisationSettingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<JournalEntriesNumberCustomisationSettingRegistryDataType>
	{
		#region Implementation

		protected override JournalEntriesNumberCustomisationSettingRegistryDataType GetNewDataType() => new JournalEntriesNumberCustomisationSettingRegistryDataType();

		protected override string ExpectedEditorName => "JournalEntriesNumberCustomisationSettingRegistryItemEditor";

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var journalEntriesNumberCustomisationSetting1 = new JournalEntriesNumberCustomisationSetting();
			journalEntriesNumberCustomisationSetting1.NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL;
			journalEntriesNumberCustomisationSetting1.AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON;

			var journalEntriesNumberCustomisationSetting2 = new JournalEntriesNumberCustomisationSetting();
			journalEntriesNumberCustomisationSetting2.NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.TRN;
			journalEntriesNumberCustomisationSetting2.AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(journalEntriesNumberCustomisationSetting1, new JournalEntriesNumberCustomisationSettingRegistryDataType().Serialise(journalEntriesNumberCustomisationSetting1)),
				new ValidSampleAndBinaryValueInDB(journalEntriesNumberCustomisationSetting2, new JournalEntriesNumberCustomisationSettingRegistryDataType().Serialise(journalEntriesNumberCustomisationSetting2))
			};
		}

		[TestDate(2024, 10, 8)]
		public override void TestGetSetValidValues()
		{
			var samples = GetValidSamples();
			var registryItem = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation;

			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			testObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			testObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2024);
			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions
				.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			foreach (var sample in samples)
			{
				registryItem.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, sample.ValidSample as JournalEntriesNumberCustomisationSetting);
				var readValue = registryItem.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
				AssertValuesEqual("New value set.", sample.ValidSample, readValue);
			}
		}

		#endregion

		#region Secdondary confirmation

		[TestDate(2024, 7, 21)]
		public void TestSecondaryConfirmation_NotShown_ProposedAllocationOptionIsNon_BeforeSave()
		{
			var companyPk = Env.CurrentCompanyPK;
			var fallbackLevel = new FallbackLevel(companyPk, Guid.Empty, Guid.Empty);
			var registryItem = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation;
			var proposedSetting = new JournalEntriesNumberCustomisationSetting(fallbackLevel)
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON
			};
			using (InitBeforeTestSecondaryConfirmation(registryItem, fallbackLevel, AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				var registryDataType = GetNewDataType();
				registryDataType.ValidateBeforeRegistryFormSave(registryItem, proposedSetting, fallbackLevel.CompanyPK(false), fallbackLevel.BranchPK, fallbackLevel.DepartmentPK);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		[TestDate(2024, 7, 21)]
		public void TestSecondaryConfirmation_Shown_Yes_ProposedAllocationOptionIsGen_BeforeSave()
		{
			var companyPk = Env.CurrentCompanyPK;
			var fallbackLevel = new FallbackLevel(companyPk, Guid.Empty, Guid.Empty);
			var registryItem = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation;
			var proposedSetting = new JournalEntriesNumberCustomisationSetting(fallbackLevel)
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN
			};
			using (InitBeforeTestSecondaryConfirmation(registryItem, fallbackLevel, AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				var registryDataType = GetNewDataType();
				registryDataType.ValidateBeforeRegistryFormSave(registryItem, proposedSetting, fallbackLevel.CompanyPK(false), fallbackLevel.BranchPK, fallbackLevel.DepartmentPK);
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				var previousMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals(true, previousMessage.WasQuestion);
				AssertEquals("Confirm Changing Journal Entries Number Customization", previousMessage.Caption);
				AssertEquals(
					"Number Rule and Allocation Option cannot be edited any more once Allocation Option has been set to GEN. Are you sure you want to save the change?",
					previousMessage.Text);
			}
		}

		[TestDate(2024, 7, 21)]
		public void TestSecondaryConfirmation_Shown_Cancel_ProposedAllocationOptionIsGen_BeforeSave()
		{
			var companyPk = Env.CurrentCompanyPK;
			var fallbackLevel = new FallbackLevel(companyPk, Guid.Empty, Guid.Empty);
			var registryItem = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation;
			var proposedSetting = new JournalEntriesNumberCustomisationSetting(fallbackLevel)
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN
			};
			using (InitBeforeTestSecondaryConfirmation(registryItem, fallbackLevel, AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				var registryDataType = GetNewDataType();
				AssertExceptionThrown<RegistryValidationException>(
					"Exception occurs if user canceled saving",
					"Canceled saving this registry value",
					() => registryDataType.ValidateBeforeRegistryFormSave(registryItem, proposedSetting, fallbackLevel.CompanyPK(false), fallbackLevel.BranchPK, fallbackLevel.DepartmentPK));
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		[TestDate(2024, 7, 21)]
		public void TestValidateException_OriginalAllocationOptionIsGen_BeforeSave()
		{
			var companyPk = Env.CurrentCompanyPK;
			var fallbackLevel = new FallbackLevel(companyPk, Guid.Empty, Guid.Empty);
			var registryItem = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation;
			string[] optionList =
			{
				AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON,
				AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
			};
			string[] ruleList =
			{
				AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL,
				AccountingConstants.JournalEntriesNumberCustomisationNumberRule.TRN,
				AccountingConstants.JournalEntriesNumberCustomisationNumberRule.GRP,
			};
			var proposedSettingList = new List<JournalEntriesNumberCustomisationSetting>();
			foreach (var option in optionList)
			{
				foreach (var rule in ruleList)
				{
					proposedSettingList.Add(
						new JournalEntriesNumberCustomisationSetting(fallbackLevel)
						{
							AllocationOption = option, NumberRule = rule
						}
					);
				}
			}

			var groupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var groupCode = groupCodeCollection.AddNew();
			groupCode.Code = "xxx";
			groupCode.Description = "Test Description";
			var groupCollection = new JournalEntriesClassificationGroupCollection(fallbackLevel);
			var group = groupCollection.AddNew();
			group.GroupCode = "xxx";
			group.TransactionType = "yyy";
			group.Ledger = "zz";
			using (AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetTemporaryValue(fallbackLevel.CompanyPK(false), fallbackLevel.BranchPK, fallbackLevel.DepartmentPK, groupCodeCollection))
			using (AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroup.SetTemporaryValue(fallbackLevel.CompanyPK(false), fallbackLevel.BranchPK, fallbackLevel.DepartmentPK, groupCollection))
			using (InitBeforeTestSecondaryConfirmation(registryItem, fallbackLevel, AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN))
			{
				foreach (var proposedSetting in proposedSettingList)
				{
					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
					var registryDataType = GetNewDataType();
					AssertExceptionThrown<RegistryValidationException>(
						"Exception occurs if original Allocation Option is GEN",
						"Number Rule and Allocation Option cannot be edited any more once Allocation Option has been set to GEN.",
						() => registryDataType.ValidateBeforeRegistryFormSave(registryItem, proposedSetting, fallbackLevel.CompanyPK(false), fallbackLevel.BranchPK, fallbackLevel.DepartmentPK));
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		IDisposable InitBeforeTestSecondaryConfirmation(JournalEntriesNumberCustomisationSettingRegistryItem registryItem, FallbackLevel fallbackLevel, string originalAllocationOption)
		{
			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			testObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2024);
			testObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions
				.SetValue(fallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty, true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var originalSetting = new JournalEntriesNumberCustomisationSetting(fallbackLevel)
			{
				AllocationOption = originalAllocationOption
			};
			return registryItem.SetTemporaryValue(fallbackLevel.CompanyPK(false), fallbackLevel.BranchPK, fallbackLevel.DepartmentPK, originalSetting);
		}

		#endregion
	}
}
