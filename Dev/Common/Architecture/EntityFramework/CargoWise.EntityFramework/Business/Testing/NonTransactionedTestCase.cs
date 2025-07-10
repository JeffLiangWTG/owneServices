#if DEBUG

using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[UseSnapshotProtection]
	public abstract class NonTransactionedTestCase : TestCase
	{
		public static void AssertHasError(ZPropertyInfo info, string notificationExpectedToBeFound)
		{
			TestCaseWithFactory.AssertHasError(info, notificationExpectedToBeFound);
		}

		public static void AssertNoErrors(BusinessObject bizo)
		{
			TestCaseWithFactory.AssertNoErrors(bizo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (ShouldApplyClientSpecificDatabaseSchema)
			{
				TransactionedTestCase.RunClientDbCreateScripts();
			}

			Factory = new BusinessObjectFactory();
		}

		protected BusinessObjectFactory Factory { get; private set; }

		protected DbConnection TestConnection => Db.Connection;

		protected virtual bool ShouldApplyClientSpecificDatabaseSchema => false;
	}
}

#endif
