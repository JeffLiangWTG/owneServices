using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocumentWrapperFactoryTest : TestCaseWithFactory
	{
		public void TestConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol docForwardingConsol = (DocForwardingConsol)DocumentWrapperFactory.CreateWrapper(Enterprise.Core.Constants.DataContext.ForwardingConsol, consol);
			AssertNotNull("Should not return null", docForwardingConsol);
		}

		public void TestUNLOCO()
		{
			var uNLOCO = Factory.New<RefUNLOCO>();
			DocUNLOCO docUNLOCO = (DocUNLOCO)DocumentWrapperFactory.CreateWrapper(Enterprise.Core.Constants.DataContext.UNLOCO, uNLOCO);
			AssertNotNull("Should not return null", docUNLOCO);
		}
	}
}
