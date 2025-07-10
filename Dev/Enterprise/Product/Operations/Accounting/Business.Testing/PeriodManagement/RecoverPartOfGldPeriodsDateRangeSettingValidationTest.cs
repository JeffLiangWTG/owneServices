using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PeriodManagement.Testing
{
	public class RecoverPartOfGldPeriodsDateRangeSettingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParentType()
		{
			AssertType<RecoverPartOfGldPeriodsDateRangeSetting>(Validator.Parent);
		}

		public void TestCheckStartDate()
		{
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2022, 07, 01));

			AssertEmpty();

			AssertJournalEntriesLastProcessedDate();

			AssertInFinancialPeriod();

			void AssertEmpty()
			{
				const string errorMsg = "Please enter a Start Date.";

				Validator.Parent.StartDate = ZDateTime.Empty;
				Validator.ValidateStartDate();
				AssertHasError(Validator.Parent.StartDateInfo, errorMsg);

				Validator.Parent.StartDate = new ZDateTime(2022, 06, 30);
				Validator.ValidateStartDate();
				AssertNoError(Validator.Parent.StartDateInfo, errorMsg);
			}

			void AssertJournalEntriesLastProcessedDate()
			{
				const string errorMsg = "Start Date cannot be set to before General Ledger Data Creation Date.";

				Validator.Parent.StartDate = new ZDateTime(2022, 06, 30);
				AssertLessThan("PreCondition", Validator.Parent.StartDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.Value);
				Validator.ValidateStartDate();
				AssertHasError(Validator.Parent.StartDateInfo, errorMsg);

				Validator.Parent.StartDate = new ZDateTime(2022, 07, 01);
				AssertGreaterThanOrEqualTo("PreCondition", Validator.Parent.StartDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.Value);
				Validator.ValidateStartDate();
				AssertNoError(Validator.Parent.StartDateInfo, errorMsg);
			}

			void AssertInFinancialPeriod()
			{
				const string errorMsg = "Start Date must be in a Financial Period.";

				AssertNull("PreCondition", PeriodCalculator.GetPeriodManagementFromDate(new ZDateTime(3000, 01, 01)));
				Validator.Parent.StartDate = new ZDateTime(3000, 01, 01);
				Validator.ValidateStartDate();
				AssertHasError(Validator.Parent.StartDateInfo, errorMsg);

				AssertNotNull("PreCondition", PeriodCalculator.GetPeriodManagementFromDate(new ZDateTime(2022, 07, 02)));
				Validator.Parent.StartDate = new ZDateTime(2022, 07, 02);
				Validator.ValidateStartDate();
				AssertNoError(Validator.Parent.StartDateInfo, errorMsg);
			}
		}

		public void TestCheckEndDate()
		{
			AssertEmpty();

			AssertEndDateNotLessThanStartDate();

			AssertEndDateIsInSameAccountingPeriodToStartDate();

			void AssertEmpty()
			{
				const string errorMsg = "Please enter an End Date.";

				Validator.Parent.EndDate = ZDateTime.Empty;
				Validator.ValidateEndDate();
				AssertHasError(Validator.Parent.EndDateInfo, errorMsg);

				Validator.Parent.EndDate = new ZDateTime(2022, 06, 30);
				Validator.ValidateEndDate();
				AssertNoError(Validator.Parent.EndDateInfo, errorMsg);
			}

			void AssertEndDateNotLessThanStartDate()
			{
				const string errorMsg = "End Date cannot be earlier than Start Date.";

				Validator.Parent.StartDate = new ZDateTime(2022, 07, 02);

				Validator.Parent.EndDate = new ZDateTime(2022, 07, 01);
				Validator.ValidateEndDate();
				AssertHasError("When End Date is less than Start Date.", Validator.Parent.EndDateInfo, errorMsg);

				Validator.Parent.EndDate = new ZDateTime(2022, 07, 02);
				Validator.ValidateEndDate();
				AssertNoError("When End Date equal to Start Date.", Validator.Parent.EndDateInfo, errorMsg);

				Validator.Parent.EndDate = new ZDateTime(2022, 07, 03);
				Validator.ValidateEndDate();
				AssertNoError("When End Date is greater than Start Date.", Validator.Parent.EndDateInfo, errorMsg);
			}

			void AssertEndDateIsInSameAccountingPeriodToStartDate()
			{
				const string errorMsg = "End Date must lie in same Accounting Period as entered Start Date.";

				var period07 = PeriodCalculator.GetPeriodManagementFromDate(new ZDateTime(2022, 07, 01));
				AssertEquals("PreCondition, period07 Start Date.", new ZDateTime(2022, 07, 01), period07.AM_StartDate.Date);
				AssertEquals("PreCondition, period07 End Date.", new ZDateTime(2022, 07, 31), period07.AM_EndDate.Date);

				var period08 = PeriodCalculator.GetPeriodManagementFromDate(new ZDateTime(2022, 08, 01));
				AssertEquals("PreCondition, period08 Start Date.", new ZDateTime(2022, 08, 01), period08.AM_StartDate.Date);
				AssertEquals("PreCondition, period08 End Date.", new ZDateTime(2022, 08, 31), period08.AM_EndDate.Date);

				Validator.Parent.StartDate = new ZDateTime(2022, 07, 15);
				Validator.Parent.EndDate = new ZDateTime(2022, 08, 01);
				Validator.ValidateEndDate();
				AssertHasError("When Start Date(2022-07-15) and End Date(2022-08-01) in different period.", Validator.Parent.EndDateInfo, errorMsg);

				Validator.Parent.StartDateInfo.AddError("Dummy Error");
				Validator.ValidateEndDate();
				AssertNoError("When Start Date(2022-07-15) and End Date(2022-08-01) in different period. However Start Date has error."
					, Validator.Parent.EndDateInfo
					, errorMsg
				);

				Validator.Parent.StartDate = new ZDateTime(2022, 07, 10);
				Validator.Parent.EndDate = new ZDateTime(2022, 07, 15);
				Validator.ValidateEndDate();
				AssertNoErrors("PreCondition, Start Date has no error.", Validator.Parent.StartDateInfo);
				AssertNoError("When Start Date(2022-07-10) and End Date(2022-07-15) in same period", Validator.Parent.EndDateInfo, errorMsg);

				Validator.Parent.StartDate = new ZDateTime(2022, 08, 01);
				Validator.Parent.EndDate = new ZDateTime(2022, 08, 31);
				Validator.ValidateEndDate();
				AssertNoErrors("PreCondition, Start Date has no error.", Validator.Parent.StartDateInfo);
				AssertNoError("When Start Date(2022-08-01) and End Date(2022-08-31) in same period", Validator.Parent.EndDateInfo, errorMsg);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Validator = new RecoverPartOfGldPeriodsDateRangeSettingValidation(new RecoverPartOfGldPeriodsDateRangeSetting());

			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);

			Factory.Save();
		}

		RecoverPartOfGldPeriodsDateRangeSettingValidation Validator;

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		TestObjectCreator fTestObjectCreator;

		protected AccountingPeriodCalculator PeriodCalculator => calc ?? (calc = new AccountingPeriodCalculator(new BusinessObjectFactory()));
		AccountingPeriodCalculator calc;
	}
}