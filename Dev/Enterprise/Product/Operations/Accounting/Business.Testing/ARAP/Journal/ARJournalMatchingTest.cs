using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Journal.Testing
{
	[TestedType(typeof(ARJournal))]
	public class ARJournalMatchingTest : JournalMatchingTest
	{
		protected override Journal GetNewJournal()
		{
			return Factory.New<ARJournal>();
		}
	}
}
