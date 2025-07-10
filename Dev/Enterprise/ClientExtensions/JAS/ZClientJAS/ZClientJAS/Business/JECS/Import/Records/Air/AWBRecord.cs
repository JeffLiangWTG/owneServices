using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public abstract class AWBRecord : JXCRecord
	{
		public AWBRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		#region Header Segment

		protected ZString MasterBillNum
		{
			get { return AirlinePrefix + MAWBSerialNumber; }
		}

		protected ZString OriginCityCode
		{
			get { return Fields.GetFieldValue(FieldPosition.OriginCityCode); }
		}

		protected ZString AirlinePrefix
		{
			get { return Fields.GetFieldValue(FieldPosition.AirlinePrefix); }
		}

		ZString MAWBSerialNumber
		{
			get { return Fields.GetFieldValue(FieldPosition.MAWBSerialNo); }
		}

		void PopulateOriginCode(ExportAWBHeader aWBHeader)
		{
			aWBHeader.EH_AWBOriginCode = OriginCityCode.Left(ExportAWBHeader.Schema.EH_AWBOriginCodeMaxLength);
		}

		#endregion

		#region Shipper Segment

		ZString ShipperName
		{
			get { return Fields.GetFieldValue(FieldPosition.ShipperName); }
		}

		ZString ShipperAddress1
		{
			get { return Fields.GetFieldValue(FieldPosition.ShipperAddress1); }
		}

		ZString ShipperAddress2
		{
			get { return Fields.GetFieldValue(FieldPosition.ShipperAddress2); }
		}

		ZString ShipperCity
		{
			get { return Fields.GetFieldValue(FieldPosition.ShipperCity); }
		}

		ZString ShipperState
		{
			get { return Fields.GetFieldValue(FieldPosition.ShipperState); }
		}

		ZString ShipperPostCode
		{
			get { return Fields.GetFieldValue(FieldPosition.ShipperPostCode); }
		}

		ZString ShipperCountryCode
		{
			get { return Fields.GetFieldValue(FieldPosition.ShipperCountryCode); }
		}

		ZString ShipperAccountNo
		{
			get { return Fields.GetFieldValue(FieldPosition.ShipperAccountNo); }
		}

		protected virtual Xsd.Organisation PopulateShipperDetailsIntoValueObject()
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
			return result;
		}

		void PopulateShipperDetails(ExportAWBHeader aWBHeader)
		{
			JASOrgHeader shipperOrg = ((IJASExportAWBHeader)aWBHeader).Shipper;
			if (shipperOrg == null || IsDefaultUnmatchedOrg(shipperOrg))
			{
				PopulateShipperDetailsCore(aWBHeader);
			}
		}

		protected virtual void PopulateShipperDetailsCore(ExportAWBHeader aWBHeader)
		{
			aWBHeader.EH_ShipperName = ShipperName.Left(ExportAWBHeader.Schema.EH_ShipperNameMaxLength);
			aWBHeader.EH_ShipperAddress = ShipperAddress1.Left(ExportAWBHeader.Schema.EH_ShipperAddressMaxLength);
			aWBHeader.EH_ShipperAddress2 = ShipperAddress2.Left(ExportAWBHeader.Schema.EH_ShipperAddress2MaxLength);
			aWBHeader.EH_ShipperPlace = ShipperCity.Left(ExportAWBHeader.Schema.EH_ShipperPlaceMaxLength);
			aWBHeader.EH_ShipperState = ShipperState.Left(ExportAWBHeader.Schema.EH_ShipperStateMaxLength);
			aWBHeader.EH_ShipperPostCode = ShipperPostCode.Left(ExportAWBHeader.Schema.EH_ShipperPostCodeMaxLength);
			aWBHeader.EH_ShipperCountryCode = ShipperCountryCode.Left(ExportAWBHeader.Schema.EH_ShipperCountryCodeMaxLength);
			aWBHeader.EH_ShipperAccount = ShipperAccountNo.Left(ExportAWBHeader.Schema.EH_ShipperAccountMaxLength);
		}

		#endregion

		#region Consignee Segment

		ZString ConsigneeName
		{
			get { return Fields.GetFieldValue(FieldPosition.ConsigneeName); }
		}

		ZString ConsigneeAddress1
		{
			get { return Fields.GetFieldValue(FieldPosition.ConsigneeAddress1); }
		}

		ZString ConsigneeAddress2
		{
			get { return Fields.GetFieldValue(FieldPosition.ConsigneeAddress2); }
		}

		ZString ConsigneeCity
		{
			get { return Fields.GetFieldValue(FieldPosition.ConsigneeCity); }
		}

		ZString ConsigneeState
		{
			get { return Fields.GetFieldValue(FieldPosition.ConsigneeState); }
		}

		ZString ConsigneePostCode
		{
			get { return Fields.GetFieldValue(FieldPosition.ConsigneePostCode); }
		}

		ZString ConsigneeCountryCode
		{
			get { return Fields.GetFieldValue(FieldPosition.ConsigneeCountryCode); }
		}

		ZString ConsigneeAccountNo
		{
			get { return Fields.GetFieldValue(FieldPosition.ConsigneeAccountNo); }
		}

		protected virtual Xsd.Organisation PopulateConsigneeDetailsIntoValueObject()
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
			return result;
		}

		void PopulateConsigneeDetails(ExportAWBHeader aWBHeader)
		{
			JASOrgHeader consigneeOrg = ((IJASExportAWBHeader)aWBHeader).Consignee;
			if (consigneeOrg == null || IsDefaultUnmatchedOrg(consigneeOrg))
			{
				PopulateConsigneeDetailsCore(aWBHeader);
			}
		}

		protected virtual void PopulateConsigneeDetailsCore(ExportAWBHeader aWBHeader)
		{
			aWBHeader.EH_ConsigneeName = ConsigneeName.Left(ExportAWBHeader.Schema.EH_ConsigneeNameMaxLength);
			aWBHeader.EH_ConsigneeAddress = ConsigneeAddress1.Left(ExportAWBHeader.Schema.EH_ConsigneeAddressMaxLength);
			aWBHeader.EH_ConsigneeAddress2 = ConsigneeAddress2.Left(ExportAWBHeader.Schema.EH_ConsigneeAddress2MaxLength);
			aWBHeader.EH_ConsigneePlace = ConsigneeCity.Left(ExportAWBHeader.Schema.EH_ConsigneePlaceMaxLength);
			aWBHeader.EH_ConsigneeState = ConsigneeState.Left(ExportAWBHeader.Schema.EH_ConsigneeStateMaxLength);
			aWBHeader.EH_ConsigneePostCode = ConsigneePostCode.Left(ExportAWBHeader.Schema.EH_ConsigneePostCodeMaxLength);
			aWBHeader.EH_ConsigneeCountryCode = ConsigneeCountryCode.Left(ExportAWBHeader.Schema.EH_ConsigneeCountryCodeMaxLength);
			aWBHeader.EH_ConsigneeAccount = ConsigneeAccountNo.Left(ExportAWBHeader.Schema.EH_ConsigneeAccountMaxLength);
		}

		#endregion

		#region Accounting Info Segment

		void PopulateAccountingInfos(ExportAWBHeader aWBHeader)
		{
			aWBHeader.AWBAccountingInformations.RemoveAndDeleteAll();

			for (int i = FieldPosition.AccountingInfo1; i < FieldPosition.AccountingInfo1 + 7; i++)
			{
				ZString accountingInfo = Fields.GetFieldValue(i);
				if (!accountingInfo.IsEmpty)
				{
					ExportAWBAccountingInformation aWBAccountingInfo = aWBHeader.AWBAccountingInformations.AddNew();
					aWBAccountingInfo.EA_Information = accountingInfo.Left(ExportAWBAccountingInformation.Schema.EA_InformationMaxLength);
				}
			}
		}

		#endregion

		#region Routing Segment

		ZString To1st
		{
			get { return Fields.GetFieldValue(FieldPosition.To1st); }
		}

		ZString By1st
		{
			get { return Fields.GetFieldValue(FieldPosition.By1st); }
		}

		ZString To2nd
		{
			get { return Fields.GetFieldValue(FieldPosition.To2nd); }
		}

		ZString By2nd
		{
			get { return Fields.GetFieldValue(FieldPosition.By2nd); }
		}

		ZString To3rd
		{
			get { return Fields.GetFieldValue(FieldPosition.To3rd); }
		}

		ZString By3rd
		{
			get { return Fields.GetFieldValue(FieldPosition.By3rd); }
		}

		protected ZString LastDestination
		{
			get
			{
				if (fLastDestination.IsEmpty)
				{
					if (!To3rd.IsEmpty)
					{
						fLastDestination = To3rd;
					}
					else if (!To2nd.IsEmpty)
					{
						fLastDestination = To2nd;
					}
					else
					{
						fLastDestination = To1st;
					}
				}
				return fLastDestination;
			}
		}

		void PopulateRoutingDetails(ExportAWBHeader aWBHeader)
		{
			aWBHeader.EH_To1st = To1st.Left(ExportAWBHeader.Schema.EH_To1stMaxLength);
			aWBHeader.EH_By1st = By1st.Left(ExportAWBHeader.Schema.EH_By1stMaxLength);
			aWBHeader.EH_To2nd = To2nd.Left(ExportAWBHeader.Schema.EH_To2ndMaxLength);
			aWBHeader.EH_By2nd = By2nd.Left(ExportAWBHeader.Schema.EH_By2ndMaxLength);
			aWBHeader.EH_To3rd = To3rd.Left(ExportAWBHeader.Schema.EH_To3rdMaxLength);
			aWBHeader.EH_By3rd = By3rd.Left(ExportAWBHeader.Schema.EH_By3rdMaxLength);
		}

		ZString fLastDestination;

		#endregion

		#region Freight Declaration Segment

		protected ZString ChargeCode
		{
			get { return Fields.GetFieldValue(FieldPosition.ChargeCode); }
		}

		ZString WeightPrepaidOrCollect
		{
			get { return GetPrepaidCollectKeyword(Fields.GetFieldValue(FieldPosition.WeightPPDorCOL)); }
		}

		ZString OtherChargesPrepaidCollect
		{
			get { return GetPrepaidCollectKeyword(Fields.GetFieldValue(FieldPosition.OtherPPDorCOL)); }
		}

		protected ZDecimal DeclaredValue
		{
			get { return Fields.GetDecimalFieldValue(FieldPosition.DeclaredValue); }
		}

		protected ZString DeclaredValueCurrency
		{
			get { return Fields.GetFieldValue(FieldPosition.CurrencyCodeForDeclaredValue); }
		}

		protected ZDecimal CustomsValue
		{
			get { return Fields.GetDecimalFieldValue(FieldPosition.CustomsValue); }
		}

		protected ZString CustomsValueCurrency
		{
			get { return Fields.GetFieldValue(FieldPosition.CurrencyCodeForCustomsValue); }
		}

		ZString Currency
		{
			get { return Fields.GetFieldValue(FieldPosition.Currency); }
		}

		ZString GetPrepaidCollectKeyword(ZString prepaidOrCollect)
		{
			ZString result;

			switch (prepaidOrCollect.ToUpper())
			{
				case "P":
					result = "PPD";
					break;
				case "C":
					result = "COL";
					break;
				default:
					result = "";
					break;
			}

			return result;
		}

		void PopulateFreightDeclarationDetails(ExportAWBHeader aWBHeader)
		{
			aWBHeader.EH_ChargesCode = ChargeCode.Left(ExportAWBHeader.Schema.EH_ChargesCodeMaxLength);
			aWBHeader.EH_WeightVPPDCOL = WeightPrepaidOrCollect;
			aWBHeader.EH_OtherPPDCOL = OtherChargesPrepaidCollect;
			aWBHeader.EH_DeclaredValue = DeclaredValue;
			aWBHeader.EH_HouseDeclaredValueCurrency = DeclaredValueCurrency.Left(ExportAWBHeader.Schema.EH_HouseDeclaredValueCurrencyMaxLength);
			aWBHeader.EH_CustomsValue = CustomsValue;
			aWBHeader.EH_HouseCustomsValueCurrency = CustomsValueCurrency.Left(ExportAWBHeader.Schema.EH_HouseCustomsValueCurrencyMaxLength);
			aWBHeader.EH_Currency = Currency.Left(ExportAWBHeader.Schema.EH_CurrencyMaxLength);
		}

		#endregion

		#region Flight Info Segment

		ZString AirportOfDeparture
		{
			get { return Fields.GetFieldValue(FieldPosition.AirportOfDeparture); }
		}

		ZString AirportOfDestination
		{
			get { return Fields.GetFieldValue(FieldPosition.AirportOfDestination); }
		}

		ZString FlightNo1
		{
			get { return Fields.GetFieldValue(FieldPosition.FlightNo1); }
		}

		protected ZDateTime FlightDate1
		{
			get { return Fields.GetDateTimeFieldValue(FieldPosition.FlightDate1); }
		}

		ZString FlightNo2
		{
			get { return Fields.GetFieldValue(FieldPosition.FlightNo2); }
		}

		ZDateTime FlightDate2
		{
			get { return Fields.GetDateTimeFieldValue(FieldPosition.FlightDate2); }
		}

		ZDecimal InsuranceValue
		{
			get { return Fields.GetDecimalFieldValue(FieldPosition.Insurance); }
		}

		protected ZDateTime LastFlightDate
		{
			get { return (FlightDate2.IsEmpty) ? FlightDate1 : FlightDate2; }
		}

		void UnlinkAndRemoveManagedTransports(JASForwardingConsol consol)
		{
			foreach (Transport transport in consol.Transports)
			{
				transport.JW_IsLinked = false;
			}
			consol.Transports.RemoveAndDeleteAll();
		}

		void PopulateFlightDetails(ExportAWBHeader aWBHeader)
		{
			aWBHeader.EH_AirportOfDepartureAndRequestRouteText = AirportOfDeparture.Left(ExportAWBHeader.Schema.EH_AirportOfDepartureAndRequestRouteTextMaxLength);
			aWBHeader.EH_AirportOfDestinationText = AirportOfDestination.Left(ExportAWBHeader.Schema.EH_AirportOfDestinationTextMaxLength);
			aWBHeader.EH_Booking1stCarrier = FlightNo1.Left(2);
			aWBHeader.EH_Booking1stFlight = FlightNo1.SubstringSafe(2, ExportAWBHeaderSchema.EH_Booking1stFlight.MaxLength);
			aWBHeader.EH_Booking1stFlightDate = GetFlightDayFromDateTime(FlightDate1);
			aWBHeader.EH_Booking2ndCarrier = FlightNo2.Left(2);
			aWBHeader.EH_Booking2ndFlight = FlightNo2.SubstringSafe(2, ExportAWBHeaderSchema.EH_Booking2ndFlight.MaxLength);
			aWBHeader.EH_Booking2ndFlightDate = GetFlightDayFromDateTime(FlightDate2);
			aWBHeader.EH_InsuranceValue = InsuranceValue;
		}

		ZString GetFlightDayFromDateTime(ZDateTime flightDate)
		{
			return (flightDate.IsEmpty) ? "" : flightDate.Day.ToString("00");
		}

		#endregion

		#region Handling Info Segment

		ZString HandlingInfo1
		{
			get { return Fields.GetFieldValue(FieldPosition.HandlingInfo1); }
		}

		ZString HandlingInfo2
		{
			get { return Fields.GetFieldValue(FieldPosition.HandlingInfo2); }
		}

		ZString HandlingInfo3
		{
			get { return Fields.GetFieldValue(FieldPosition.HandlingInfo3); }
		}

		protected void AddHandlingInfoNote(BusinessObject bizO)
		{
			StmNote[] notes = bizO.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			ZString handlingInfo = ZString.Format("{0}\r\n{1}\r\n{2}", HandlingInfo1, HandlingInfo2, HandlingInfo3).Left(StmNote.Schema.ST_NoteTextMaxLength);

			if (!handlingInfo.IsEmpty)
			{
				if (notes.Length == 0)
				{
					bizO.GetNotes().AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, handlingInfo);
				}
				else
				{
					notes[0].ST_NoteText = handlingInfo;
				}
			}
		}

		void PopulateHandlingInfo(ExportAWBHeader aWBHeader)
		{
			aWBHeader.EH_HandlingInformation = new ZString(HandlingInfo1 + " " + HandlingInfo2 + " " + HandlingInfo3).Left(aWBHeader.EH_HandlingInformationInfo.MaxLength);
		}

		#endregion

		#region Charges Segment

		ZDecimal PrepaidValuationCharge
		{
			get { return Fields.GetDecimalFieldValue(FieldPosition.PrepaidValuationCharge); }
		}

		ZDecimal CollectValuationCharge
		{
			get { return Fields.GetDecimalFieldValue(FieldPosition.CollectValuationCharge); }
		}

		ZDecimal PrepaidTax
		{
			get { return Fields.GetDecimalFieldValue(FieldPosition.PrepaidTax); }
		}

		ZDecimal CollectTax
		{
			get { return Fields.GetDecimalFieldValue(FieldPosition.CollectTax); }
		}

		void PopulateChargesDetails(ExportAWBHeader aWBHeader)
		{
			aWBHeader.EH_ValuationPPD = PrepaidValuationCharge;
			aWBHeader.EH_ValuationCOL = CollectValuationCharge;
			aWBHeader.EH_TaxesPPD = PrepaidTax;
			aWBHeader.EH_TaxesCOL = CollectTax;
		}

		#endregion

		#region Footer Segment

		ZString SignatureOfShipperOrAgent
		{
			get { return Fields.GetFieldValue(FieldPosition.SignatureOfShipperOrAgent); }
		}

		ZDateTime DateOfIssue
		{
			get { return Fields.GetDateTimeFieldValue(FieldPosition.DateOfIssue); }
		}

		ZString PlaceOfIssue
		{
			get { return Fields.GetFieldValue(FieldPosition.PlaceOfIssue); }
		}

		ZString SignatureOfCarrierOrAgent
		{
			get { return Fields.GetFieldValue(FieldPosition.SignatureOfCarrierOrAgent); }
		}

		void PopulateFooterDetails(ExportAWBHeader aWBHeader)
		{
			aWBHeader.EH_ShippersSignature = SignatureOfShipperOrAgent.Left(ExportAWBHeader.Schema.EH_ShippersSignatureMaxLength);
			aWBHeader.EH_AWBIssueDate = DateOfIssue;
			aWBHeader.EH_AWBIssuePlace = PlaceOfIssue.Left(ExportAWBHeader.Schema.EH_AWBIssuePlaceMaxLength);
			aWBHeader.EH_AWBAgentsSignature = SignatureOfCarrierOrAgent.Left(ExportAWBHeader.Schema.EH_AWBAgentsSignatureMaxLength);
		}

		#endregion

		public JASForwardingConsol LoadOrCreateConsol(BusinessObjectFactoryProvider factoryProvider)
		{
			JASForwardingConsol result = null;

			result = new ConsolLocator<JASForwardingConsol>().Find(factoryProvider.Current, MasterBillNum, "", Core.Constants.TransportModes.Air, "", FlightNo1, FlightDate1);
			if (result == null)
			{
				result = (JASForwardingConsol)factoryProvider.Current.New(typeof(JASForwardingConsol));
				SetConsolDefaultValues(result);
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
					UpdateConsolCore(hEADRecord, consol, notificationSubscriber);
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

		public void UpdateAWBHeader(IAWBParent aWBParent, INotifications notificationSubscriber)
		{
			if (aWBParent.AWBHeader != null)
			{
				ExportAWBHeader aWBHeader = aWBParent.AWBHeader;
				bool isValidationSuspended = aWBHeader.IsValidationSuspended;

				try
				{
					aWBHeader.SuspendValidation();
					aWBHeader.Populate();
					UpdateAWBHeaderCore(aWBHeader, notificationSubscriber);
				}
				finally
				{
					if (!isValidationSuspended)
					{
						aWBHeader.ResumeValidation();
					}
				}
			}
		}

		protected virtual void UpdateConsolCore(HEADRecord hEADRecord, JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			PopulateFlightDetails(hEADRecord, consol);
		}

		protected virtual void UpdateAWBHeaderCore(ExportAWBHeader aWBHeader, INotifications notificationSubscriber)
		{
			PopulateOriginCode(aWBHeader);
			PopulateShipperDetails(aWBHeader);
			PopulateConsigneeDetails(aWBHeader);
			PopulateAccountingInfos(aWBHeader);
			PopulateRoutingDetails(aWBHeader);
			PopulateFreightDeclarationDetails(aWBHeader);
			PopulateFlightDetails(aWBHeader);
			PopulateHandlingInfo(aWBHeader);
			PopulateChargesDetails(aWBHeader);
			PopulateFooterDetails(aWBHeader);
		}

		void SetConsolDefaultValues(JASForwardingConsol consol)
		{
			consol.JK_IsNeutralMaster = false;
			consol.JK_AgentType = AgentType;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			consol.JK_OverrideWaybillDefaults = true;
		}

		void SetTrimmedValue(ZPropertyInfo info, ZString value)
		{
			info.Value = value.Left(info.MaxLength);
		}

		void PopulateFlightDetails(HEADRecord hEADRecord, JASForwardingConsol consol)
		{
			consol.JK_MasterBillNum = MasterBillNum.Left(JASForwardingConsol.Schema.JK_MasterBillNumMaxLength);

			UnlinkAndRemoveManagedTransports(consol);

			ZString lastPort = PortCodeFinder.GetPortCodeFromIATACode(consol.Factory, OriginCityCode, hEADRecord.SendingOfficeCode.Left(2));
			ZDateTime lastDate = ZDateTime.Empty;

			bool firstTransportUsed = false;

			if (!FlightNo1.IsEmpty || To1st.IsEmpty)
			{
				Transport transport = consol.Transports[0];
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_RL_NKLoadPort = lastPort;
				transport.JW_RL_NKDiscPort = lastPort = PortCodeFinder.GetPortCodeFromIATACode(consol.Factory, To1st);
				transport.JW_ETD = FlightDate1.IsEmpty ? lastDate : FlightDate1;
				transport.JW_ETA = lastDate = GetArrivalDateByProximity(consol.Factory, transport.JW_ETD, transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort);
				SetTrimmedValue(transport.JW_VoyageFlightInfo, FlightNo1);
				transport.JW_IsLinked = transport.JW_JX.IsValid;
				firstTransportUsed = true;
			}

			if (!FlightNo2.IsEmpty || !To2nd.IsEmpty)
			{
				Transport transport = firstTransportUsed ? consol.Transports.AddNew() : consol.Transports[0];
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_RL_NKLoadPort = lastPort;
				transport.JW_RL_NKDiscPort = lastPort = PortCodeFinder.GetPortCodeFromIATACode(consol.Factory, To2nd);
				transport.JW_ETD = FlightDate2.IsEmpty ? lastDate : FlightDate2;
				transport.JW_ETA = lastDate = GetArrivalDateByProximity(consol.Factory, transport.JW_ETD, transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort);
				SetTrimmedValue(transport.JW_VoyageFlightInfo, FlightNo2);
				transport.JW_IsLinked = transport.JW_JX.IsValid;
				firstTransportUsed = true;
			}

			if (!To3rd.IsEmpty)
			{
				Transport transport = firstTransportUsed ? consol.Transports.AddNew() : consol.Transports[0];
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_RL_NKLoadPort = lastPort;
				transport.JW_RL_NKDiscPort = lastPort = PortCodeFinder.GetPortCodeFromIATACode(consol.Factory, To3rd);
				transport.JW_ETD = lastDate;
				transport.JW_ETA = lastDate = GetArrivalDateByProximity(consol.Factory, transport.JW_ETD, transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort);

				transport.JW_IsLinked = false;
			}

			if (!hEADRecord.FreightDestination.IsEmpty && hEADRecord.FreightDestination != lastPort)
			{
				Transport transport = firstTransportUsed ? consol.Transports.AddNew() : consol.Transports[0];
				transport.JW_RL_NKLoadPort = lastPort;
				transport.JW_RL_NKDiscPort = lastPort = hEADRecord.FreightDestination;
				transport.JW_ETD = lastDate;
				transport.JW_ETA = lastDate = GetArrivalDateByProximity(consol.Factory, transport.JW_ETD, transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort);
				transport.JW_IsLinked = false;
			}

			consol.JK_RL_NKLoadPort = consol.Transports.DepartureTransport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = consol.Transports.ArrivalTransport.JW_RL_NKDiscPort;
		}

		#region Arrival Date Calculation

		public
 ZDateTime GetArrivalDateByProximity(BusinessObjectFactory factory, ZDateTime departureDate, ZString portOfLoading, ZString portOfDischarge)
		{
			ZoneForArrivalDateCalculation loadingZone = GetZoneForArrivalDateCalculation(factory, portOfLoading);
			ZoneForArrivalDateCalculation dischargeZone = GetZoneForArrivalDateCalculation(factory, portOfDischarge);
			return GetArrivalDateByProximity(departureDate, loadingZone, dischargeZone);
		}

		ZDateTime GetArrivalDateByProximity(ZDateTime departureDate, ZoneForArrivalDateCalculation loadingZone, ZoneForArrivalDateCalculation dischargeZone)
		{
			ZDateTime result = departureDate;

			if (!departureDate.IsEmpty && loadingZone != ZoneForArrivalDateCalculation.None && dischargeZone != ZoneForArrivalDateCalculation.None)
			{
				int dayOffset = 0;
				switch (loadingZone)
				{
					#region Oceania

					case ZoneForArrivalDateCalculation.Oceania:
						switch (dischargeZone)
						{
							case ZoneForArrivalDateCalculation.Europe: dayOffset = 2; break;
							case ZoneForArrivalDateCalculation.Asia: dayOffset = 1; break;
							case ZoneForArrivalDateCalculation.America: dayOffset = 2; break;
							case ZoneForArrivalDateCalculation.Africa: dayOffset = 1; break;
						}
						break;

					#endregion

					#region Europe

					case ZoneForArrivalDateCalculation.Europe:
						switch (dischargeZone)
						{
							case ZoneForArrivalDateCalculation.Oceania: dayOffset = 2; break;
							case ZoneForArrivalDateCalculation.Asia: dayOffset = 1; break;
							case ZoneForArrivalDateCalculation.America: dayOffset = 1; break;
							case ZoneForArrivalDateCalculation.Africa: dayOffset = 1; break;
						}
						break;

					#endregion

					#region Asia

					case ZoneForArrivalDateCalculation.Asia:
						switch (dischargeZone)
						{
							case ZoneForArrivalDateCalculation.Oceania: dayOffset = 1; break;
							case ZoneForArrivalDateCalculation.Europe: dayOffset = 1; break;
							case ZoneForArrivalDateCalculation.America: dayOffset = 1; break;
							case ZoneForArrivalDateCalculation.Africa: dayOffset = 1; break;
						}
						break;

					#endregion

					#region America

					case ZoneForArrivalDateCalculation.America:
						switch (dischargeZone)
						{
							case ZoneForArrivalDateCalculation.Oceania: dayOffset = 2; break;
							case ZoneForArrivalDateCalculation.Europe: dayOffset = 1; break;
							case ZoneForArrivalDateCalculation.Asia: dayOffset = 1; break;
							case ZoneForArrivalDateCalculation.Africa: dayOffset = 2; break;
						}
						break;

					#endregion

					#region Africa

					case ZoneForArrivalDateCalculation.Africa:
						switch (dischargeZone)
						{
							case ZoneForArrivalDateCalculation.Oceania: dayOffset = 2; break;
							case ZoneForArrivalDateCalculation.Europe: dayOffset = 1; break;
							case ZoneForArrivalDateCalculation.Asia: dayOffset = 1; break;
							case ZoneForArrivalDateCalculation.America: dayOffset = 2; break;
						}
						break;

					#endregion
				}

				result = result.AddDays(dayOffset);
			}

			return result;
		}

		ZoneForArrivalDateCalculation GetZoneForArrivalDateCalculation(BusinessObjectFactory factory, ZString portCode)
		{
			ILocation port = new RefUNLOCO.Loader(factory).Load(portCode);
			if (port != null && port.Zones != null && port.Zones.Length > 0)
			{
				ILocation country = port.Country;
				ZIterator<RefZoneHeader> zoneHeaders = (country != null && country.Zones != null && country.Zones.Length > 0)
					? new ZIterator<RefZoneHeader>(port.Zones, country.Zones)
					: new ZIterator<RefZoneHeader>(port.Zones);

				foreach (RefZoneHeader zoneHeader in zoneHeaders)
				{
					foreach (KeyValuePair<ZoneForArrivalDateCalculation, string[]> zoneCodesEntry in ZoneCodesDictionary)
					{
						if (Array.Exists(zoneCodesEntry.Value, delegate(string zoneCode) { return zoneHeader.Code == zoneCode; }))
						{
							return zoneCodesEntry.Key;
						}
					}
				}
			}

			return ZoneForArrivalDateCalculation.None;
		}

		enum ZoneForArrivalDateCalculation
		{
			None,
			Oceania,
			Asia,
			Europe,
			Africa,
			America
		}

		#region Predefined Zones (ordered by most commonly used zones)

		Dictionary<ZoneForArrivalDateCalculation, string[]> ZoneCodesDictionary
		{
			get
			{
				if (fZoneCodesDictionary == null)
				{
					fZoneCodesDictionary = new Dictionary<ZoneForArrivalDateCalculation, string[]>();
					fZoneCodesDictionary.Add(ZoneForArrivalDateCalculation.Oceania, OceanianZones);
					fZoneCodesDictionary.Add(ZoneForArrivalDateCalculation.Africa, AfricanZones);
					fZoneCodesDictionary.Add(ZoneForArrivalDateCalculation.Asia, AsianZones);
					fZoneCodesDictionary.Add(ZoneForArrivalDateCalculation.Europe, EuropeanZones);
					fZoneCodesDictionary.Add(ZoneForArrivalDateCalculation.America, AmericanZones);
				}
				return fZoneCodesDictionary;
			}
		}

		Dictionary<ZoneForArrivalDateCalculation, string[]> fZoneCodesDictionary;

		string[] OceanianZones
		{
			get
			{
				return new string[]
				{
					"AUSR",
					"NZDR",
					"PACR",
					"AUEC",
					"NZNI"
				};
			}
		}

		string[] EuropeanZones
		{
			get
			{
				return new string[]
				{
					"EUOR"
				};
			}
		}

		string[] AmericanZones
		{
			get
			{
				return new string[]
				{
					"USAR",
					"SAMR",
					"CAMR",
					"NAMR",
					"USCA",
					"USMW",
					"USEC"
				};
			}
		}

		string[] AsianZones
		{
			get
			{
				return new string[]
				{
					"SEAR",
					"MEAR",
					"INDR",
					"NASR"
				};
			}
		}

		string[] AfricanZones
		{
			get
			{
				return new string[]
				{
					"SAFR",
					"AFOR"
				};
			}
		}

		#endregion

		#endregion

		protected PortCodeFinder PortCodeFinder
		{
			get
			{
				if (fPortCodeFinder == null)
				{
					fPortCodeFinder = new PortCodeFinder();
				}
				return fPortCodeFinder;
			}
		}

		protected virtual ZString AgentType
		{
			get { return Core.Constants.AgentType.Agent; }
		}

		protected abstract JXCConstants.AWBFieldPositions FieldPosition { get; }

		PortCodeFinder fPortCodeFinder;
	}
}
