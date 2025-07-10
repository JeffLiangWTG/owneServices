using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JournalEntriesNumberCustomisationCollection))]
	public class JournalEntriesNumberCustomisationCollectionTest : TransactionNumberSequenceCustomisationCollectionTest
	{
		protected override TransactionNumberSequenceCustomisationCollection GetCollectionToTest()
		{
			return new JournalEntriesNumberCustomisationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new JournalEntriesNumberCustomisation();
		}
	}
}
