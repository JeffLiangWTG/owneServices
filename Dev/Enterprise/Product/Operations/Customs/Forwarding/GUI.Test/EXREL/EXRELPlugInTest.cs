using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Forwarding.GUI.Testing
{
	class EXRELPlugInTest : TestCaseWithFactory
	{
		public void Test_Visibility()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			using (FormForTest form = new FormForTest(shipment))
			{
				form.Show();
				AssertEquals(true, form.EXRELPlugIn.Enabled);
			}
		}

		class FormForTest : ZTemplateForm
		{
			public FormForTest(ForwardingShipment businessEntity) : base(businessEntity)
			{
				PlugIns.Add(ControllerIDs.ExportConsignmentReleaseAdvice);
			}

			public EXRELPlugIn EXRELPlugIn
			{
				get
				{
					return (EXRELPlugIn)PlugIns.GetPlugIn(ControllerIDs.ExportConsignmentReleaseAdvice);
				}
			}
		}
	}
}
