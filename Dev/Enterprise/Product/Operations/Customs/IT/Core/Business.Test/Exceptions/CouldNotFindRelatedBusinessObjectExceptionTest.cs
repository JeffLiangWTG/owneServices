using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CouldNotFindRelatedBusinessObjectExceptionTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var ex = new CouldNotFindRelatedBusinessObjectException("Unable to locate the related sent interchange.");
		AssertEquals("Message", "Unable to locate the related sent interchange.", ex.Message);
	}
}
