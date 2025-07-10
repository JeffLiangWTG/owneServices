using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IN.Business.Testing;

class TemplateTestCase : TestCaseWithFactory
{
	protected void AssertTemplate()
	{
		AssertEquals("Please implement a test before checkin", true, IsTemplate);
	}

	protected bool IsTemplate => GetType().FullName.IndexOf("IN") > -1;
}
