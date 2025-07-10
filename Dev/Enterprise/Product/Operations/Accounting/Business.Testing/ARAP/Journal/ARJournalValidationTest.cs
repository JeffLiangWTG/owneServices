using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Journal.Testing
{
	public class ARJournalValidationTest : JournalValidationTest
	{
		protected override Type GetExpectedType() => typeof(ARJournal);

		protected override void AssertWhenTransactionCategoryIsPBW(ZPropertyInfo aH_TransactionCategoryInfo)
		{
			AssertHasErrors($"{aH_TransactionCategoryInfo.Value} is not permitted for AR journal", aH_TransactionCategoryInfo);
		}

		protected override bool ShouldTestAH_AG
		{
			get { return true; }
		}

		protected override Type HeaderType
		{
			get { return typeof(ARJournal); }
		}
	}
}
