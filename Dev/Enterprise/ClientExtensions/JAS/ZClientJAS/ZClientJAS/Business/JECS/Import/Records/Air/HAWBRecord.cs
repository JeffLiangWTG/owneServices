using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class HAWBRecord : AWBRecord
	{
		public HAWBRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		public JASForwardingShipment LoadOrCreateShipment(JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			JASForwardingShipment result = null;

			if (consol != null)
			{
				result = FindShipment(consol, notificationSubscriber);
				if (result == null)
				{
					result = (JASForwardingShipment)consol.Shipments.AddNew();
					SetShipmentDefaultValues(result);
					NotifyAttachingNewShipmentToConsol(consol, notificationSubscriber);
				}
			}

			return result;
		}

		public JASForwardingShipment LoadOrCreateShipment(BusinessObjectFactory factory)
		{
			JASForwardingShipment result = null;

			if (factory != null)
			{
				result = FindShipment(factory);
				if (result == null)
				{
					result = factory.New<JASForwardingShipment>();
					SetShipmentDefaultValues(result);
				}
			}

			return result;
		}

		public void UpdateShipment(JASForwardingShipment shipment, INotifications notificationSubscriber)
		{
			bool isValidationSuspended = shipment.IsValidationSuspended;

			try
			{
				shipment.SuspendValidation();
				using (new DataImportFlagChanger(shipment))
				{
					UpdateShipmentCore(shipment, notificationSubscriber);
					shipment.JS_OverrideWaybillDefaults = true;
					UpdateAWBHeader(shipment, notificationSubscriber);
				}
			}
			finally
			{
				if (!isValidationSuspended)
				{
					shipment.ResumeValidation();
				}
			}
		}

		#region Header Segment

		ZString OriginOfficeCode
		{
			get { return Fields.GetFieldValue(JXCConstants.HAWBFieldPositions.OriginOfficeCode); }
		}

		ZString HAWBSerialNo
		{
			get { return Fields.GetFieldValue(JXCConstants.HAWBFieldPositions.HAWBSerialNo); }
		}

		void PopulateHouseBillNumber(JASForwardingShipment shipment)
		{
			shipment.JS_HouseBill = HAWBSerialNo.Left(AutoJobShipment.Schema.JS_HouseBillMaxLength);
		}

		void PopulateShipmentOriginFromOriginOfficeCodeIfNotYetPopulated(JASForwardingShipment shipment)
		{
			if (shipment.JS_RL_NKOrigin.IsEmpty)
			{
				JASOrgHeader originOffice = JASOrgHeader.FindOrgHeaderByOfficeCode(shipment.Factory, OriginOfficeCode);
				if (originOffice != null)
				{
					shipment.JS_RL_NKOrigin = originOffice.OH_RL_NKClosestPort;
				}
			}
		}

		#endregion

		#region Shipper

		ZString ShipperPhone
		{
			get { return Fields.GetFieldValue(JXCConstants.HAWBFieldPositions.ShipperPhone); }
		}

		ZString ShipperEmail
		{
			get { return Fields.GetFieldValue(JXCConstants.HAWBFieldPositions.ShipperEmail); }
		}

		protected override Xsd.Organisation PopulateShipperDetailsIntoValueObject()
		{
			Xsd.Organisation result = base.PopulateShipperDetailsIntoValueObject();
			PopulateTelephoneNumber(result, ShipperPhone);
			result.OrganisationDetails.Addresses.GetOrCreateMainAddress().Email = ShipperEmail;
			return result;
		}

		void PopulateShipperDetails(JASForwardingShipment shipment, INotifications notificationSubscriber)
		{
			Xsd.Organisation orgValueObject = PopulateShipperDetailsIntoValueObject();
			shipment.ConsignorPK = FindOrCreateTempOrganisationPK(orgValueObject, OrganisationTypes.Consignor, shipment, notificationSubscriber);
		}

		protected override void PopulateShipperDetailsCore(ExportAWBHeader aWBHeader)
		{
			base.PopulateShipperDetailsCore(aWBHeader);
			aWBHeader.EH_ShipperContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
			aWBHeader.EH_ShipperContactDetail = ShipperPhone.Left(ExportAWBHeader.Schema.EH_ShipperContactDetailMaxLength);
		}

		#endregion

		#region Consignee

		ZString ConsigneePhone
		{
			get { return Fields.GetFieldValue(JXCConstants.HAWBFieldPositions.ConsigneePhone); }
		}

		ZString ConsigneeEmail
		{
			get { return Fields.GetFieldValue(JXCConstants.HAWBFieldPositions.ConsigneeEmail); }
		}

		void PopulateConsigneeDetails(JASForwardingShipment shipment, INotifications notificationSubscriber)
		{
			Xsd.Organisation orgValueObject = PopulateConsigneeDetailsIntoValueObject();
			shipment.ConsigneePK = FindOrCreateTempOrganisationPK(orgValueObject, OrganisationTypes.Consignee, shipment, notificationSubscriber);
		}

		protected override Xsd.Organisation PopulateConsigneeDetailsIntoValueObject()
		{
			Xsd.Organisation result = base.PopulateConsigneeDetailsIntoValueObject();
			PopulateTelephoneNumber(result, ConsigneePhone);
			result.OrganisationDetails.Addresses.GetOrCreateMainAddress().Email = ConsigneeEmail;
			return result;
		}

		protected override void PopulateConsigneeDetailsCore(ExportAWBHeader aWBHeader)
		{
			base.PopulateConsigneeDetailsCore(aWBHeader);
			aWBHeader.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
			aWBHeader.EH_ConsigneeContactDetail = ConsigneePhone.Left(ExportAWBHeader.Schema.EH_ConsigneeContactDetailMaxLength);
		}

		#endregion

		#region Notify Party

		ZString NotifyPartyName
		{
			get { return Fields.GetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyName); }
		}

		ZString NotifyPartyAddress1
		{
			get { return Fields.GetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress1); }
		}

		ZString NotifyPartyAddress2
		{
			get { return Fields.GetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress2); }
		}

		ZString NotifyPartyCity
		{
			get { return Fields.GetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPlace); }
		}

		ZString NotifyPartyPhone
		{
			get { return Fields.GetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPhone); }
		}

		void UpdateNotifyPartyDetails(ExportAWBHeader aWBHeader)
		{
			aWBHeader.EH_AlsoNotifyName = NotifyPartyName.Left(ExportAWBHeader.Schema.EH_AlsoNotifyNameMaxLength);
			aWBHeader.EH_AlsoNotifyAddress = NotifyPartyAddress1.Left(ExportAWBHeader.Schema.EH_AlsoNotifyAddressMaxLength);
			aWBHeader.EH_AlsoNotifyAddress2 = NotifyPartyAddress2.Left(ExportAWBHeader.Schema.EH_AlsoNotifyAddress2MaxLength);
			aWBHeader.EH_AlsoNotifyPlace = NotifyPartyCity.Left(ExportAWBHeader.Schema.EH_AlsoNotifyPlaceMaxLength);
			aWBHeader.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
			aWBHeader.EH_AlsoNotifyContactDetail = NotifyPartyPhone.Left(ExportAWBHeader.Schema.EH_AlsoNotifyContactDetailMaxLength);
		}

		#endregion

		#region Routing Details

		void PopulateShipmentRoutingDetails(JASForwardingShipment shipment)
		{
			shipment.JS_RL_NKOrigin = PortCodeFinder.GetPortCodeFromIATACode(shipment.Factory, OriginCityCode);
			shipment.JS_RL_NKDestination = PortCodeFinder.GetPortCodeFromIATACode(shipment.Factory, LastDestination);
			PopulateShipmentOriginFromOriginOfficeCodeIfNotYetPopulated(shipment);

			shipment.JS_E_DEP = FlightDate1;
			shipment.JS_E_ARV = LastFlightDate;
		}

		#endregion

		#region Freight Declaration

		void PopulateFreightDeclaration(JASForwardingShipment shipment)
		{
			shipment.JS_INCO = (ChargeCode.StartsWith("C")) ? Core.Constants.IncoTerms.ExWorks : Core.Constants.IncoTerms.CostAndFreight;

			ZDecimal goodsValue;
			ZString goodsCurrency;
			if (DeclaredValue.IsEmpty)
			{
				goodsValue = CustomsValue;
				goodsCurrency = CustomsValueCurrency;
			}
			else
			{
				goodsValue = DeclaredValue;
				goodsCurrency = DeclaredValueCurrency;
			}
			shipment.JS_GoodsValue = goodsValue;
			RefCurrency refCurrency = RefCurrency.LoadFromCurrencyCode(shipment.Factory, goodsCurrency);
			if (refCurrency != null)
			{
				shipment.JS_RX_NKGoodsValueCurr = refCurrency.RX_Code;
			}
		}

		#endregion

		#region Freight Info

		ZInt TotalNoOfPieces
		{
			get { return Fields.GetIntFieldValue(FieldPosition.TotalNoOfPieces); }
		}

		ZDecimal TotalGrossWeight
		{
			get { return Fields.GetDecimalFieldValue(FieldPosition.TotalGrossWeight); }
		}

		ZString WeightUnit
		{
			get { return Fields.GetFieldValue(FieldPosition.WeightUnit); }
		}

		void PopulateFreightInfo(JASForwardingShipment shipment)
		{
			shipment.JS_OuterPacks = TotalNoOfPieces;
			shipment.JS_ActualWeight = TotalGrossWeight;
			shipment.JS_UnitOfWeight = ConvertJASWeightUnit(WeightUnit);
		}

		#endregion

		#region Implementation

		protected override sealed JXCConstants.AWBFieldPositions FieldPosition
		{
			get { return new JXCConstants.HAWBFieldPositions(); }
		}

		protected override void UpdateAWBHeaderCore(ExportAWBHeader aWBHeader, INotifications notificationSubscriber)
		{
			base.UpdateAWBHeaderCore(aWBHeader, notificationSubscriber);
			UpdateNotifyPartyDetails(aWBHeader);
		}

		JASForwardingShipment FindShipment(BusinessObjectFactory factory)
		{
			JASForwardingShipmentLocator shipmentLocator = new JASForwardingShipmentLocator();
			ZString origin = PortCodeFinder.GetPortCodeFromIATACode(factory, OriginCityCode);
			ZString destination = PortCodeFinder.GetPortCodeFromIATACode(factory, LastDestination);
			return shipmentLocator.Find(factory, HAWBSerialNo, Core.Constants.TransportModes.Air, origin, destination);
		}

		JASForwardingShipment FindShipment(JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			JASForwardingShipmentLocator shipmentLocator = new JASForwardingShipmentLocator();
			ZString origin = PortCodeFinder.GetPortCodeFromIATACode(consol.Factory, OriginCityCode);
			ZString destination = PortCodeFinder.GetPortCodeFromIATACode(consol.Factory, LastDestination);
			JASForwardingShipment shipment = shipmentLocator.Find(consol, HAWBSerialNo, Core.Constants.TransportModes.Air, origin, destination);
			if (shipment != null && !consol.Shipments.Contains(shipment.PK))
			{
				consol.Shipments.Add(shipment);
				NotifyAttachingExistingShipmentToConsol(consol, shipment, notificationSubscriber);
			}
			return shipment;
		}

		void UpdateShipmentCore(JASForwardingShipment shipment, INotifications notificationSubscriber)
		{
			PopulateHouseBillNumber(shipment);
			PopulateShipperDetails(shipment, notificationSubscriber);
			PopulateConsigneeDetails(shipment, notificationSubscriber);
			AddHandlingInfoNote(shipment);
			PopulateShipmentRoutingDetails(shipment);
			PopulateFreightDeclaration(shipment);
			PopulateFreightInfo(shipment);
		}

		void SetShipmentDefaultValues(JASForwardingShipment shipment)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.OriginalReq;
		}

		void PopulateTelephoneNumber(Xsd.OrgAddress addressValueObject, ZString phoneNumber)
		{
			Xsd.TelephoneNumber telephoneNumber = addressValueObject.TelephoneNumbers.AddNew();
			telephoneNumber.NumberType = Xsd.TelephoneNumberNumberType.Business;
			telephoneNumber.Value = phoneNumber;
		}

		void PopulateTelephoneNumber(Xsd.Organisation orgValueObject, ZString phoneNumber)
		{
			Xsd.OrgAddress mainAddress = orgValueObject.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			PopulateTelephoneNumber(mainAddress, phoneNumber);
		}

		#endregion
	}
}
