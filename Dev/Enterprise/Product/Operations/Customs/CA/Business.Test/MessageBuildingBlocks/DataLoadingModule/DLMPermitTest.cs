using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	sealed class DLMPermitTest : TestCase
	{
		public void TestSerialise()
		{
			DLMPermit permit = new DLMPermit();
			permit.PermitNumber = "PERMIT23432";
			AssertEquals("PPERMIT23432".PadRight(36), permit.Serialise());
		}
	}
}
