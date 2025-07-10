
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.TNT
{
	internal class Exit2QuantumShipmentRecord : QuantumShipmentRecord
	{
		public Exit2QuantumShipmentRecord(ZString branchCode, ZString mBagNo, ZString line)
			: base(branchCode, mBagNo, line)
		{
		}

		protected override void MapConsigneeConsignor(ForwardingShipment shipment, BusinessObjectFactory factory, INotifications notify)
		{
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;

			MapOrganisation(shipment.ConsigneeDocumentaryAddress, ConsigneeName, ConsigneeAddress1, ConsigneeAddress2, ConsigneeCity, ConsigneeState, ConsigneePostCode, ConsigneePhone, ConsigneeFax, ConsigneeCountry, ConsigneeContactName, notify);
			MapOrganisation(shipment.ConsignorDocumentaryAddress, ConsignorName, ConsignorAddress1, ConsignorAddress2, ConsignorCity, ConsignorState, ConsignorPostCode, ConsignorPhone, ZString.Empty, ConsignorCountry, ConsignorContactName, notify);
			if (!DeliveryAddress1.IsEmpty)
			{
				MapOrganisation(shipment.ConsigneeDeliveryAddress, DeliveryName, DeliveryAddress1, DeliveryAddress2, DeliveryCity, DeliveryState, DeliveryPostCode, DeliveryPhone, ZString.Empty, DeliveryCountry, DeliveryContactName, notify);
			}
			if (!PickupAddress1.IsEmpty)
			{
				MapOrganisation(shipment.ConsignorPickupAddress, PickupName, PickupAddress1, PickupAddress2, PickupCity, PickupState, PickupPostCode, PickupPhone, ZString.Empty, PickupCountry, PickupContactName, notify);
			}
			if (!ConsigneeName.IsEmpty && !ConsigneeAddress1.IsEmpty && !ConsigneeCity.IsEmpty && !ConsigneeContactName.IsEmpty)
			{
				shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
				shipment.NotifyPartyDocumentaryAddress.E2_CompanyName = ConsigneeName;
				shipment.NotifyPartyDocumentaryAddress.E2_Address1 = ConsigneeAddress1;
				shipment.NotifyPartyDocumentaryAddress.E2_City = ConsigneeCity;
				shipment.NotifyPartyDocumentaryAddress.E2_Contact = ConsigneeContactName;
			}
		}

		void MapOrganisation(JobDocAddress jobDocAddress, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString phone, ZString fax, ZString country, ZString contactName, INotifications notify)
		{
			jobDocAddress.E2_AddressOverride = true;
			Mapper.SetPropertyInfoValue(jobDocAddress.E2_CompanyNameInfo, name, notify);
			Mapper.SetPropertyInfoValue(jobDocAddress.E2_Address1Info, address1, notify);
			Mapper.SetPropertyInfoValue(jobDocAddress.E2_Address2Info, address2, notify);
			Mapper.SetPropertyInfoValue(jobDocAddress.E2_CityInfo, city, notify);
			Mapper.SetPropertyInfoValue(jobDocAddress.E2_StateInfo, state, notify);
			Mapper.SetPropertyInfoValue(jobDocAddress.E2_PostcodeInfo, postCode, notify);
			Mapper.SetPropertyInfoValue(jobDocAddress.E2_PhoneInfo, phone, notify);
			Mapper.SetPropertyInfoValue(jobDocAddress.E2_FaxInfo, fax, notify);
			Mapper.SetPropertyInfoValue(jobDocAddress.E2_RN_NKCountryCodeInfo, country, notify);
			Mapper.SetPropertyInfoValue(jobDocAddress.E2_ContactInfo, contactName, notify);
		}
	}
}
