using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.MacroEvaluator.Testing
{
	abstract class EvaluatorManagerTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected BusinessObject ShipmentBO
		{
			get
			{
				if (shipmentBO == null)
				{
					shipmentBO = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				}

				return shipmentBO;
			}
		}
		BusinessObject shipmentBO;

		#endregion
	}
}
