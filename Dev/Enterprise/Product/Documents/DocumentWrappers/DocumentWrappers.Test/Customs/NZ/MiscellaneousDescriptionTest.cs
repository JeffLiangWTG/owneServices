using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	sealed class MiscellaneousDescriptionTest : TestCaseWithFactory
	{
		public void TestDescription()
		{
			AssertEquals("Description", "some description", new MiscellaneousDescription(Factory, "some description").Description);
		}
	}
}
