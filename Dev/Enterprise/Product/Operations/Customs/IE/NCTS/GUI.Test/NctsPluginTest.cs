using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using NctsHeader = Enterprise.Customs.IE.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	sealed class NctsPluginTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (var plugIn = new NctsPluginForTest(consol))
			using (var control = plugIn.UserControl)
			{
				AssertType<Phase5NctsUserControlForPlugin>(control);
			}
		}

		public void TestSetNCTSPhaseIfNeeded()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (var plugIn = new NctsPluginForTest(consol))
			using (var control = plugIn.UserControl)
			{
				var header = plugIn.InBond as NctsHeader;
				AssertEquals("Application Code Is NC5", CusInBondApplicationCodeList.Codes.NCTS5, header.BH_ApplicationCode);
			}
		}
	}

	class NctsPluginForTest : NctsPlugin
	{
		public NctsPluginForTest(ICusInBondParent host) : base(host)
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			InternalInBond = header;
		}
	}
}
