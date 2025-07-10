using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	sealed class DLMContainerTest : TestCase
	{
		public void TestSerialise()
		{
			DLMContainer container = new DLMContainer();
			container.ContainerNumber = "TURE2342222";
			AssertEquals("CTURE2342222".PadRight(26), container.Serialise());
		}
	}
}
