using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(NctsPlugin))]
sealed class NctsPluginTest : TestCaseWithFactory
{
	public void TestGetNewUserControl()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var plugIn = new NctsPluginForTest(shipment);
		var control = plugIn.GetNewUserControlExposed();
		AssertType<Phase5NctsUserControlForPlugin>(control);
		control.Dispose();
		plugIn.Dispose();
	}

	sealed class NctsPluginForTest : NctsPlugin
	{
		public NctsPluginForTest(ICusInBondParent host) : base(host)
		{
			var header = Factory.New<Business.NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			InternalInBond = header;
		}

		public Control GetNewUserControlExposed() => base.GetNewUserControl();
	}
}
