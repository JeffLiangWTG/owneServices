using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class LoadListConsolSCDPlugInTest : BaseSCDPlugInTest
	{
		protected override SCDPlugIn GetPlugIn()
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			return new SCDLoadListPlugIn(loadList);
		}
	}
}
