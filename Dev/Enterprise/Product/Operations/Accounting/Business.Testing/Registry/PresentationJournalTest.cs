using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	class PresentationJournalTest : TestCase
	{
		public void TestPresentationJournalDefaultValueForBI()
		{
			var item = new GLPresentationJournalCategoryRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System);
			AssertEquals("DefaultValue.Count", 1, item.DefaultValue.Count);
			AssertEquals("Defualt code value for presentation journal has been changed.It is needed to update initial load query of CUS__PresentationJournal", "ELM", item.Value[0].Code);
			AssertEquals("Defualt description value for presentation journal has been changed.It is needed to update initial load query of CUS__PresentationJournal", "Elimination Category", item.Value[0].Description);
		}
	}
}