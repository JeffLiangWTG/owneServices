using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class ContainerSCDPlugInTest : BaseSCDPlugInTest
	{
		protected override SCDPlugIn GetPlugIn()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var container = consol.Containers.AddNew();
			return new SCDContainerPlugIn(container);
		}
	}
}
