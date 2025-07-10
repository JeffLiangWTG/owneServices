using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.SWL.Testing
{
	public class SWLRegistryItemTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			SWLRegistryItem item = new SWLRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryOptions.IsHidden);
			AssertEquals("Name", "Name", item.Name);
			AssertEquals("Category", "Category", item.Category);
			AssertEquals("Caption", "Caption", item.Caption);
			AssertEquals("Hint", "Hint", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.CompanyDepartment, item.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden, item.Options);
		}
	}
}
