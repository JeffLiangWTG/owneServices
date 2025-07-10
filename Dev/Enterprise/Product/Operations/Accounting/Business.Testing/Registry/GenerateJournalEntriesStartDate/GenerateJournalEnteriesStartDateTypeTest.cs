using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GenerateJournalEntriesStartDateType))]
	class GenerateJournalEntriesStartDateTypeTest : RegistryDataTypeTestCase<GenerateJournalEntriesStartDateType>
	{
		[TestDate(2023, 3, 28, 0, 0, 0)]
		public override void TestGetSetValidValues()
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var validSamples = GetValidSamples();
			var newRegistryItemWithDefaultDefaultValue = new RegistryItemImpl("name", (NoResString)"TestCategory/TestSubCat", (NoResString)"Caption", (NoResString)"Hint", DataType, RegistryStorageFlags.All);

			for (var i = 0; i < validSamples.Length; i++)
			{
				var validSampleAndBinaryValueInDB = validSamples[i];
				newRegistryItemWithDefaultDefaultValue.SetValue(companyPK, Guid.Empty, Guid.Empty, (DateTime)validSampleAndBinaryValueInDB.ValidSample);
				AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, (DateTime)validSampleAndBinaryValueInDB.ValidSample);
				AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, (DateTime)validSampleAndBinaryValueInDB.ValidSample);
				var valueWithoutFallback = newRegistryItemWithDefaultDefaultValue.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
				AssertValuesEqual("New value set.", validSampleAndBinaryValueInDB.ValidSample, valueWithoutFallback);
			}
		}

		public void TestSetNonCurrentCompanyValue()
		{
			var companyPK = GlbCompany.GetDemoCompany(Factory).PK.ToGuid();
			var firstStartDate = new DateTime(2023, 01, 01);
			var year = 2023;
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(year, companyPK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(year - 1, companyPK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			var newRegistryItemWithDefaultDefaultValue = new RegistryItemImpl("name", (NoResString)"TestCategory/TestSubCat", (NoResString)"Caption", (NoResString)"Hint", DataType, RegistryStorageFlags.All);

			AssertNoExceptionThrown(() => newRegistryItemWithDefaultDefaultValue.SetValue(companyPK, Guid.Empty, Guid.Empty, firstStartDate));
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.SetValue(companyPK, Guid.Empty, Guid.Empty, firstStartDate);
			AssertExceptionThrown<RegistryValidationException>("Exception occurs when LastProcessedDate is empty", () => newRegistryItemWithDefaultDefaultValue.SetValue(companyPK, Guid.Empty, Guid.Empty, firstStartDate));

			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(companyPK, Guid.Empty, Guid.Empty, firstStartDate);
			AssertNoExceptionThrown("No exception if LastProcessedDate has value", () => newRegistryItemWithDefaultDefaultValue.SetValue(companyPK, Guid.Empty, Guid.Empty, new DateTime(2022, 01, 01)));
		}

		#region Control Accounts Validation

		public void TestForcedControlAccountsValidation()
		{
			var company = GlbCompany.CurrentCompany;
			company.GC_IsGSTCashBasis = false;
			company.GC_IsGSTRegistered = false;
			company.Factory.Save();

			AccountingConfigurationRegistry.Instance.ClearAllControlAccountRegistryItems();

			var controlAccountList = new List<GuidRegistryItem>
			{
				AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount,
				AccountingConfigurationRegistry.Instance.AccruedCostControlAccount,
				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount,
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount,
				AccountingConfigurationRegistry.Instance.ARControlAccount,
				AccountingConfigurationRegistry.Instance.APControlAccount
			};

			AssertControlAccountValidation(controlAccountList);
		}

		public void TestGSTCashBasisControlAccountsValidation()
		{
			var company = GlbCompany.CurrentCompany;
			company.GC_IsGSTCashBasis = true;
			company.GC_IsGSTRegistered = true;
			company.Factory.Save();

			using (AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
			using (AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
			using (AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
			using (AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var controlAccountList = new List<GuidRegistryItem>
				{
					AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount,
					AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount,
					AccountingConfigurationRegistry.Instance.GSTOutputControlAccount,
					AccountingConfigurationRegistry.Instance.GSTInputControlAccount
				};

				AssertControlAccountValidation(controlAccountList);
			}
		}

		public void TestGSTRegisteredControlAccountsValidation()
		{
			var company = GlbCompany.CurrentCompany;
			company.GC_IsGSTCashBasis = false;
			company.GC_IsGSTRegistered = true;
			company.Factory.Save();

			using (AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
			using (AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var controlAccountList = new List<GuidRegistryItem>
				{
					AccountingConfigurationRegistry.Instance.GSTOutputControlAccount,
					AccountingConfigurationRegistry.Instance.GSTInputControlAccount
				};

				AssertControlAccountValidation(controlAccountList);
			}
		}

		void AssertControlAccountValidation(List<GuidRegistryItem> controlAccounts)
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var firstFinacialDay = new DateTime(2023, 1, 1);
			var newRegistryItemWithDefaultDefaultValue = new RegistryItemImpl("ControlAccountValidation", (NoResString)"TestCategory/TestSubCat", (NoResString)"Caption", (NoResString)"Hint", DataType, RegistryStorageFlags.All);

			controlAccounts.ForEach(controlAccount =>
			{
				AssertExceptionThrown(typeof(RegistryValidationException), "Please set up the following Control Accounts in the registry", () => newRegistryItemWithDefaultDefaultValue.SetValue(companyPK, Guid.Empty, Guid.Empty, firstFinacialDay), true);
				controlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			});

			AssertNoExceptionThrown("Succeed", () => newRegistryItemWithDefaultDefaultValue.SetValue(companyPK, Guid.Empty, Guid.Empty, firstFinacialDay));
		}

		#endregion

		protected override GenerateJournalEntriesStartDateType GetNewDataType()
		{
			return new GenerateJournalEntriesStartDateType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[] { new ValidSampleAndBinaryValueInDB(new DateTime(2023, 1, 1), GetNewDataType().Serialise(new DateTime(2023, 1, 1))),
			new ValidSampleAndBinaryValueInDB(new DateTime(2022, 1, 1), GetNewDataType().Serialise(new DateTime(2022, 1, 1))) };
		}

		[TestDate(2023, 3, 28, 0, 0, 0)]
		protected override object[] GetInvalidSamples()
		{
			var today = ZDateTime.Today.ToDateTime();
			return new object[] { today, today.AddDays(1) };
		}

		protected override void SetUp()
		{
			base.SetUp();
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var year = 2023;
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(year, companyPK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(year - 1, companyPK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			TestObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator fTestObjectCreator;
	}
}
