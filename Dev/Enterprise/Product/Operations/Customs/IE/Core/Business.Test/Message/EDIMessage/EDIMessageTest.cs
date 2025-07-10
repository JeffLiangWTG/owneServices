using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class EDIMessageTest : TestCaseWithFactory
	{
		public void TestTypeDecider()
		{
			AssertType<EDIMessageTypeDecider>(EDIMessage.TypeDecider);
		}
	}
}
