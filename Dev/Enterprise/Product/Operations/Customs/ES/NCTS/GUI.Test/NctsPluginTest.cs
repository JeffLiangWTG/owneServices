using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	public class NctsPluginTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (var plugIn = new NctsPluginForTest(consol))
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
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			InternalInBond = header;
		}
	}

	class Phase5NctsPluginForTest : NctsPlugin
	{
		public Phase5NctsPluginForTest(ICusInBondParent host) : base(host)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			InternalInBond = header;
		}
	}
}
