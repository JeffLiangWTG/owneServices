using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCMessageDecoder
	{
		public EXDOCMessageDecoder(SANCRTMessage message, BusinessObjectFactory factory)
		{
			this.message = message;
			this.factory = factory;
			Notices = new List<EXDOCMessageDecoderNotice>();
		}

		#region Process

		public void Process()
		{
			ProcessBGM();
			ProcessDTM();
			ProcessSTS();
			ProcessLOC();
			ProcessRFF();
			ProcessFTX();
			ProcessMEA();
			ProcessMOA();
			ProcessGIS();
			ProcessGroup1();
			ProcessGroup2And3();
			ProcessGroup4();
			ProcessGroup7();
			ProcessGroup8And9();
			ProcessLines();
		}

		#region ProcessBGM

		void ProcessBGM()
		{
			switch (message.BGM[0].DocumentMessageName.DocumentMessageNameCoded)
			{
				case "D":
					ProduceType = EXDOCCommodityCodes.Codes.Dairy;
					break;
				case "E":
					ProduceType = EXDOCCommodityCodes.Codes.Eggs;
					break;
				case "F":
					ProduceType = EXDOCCommodityCodes.Codes.Fish;
					break;
				case "G":
					ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
					break;
				case "H":
					ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
					break;
				case "I":
					ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
					break;
				case "M":
					ProduceType = EXDOCCommodityCodes.Codes.Meat;
					break;
				case "S":
					ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
					break;
				case "W":
					ProduceType = EXDOCCommodityCodes.Codes.Wool;
					break;
			}

			RequestIdentificationNumber = message.BGM[0].DocumentMessageIdentification.DocumentMessageNumber;
			IsAccepted = message.BGM[0].ResponseTypeCoded == ResponseTypeCodedList.Accepted;

			IsEnquiryAdvice = message.BGM[0].MessageFunctionCoded == MessageFunctionCodedList.Duplicate;
			IsAccpetTransfer = message.BGM[0].MessageFunctionCoded == MessageFunctionCodedList.GetFromString("93");
			IsTransfer = message.BGM[0].MessageFunctionCoded == MessageFunctionCodedList.GetFromString("91");
			IsForward = message.BGM[0].MessageFunctionCoded == MessageFunctionCodedList.GetFromString("80");
			IsReissue = message.BGM[0].MessageFunctionCoded == MessageFunctionCodedList.Reissue;
			IsAcknowledgement = message.BGM[0].MessageFunctionCoded == MessageFunctionCodedList.Response;
			IsCertificateRequestAcknowledgement = message.BGM[0].MessageFunctionCoded == MessageFunctionCodedList.GetFromString(EXDOCCertificateReasonCodes.Codes.CertificateRequestAcknowledgement);
		}

		#endregion

		#region ProcessDTM

		void ProcessDTM()
		{
			foreach (DTMSegment dTM in message.DTM)
			{
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.PackagingDate)
				{
					ZDateTime packDate;
					if (ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out packDate, "yyyyMMdd"))
					{
						PackDate = packDate;
					}
				}
			}
		}

		#endregion

		#region ProcessSTS

		void ProcessSTS()
		{
			foreach (STSSegment sTS in message.STS)
			{
				if (sTS.StatusType.StatusTypeCoded == StatusTypeCodedList.GetFromString("COM"))
				{
					ComplianceStatus = sTS.StatusReason1.StatusReason;
				}
				if (sTS.StatusType.StatusTypeCoded == StatusTypeCodedList.GetFromString("TRN"))
				{
					TransferStatus = sTS.StatusReason1.StatusReasonCoded.ToString();
				}
				if (sTS.StatusType.StatusTypeCoded == StatusTypeCodedList.GetFromString("EMB"))
				{
					EmbargoStatus = sTS.StatusReason1.StatusReasonCoded.ToString();
				}
				if (sTS.StatusType.StatusTypeCoded == StatusTypeCodedList.GetFromString("CRS"))
				{
					CertificateRequestStatus = sTS.StatusReason1.StatusReason;
				}
			}
		}

		#endregion

		#region ProcessLOC

		void ProcessLOC()
		{
			foreach (LOCSegment lOC in message.LOC)
			{
				if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.PlaceOfCustomsExamination)
				{
					BorderInspectionPort = lOC.LocationIdentification.PlaceLocationIdentification;
				}
				if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.PlacePortOfLoading)
				{
					LoadingPort = "AU" + lOC.LocationIdentification.PlaceLocationIdentification;
				}
				if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.PortOfDischarge)
				{
					DischargePort = lOC.LocationIdentification.PlaceLocationIdentification;
				}
				if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.PlaceOfDestination)
				{
					destinationCity = lOC.LocationIdentification.PlaceLocationIdentification;
				}
				if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.CountryOfUltimateDestination)
				{
					destinationCountry = lOC.LocationIdentification.PlaceLocationIdentification;
				}
				if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.CountryOfSource)
				{
					ProductSourceCountry = lOC.LocationIdentification.PlaceLocationIdentification;
				}
				if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.PlaceOfDocumentIssue)
				{
					CertificateRequiredLocation = lOC.LocationIdentification.PlaceLocationIdentification;
				}
				if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.CountryOfTransit)
				{
					TransitCountry = lOC.LocationIdentification.PlaceLocationIdentification;
				}
				if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.RegionOfProduction)
				{
					AqisRegion = lOC.LocationIdentification.PlaceLocationIdentification;
				}
			}
		}

		#endregion

		#region ProcessRFF

		void ProcessRFF()
		{
			foreach (RFFSegment rFF in message.RFF)
			{
				if (rFF.Reference.ReferenceQualifier == ReferenceQualifierList.DeclarantsReferenceNumber)
				{
					ExporterReference = rFF.Reference.ReferenceNumber;
				}
				if (rFF.Reference.ReferenceQualifier == ReferenceQualifierList.ExportPermitNumber)
				{
					ExportPermitNumber = rFF.Reference.ReferenceNumber;
				}
				if (rFF.Reference.ReferenceQualifier == ReferenceQualifierList.GoodsDeclarationNumber)
				{
					CustomsAuthorityNumber = rFF.Reference.ReferenceNumber;
				}
			}
		}

		#endregion

		#region ProcessFTX

		void ProcessFTX()
		{
			foreach (FTXSegment fTX in message.FTX)
			{
				ZStringBuilder builder = new ZStringBuilder(new ZString[] { fTX.TextLiteral.FreeText1, fTX.TextLiteral.FreeText2, fTX.TextLiteral.FreeText3, fTX.TextLiteral.FreeText4, fTX.TextLiteral.FreeText5, fTX.TextLiteral.FreeText6, fTX.TextLiteral.FreeText7, fTX.TextLiteral.FreeText8 });
				var appendStrings = builder.ToString();
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.Declaration)
				{
					ExporterDeclaration += appendStrings;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.CertificationStatements)
				{
					InspectorComments += appendStrings;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.PartyInstructions)
				{
					NotifyPartyText += appendStrings;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.LetterOfCreditInformation)
				{
					LetterOfCreditText += appendStrings;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.AdditionalInformation)
				{
					AdditionalInformation += appendStrings;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.ChangeInformation)
				{
					AmendmentReason += appendStrings;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.GetFromString(RequestForPermitFishHeaderMessageBuilder.OriginCatchingZoneSubjectQualifer))
				{
					OriginCatchingZone += appendStrings;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.AdditionalMarksNumbersInformation)
				{
					LotNumber += appendStrings;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.CustomsClearanceInstructions)
				{
					EmbargoMessage += appendStrings;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.AdditionalAttributeInformation)
				{
					AvAnimalAge += fTX.TextLiteral.FreeText1;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.ErrorDescriptionFreeText)
				{
					if (fTX.TextLiteral.FreeText1.ToUpper().Contains("RFP HAS BEEN DELETED SUCCESSFULLY") && IsAccepted)
					{
						IsAcceptedWithdrawal = true;
					}

					EXDOCMessageDecoderNotice notice = new EXDOCMessageDecoderNotice();
					notice.Process(appendStrings);
					Notices.Add(notice);
				}
			}
		}

		#endregion

		#region ProcessMEA

		void ProcessMEA()
		{
			foreach (MEASegment mEA in message.MEA)
			{
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.Temperature &&
					mEA.MeasurementDetails.PropertyMeasuredCoded == PropertyMeasuredCodedList.X_ShippingTolerance)
				{
					TemperatureUnit = mEA.ValueRange.MeasureUnitQualifier;
					ZDecimal temperature;
					if (ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out temperature))
					{
						AbsoluteTemperature = temperature;
					}

					if (ZDecimal.TryParse(mEA.ValueRange.RangeMinimum, out temperature))
					{
						MinimumTemperature = temperature;
					}

					if (ZDecimal.TryParse(mEA.ValueRange.RangeMaximum, out temperature))
					{
						MaximumTemperature = temperature;
					}
				}
			}
		}

		#endregion

		#region ProcessMOA

		void ProcessMOA()
		{
			foreach (MOASegment mOA in message.MOA)
			{
				if (mOA.MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.FobValue)
				{
					fobCurrency = mOA.MonetaryAmount.CurrencyCoded;
				}
			}
		}

		#endregion

		#region ProcessGIS

		void ProcessGIS()
		{
			foreach (GISSegment gIS in message.GIS)
			{
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitHeaderMessageBuilder.CertificatePrintIndicatorQualifier))
				{
					CertificatePrintIndicator = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString();
				}
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitHeaderMessageBuilder.SeperateCertificateContainerIndicatorQualifier))
				{
					SeparateCertificateContainerIndicator = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString() == "Y";
				}
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitHeaderMessageBuilder.SeperateCertificateMarksIndicatorQualifier))
				{
					SeparateCertificateMarksIndicator = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString() == "Y";
				}
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitHeaderMessageBuilder.SeperateCertificatePackerIndicatorQualifier))
				{
					SeparateCertificatePackerIndicator = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString() == "Y";
				}
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitMeatHeaderMessageBuilder.ShipStoresIndicatorQualifier))
				{
					ShipsStoresIndicator = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString() == "Y";
				}
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitHeaderMessageBuilder.RFPForwardStatusQualifier))
				{
					ForwardStatus = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString();
				}
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitHeaderMessageBuilder.AMLCQuotaIndicatorQualifier))
				{
					AMLCQuotaIndicator = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString() == "Y";
				}
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitHeaderMessageBuilder.CustomsAgentIndicatorQualifier))
				{
					CustomsAgentIndicator = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString() == "Y";
				}
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitHeaderMessageBuilder.DeclarationEstimateIndicatorQualifier))
				{
					declarationEstimate = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString();
				}
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitDairyHeaderMessageBuilder.DeclarationOfComplianceIndicator))
				{
					switch (gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString())
					{
						case "Y":
							DeclarationOfComplianceIndicator = EXDOCYesNoEmpty.Codes.Yes;
							break;
						case "N":
							DeclarationOfComplianceIndicator = EXDOCYesNoEmpty.Codes.No;
							break;
						default:
							DeclarationOfComplianceIndicator = EXDOCYesNoEmpty.Codes.Empty;
							break;
					}
				}
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitDairyHeaderMessageBuilder.TrueAndCompleteIndicator))
				{
					switch (gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString())
					{
						case "Y":
							TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.Yes;
							break;
						case "N":
							TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.No;
							break;
						default:
							TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.Empty;
							break;
					}
				}
				if (gIS.ProcessingIndicator.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitDairyHeaderMessageBuilder.ImportedProductFlagIndicator))
				{
					switch (gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString())
					{
						case "Y":
							ImportedProductIndicator = EXDOCYesNoEmpty.Codes.Yes;
							break;
						case "N":
							ImportedProductIndicator = EXDOCYesNoEmpty.Codes.No;
							break;
						default:
							ImportedProductIndicator = EXDOCYesNoEmpty.Codes.Empty;
							break;
					}
				}
			}
		}

		#endregion

		#region ProcessGroup1

		void ProcessGroup1()
		{
			Permits = new List<EXDOCMesageDecoderPermit>();
			EXDOCMesageDecoderPermit permit;
			foreach (SegmentGroup1 group1 in message.Group1)
			{
				permit = new EXDOCMesageDecoderPermit(group1);
				permit.Process();
				Permits.Add(permit);
			}
		}

		#endregion

		#region ProcessGroup2And3

		void ProcessGroup2And3()
		{
			foreach (SegmentGroup2 group2 in message.Group2)
			{
				ProcessPNA(group2);
				ProcessADR(group2);
			}
		}

		#region ProcessPNA

		void ProcessPNA(SegmentGroup2 group2)
		{
			foreach (PNASegment pNA in group2.PNA)
			{
				if (pNA.PartyQualifier == PartyQualifierList.Exporter)
				{
					OwnerExporterNumber = pNA.IdentificationNumber.IdentityNumber;
					ProcessForwardeeEDIUserIdentifier(group2);
				}
				if (pNA.PartyQualifier == PartyQualifierList.Consignee)
				{
					ConsigneeReferenceNumber = pNA.IdentificationNumber.IdentityNumber;
					ConsigneeName = pNA.NameComponentDetails1.NameComponent.Trim();
					ProcessConsigneeRepresentativeName(group2);
					IsToOrderImporter = ConsigneeName == "TO ORDER";
				}
				if (pNA.PartyQualifier == PartyQualifierList.ConsigneesAgent)
				{
					forwarderName = pNA.NameComponentDetails1.NameComponent;
				}
				if (pNA.PartyQualifier == PartyQualifierList.TransferTo)
				{
					TransfereeExporterNumber = pNA.IdentificationNumber.IdentityNumber;
					ProcessTransfereeEDIUserIdentifier(group2);
				}
			}
		}

		void ProcessForwardeeEDIUserIdentifier(SegmentGroup2 group2)
		{
			foreach (SegmentGroup3 group3 in group2.Group3)
			{
				foreach (CTASegment cTA in group3.CTA)
				{
					if (cTA.ContactFunctionCoded == ContactFunctionCodedList.Agent)
					{
						ForwardeeEDIUserIdentifier = cTA.DepartmentOrEmployeeDetails.DepartmentOrEmployeeIdentification;
					}
				}
			}
		}

		void ProcessConsigneeRepresentativeName(SegmentGroup2 group2)
		{
			foreach (SegmentGroup3 group3 in group2.Group3)
			{
				foreach (CTASegment cTA in group3.CTA)
				{
					if (cTA.ContactFunctionCoded == ContactFunctionCodedList.Agent)
					{
						forwarderName = cTA.DepartmentOrEmployeeDetails.DepartmentOrEmployee;
					}
				}
			}
		}

		void ProcessTransfereeEDIUserIdentifier(SegmentGroup2 group2)
		{
			foreach (SegmentGroup3 group3 in group2.Group3)
			{
				foreach (CTASegment cTA in group3.CTA)
				{
					if (cTA.ContactFunctionCoded == ContactFunctionCodedList.Agent)
					{
						TransfereeEDIUserIdentifier = cTA.DepartmentOrEmployeeDetails.DepartmentOrEmployeeIdentification;
					}
				}
			}
		}

		#endregion

		#region ProcessADR

		void ProcessADR(SegmentGroup2 group2)
		{
			foreach (ADRSegment aDR in group2.ADR)
			{
				if (aDR.AddressDetails.AddressFormatCoded == AddressFormatCodedList.UnstructuredAddress)
				{
					ConsigneeAddressLine1 = aDR.AddressDetails.AddressComponent1.Trim();
					ConsigneeAddressLine2 = aDR.AddressDetails.AddressComponent2.Trim();
					ConsigneeCity = aDR.CityName.Trim();
					ConsigneePostcode = aDR.PostcodeIdentification.Trim();
					ConsigneeCountry = aDR.CountryCoded.Trim();
					ConsigneeState = aDR.CountrySubEntityDetails.CountrySubEntity.Trim();
				}
			}
		}

		#endregion

		#endregion

		#region ProcessGroup4

		void ProcessGroup4()
		{
			foreach (SegmentGroup4 group4 in message.Group4)
			{
				ProcessTDT(group4);
				ProcessDTM(group4);
			}
		}

		#region ProcessTDT

		void ProcessTDT(SegmentGroup4 group4)
		{
			foreach (TDTSegment tDT in group4.TDT)
			{
				if (tDT.TransportStageQualifier == TransportStageQualifierList.AtDeparture)
				{
					VoyageFlightNumber = tDT.ConveyanceReferenceNumber;
					transportmode = tDT.ModeOfTransport.ModeOfTransportCoded;
					carrierName = tDT.Carrier.CarrierName;
					VesselName = tDT.TransportIdentification.IdOfTheMeansOfTransport;
				}
			}
		}

		#endregion

		#region ProcessDTM

		void ProcessDTM(SegmentGroup4 group4)
		{
			foreach (DTMSegment dTM in group4.DTM)
			{
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.DepartureDateTime)
				{
					ZDateTime departureDate;
					if (ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out departureDate, "yyyyMMdd"))
					{
						DepartureDate = departureDate;
					}
				}
			}
		}

		#endregion

		#endregion

		#region ProcessGroup7

		void ProcessGroup7()
		{
			foreach (SegmentGroup6 group6 in message.Group6)
			{
				foreach (SegmentGroup7 group7 in group6.Group7)
				{
					ProcessSEL(group7);
				}
			}
		}

		#region ProcessSEL

		void ProcessSEL(SegmentGroup7 group7)
		{
			foreach (SELSegment sEL in group7.SEL)
			{
				StartSealNumber = sEL.SealNumber;
				EndSealNumber = sEL.SealIssuer.SealingPartyCoded.ToString();
			}
		}

		#endregion

		#endregion

		#region ProcessGroup8And9

		void ProcessGroup8And9()
		{
			foreach (SegmentGroup8 group8 in message.Group8)
			{
				ProcessDTM(group8);
				ProcessGroup9(group8);
				ProcessShipsCompartments(group8);
			}
		}

		#region ProcessDTM

		void ProcessDTM(SegmentGroup8 group8)
		{
			foreach (DTMSegment dTM in group8.DTM)
			{
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.RequestDate)
				{
					ZDateTime inspectionRequestedDate;
					if (dTM.DateTimePeriod.DateTimePeriodFormatQualifier == DateTimePeriodFormatQualifierList.Ccyymmddhhmmss)
					{
						if (ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out inspectionRequestedDate, "yyyyMMddhhmmss"))
						{
							InspectionRequestedDate = inspectionRequestedDate;
						}
					}
					else
					{
						if (ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out inspectionRequestedDate, "yyyyMMdd"))
						{
							InspectionRequestedDate = inspectionRequestedDate;
						}
					}
				}

				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.StartDateTime)
				{
					ZDateTime authorisedStartDate;
					if (ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out authorisedStartDate, "yyyyMMdd"))
					{
						AuthorisedStartDate = authorisedStartDate;
					}
				}

				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.TestCompletionDate)
				{
					ZDateTime authorisedEndDate;
					if (ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out authorisedEndDate, "yyyyMMdd"))
					{
						AuthorisedEndDate = authorisedEndDate;
					}
				}
			}
		}

		#endregion

		#region ProcessShipsCompartments

		void ProcessShipsCompartments(SegmentGroup8 group8)
		{
			Compartments = new List<EXDOCMessageDecoderShipsCompartment>();
			EXDOCMessageDecoderShipsCompartment compartment;
			foreach (PRCSegment pRC in group8.PRC)
			{
				if (pRC.ProcessTypeAndDescription.ProcessTypeIdentification == ProcessTypeIdentificationList.GetFromString(RequestForPermitGrainsAndPlantsHeaderMessageBuilder.ShipsHoldInspection))
				{
					compartment = new EXDOCMessageDecoderShipsCompartment(group8, factory);
					compartment.Process();
					Compartments.Add(compartment);
				}
			}
		}

		#endregion

		#region ProcessGroup9

		void ProcessGroup9(SegmentGroup8 group8)
		{
			foreach (SegmentGroup9 group9 in group8.Group9)
			{
				ProcessPNA(group9);
			}
		}

		#region ProcessPNA

		void ProcessPNA(SegmentGroup9 group9)
		{
			foreach (PNASegment pNA in group9.PNA)
			{
				if (pNA.PartyQualifier == PartyQualifierList.PartyPerformingInspection)
				{
					AuthorisationEstablishment = pNA.IdentificationNumber.IdentityNumber.Trim();
				}
				if (pNA.PartyQualifier == PartyQualifierList.AuthorizingOfficial)
				{
					AuthorisingOfficeIdentifier = pNA.IdentificationNumber.IdentityNumber;
				}
				if (pNA.PartyQualifier == PartyQualifierList.ManufacturingPlant)
				{
					StoreageEstablishmentNumber = pNA.IdentificationNumber.IdentityNumber;
				}
				if (pNA.PartyQualifier == PartyQualifierList.CertifyingParty)
				{
					ApprovedCertifier = pNA.IdentificationNumber.IdentityNumber;
				}
			}
		}

		#endregion

		#endregion

		#endregion

		#region ProcessLines

		void ProcessLines()
		{
			Lines = new List<EXDOCMessageDecoderLine>();
			EXDOCMessageDecoderLine line;
			foreach (SegmentGroup11 group11 in message.Group11)
			{
				line = new EXDOCMessageDecoderLine(group11);
				line.Process();
				Lines.Add(line);
			}
		}

		#endregion

		#endregion

		#region Properties

		public ZString FinalDestination
		{
			get
			{
				if (finalDestination.IsEmpty)
				{
					RefUNLOCO finalDestinationUNLOCO = RefUNLOCO.GetPortFromNameAndCountryCode(factory, destinationCity, destinationCountry);
					if (finalDestinationUNLOCO != null)
					{
						finalDestination = finalDestinationUNLOCO.RL_Code;
					}
				}
				return finalDestination;
			}
		}

		public ZGuid FobCurrencyUnit
		{
			get
			{
				if (fobCurrencyGuid.IsEmpty)
				{
					RefCurrency currency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, ExDocCurrencyCodeConverter.ConvertToIso4217(fobCurrency));
					if (currency != null)
					{
						fobCurrencyGuid = currency.PK;
					}
				}
				return fobCurrencyGuid;
			}
		}

		public ZString DeclarationEstimate
		{
			get
			{
				switch (declarationEstimate)
				{
					case "N":
						return JobDeclaration.MessageSubType.NonConfirming;
					case "C":
						return JobDeclaration.MessageSubType.Confirming;
					case "M":
						return JobDeclaration.MessageSubType.Manual;
					default:
						return JobDeclaration.MessageSubType.NonConfirming;
				}
			}
		}

		public ZString TransportMode
		{
			get
			{
				switch (transportmode)
				{
					case EXDOCTransportModeCodes.Codes.Sea:
						return Core.Constants.TransportModes.Sea;
					case EXDOCTransportModeCodes.Codes.Air:
						return Core.Constants.TransportModes.Air;
					case EXDOCTransportModeCodes.Codes.Mail:
						return Core.Constants.TransportModes.Mail;
					default:
						return ZString.Empty;
				}
			}
		}

		public ZGuid Carrier
		{
			get
			{
				if (carrier.IsEmpty && !carrierName.IsEmpty)
				{
					var matching = new AUOrganisationMatching(new BusinessObjectFactoryProvider(factory), Xsd.XmlInterchange.Empty, new NotificationBuffer());
					carrier = matching.Match(carrierName, OrganisationTypes.Carrier);
				}
				return carrier;
			}
		}

		public ZGuid Supplier
		{
			get
			{
				if (supplier.IsEmpty)
				{
					ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, IsTransfer ? TransfereeExporterNumber : OwnerExporterNumber);
					query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber);
					OrgCusCode cusCode = factory.LoadTop1<OrgCusCode>(query);
					if (cusCode != null)
					{
						supplier = cusCode.OK_OH;
					}
				}
				return supplier;
			}
		}

		public ZGuid Importer
		{
			get
			{
				if (importer.IsEmpty)
				{
					var matching = new AUOrganisationMatching(new BusinessObjectFactoryProvider(factory), Xsd.XmlInterchange.Empty, new NotificationBuffer());
					Xsd.Organisation criteria = new Xsd.Organisation();
					criteria.OrganisationDetails.Name = ConsigneeName;
					criteria.OrganisationDetails.Location.Country = ConsigneeCountry;
					criteria.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
					Xsd.OrgAddress address = criteria.OrganisationDetails.Addresses.AddNew();
					address.AddressLine1 = ConsigneeAddressLine1;
					address.AddressLine2 = ConsigneeAddressLine2;
					address.CityOrSuburb = ConsigneeCity;
					address.PostCode = ConsigneePostcode;
					address.StateOrProvince = ConsigneeState;
					OrgMatchingResult result = matching.Match(criteria, OrganisationTypes.Consignee);
					if (result.MatchFound || !result.TempOrgCouldNotBeCreated)
					{
						importer = result.Match.PK;
					}
				}
				return importer;
			}
		}

		public ZString UnmatchedNoteInformation
		{
			get
			{
				ZString unmatchedNoteInformation = ZString.Empty;

				List<UnmatchOrgRecord> unmatchRecords = new List<UnmatchOrgRecord>();

				OrgHeader unmatchedConsigneeOrganisation = factory.Load<OrgHeader>(importer);
				if (unmatchedConsigneeOrganisation != null && unmatchedConsigneeOrganisation.OH_Code == OrgHeader.UnmatchedOrganisationCode)
				{
					UnmatchOrgRecord consigneeRec = new UnmatchOrgRecord
					{
						OrganisationName = ConsigneeName + " (Threshold=" + Registry.Business.OrganisationsDataRegistry.Instance.OrgMatchThreshold.Value + ")",
						AddressLine1 = ConsigneeAddressLine1,
						AddressLine2 = ConsigneeAddressLine2,
						City = ConsigneeCity,
						PostCode = ConsigneePostcode,
						StateOrProvince = ConsigneeState,
						Country = ConsigneeCountry,
						OrganisationType = nameof(OrganisationTypes.Consignee),
						OrganisationSubType = nameof(OrganisationTypes.Consignee)
					};

					unmatchRecords.Add(consigneeRec);
				}
				OrgHeader unmatchedForwarderOrganisation = factory.Load<OrgHeader>(forwarder);
				if (unmatchedForwarderOrganisation != null && unmatchedForwarderOrganisation.OH_Code == OrgHeader.UnmatchedOrganisationCode)
				{
					UnmatchOrgRecord forwarderRec = new UnmatchOrgRecord
					{
						OrganisationName = forwarderName,
						OrganisationType = nameof(OrganisationTypes.Forwarder),
						OrganisationSubType = nameof(OrganisationTypes.Forwarder),
					};

					unmatchRecords.Add(forwarderRec);
				}
				OrgHeader unmatchedCarrierOrganisation = factory.Load<OrgHeader>(carrier);
				if (unmatchedCarrierOrganisation != null && unmatchedCarrierOrganisation.OH_Code == OrgHeader.UnmatchedOrganisationCode)
				{
					UnmatchOrgRecord carrierRec = new UnmatchOrgRecord
					{
						OrganisationName = carrierName,
						OrganisationType = nameof(OrganisationTypes.Carrier),
						OrganisationSubType = nameof(OrganisationTypes.Carrier)
					};

					unmatchRecords.Add(carrierRec);
				}

				if (unmatchRecords.Count > 0)
				{
					UnmatchOrgRecords orgRecords = new UnmatchOrgRecords(factory) { OrgDetailsList = unmatchRecords.ToArray() };
					unmatchedNoteInformation = orgRecords.AsXml();
				}
				return unmatchedNoteInformation;
			}
		}

		public ZGuid Forwarder
		{
			get
			{
				if (forwarder.IsEmpty && !forwarderName.IsEmpty)
				{
					var matching = new AUOrganisationMatching(new BusinessObjectFactoryProvider(factory), Xsd.XmlInterchange.Empty, new NotificationBuffer());
					forwarder = matching.Match(forwarderName, OrganisationTypes.Forwarder);
				}
				return forwarder;
			}
		}

		public ZString RequestIdentificationNumber { get; private set; }
		//public ZString MessageRole { get; private set; }

		public ZBool IsToOrderImporter { get; private set; }
		public ZBool IsEnquiryAdvice { get; private set; }
		public ZBool IsAccpetTransfer { get; private set; }
		public ZBool IsTransfer { get; private set; }
		public ZBool IsForward { get; private set; }
		public ZBool IsReissue { get; private set; }
		public ZBool IsAcknowledgement { get; private set; }
		public ZBool IsCertificateRequestAcknowledgement { get; private set; }
		public ZBool IsAccepted { get; private set; }
		public ZBool IsAcceptedWithdrawal { get; private set; }
		public ZDateTime PackDate { get; private set; }
		public ZString ProduceType { get; private set; }
		public ZString BorderInspectionPort { get; private set; }
		public ZString LoadingPort { get; private set; }
		public ZString DischargePort { get; private set; }
		public ZString TransitCountry { get; private set; }
		public ZString ProductSourceCountry { get; private set; }
		public ZString CertificateRequiredLocation { get; private set; }
		public ZString AqisRegion { get; private set; }
		public ZString ExporterDeclaration { get; private set; }
		public ZString InspectorComments { get; private set; }
		public ZString NotifyPartyText { get; private set; }
		public ZString LetterOfCreditText { get; private set; }
		public ZString AdditionalInformation { get; private set; }
		public ZString AmendmentReason { get; private set; }
		public ZString OriginCatchingZone { get; private set; }
		public ZString LotNumber { get; private set; }
		public ZString TemperatureUnit { get; private set; }
		public ZDecimal AbsoluteTemperature { get; private set; }
		public ZDecimal MinimumTemperature { get; private set; }
		public ZDecimal MaximumTemperature { get; private set; }
		public ZString CertificatePrintIndicator { get; private set; }
		public ZBool SeparateCertificateContainerIndicator { get; private set; }
		public ZBool SeparateCertificateMarksIndicator { get; private set; }
		public ZBool SeparateCertificatePackerIndicator { get; private set; }
		public ZBool ShipsStoresIndicator { get; private set; }
		public ZBool AMLCQuotaIndicator { get; private set; }
		public ZBool CustomsAgentIndicator { get; private set; }
		public ZString VoyageFlightNumber { get; private set; }
		public ZString DeclarationOfComplianceIndicator { get; private set; }
		public ZString TrueAndCompleteIndicator { get; private set; }
		public ZString ImportedProductIndicator { get; private set; }
		public ZString VesselName { get; private set; }
		public ZDateTime DepartureDate { get; private set; }
		public ZString StartSealNumber { get; private set; }
		public ZString EndSealNumber { get; private set; }
		public ZDateTime InspectionRequestedDate { get; private set; }
		public ZDateTime AuthorisedStartDate { get; private set; }
		public ZDateTime AuthorisedEndDate { get; private set; }
		public ZString AuthorisationEstablishment { get; private set; }
		public ZString AuthorisingOfficeIdentifier { get; private set; }
		public ZString StoreageEstablishmentNumber { get; private set; }
		//public ZDateTime ShipsHoldInspectionDate { get; private set; }
		public ZString ComplianceStatus { get; private set; }
		public ZString CertificateRequestStatus { get; private set; }
		public ZString TransferStatus { get; private set; }
		public ZString EmbargoStatus { get; private set; }
		public ZString ExporterReference { get; private set; }
		public ZString ExportPermitNumber { get; private set; }
		public ZString CustomsAuthorityNumber { get; private set; }
		public ZString EmbargoMessage { get; private set; }
		public List<EXDOCMessageDecoderNotice> Notices { get; private set; }
		public ZString ForwardStatus { get; private set; }
		public ZString OwnerExporterNumber { get; private set; }
		public ZString TransfereeExporterNumber { get; private set; }
		public ZString ConsigneeReferenceNumber { get; private set; }
		public ZString ConsigneeName { get; private set; }
		public ZString ConsigneeAddressLine1 { get; private set; }
		public ZString ConsigneeAddressLine2 { get; private set; }
		public ZString ConsigneeCity { get; private set; }
		public ZString ConsigneePostcode { get; private set; }
		public ZString ConsigneeCountry { get; private set; }
		public ZString ConsigneeState { get; private set; }
		public ZString ForwardeeEDIUserIdentifier { get; private set; }
		public ZString TransfereeEDIUserIdentifier { get; private set; }
		public ZString ApprovedCertifier { get; private set; }
		public ZString AvAnimalAge { get; private set; }
		public List<EXDOCMessageDecoderLine> Lines { get; private set; }
		public List<EXDOCMessageDecoderShipsCompartment> Compartments { get; private set; }
		public List<EXDOCMesageDecoderPermit> Permits { get; private set; }

		#endregion

		#region Implementation

		readonly BusinessObjectFactory factory;
		readonly SANCRTMessage message;
		ZString transportmode;
		ZString declarationEstimate;
		ZString destinationCity;
		ZString destinationCountry;
		ZString finalDestination;
		ZString fobCurrency;
		ZGuid fobCurrencyGuid;
		ZString carrierName;
		ZGuid carrier;
		ZGuid supplier;
		ZGuid importer;
		ZString forwarderName;
		ZGuid forwarder;

		#endregion
	}
}
