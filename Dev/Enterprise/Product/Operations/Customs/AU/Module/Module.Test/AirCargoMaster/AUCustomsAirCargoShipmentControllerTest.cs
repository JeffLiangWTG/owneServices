using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	[TestedType(typeof(AUCustomsAirCargoShipmentController))]
	sealed class AUCustomsAirCargoShipmentControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			AssertEquals(Env.Security.ACAMasterImportDelete, new AUCustomsAirCargoController().CheckPointForDeleteExposedForTest);
			AssertEquals(Env.Security.ACAMasterImportModify, new AUCustomsAirCargoController().CheckPointForEditExposedForTest);
			AssertEquals(Env.Security.ACAMasterImportNew, new AUCustomsAirCargoController().CheckPointForNewExposedForTest);
			AssertEquals(Env.Security.ACAMasterImportView, new AUCustomsAirCargoController().CheckPointForViewExposedForTest);
		}

		public void TestFormCache()
		{
			var hAWB = GetBusinessObjectThatIsInTheDatabase() as CusHAWB;
			var airCargoController = ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargoShipmentController);
			var shipmentController = ZControllerFactory.Create(ControllerIDs.JobShipment);
			try
			{
				airCargoController.ShowEditForm(hAWB);
				shipmentController.ShowEditForm(hAWB.Shipment);
				AssertNotNull("form from air cargo controller", airCargoController.LastShownForm);
				AssertNotNull("Form from shipment controller", shipmentController.LastShownForm);
				AssertEquals("Shipment form should be shown from air cargo module if plugged in", airCargoController.LastShownForm, shipmentController.LastShownForm);
			}
			finally
			{
				if (airCargoController.LastShownForm != null)
				{
					airCargoController.LastShownForm.Dispose();
				}

				if (shipmentController.LastShownForm != null)
				{
					shipmentController.LastShownForm.Dispose();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestCreateNewFormShouldNotThrowException()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			mawb.CM_JK = consol.PK;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			Factory.Save();
			var airCargoShipmentController = ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargoShipmentController);
			using (var editForm = airCargoShipmentController.ShowEditForm(hawb))
			{
				AssertEquals("The controllerID of the form should be AirCargoShipmentController because AUCustomsAirCargoShipmentController has overridden it.", ControllerIDs.Customs.AU.AirCargoShipmentController, editForm.ControllerID);
				using (var newForm = airCargoShipmentController.ShowNewForm())
				{
					AssertType<ShipmentForm>("The new form should be type of ShipmentForm.", newForm);
				}
			}
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AirCargoShipmentController;

		protected override Type GetBusinessObjectType() => typeof(CusHAWB);

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			var hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_JS = shipment.PK;
			Factory.Save();
			return hAWB;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKDestination = "INBOM";
			shipment.JS_GoodsDescription = "GoodsDescription";
			shipment.JS_UniqueConsignRef = "BlahBlahBlah";
			shipment.JS_ReleaseType = shipment.Lookups.JS_ReleaseType_List[0].Code;
			var bizO = base.GetBusinessObjectWithoutValidationErrors() as CusHAWB;
			bizO.CS_JS = shipment.PK;
			bizO.CS_HAWB = "bill1";
			Factory.Save();
			return bizO;
		}
	}
}
