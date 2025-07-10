using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class NctsPluginTest : TestCaseWithFactory
{
	public void TestUserControlForPlugin()
	{
		using (var plugIn = new NctsPluginForTest(Factory.New<ForwardingConsol>()))
		using (var control = plugIn.UserControl)
		{
			AssertType<NctsUserControlForPlugin>(control);
		}
	}

	public void TestUserControlPhase5()
	{
		var consol = Factory.New<ForwardingConsol>();
		using (var plugIn = new Phase5NctsPluginForTest(consol))
		using (var control = plugIn.UserControl)
		{
			AssertType<Phase5NctsUserControlForPlugin>(control);
		}
	}
}

class NctsPluginForTest : NctsPlugin
{
	public NctsPluginForTest(ICusInBondParent host) : base(host)
	{
		var header = Factory.NewDepartureNctsHeader();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		InternalInBond = header;
	}
}

class Phase5NctsPluginForTest : NctsPlugin
{
	public Phase5NctsPluginForTest(ICusInBondParent host) : base(host)
	{
		var header = Factory.NewDepartureNctsHeader();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		InternalInBond = header;
	}
}
