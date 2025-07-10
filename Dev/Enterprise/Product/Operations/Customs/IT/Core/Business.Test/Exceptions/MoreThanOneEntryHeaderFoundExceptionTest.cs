using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class MoreThanOneEntryHeaderFoundExceptionTest : TestCaseWithFactory
{
	public void TestMoreThanOneEntryHeaderFoundException()
	{
		var ex = new MoreThanOneEntryHeaderFoundException();
		AssertEquals("Message", "More than one entry header found.", ex.Message);
	}
}
