using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	[TestedType(typeof(GLJournalLineSubAccountForADAWCollection))]
	public class GLJournalLineSubAccountForADAWCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GLJournalLineSubAccountForADAWCollection>
	{
		protected override GLJournalLineSubAccountForADAWCollection GetCollectionToTest()
		{
			return new GLJournalLineSubAccountForADAWCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GLJournalLineSubAccountForADAW(Factory);
		}
	}
}
