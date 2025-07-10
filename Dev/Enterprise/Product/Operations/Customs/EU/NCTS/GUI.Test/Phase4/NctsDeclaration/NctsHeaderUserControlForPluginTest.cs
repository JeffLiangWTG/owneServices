using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(DummyHostFormForBashing))]
	sealed class NctsHeaderUserControlForPluginTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var form = new DummyHostFormForBashing(shipment);
			form.CaptionRenderingEnabled = true;
			form.Text = "ZTemplateForm@#$_Basher_Test";
			return form;
		}
	}

	sealed class DummyHostFormForBashing : ZTemplateForm
	{
		public DummyHostFormForBashing(BusinessObject bizo)
			: base(bizo)
		{
			PlugIns.Add(ControllerIDs.Customs.EU.NctsMovementController);
		}
	}
}
