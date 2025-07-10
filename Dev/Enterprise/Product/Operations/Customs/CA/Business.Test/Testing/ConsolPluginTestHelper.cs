using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	class ConsolPluginTestHelper : MasterFilesTestHelper
	{
		public ConsolPluginTestHelper() { }

		public ConsolPluginTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public const string MasterBillNum = "OBL123456";
		public const string Container1Num = "OCLU4320011";
		public const string Container2Num = "OCLU5430029";
		public const string Container3Num = "OCLU4320032";

		public ForwardingConsol Consol
		{
			get { return fConsol ?? (fConsol = GetNewConsol()); }
		}
		ForwardingConsol fConsol;

		protected virtual ForwardingConsol GetNewConsol()
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Core.Constants.TransportModes.Sea;
			result.JK_MasterBillNum = MasterBillNum;
			result.JK_RL_NKDischargePort = "CATOR";
			return result;
		}

		public ForwardingContainer Container1
		{
			get { return fContainer1 ?? (fContainer1 = GetNewContainer(Container1Num, "20GP")); }
		}
		ForwardingContainer fContainer1;

		public ForwardingContainer Container2
		{
			get { return fContainer2 ?? (fContainer2 = GetNewContainer(Container2Num, "20GP")); }
		}
		ForwardingContainer fContainer2;

		public ForwardingContainer Container3
		{
			get { return fContainer3 ?? (fContainer3 = GetNewContainer(Container3Num, "40GP")); }
		}
		ForwardingContainer fContainer3;

		protected virtual ForwardingContainer GetNewContainer(ZString containerNum, ZString containerCode)
		{
			var result = Consol.Containers.AddNew();
			result.JC_ContainerNum = containerNum;
			result.JC_RC = GetRefContainer(containerCode).PK;
			return result;
		}

		RefContainer GetRefContainer(ZString containerCode)
		{
			return Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerCode);
		}

		public ForwardingShipment Shipment
		{
			get { return fShipment ?? (fShipment = GetNewShipment()); }
		}
		ForwardingShipment fShipment;

		protected virtual ForwardingShipment GetNewShipment()
		{
			var result = Consol.Shipments.AddNew();
			result.JS_HouseBill = "HBL1234";
			result.JS_UniqueConsignRef = "S12345678";
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE NAME WHICH IS MORE THAN 35 CHARACTERS";
			consignee.MainAddress.OA_Address1 = "CONSIGNEE ADDRESS LINE 1";
			consignee.MainAddress.OA_Address2 = "CONSIGNEE ADDRESS LINE 2";
			consignee.MainAddress.OA_City = "MANHATTAN";
			consignee.MainAddress.OA_PostCode = "12986";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "USNYK";
			consignee.MainAddress.OA_State = "NY";
			consignee.MainAddress.OA_Phone = "2125555212";
			OrgContact contact = consignee.Contacts.AddNew();
			contact.OC_ContactName = "FRANK";
			result.ConsigneeDocumentaryAddress.ContactPK = contact.PK;
			consignee.OH_Code = "CONSIGNEE";
			result.ConsigneePK = consignee.PK;
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR NAME LINE 1";
			consignor.MainAddress.OA_Address1 = "CONSIGNOR ADDRESS LINE 1";
			consignor.MainAddress.OA_City = "PARIS";
			consignor.MainAddress.OA_PostCode = "";
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "FRLEO";
			consignor.MainAddress.OA_State = "";
			consignor.MainAddress.OA_Phone = "0129337218";
			OrgContact contact2 = consignor.Contacts.AddNew();
			contact2.OC_ContactName = "GILLES";
			result.ConsignorDocumentaryAddress.ContactPK = contact2.PK;
			consignor.OH_Code = "CONSIGNOR";
			result.ConsignorPK = consignor.PK;
			result.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			result.ConsigneeDeliveryAddress.E2_CompanyName = "DELIVERED TO 1";
			result.ConsigneeDeliveryAddress.E2_Address1 = "DELIVERY 1 ADDRESS LINE 1";
			result.ConsigneeDeliveryAddress.E2_Address2 = "";
			result.ConsigneeDeliveryAddress.E2_City = "MANHATTAN";
			result.ConsigneeDeliveryAddress.E2_State = "NY";
			result.ConsigneeDeliveryAddress.E2_Postcode = "12783";
			result.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "US";
			result.ConsigneeDeliveryAddress.E2_Contact = "ELIZABETH";
			result.ConsigneeDeliveryAddress.E2_Phone = "2125551212";
			OrgHeader notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "NOTIFY PARTY 1";
			notifyParty.MainAddress.OA_Address1 = "ADDRESS LINE 1";
			notifyParty.MainAddress.OA_City = "NEW YORK";
			notifyParty.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			notifyParty.MainAddress.OA_PostCode = "12345";
			notifyParty.MainAddress.OA_State = "NY";
			notifyParty.MainAddress.OA_Phone = "6475551212";
			OrgContact contact3 = notifyParty.Contacts.AddNew();
			contact3.OC_ContactName = "SUZANNE";
			result.NotifyPartyDocumentaryAddress.ContactPK = contact3.PK;
			notifyParty.OH_Code = "NOTIFY";
			result.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;
			result.JS_RL_NKDestination = "CATOR";
			return result;
		}
	}
}
