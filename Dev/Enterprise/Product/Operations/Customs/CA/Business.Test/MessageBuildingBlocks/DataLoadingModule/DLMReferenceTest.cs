using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	sealed class DLMReferenceTest : TestCase
	{
		public void TestSerialise()
		{
			DLMReference reference = new DLMReference();
			reference.ReferenceNumber = "INV23432";
			AssertEquals("RINV23432".PadRight(36), reference.Serialise());
		}
	}
}
