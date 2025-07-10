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
	public class TGEConsolValueObjectDataAdapterTest : TestCaseWithFactory
	{
		public void TestAddNewShipmentsIfConsolExists()
		{
			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SystemDataRegistry.Instance.UpdateConsolDuringAutomaticImportAir.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				SystemDataRegistry.Instance.UpdateConsolShipmentsDuringAutomaticImportAir.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				Transport transport = consol.Transports[0];
				consol.JK_MasterBillNum = "MAWB";
				transport.JW_RL_NKLoadPort = "AUMEL";
				transport.JW_RL_NKDiscPort = "HKHKG";
				Factory.Save();
				Xsd.Consol consolXSD = new Xsd.Consol();
				consolXSD.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.AIR;
				consolXSD.ConsolDetail.TransportModeSpecified = true;
				consolXSD.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUBNE");
				consolXSD.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "HKHKG");
				consolXSD.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.AIR;
				consolXSD.ConsolDetail.ConsolType = Xsd.ConsolType.Direct;
				Xsd.ConsolIdentifier cIdentifier = consolXSD.ConsolIdentifier.AddNew();
				cIdentifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
				cIdentifier.Value = "MAWB";
				Xsd.Shipment shipmentXSD = consolXSD.Shipments.AddNew();
				Xsd.ShipmentIdentifier sIdentifier = shipmentXSD.ShipmentIdentifier.AddNew();
				sIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
				sIdentifier.Value = "HAWB";
				shipmentXSD.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;
				shipmentXSD.ShipmentDetails.PortOfOrigin.Port.Value = "AUBNE";
				shipmentXSD.ShipmentDetails.PortofDestination.Port.Value = "SGSIN";
				shipmentXSD.ShipmentDetails.Consignee = new Xsd.Organisation { EDICode = "EDICUS" };
				AssertEquals("Preconditions: there is no shipment attached to consol", 0, consol.Shipments.Count);
				int consolCount = Factory.GetDatabaseCount(typeof(ForwardingConsol));
				TGEConsolValueObjectDataAdapter adapter = new TGEConsolValueObjectDataAdapter();
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				adapter.CreateOrUpdateFromValueObject(consolXSD, context);
				AssertEquals("No consol is created", consolCount, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
				AssertEquals("Consol 's MAWB", "MAWB", consol.JK_MasterBillNum);
				AssertEquals("Consol 's port of loading is not updated", "AUMEL", consol.JK_RL_NKLoadForExportTransport);
				AssertEquals("Consol 's port of destination is not updated", "HKHKG", consol.JK_RL_NKDiscForExportTransport);
				AssertEquals("Consol 's agent type is not updated", "AGT", consol.JK_AgentType);
				AssertEquals("Consol has one shipment", 1, consol.Shipments.Count);
				ForwardingShipment shipment = consol.Shipments[0];
				AssertEquals("Shipment HAWB", "HAWB", shipment.JS_HouseBill);
			}
		}
	}
}
