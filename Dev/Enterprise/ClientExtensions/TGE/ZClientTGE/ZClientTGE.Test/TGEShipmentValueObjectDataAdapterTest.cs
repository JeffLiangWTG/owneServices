using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.TGE.PMS.Testing
{
	public class TGEShipmentValueObjectDataAdapterTest : TestCaseWithFactory
	{
		public void TestSetNotificationIfShipmentExists()
		{
			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SystemDataRegistry.Instance.UpdateConsolShipmentsDuringAutomaticImportOther.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				Transport transport = consol.Transports[0];
				consol.JK_MasterBillNum = "MAWB";
				transport.JW_RL_NKLoadPort = "AUMEL";
				transport.JW_RL_NKDiscPort = "HKHKG";
				ForwardingShipment shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HAWB";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKDestination = "NZAKL";
				shipment.JS_RL_NKOrigin = "AUSYD";
				Factory.Save();
				Xsd.Shipment shipmentXSD = new Xsd.Shipment();
				Xsd.ShipmentIdentifier idenitifer = shipmentXSD.ShipmentIdentifier.AddNew();
				idenitifer.Value = "HAWB";
				idenitifer.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
				shipmentXSD.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;
				shipmentXSD.ShipmentDetails.PortOfOrigin.Port.Value = "AUSYD";
				shipmentXSD.ShipmentDetails.PortofDestination.Port.Value = "NZAKL";
				int shipmentCount = Factory.GetDatabaseCount(typeof(ForwardingShipment));
				TGEShipmentValueObjectDataAdapter adapter = new TGEShipmentValueObjectDataAdapter(consol);
				NotificationBuffer buffer = new NotificationBuffer();
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
				AssertEquals("Preconditions: Buffer doesnt have any error", false, buffer.HasErrors);
				adapter.CreateOrUpdateFromValueObject(shipmentXSD, context);
				AssertEquals("Shipment has not been created", shipmentCount, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals("Notification has error", true, buffer.HasErrors);
			}
		}
	}
}
