using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.JAS.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	[TestedType(typeof(GsumShipmentWrapper))]
	class GsumShipmentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOriginOfficeOrg()
		{
			AssertNull("Pre-condition", ShipmentWrapper.OriginOfficeOrg);
			Shipment.JS_RL_NKOrigin = "USNYC";
			JASForwardingConsol departureConsol = (JASForwardingConsol)Shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "USJEC";
			AssertNull("Departure Consols's Sending forwarder not specified", ShipmentWrapper.OriginOfficeOrg);
			departureConsol.SetDefaultSendingForwarderAddress(Factory.New<JASOrgHeader>());
			AssertEquals(departureConsol.SendingForwarder, ShipmentWrapper.OriginOfficeOrg);
		}

		public void TestSendingForwarder()
		{
			AssertNull("Pre-condition", ShipmentWrapper.SendingForwarder);
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
			AssertEquals("Should be defaulted to JASWW if specified in the registry", JASDataRegistry.Instance.GetJASWWOrganisation(Factory), ShipmentWrapper.SendingForwarder);
			JASOrgHeader currentOrgProxy = (JASOrgHeader)GlbCompany.CurrentCompany.OrgProxy;
			currentOrgProxy.OfficeCode = "USNYC";
			AssertEquals("Sending forwarder should be the current OrgProxy", GlbCompany.CurrentCompany.OrgProxy, ShipmentWrapper.SendingForwarder);
		}

		public void TestReceivingForwarder_ExportShipment()
		{
			AssertNull("Pre-condition", ShipmentWrapper.ReceivingForwarder);
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
			AssertEquals("Should be defaulted to JASWW if specified in the registry", JASDataRegistry.Instance.GetJASWWOrganisation(Factory), ShipmentWrapper.ReceivingForwarder);
			Shipment.JS_RL_NKOrigin = "AUBNE";
			Shipment.JS_RL_NKDestination = "THBKK";
			JASForwardingConsol departureConsol = (JASForwardingConsol)Shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "AUBNE";
			departureConsol.JK_RL_NKDischargePort = "SGSIN";
			JASForwardingConsol arrivalConsol = (JASForwardingConsol)Shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "SGSIN";
			arrivalConsol.JK_RL_NKDischargePort = "THBKK";
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
			AssertEquals("Should still use JASWW if Receiving/Sending Forwarder isn't specified", JASDataRegistry.Instance.GetJASWWOrganisation(Factory), ShipmentWrapper.ReceivingForwarder);
			departureConsol.SetDefaultSendingForwarderAddress(Factory.New<JASOrgHeader>());
			departureConsol.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			arrivalConsol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			arrivalConsol.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			AssertEquals("Should still use JASWW if JAS Office Code isn't specified", JASDataRegistry.Instance.GetJASWWOrganisation(Factory), ShipmentWrapper.ReceivingForwarder);
			arrivalConsol.ReceivingForwarder.OfficeCode = "USLAX";
			AssertEquals("Should be using other office's OrgHeader from Arrival Consol", arrivalConsol.ReceivingForwarder, ShipmentWrapper.ReceivingForwarder);
			arrivalConsol.SetDefaultSendingForwarderAddress(Factory.New<JASOrgHeader>());
			arrivalConsol.SetDefaultReceivingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			arrivalConsol.SendingForwarder.OfficeCode = "USBOS";
			AssertEquals("Should be using other office's OrgHeader from Arrival Consol", arrivalConsol.SendingForwarder, ShipmentWrapper.ReceivingForwarder);
		}

		public void TestReceivingForwarder_ImportShipment()
		{
			AssertNull("Pre-condition", ShipmentWrapper.ReceivingForwarder);
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
			AssertEquals("Should be defaulted to JASWW if specified in the registry", JASDataRegistry.Instance.GetJASWWOrganisation(Factory), ShipmentWrapper.ReceivingForwarder);
			Shipment.JS_RL_NKOrigin = "THBKK";
			Shipment.JS_RL_NKDestination = "SGSIN";
			JASForwardingConsol departureConsol = (JASForwardingConsol)Shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "THBKK";
			departureConsol.JK_RL_NKDischargePort = "SGSIN";
			JASForwardingConsol arrivalConsol = (JASForwardingConsol)Shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "SGSIN";
			arrivalConsol.JK_RL_NKDischargePort = "AUBNE";
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
			AssertEquals("Should still use JASWW if Receiving/Sending Forwarder isn't specified", JASDataRegistry.Instance.GetJASWWOrganisation(Factory), ShipmentWrapper.ReceivingForwarder);
			arrivalConsol.SetDefaultSendingForwarderAddress(Factory.New<JASOrgHeader>());
			arrivalConsol.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			departureConsol.SetDefaultSendingForwarderAddress(Factory.New<JASOrgHeader>());
			departureConsol.SetDefaultReceivingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			AssertEquals("Should still use JASWW if JAS Office Code isn't specified", JASDataRegistry.Instance.GetJASWWOrganisation(Factory), ShipmentWrapper.ReceivingForwarder);
			departureConsol.SendingForwarder.OfficeCode = "USLAX";
			AssertEquals("Should be using other office's OrgHeader from Departure Consol", departureConsol.SendingForwarder, ShipmentWrapper.ReceivingForwarder);
			departureConsol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			departureConsol.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			departureConsol.ReceivingForwarder.OfficeCode = "USBOS";
			AssertEquals("Should be using other office's OrgHeader from Arrival Consol", departureConsol.ReceivingForwarder, ShipmentWrapper.ReceivingForwarder);
		}

		public void TestReceivingForwarder_NonImportOrExportShipment()
		{
			AssertNull("Pre-condition", ShipmentWrapper.ReceivingForwarder);
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
			AssertEquals("Should be defaulted to JASWW if specified in the registry", JASDataRegistry.Instance.GetJASWWOrganisation(Factory), ShipmentWrapper.ReceivingForwarder);
			Shipment.JS_RL_NKOrigin = "THBKK";
			Shipment.JS_RL_NKDestination = "SGSIN";
			JASForwardingConsol consol1 = (JASForwardingConsol)Shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "THBKK";
			consol1.JK_RL_NKDischargePort = "SGSIN";
			JASForwardingConsol consol2 = (JASForwardingConsol)Shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "DEFRA";
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
			AssertEquals("Should still use JASWW if Receiving/Sending Forwarder isn't specified", JASDataRegistry.Instance.GetJASWWOrganisation(Factory), ShipmentWrapper.ReceivingForwarder);
			consol1.SetDefaultSendingForwarderAddress(Factory.New<JASOrgHeader>());
			consol1.SetDefaultReceivingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			consol2.SetDefaultSendingForwarderAddress(Factory.New<JASOrgHeader>());
			consol2.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			AssertEquals("Should still use JASWW if JAS Office Code isn't specified", JASDataRegistry.Instance.GetJASWWOrganisation(Factory), ShipmentWrapper.ReceivingForwarder);
			consol1.SendingForwarder.OfficeCode = "USLAX";
			AssertEquals("Should be using other office's OrgHeader from the first consol", consol1.SendingForwarder, ShipmentWrapper.ReceivingForwarder);
			consol1.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			consol1.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			consol1.ReceivingForwarder.OfficeCode = "USBOS";
			AssertEquals("Should be using other office's OrgHeader from the first consol", consol1.ReceivingForwarder, ShipmentWrapper.ReceivingForwarder);
		}

		public void TestFreightDestination()
		{
			AssertEquals("Pre-condition", "", ShipmentWrapper.FreightDest);
			Shipment.JS_RL_NKDestination = "IEDUB";
			AssertEquals("Pre-condition", "IEDUB", ShipmentWrapper.FreightDest);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return ShipmentWrapper;
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
				}

				return fShipment;
			}
		}

		GsumShipmentWrapper ShipmentWrapper
		{
			get
			{
				if (fShipmentWrapper == null)
				{
					fShipmentWrapper = new GsumShipmentWrapper(Shipment);
				}

				return fShipmentWrapper;
			}
		}

		JASForwardingShipment fShipment;
		GsumShipmentWrapper fShipmentWrapper;
		ZGuid initialProxyOrgPK;
		protected override void SetUp()
		{
			initialProxyOrgPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			base.SetUp();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "1234567890123456789012345678901234567890";
			org.MainAddress.OA_Address1 = "2345678901234567890123456789012345678901";
			org.MainAddress.OA_Address2 = "3456789012345678901234567890123456789012";
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = initialProxyOrgPK;
		}
	}
}
