using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class OHBLRecord : JXCRecord
	{
		public OHBLRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		public JASForwardingConsol LoadOrCreateConsol(BusinessObjectFactory factory)
		{
			return LoadOrCreateConsol(factory, null);
		}

		public JASForwardingConsol LoadOrCreateConsol(BusinessObjectFactory factory, OMANRecord oMANRecord)
		{
			JASForwardingConsol result = null;

			if (factory != null)
			{
				result = LoadConsol(factory);
				if (result == null)
				{
					result = GetConsolFromOMANRecord(factory, oMANRecord);
				}

				if (result == null)
				{
					result = factory.New<JASForwardingConsol>();
					SetConsolDefaultValues(result);
				}
			}

			return result;
		}

		public void UpdateConsol(HEADRecord hEADRecord, JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			bool isValidationSuspended = consol.IsValidationSuspended;

			try
			{
				consol.SuspendValidation();
				using (new DataImportFlagChanger(consol))
				{
					PopulateConsolMandatoryDetails(consol);
					PopulateShippingLineIfNotAlreadyPopulated(consol, notificationSubscriber);
					PopulateReceivingAgentIfNotAlreadyPopulated(hEADRecord, consol, notificationSubscriber);
				}
			}
			finally
			{
				if (!isValidationSuspended)
				{
					consol.ResumeValidation();
				}
			}
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
					PopulateShipmentMandatoryDetails(shipment);
					PopulateShipperDetails(shipment, notificationSubscriber);
					PopulateConsigneeDetails(shipment, notificationSubscriber);
					PopulateDeliveryAgentDetails(shipment, notificationSubscriber);
					PopulateShippedOnBoardDate(shipment);
					PopulateShipmentSpecialInstructions(shipment);
					PopulateFreightInfo(shipment);
					PopulateShipmentHandlingInstructions(shipment);
					PopulateShipmentMarksAndNumbers(shipment);
					PopulateNoOfOriginalBillOfLadings(shipment);
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

		#region Header & Ports

		ZString BillOfLadingNumber
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.BillOfLadingNo); }
		}

		ZString HouseBillNumber
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.HouseBillOfLadingNo); }
		}

		ZString VesselName
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.VesselName); }
		}

		ZString LloydsCode
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.LloydsCode); }
		}

		ZString VoyageNumber
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.VoyageSSELNumber); }
		}

		ZString PortOfLoading
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.PortOfLoadingCode); }
		}

		ZString PortOfDischarge
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.PortOfDischargeCode); }
		}

		Transport GetTransportToUpdate(CommonConsol consol)
		{
			Transport result = consol.Transports[0];
			foreach (Transport transport in consol.Transports)
			{
				if (transport.JW_TransportMode == consol.JK_TransportMode &&
					(transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight1 ||
					transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel))
				{
					result = transport;
					break;
				}
			}
			return result;
		}

		void PopulateConsolMandatoryDetails(JASForwardingConsol consol)
		{
			PopulateVesselIfNotAlreadyPopulated(consol);

			Transport transport = GetTransportToUpdate(consol);
			if (transport.JW_VoyageFlight.IsEmpty)
			{
				transport.JW_VoyageFlight = VoyageNumber.Left(transport.JW_VoyageFlightInfo.MaxLength);
			}

			if (transport.JW_RL_NKLoadPort.IsEmpty)
			{
				transport.JW_RL_NKLoadPort = PortOfLoading.Left(transport.JW_RL_NKLoadPortInfo.MaxLength);
			}

			if (transport.JW_RL_NKDiscPort.IsEmpty)
			{
				transport.JW_RL_NKDiscPort = PortOfDischarge.Left(transport.JW_RL_NKDiscPortInfo.MaxLength);
			}

			consol.JK_MasterBillNum = BillOfLadingNumber.Left(consol.JK_MasterBillNumInfo.MaxLength);
		}

		void PopulateVesselIfNotAlreadyPopulated(JASForwardingConsol consol)
		{
			Transport transport = GetTransportToUpdate(consol);

			if (transport.JW_Vessel.IsEmpty)
			{
				var vessel = RefVessel.LookupVesselByCode(VesselName, consol.Factory) ?? RefVessel.LookupVesselByLloyds(LloydsCode, consol.Factory);

				if (vessel != null)
				{
					transport.JW_Vessel = vessel.RV_Code;
				}
			}
		}

		void PopulateShipmentMandatoryDetails(JASForwardingShipment shipment)
		{
			shipment.JS_HouseBill = HouseBillNumber.Left(shipment.JS_HouseBillInfo.MaxLength);
		}

		#endregion

		#region Carrier

		ZString CarrierName
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.CarrierName); }
		}

		ZString CarrierSSLCode
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.CarrierSSLCode); }
		}

		ZString CarrierAddress1
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.CarrierAddress1); }
		}

		ZString CarrierAddress2
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.CarrierAddress2); }
		}

		ZString CarrierCity
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.CarrierCity); }
		}

		void PopulateShippingLineIfNotAlreadyPopulated(JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			if (consol.ShippingLinePK.IsEmpty)
			{
				JASOrgHeader shippingLine = JASOrgHeader.FindOrgHeaderByJASWWMappedCode(consol.Factory, CarrierSSLCode);
				if (shippingLine == null)
				{
					Xsd.Organisation shippingLineValueObject = GetShippingLineValueObject();
					shippingLine = FindOrCreateTempOrganisation(shippingLineValueObject, OrganisationTypes.Carrier, consol, notificationSubscriber);
				}
				consol.SetDefaultShippingLineAddress(shippingLine);
			}
		}

		Xsd.Organisation GetShippingLineValueObject()
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

		#region Shipper

		ZString ShipperName
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ShipperName); }
		}

		ZString ShipperAddress1
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ShipperAddress1); }
		}

		ZString ShipperAddress2
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ShipperAddress2); }
		}

		ZString ShipperCity
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ShipperCity); }
		}

		ZString ShipperState
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ShipperState); }
		}

		ZString ShipperPostCode
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ShipperPostCode); }
		}

		ZString ShipperCountryCode
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ShipperCountryCode); }
		}

		ZString ShipperPhone
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ShipperPhone); }
		}

		ZString ShipperEmail
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ShipperEmail); }
		}

		Xsd.Organisation PopulateShipperDetailsIntoValueObject()
		{
			Xsd.Organisation result = new Xsd.Organisation();
			result.OrganisationDetails.Name = ShipperName;
			Xsd.OrgAddress mainAddress = result.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			mainAddress.AddressLine1 = ShipperAddress1;
			mainAddress.AddressLine2 = ShipperAddress2;
			mainAddress.CityOrSuburb = ShipperCity;
			mainAddress.StateOrProvince = ShipperState;
			mainAddress.PostCode = ShipperPostCode;
			mainAddress.Location.Country = ShipperCountryCode;
			mainAddress.Email = ShipperEmail;
			PopulateTelephoneNumber(mainAddress, ShipperPhone);
			return result;
		}

		void PopulateShipperDetails(JASForwardingShipment shipment, INotifications notificationSubscriber)
		{
			Xsd.Organisation orgValueObject = PopulateShipperDetailsIntoValueObject();
			shipment.ConsignorPK = FindOrCreateTempOrganisationPK(orgValueObject, OrganisationTypes.Consignor, shipment, notificationSubscriber);
			shipment.JS_RL_NKOrigin = PortOfLoading.Left(shipment.JS_RL_NKOriginInfo.MaxLength);
		}

		#endregion

		#region Consignee

		ZString ConsigneeName
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeName); }
		}

		ZString ConsigneeAddress1
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeAddress1); }
		}

		ZString ConsigneeAddress2
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeAddress2); }
		}

		ZString ConsigneeCity
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeCity); }
		}

		ZString ConsigneeState
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeState); }
		}

		ZString ConsigneePostCode
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneePostCode); }
		}

		ZString ConsigneeCountryCode
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeCountryCode); }
		}

		ZString ConsigneePhone
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneePhone); }
		}

		ZString ConsigneeEmail
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeEmail); }
		}

		Xsd.Organisation PopulateConsigneeDetailsIntoValueObject()
		{
			Xsd.Organisation result = new Xsd.Organisation();
			result.OrganisationDetails.Name = ConsigneeName;
			Xsd.OrgAddress mainAddress = result.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			mainAddress.AddressLine1 = ConsigneeAddress1;
			mainAddress.AddressLine2 = ConsigneeAddress2;
			mainAddress.CityOrSuburb = ConsigneeCity;
			mainAddress.StateOrProvince = ConsigneeState;
			mainAddress.PostCode = ConsigneePostCode;
			mainAddress.Location.Country = ConsigneeCountryCode;
			mainAddress.Email = ConsigneeEmail;
			PopulateTelephoneNumber(mainAddress, ConsigneePhone);
			return result;
		}

		void PopulateConsigneeDetails(JASForwardingShipment shipment, INotifications notificationSubscriber)
		{
			Xsd.Organisation orgValueObject = PopulateConsigneeDetailsIntoValueObject();
			shipment.ConsigneePK = FindOrCreateTempOrganisationPK(orgValueObject, OrganisationTypes.Consignee, shipment, notificationSubscriber);
			shipment.JS_RL_NKDestination = PortOfDischarge.Left(shipment.JS_RL_NKDestinationInfo.MaxLength);
		}

		#endregion

		#region Delivery Agent

		ZString DeliveryAgentName
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentName); }
		}

		ZString DeliveryAgentAddress1
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentAddress1); }
		}

		ZString DeliveryAgentAddress2
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentAddress2); }
		}

		ZString DeliveryAgentCity
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentCity); }
		}

		Xsd.Organisation PopulateDeliveryAgentDetailsIntoValueObject()
		{
			Xsd.Organisation result = new Xsd.Organisation();
			result.OrganisationDetails.Name = DeliveryAgentName;
			Xsd.OrgAddress mainAddress = result.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			mainAddress.AddressLine1 = DeliveryAgentAddress1;
			mainAddress.AddressLine2 = DeliveryAgentAddress2;
			mainAddress.CityOrSuburb = DeliveryAgentCity;
			return result;
		}

		void PopulateDeliveryAgentDetails(JASForwardingShipment shipment, INotifications notificationSubscriber)
		{
			Xsd.Organisation orgValueObject = PopulateDeliveryAgentDetailsIntoValueObject();
			shipment.JS_OH_DeliveryAgent = FindOrCreateTempOrganisationPK(orgValueObject, OrganisationTypes.Forwarder, shipment, notificationSubscriber);
		}

		#endregion

		#region OnBoardDate

		ZDateTime OnBoardDate
		{
			get { return Fields.GetDateTimeFieldValue(JXCConstants.OHBLFieldPositions.OnBoardDate); }
		}

		void PopulateShippedOnBoardDate(JASForwardingShipment shipment)
		{
			shipment.JS_ShippedOnBoardDate = OnBoardDate;
		}

		#endregion

		#region Special Instructions

		ZString SpecialInstructions1
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.SpecialInstructions1); }
		}

		ZString SpecialInstructions2
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.SpecialInstructions2); }
		}

		ZString SpecialInstructions3
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.SpecialInstructions3); }
		}

		void PopulateShipmentSpecialInstructions(JASForwardingShipment shipment)
		{
			ZString specialInstructions = string.Format("{0}\r\n{1}\r\n{2}", SpecialInstructions1, SpecialInstructions2, SpecialInstructions3);
			AddShipmentNote(shipment, false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, specialInstructions);
		}

		#endregion

		#region Freight Info

		ZString PrepaidOrCollect
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.PrepaidOrCollect); }
		}

		ZString CurrencyCode
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.Currency); }
		}

		ZInt TotalNoOfPackages
		{
			get { return Fields.GetIntFieldValue(JXCConstants.OHBLFieldPositions.TotalNoOfPackages); }
		}

		ZString PackageType
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.PieceTypeCode); }
		}

		ZDecimal GrossWeight
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.OHBLFieldPositions.GrossWeight); }
		}

		ZString KilosOrPounds
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.KilosOrPounds); }
		}

		ZDecimal MeasurementInCBM
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.OHBLFieldPositions.MeasurementInCBM); }
		}

		ZDecimal FreightRate
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.OHBLFieldPositions.Rate); }
		}

		ZString DescriptionOfPackagesAndGoods
		{
			get
			{
				return string.Format("{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n{5}\r\n{6}\r\n{7}",
					Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods1),
					Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods2),
					Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods3),
					Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods4),
					Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods5),
					Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods6),
					Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods7),
					Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods8)).Trim();
			}
		}

		void PopulateFreightInfo(JASForwardingShipment shipment)
		{
			shipment.JS_INCO = (PrepaidOrCollect.ToUpper() == "C") ? Core.Constants.IncoTerms.ExWorks : Core.Constants.IncoTerms.CostAndFreight;
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(shipment.Factory, CurrencyCode);
			if (currency != null)
			{
				shipment.JS_RX_NKFrtRateCurrency = currency.RX_Code;
			}
			shipment.JS_OuterPacks = TotalNoOfPackages;
			shipment.JS_F3_NKPackType = UnitConverter.GetShipmentOuterPacksTypeFromJASPackageUnit(PackageType);
			shipment.JS_UnitOfWeight = (KilosOrPounds == "K") ? Core.Constants.Weight.Kilograms : Core.Constants.Weight.Pounds;
			shipment.JS_ActualWeight = GrossWeight;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment.JS_ActualVolume = MeasurementInCBM;
			shipment.JS_UnitFreightRate = FreightRate;
			shipment.DetailedGoodsDescriptionNoteText = DescriptionOfPackagesAndGoods.Left(shipment.DetailedGoodsDescriptionNoteTextInfo.MaxLength);
		}

		#endregion

		#region Receiving Agent

		ZString ReceivingAgentName
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ReceivingAgentName); }
		}

		ZString ReceivingAgentAddress
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ReceivingAgentAddress); }
		}

		ZString ReceivingAgentCity
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.ReceivingAgentCity); }
		}

		Xsd.Organisation PopulateReceivingAgentIntoValueObject(HEADRecord hEADRecord)
		{
			Xsd.Organisation result = new Xsd.Organisation();
			result.OrganisationDetails.Name = ReceivingAgentName;
			Xsd.OrgAddress mainAddress = result.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			mainAddress.AddressLine1 = ReceivingAgentAddress;
			mainAddress.CityOrSuburb = ReceivingAgentCity;

			Xsd.RegistrationNumber officeCodeReg = result.OrganisationDetails.RegistrationNumbers.AddNew();
			officeCodeReg.NumberType = Xsd.RegistrationNumberTypes.UOC;
			officeCodeReg.Number = hEADRecord.DestinationOfficeCode;

			Xsd.RegistrationNumber nettingCodeReg = result.OrganisationDetails.RegistrationNumbers.AddNew();
			nettingCodeReg.NumberType = Xsd.RegistrationNumberTypes.UNC;
			nettingCodeReg.Number = hEADRecord.DestinationNettingCode;

			return result;
		}

		void PopulateReceivingAgentIfNotAlreadyPopulated(HEADRecord hEADRecord, JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			if (consol.ReceivingForwarderPK.IsEmpty)
			{
				Xsd.Organisation orgValueObject = PopulateReceivingAgentIntoValueObject(hEADRecord);
				consol.SetDefaultReceivingForwarderAddress(FindOrCreateTempOrganisationPK(orgValueObject, OrganisationTypes.Forwarder, consol, notificationSubscriber));
			}
		}

		#endregion

		#region Handling Instructions

		ZString HandlingInstructions1
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.HandlingInstructions1); }
		}

		ZString HandlingInstructions2
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.HandlingInstructions2); }
		}

		ZString HandlingInstructions3
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.HandlingInstructions3); }
		}

		void PopulateShipmentHandlingInstructions(JASForwardingShipment shipment)
		{
			ZString handlingInstructions = string.Format("{0}\r\n{1}\r\n{2}", HandlingInstructions1, HandlingInstructions2, HandlingInstructions3);
			AddShipmentNote(shipment, false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, handlingInstructions);
		}

		#endregion

		#region Marks And Numbers

		ZString MarksAndNumbers
		{
			get { return Fields.GetFieldValue(JXCConstants.OHBLFieldPositions.MarksAndNumbers); }
		}

		void PopulateShipmentMarksAndNumbers(JASForwardingShipment shipment)
		{
			shipment.JS_MarksAndNumbers = MarksAndNumbers.Left(shipment.JS_MarksAndNumbersInfo.MaxLength);
		}

		#endregion

		#region No of Bill Of Ladings

		ZByte NoOfBillOfLadings
		{
			get { return Fields.GetByteFieldValue(JXCConstants.OHBLFieldPositions.NoOfOriginalBillOfLadings); }
		}

		void PopulateNoOfOriginalBillOfLadings(JASForwardingShipment shipment)
		{
			shipment.JS_NoOriginalBills = NoOfBillOfLadings;
		}

		#endregion

		JASUnitConverter UnitConverter
		{
			get
			{
				if (fUnitConverter == null)
				{
					fUnitConverter = new JASUnitConverter();
				}
				return fUnitConverter;
			}
		}

		void SetConsolDefaultValues(JASForwardingConsol consol)
		{
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
		}

		void SetShipmentDefaultValues(JASForwardingShipment shipment)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.OriginalReq;
		}

		void AddShipmentNote(JASForwardingShipment shipment, bool isCustomDescription, ZString noteDescription, ZString noteText)
		{
			StmNote[] notes = shipment.Notes.FindByDescription(noteDescription);
			ZString trimmedNoteText = noteText.Left(StmNote.Schema.ST_NoteTextMaxLength).Trim();
			if (!trimmedNoteText.IsEmpty)
			{
				if (notes.Length == 0)
				{
					shipment.Notes.AddNew(isCustomDescription, noteDescription, trimmedNoteText);
				}
				else
				{
					notes[0].ST_NoteText = trimmedNoteText;
				}
			}
		}

		void PopulateTelephoneNumber(Xsd.OrgAddress addressValueObject, ZString phoneNumber)
		{
			Xsd.TelephoneNumber telephoneNumber = addressValueObject.TelephoneNumbers.AddNew();
			telephoneNumber.NumberType = Xsd.TelephoneNumberNumberType.Business;
			telephoneNumber.Value = phoneNumber;
		}

		JASForwardingShipment FindShipment(JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			JASForwardingShipmentLocator shipmentLocator = new JASForwardingShipmentLocator();
			JASForwardingShipment shipment = shipmentLocator.Find(consol, HouseBillNumber, Core.Constants.TransportModes.Sea, PortOfLoading, PortOfDischarge);
			if (shipment != null && !consol.Shipments.Contains(shipment.PK))
			{
				consol.Shipments.Add(shipment);
				NotifyAttachingExistingShipmentToConsol(consol, shipment, notificationSubscriber);
			}
			return shipment;
		}

		JASForwardingShipment FindShipment(BusinessObjectFactory factory)
		{
			JASForwardingShipmentLocator shipmentLocator = new JASForwardingShipmentLocator();
			return shipmentLocator.Find(factory, HouseBillNumber, Core.Constants.TransportModes.Sea, PortOfLoading, PortOfDischarge);
		}

		JASForwardingConsol GetConsolFromOMANRecord(BusinessObjectFactory factory, OMANRecord oMANRecord)
		{
			return (oMANRecord != null) ? oMANRecord.LoadConsol(factory) : null;
		}

		JASForwardingConsol LoadConsol(BusinessObjectFactory factory)
		{
			return new ConsolLocator<JASForwardingConsol>().Find(factory, BillOfLadingNumber, "", Core.Constants.TransportModes.Sea, VesselName, VoyageNumber, ZDateTime.Empty);
		}

		JASUnitConverter fUnitConverter;
	}
}
