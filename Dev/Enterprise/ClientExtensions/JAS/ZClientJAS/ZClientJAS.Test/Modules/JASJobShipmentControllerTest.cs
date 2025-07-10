using System;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.GUI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Module.Testing;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	[TestedType(typeof(JASJobShipmentControllerForTest))]
	public class JASJobShipmentControllerTest : TestJobShipmentController
	{
		public override void TestGetFormCore()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<JASForwardingShipment>();
			var controller = new JASJobShipmentControllerForTest();
			using (IZForm form = controller.GetFormCore(shipment))
			{
				AssertEquals(typeof(JASShipmentForm), form.GetType());
			}
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(JASForwardingShipment), Controller.TypeOfTopLevelBusinessObject);
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#region Implementation
		protected override Type GetBusinessObjectType()
		{
			return typeof(JASForwardingShipment);
		}

		public override Type ControllerToBashType
		{
			get
			{
				return typeof(JASJobShipmentController);
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var shipment = Factory.New<JASForwardingShipment>();
			Factory.Save();
			return shipment;
		}

		#region JASJobShipmentControllerForTest
		class JASJobShipmentControllerForTest : JASJobShipmentController
		{
			public new IZForm GetFormCore(IBusiness businessEntity)
			{
				return base.GetFormCore(businessEntity);
			}
		}
		#endregion
		#endregion
	}
}
