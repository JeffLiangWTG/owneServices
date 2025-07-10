using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.FLF.Testing
{
	public class FLFForwardingConsolTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertEquals("Type of New() return value", typeof(FLFForwardingConsol), FLFForwardingConsol.New(Factory).GetType());
		}

		public void TestDocumentSupporter()
		{
			FLFForwardingConsol consol = Factory.New<FLFForwardingConsol>();
			AssertEquals("Type of DocumentSupporter", typeof(FLFForwardingConsolDocumentSupporter), consol.DocumentSupporter.GetType());
		}
	}
}
