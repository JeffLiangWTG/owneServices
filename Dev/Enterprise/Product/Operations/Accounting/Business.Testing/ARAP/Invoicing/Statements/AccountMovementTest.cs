using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(AccountMovement))]
	public class AccountMovementTest : TransactionHeaderTest
	{
		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedValue) => 0;

		public void TestZDecimalsHaveCorrectDecimalPlacesAccountMovement()
		{
			var movement = Factory.New<AccountMovement>();

			var localList = new List<string>
				{
					nameof(movement.OpeningBalance)
				};

			var tester = new DecimalPlacesAttributeTester(movement, movement.Company);
			tester.CheckLocalCurrency(localList, nameof(movement.LocalCurrencyDecimals));
		}

		public new void TestAgePeriodToDueDate()
		{
			Header.AgePeriod = PeriodManagementTestHelper.InvalidPeriodInt;
			var periodCalculator = new AccountingPeriodCalculator(Header.Factory);
			AssertEquals("Should return invalid period", periodCalculator.GetPeriodFromDate(Header.AH_PostDate), Header.AgePeriod);
			AssertEquals("As Due Date is invalid, AH_PostDate will be returned", Header.AH_PostDate, Header.AH_DueDate);

			Header.AgePeriod = 0;
			AssertEquals("Due Date should be empty for empty period", Header.AH_PostDate, Header.AH_DueDate);
		}

		public new void TestDueDateToAgePeriod()
		{
			Header.AH_DueDate = ZDateTime.Invalid;
			var periodCalculator = new AccountingPeriodCalculator(Header.Factory);
			AssertEquals("Age Period should return based on AH_PostDate", periodCalculator.GetPeriodFromDate(Header.AH_PostDate), Header.AgePeriod);

			Header.AH_DueDate = PeriodManagementTestHelper.FuturePeriod.AM_StartDate.AddDays(5);
			AssertEquals("Age Period should be future period", FuturePeriod.AM_Period, Header.AgePeriod);
		}

		public override void TestBizObjectFields()
		{
			Assert(true);
		}
	}
}
