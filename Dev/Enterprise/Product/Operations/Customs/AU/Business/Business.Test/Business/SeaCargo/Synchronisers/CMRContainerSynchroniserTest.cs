using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRContainerSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchroniseContainerModes()
		{
			ForwardingConsol consol = CreateGroupageConsol();
			CommonContainer jobContainer = consol.Containers.AddNew();
			CusSCAOceanBill bill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer sCAContainer = bill.Containers.AddNew();
			CMRContainerSynchroniser synchroniser = new CMRContainerSynchroniser(sCAContainer, jobContainer, consol);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			jobContainer.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FreightAllKind;
			AssertEquals("FAK => LCL", Enterprise.Core.Constants.ContainerModes.LCL, sCAContainer.CN_ContainerMode);
		}

		public void TestFCXIsGeneratedForBCN()
		{
			ForwardingConsol consol = CreateGroupageConsol();
			CommonContainer jobContainer = consol.Containers.AddNew();
			jobContainer.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;

			CusSCAOceanBill bill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer sCAContainer = bill.Containers.AddNew();
			CMRContainerSynchroniser synchroniser = new CMRContainerSynchroniser(sCAContainer, jobContainer, consol);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("BCN => FCX", Enterprise.Core.Constants.ContainerModes.FCLMixedShipper, sCAContainer.CN_ContainerMode);
		}
	}
}
