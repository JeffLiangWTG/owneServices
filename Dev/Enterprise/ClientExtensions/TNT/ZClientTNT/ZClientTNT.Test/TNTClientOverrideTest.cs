using System;
using CargoWise.EntityFramework;
using Enterprise.Client.TNT.AirCargo;
using Enterprise.Client.TNT.DocWrappers;
using Enterprise.Client.TNT.GUI;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class TNTClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestControllerOverrides()
		{
			ClientOverride clientOverride = ClientOverride.Instance;
			AssertNotNull(clientOverride.ControllerOverrides);
			RegistrationInfo info = clientOverride.ControllerOverrides[ControllerIDs.Customs.AU.AirCargo, Core.Constants.CountryCodes.Australia];
			Assert(info.IsClientOverride);
			Type controllerOverrideType = typeof(TNTAUCustomsAirCargoController);
			Assert(info.TypePath.IndexOf(controllerOverrideType.Assembly.FullName) > -1);
		}

		public void TestModuleOverrides()
		{
			ClientOverride clientOverride = ClientOverride.Instance;
			AssertNotNull(clientOverride.ModuleOverrides);
			RegistrationInfo info = clientOverride.ModuleOverrides[ModuleIDs.Customs.AU.AirCargo, Core.Constants.CountryCodes.Australia];
			Assert(info.IsClientOverride);
			Type moduleOverrideType = typeof(TNTAirCargoModuleOverride);
			Assert(info.TypePath.IndexOf(moduleOverrideType.Assembly.FullName) > -1);
		}

		public void TestInitialiseAndUnitialise()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CusMAWB masterBill = factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			CusHAWBMessageManager hAWBManager = new CusHAWBMessageManager(delegate
			{
				return houseBill;
			});
			CusMAWBMessageManager mAWBManager = new CusMAWBMessageManager(delegate
			{
				return masterBill;
			});
			ForwardingConsol consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			ClientOverride.Instance.Uninitialise();
			using (AirCargoShipmentMenu menu = AirCargoShipmentMenu.New(hAWBManager))
			{
				AssertEquals(typeof(AirCargoShipmentMenu), menu.GetType());
			}

			using (AirCargoMasterMenu menu = AirCargoMasterMenu.New(masterBill, mAWBManager))
			{
				AssertEquals(typeof(AirCargoMasterMenu), menu.GetType());
			}

			using (AirCargoMasterMenu menu = AirCargoMasterMenu.New(consol, mAWBManager))
			{
				AssertEquals(typeof(AirCargoMasterMenu), menu.GetType());
			}

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ExportAWBHeader aWB = shipment.AWBHeader;
			DocAWB doc = DocAWB.New(aWB, factory);
			DocShipment docShipment = DocShipment.New(shipment, factory);
			AssertEquals("Doc should be a standard DocAWB", typeof(DocAWB), doc.GetType());
			AssertEquals("DocShipment should be a standard DocShipment", typeof(DocForwardingShipment), docShipment.GetType());
			ClientOverride.Instance.Initialise();
			using (AirCargoShipmentMenu menu = AirCargoShipmentMenu.New(hAWBManager))
			{
				AssertEquals(typeof(TNTAirCargoShipmentMenu), menu.GetType());
			}

			using (AirCargoMasterMenu menu = AirCargoMasterMenu.New(masterBill, mAWBManager))
			{
				AssertEquals(typeof(TNTAirCargoMasterMenu), menu.GetType());
			}

			using (AirCargoMasterMenu menu = AirCargoMasterMenu.New(consol, mAWBManager))
			{
				AssertEquals(typeof(TNTAirCargoMasterMenu), menu.GetType());
			}

			doc = DocAWB.New(aWB, factory);
			docShipment = DocShipment.New(shipment, factory);
			AssertEquals("Doc should be a TNTHawb", typeof(TNTHawb), doc.GetType());
			AssertEquals("DocShipment should be TNTTNTDocForwardingShipment", typeof(TNTDocForwardingShipment), docShipment.GetType());
			ClientOverride.Instance.Uninitialise();
			using (AirCargoShipmentMenu menu = AirCargoShipmentMenu.New(hAWBManager))
			{
				AssertEquals(typeof(AirCargoShipmentMenu), menu.GetType());
			}

			using (AirCargoMasterMenu menu = AirCargoMasterMenu.New(masterBill, mAWBManager))
			{
				AssertEquals(typeof(AirCargoMasterMenu), menu.GetType());
			}

			using (AirCargoMasterMenu menu = AirCargoMasterMenu.New(consol, mAWBManager))
			{
				AssertEquals(typeof(AirCargoMasterMenu), menu.GetType());
			}

			doc = DocAWB.New(aWB, factory);
			docShipment = DocShipment.New(shipment, factory);
			AssertEquals("Doc should be a standard DocAWB", typeof(DocAWB), doc.GetType());
			AssertEquals("DocShipment should be a standard DocShipment", typeof(DocForwardingShipment), docShipment.GetType());
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
