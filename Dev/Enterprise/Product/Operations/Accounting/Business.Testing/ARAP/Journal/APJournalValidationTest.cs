using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Journal.Testing
{
	public class APJournalValidationTest : JournalValidationTest
	{
		protected override Type GetExpectedType() => typeof(APJournal);

		protected override void AssertWhenTransactionCategoryIsPBW(ZPropertyInfo aH_TransactionCategoryInfo)
		{
			AssertNoErrors($"{aH_TransactionCategoryInfo.Value} is permitted for AP journal", aH_TransactionCategoryInfo);
		}

		protected override bool ShouldTestAH_AG
		{
			get { return true; }
		}

		protected override Type HeaderType
		{
			get { return typeof(APJournal); }
		}
	}
}
