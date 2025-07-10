using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAContainerSynchroniserTest : SynchroniserTestCase
	{
		public void TestCusSCAContainerSynchroniser()
		{
			var source = Factory.New<ForwardingContainer>();
			var destination = Factory.New<CusSCAContainer>();
			var synchroniser = new CusSCAContainerSynchroniser(destination, source);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			source.JC_ContainerNum = "CNUM";
			source.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AssertEquals("CNUM", destination.CN_ContainerNumber);
			AssertEquals("20GP", destination.CN_RC_NKContainerType);
			AssertEquals(Core.Constants.ContainerModes.Containerised, destination.CN_ContainerMode);
			source.JC_IsEmptyContainer = true;
			AssertEquals(Core.Constants.ContainerModes.Empty, destination.CN_ContainerMode);
		}
	}
}
