using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class VisualizerMenuCustomisationFormCreatorTest : TestCase
	{
		public void TestCreateVisualizerMenuCustomisationForm()
		{
			var factory = new BusinessObjectFactory();
			var consol = (BusinessObject)factory.New<Forwarding.IForwardingConsol>();

			var creator = new VisualizerMenuCustomisationFormCreator();
			using (var form = (ZForm)creator.CreateVisualizerMenuCustomisationForm(consol))
			{
				AssertType<VisualizerMenuCustomisationForm>(form);
			}
		}

		public void TestGetCustomizeFormCheckpoint()
		{
			var factory = new BusinessObjectFactory();
			var shipment = (BusinessObject)factory.New<Forwarding.IForwardingShipment>();

			var creator = new VisualizerMenuCustomisationFormCreator();
			AssertEquals(Env.Security.MaintainShipmentCustomiseForms, (SecurityCheckpoint)creator.GetCustomizeFormCheckpoint(shipment));
		}
	}
}
