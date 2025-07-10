using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMHeaderCompanyRegisterWrapperTest : TestCaseWithFactory
{
	public void TestProperties()
	{
		var wrapper = new IMHeaderCompanyRegisterWrapper();
		AssertEquals("Number", "", wrapper.Number);
		AssertEquals("Series", "", wrapper.Series);
		AssertEquals("Date", ZDate.Empty, wrapper.Date);
	}

	public void TestConstructor()
	{
		AssertNoExceptionThrown(() => { new IMHeaderCompanyRegisterWrapper(); });
	}
}
