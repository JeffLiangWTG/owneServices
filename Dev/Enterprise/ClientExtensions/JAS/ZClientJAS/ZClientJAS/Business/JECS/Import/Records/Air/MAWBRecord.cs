using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class MAWBRecord : AWBRecord
	{
		public MAWBRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		protected override sealed JXCConstants.AWBFieldPositions FieldPosition
		{
			get { return new JXCConstants.MAWBFieldPositions(); }
		}

		protected override void UpdateConsolCore(HEADRecord hEADRecord, JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			base.UpdateConsolCore(hEADRecord, consol, notificationSubscriber);
			PopulateShipperDetails(hEADRecord, consol, notificationSubscriber);
			PopulateConsigneeDetails(hEADRecord, consol, notificationSubscriber);
			PopulateCarrierDetails(consol, notificationSubscriber);
			AddHandlingInfoNote(consol);
		}

		#region Carrier

		ZString CarrierName
		{
			get { return Fields.GetFieldValue(FieldPosition.CarrierName); }
		}

		ZString CarrierAddress1
		{
			get { return Fields.GetFieldValue(FieldPosition.CarrierAddress1); }
		}

		ZString CarrierAddress2
		{
			get { return Fields.GetFieldValue(FieldPosition.CarrierAddress2); }
		}

		ZString CarrierCity
		{
			get { return Fields.GetFieldValue(FieldPosition.CarrierCity); }
		}

		void PopulateCarrierDetails(JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			JASOrgHeader carrier = (JASOrgHeader)OrgHeader.FindBy3CharAirlineCode(consol.Factory, AirlinePrefix);
			if (carrier == null)
			{
				Xsd.Organisation carrierValueObject = GetCarrierValueObject();
				carrier = FindOrCreateTempOrganisation(carrierValueObject, OrganisationTypes.Carrier, consol, notificationSubscriber);
			}

			if (carrier != null)
			{
				consol.SetDefaultShippingLineAddress(carrier);
			}
		}

		Xsd.Organisation GetCarrierValueObject()
		{
			Xsd.Organisation result = new Xsd.Organisation();
			result.OrganisationDetails.Name = CarrierName;
			Xsd.OrgAddress mainAddress = result.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			mainAddress.AddressLine1 = CarrierAddress1;
			mainAddress.AddressLine2 = CarrierAddress2;
			mainAddress.CityOrSuburb = CarrierCity;
			return result;
		}

		#endregion

		#region Implementation

		ZString ShipperStreetAddress
		{
			get { return Fields.GetFieldValue(JXCConstants.MAWBFieldPositions.ShipperStreetAddress); }
		}

		ZString ConsigneeStreetAddress
		{
			get { return Fields.GetFieldValue(JXCConstants.MAWBFieldPositions.ConsigneeStreetAddress); }
		}

		protected override void PopulateShipperDetailsCore(ExportAWBHeader aWBHeader)
		{
			base.PopulateShipperDetailsCore(aWBHeader);
			if (aWBHeader.EH_ShipperAddress.IsEmpty)
			{
				aWBHeader.EH_ShipperAddress = ShipperStreetAddress;
			}
		}

		protected override Xsd.Organisation PopulateShipperDetailsIntoValueObject()
		{
			Xsd.Organisation result = base.PopulateShipperDetailsIntoValueObject();
			AssignAddressLine1IfEmpty(result, ShipperStreetAddress);
			return result;
		}

		protected override void PopulateConsigneeDetailsCore(ExportAWBHeader aWBHeader)
		{
			base.PopulateConsigneeDetailsCore(aWBHeader);
			if (aWBHeader.EH_ConsigneeAddress.IsEmpty)
			{
				aWBHeader.EH_ConsigneeAddress = ConsigneeStreetAddress;
			}
		}

		protected override Xsd.Organisation PopulateConsigneeDetailsIntoValueObject()
		{
			Xsd.Organisation result = base.PopulateConsigneeDetailsIntoValueObject();
			AssignAddressLine1IfEmpty(result, ConsigneeStreetAddress);
			return result;
		}

		void AssignAddressLine1IfEmpty(Xsd.Organisation organisation, ZString streetAddress)
		{
			Xsd.OrgAddress address = organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			if (address.AddressLine1.IsEmpty)
			{
				address.AddressLine1 = streetAddress;
			}
		}

		void PopulateShipperDetails(HEADRecord hEADRecord, JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			if (!consol.IsDirect && consol.SendingForwarderPK.IsEmpty)
			{
				Xsd.Organisation orgValueObject = PopulateShipperDetailsIntoValueObject();
				PopulateOfficeAndNettingCode(orgValueObject, hEADRecord.SendingOfficeCode, hEADRecord.SendingNettingCode);
				consol.SetDefaultSendingForwarderAddress(FindOrCreateTempOrganisationPK(orgValueObject, OrganisationTypes.Forwarder, consol, notificationSubscriber, "Sending Forwarder"));
			}
		}

		void PopulateConsigneeDetails(HEADRecord hEADRecord, JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			if (!consol.IsDirect && consol.ReceivingForwarderPK.IsEmpty)
			{
				Xsd.Organisation orgValueObject = PopulateConsigneeDetailsIntoValueObject();
				PopulateOfficeAndNettingCode(orgValueObject, hEADRecord.DestinationOfficeCode, hEADRecord.DestinationNettingCode);
				consol.SetDefaultReceivingForwarderAddress(FindOrCreateTempOrganisationPK(orgValueObject, OrganisationTypes.Forwarder, consol, notificationSubscriber));
			}
		}

		void PopulateOfficeAndNettingCode(Xsd.Organisation orgValueObject, ZString officeCode, ZString nettingCode)
		{
			Xsd.RegistrationNumber officeCodeReg = orgValueObject.OrganisationDetails.RegistrationNumbers.AddNew();
			officeCodeReg.NumberType = Xsd.RegistrationNumberTypes.UOC;
			officeCodeReg.Number = officeCode;

			Xsd.RegistrationNumber nettingCodeReg = orgValueObject.OrganisationDetails.RegistrationNumbers.AddNew();
			nettingCodeReg.NumberType = Xsd.RegistrationNumberTypes.UNC;
			nettingCodeReg.Number = nettingCode;
		}

		#endregion
	}
}
