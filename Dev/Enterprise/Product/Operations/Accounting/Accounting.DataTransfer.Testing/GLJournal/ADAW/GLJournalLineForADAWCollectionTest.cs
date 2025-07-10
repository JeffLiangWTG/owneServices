using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	[TestedType(typeof(GLJournalLineForADAWCollection))]
	public class GLJournalLineForADAWCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GLJournalLineForADAWCollection>
	{
		public void TestNew()
		{
			var glJournalLine = GetCollectionToTest().AddNew();
			AssertNotNull(glJournalLine);
			AssertEquals("Sequence", 0, glJournalLine.Sequence);
		}

		protected override GLJournalLineForADAWCollection GetCollectionToTest()
		{
			return new GLJournalLineForADAWCollection(Factory, Header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GLJournalLineForADAW(Factory, Header);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Header = new GLJournalHeaderForADAW(Factory, "F");
		}

		GLJournalHeaderForADAW Header;
	}
}
