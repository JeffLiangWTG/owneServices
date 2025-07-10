using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

class TemplateTestCase : TestCaseWithFactory
{
	protected void TestTemplate()
	{
		AssertEquals("Please implement a test before checkin", true, IsTemplate);
	}

	protected bool IsTemplate => GetType().FullName.IndexOf("EU.TemporaryStorage") > -1;
}
