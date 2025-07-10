using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	abstract class AmendmentDetailsManagerTest : TestCaseWithFactory
	{
		abstract public void TestAmendmentType();
		abstract public void TestAmendmentVersion();
		abstract public void TestAmendedItems();
	}
}
