using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	[TestedType(typeof(GLJournalHeaderForADAWCollection))]
	public class GLJournalHeaderForADAWCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GLJournalHeaderForADAWCollection>
	{
		public void TestNew()
		{
			var glJournalHeader = GetCollectionToTest().AddNew();
			AssertNotNull(glJournalHeader);
			AssertEquals("Header Type", "H", glJournalHeader.HeaderType);
			AssertEquals("Sequence", 0, glJournalHeader.Sequence);
		}

		protected override GLJournalHeaderForADAWCollection GetCollectionToTest()
		{
			return new GLJournalHeaderForADAWCollection(Factory, HeaderType);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GLJournalHeaderForADAW(Factory, HeaderType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			HeaderType = "H";
		}

		string HeaderType;
	}
}
