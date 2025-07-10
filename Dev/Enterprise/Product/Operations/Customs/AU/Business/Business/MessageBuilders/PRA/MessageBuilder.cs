using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business.D98B;
using Enterprise.Customs.Common.AU;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;
using IMessageBuilder = Enterprise.Integration.Customs.AU.IMessageBuilder;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MessageBuilder : IMessageBuilder
	{
		#region Interface
		public MessageBuilder(IPRAMessagingData messagingData, PRAMessageTypeConstants.MessageType messageType)
		{
			this.messagingData = messagingData;
			this.messageType = messageType;
		}

		public string GetMessageText()
		{
			GenerateIfNotAlreadyGenerated();

			return iFTERA.ToString(new UNOCCMRCharacterSet());
		}

		IBusiness IMessageBuilder.PostMessage() => PostMessage();

		public EDIMessage PostMessage()
		{
			GenerateIfNotAlreadyGenerated();

			var ediMessage = messagingData.GetEDIMessage();
			ediMessage.EM_MessageText = GetMessageText();

			switch (messagingData.LoadTerminal1StopCode)
			{
				case "CXSXAD":
					ediMessage.EM_MessageSubType = (messageType == PRAMessageTypeConstants.MessageType.Submit || messageType == PRAMessageTypeConstants.MessageType.ReSubmit) ? "XSM" : "XCN";
					break;
				default:
					ediMessage.EM_MessageSubType = (messageType == PRAMessageTypeConstants.MessageType.Submit || messageType == PRAMessageTypeConstants.MessageType.ReSubmit) ? "SSM" : "SCN";
					break;
			}

			return ediMessage;
		}

		#endregion

		#region Group Generators

		protected void GenerateGroup0()
		{
			iFTERA = new IFTERAMessage();

			var uNH = iFTERA.UNH.InstantiateAChildAndAddItToChildrenCollection();
			uNH.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			uNH.MessageIdentifier.MessageType = MessageTypeList.GetFromString("IFTERA"); // Not a real UN Message so synthesized.
			uNH.MessageIdentifier.MessageVersionNumber = MessageVersionNumberList.DraftVersionUnEdifactDirectory;
			uNH.MessageIdentifier.MessageReleaseNumber = MessageReleaseNumberList.Release1998B;
			uNH.MessageIdentifier.ControllingAgency = ControllingAgencyList.GetFromString("RT");
			uNH.MessageIdentifier.AssociationAssignedCode = "ENET54";

			var bGM = iFTERA.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bGM.DocumentMessageName.DocumentMessageNameCoded = DocumentMessageNameCodedList.GetFromString("ERA");
			bGM.DocumentMessageIdentification.DocumentMessageNumber = messagingData.SenderID;
			bGM.MessageFunctionCoded = (messageType == PRAMessageTypeConstants.MessageType.Submit || messageType == PRAMessageTypeConstants.MessageType.ReSubmit) ? MessageFunctionCodedList.Original : MessageFunctionCodedList.Cancellation;
			bGM.ResponseTypeCoded = ResponseTypeCodedList.ResponseExpected;

			var dTM = iFTERA.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dTM.DateTimePeriod.DateTimePeriodQualifier = DateTimePeriodQualifierList.DocumentMessageDateTime;
			dTM.DateTimePeriod.DateTimePeriod = messagingData.DateTimeStringForMessage;
			dTM.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmddhhmmss;

			GenerateGroup1(iFTERA, messagingData.LoadTerminal1StopCode, messagingData.SenderID, messagingData.SenderContactName, Core.MessagingConstants.eRouterPRAEmailAddress, messagingData.SenderPhone, messagingData.SenderFax); // Messaging Parties
			GenerateGroup3(iFTERA, messagingData.ShippingLineBookingReference, messagingData.MessageReference); // Document References
			GenerateGroup4(iFTERA); // Shipment Details

			var uNT = iFTERA.UNT.InstantiateAChildAndAddItToChildrenCollection();
			uNT.NumberOfSegmentsInTheMessage = iFTERA.CountIncludingUNT.ToString();
			uNT.MessageReferenceNumber = uNH.MessageReferenceNumber;
		}

		protected void GenerateGroup1(IFTERAMessage iFTERA, ZString recipientOneStopCode, ZString senderOneStopCode, ZString senderContactName, ZString senderEmailAddress, ZString senderPhoneNumber, ZString senderFaxNumber) // Messaging Parties
		{
			if (!recipientOneStopCode.IsEmpty)
			{
				var group1Recipient = iFTERA.Group1.InstantiateAChildAndAddItToChildrenCollection();
				var nADRecipient = group1Recipient.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nADRecipient.PartyQualifier = PartyQualifierList.MessageRecipient;
				nADRecipient.PartyIdentificationDetails.PartyIdentification = recipientOneStopCode;
			}

			var group1Sender = iFTERA.Group1.InstantiateAChildAndAddItToChildrenCollection();
			var nADSender = group1Sender.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nADSender.PartyQualifier = PartyQualifierList.DocumentMessageIssuerSender;
			nADSender.PartyIdentificationDetails.PartyIdentification = senderOneStopCode;
			GenerateGroup2(group1Sender, senderContactName, senderEmailAddress, senderPhoneNumber, senderFaxNumber); // Contact Details
		}

		protected void GenerateGroup2(SegmentGroup1 group1, ZString contactName, ZString email, ZString phone, ZString fax) // Contact Details
		{
			if (!contactName.IsEmpty)
			{
				var group2 = group1.Group2.InstantiateAChildAndAddItToChildrenCollection();

				var cTA = group2.CTA.InstantiateAChildAndAddItToChildrenCollection();
				cTA.ContactFunctionCoded = ContactFunctionCodedList.InformationContact;
				cTA.DepartmentOrEmployeeDetails.DepartmentOrEmployee = contactName;

				var cOMEmail = group2.COM.InstantiateAChildAndAddItToChildrenCollection();
				cOMEmail.CommunicationContact.CommunicationChannelQualifier = CommunicationChannelQualifierList.ElectronicMail;
				cOMEmail.CommunicationContact.CommunicationNumber = email;

				if (!phone.IsEmpty)
				{
					var cOM = group2.COM.InstantiateAChildAndAddItToChildrenCollection();
					cOM.CommunicationContact.CommunicationChannelQualifier = CommunicationChannelQualifierList.Telephone;
					cOM.CommunicationContact.CommunicationNumber = phone;
				}

				if (!fax.IsEmpty)
				{
					var cOM = group2.COM.InstantiateAChildAndAddItToChildrenCollection();
					cOM.CommunicationContact.CommunicationChannelQualifier = CommunicationChannelQualifierList.Telefax;
					cOM.CommunicationContact.CommunicationNumber = fax;
				}
			}
		}

		protected void GenerateGroup3(IFTERAMessage iFTERA, ZString shippingLineBookingReference, ZString consolNumber) // Document References
		{
			var group3 = iFTERA.Group3.InstantiateAChildAndAddItToChildrenCollection();

			if (!shippingLineBookingReference.IsEmpty)
			{
				var rFF = group3.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceQualifier = ReferenceQualifierList.BookingReferenceNumber;
				rFF.Reference.ReferenceNumber = shippingLineBookingReference;
			}

			if (!consolNumber.IsEmpty)
			{
				var rFF = group3.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceQualifier = ReferenceQualifierList.ExportersReferenceNumber;
				rFF.Reference.ReferenceNumber = consolNumber;
			}
		}

		protected void GenerateGroup4(IFTERAMessage iFTERA) // Shipment Details
		{
			var group4Inland = iFTERA.Group4.InstantiateAChildAndAddItToChildrenCollection();
			GenerateGroup4Inland(group4Inland, messagingData.CartageBookingReference, messagingData.ArrivingAtCTOByRail, messagingData.CartageCompanyABN, messagingData.TruckRegoNumber, messagingData.RoadOrig1StopCode, messagingData.RoadDest1StopCode, messagingData.RoadScheduledDeparture, messagingData.RoadScheduledArrival);

			var group4Maritime = iFTERA.Group4.InstantiateAChildAndAddItToChildrenCollection();
			GenerateGroup4Maritime(group4Maritime, messagingData.Voyage, messagingData.ShippingLine1StopCode, messagingData.LloydsNumber, messagingData.VesselName, messagingData.LoadTerminal1StopCode, messagingData.PortOfLoading, messagingData.PortOfDischarge, messagingData.PortOfFinalDischarge);

			GenerateGroup5(group4Maritime); // Container Details
		}

		protected void GenerateGroup5(SegmentGroup4 group4) // Container Details
		{
			var group5 = group4.Group5.InstantiateAChildAndAddItToChildrenCollection();

			GenerateGroup5EQD(group5, messagingData.ContainerNumber, messagingData.ISOContainerType, false, false, messagingData.IsEmptyContainer);
			GenerateGroup5HAN(group5, messagingData.Commodity1StopCode);
			GenerateGroup5NAD(group5, messagingData.ConsignorName);
			GenerateGroup5MEA(group5, PropertyMeasuredCodedList.GrossWeight, messagingData.ContainerGrossWeight);

			var requiredGenerateVGM = !messagingData.IsEmptyContainer
				&& (messagingData.GrossWeightVerifiedType == PRAConstants.ContainerGrossWeightVerificationTypes.Codes.Method1Container
				|| messagingData.GrossWeightVerifiedType == PRAConstants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages
				|| messagingData.GrossWeightVerifiedType == PRAConstants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal);

			if (requiredGenerateVGM && messagingData.GrossWeightVerifiedType != PRAConstants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal)
			{
				GenerateGroup5MEA(group5, PropertyMeasuredCodedList.VerifiedGrossWeight, messagingData.ContainerGrossWeight);
			}

			GenerateGroup5MEAHumidityAndVent(group5, messagingData.HumidityPercentage, messagingData.AirVentSetting, messagingData.AirVentSettingUnit);

			GenerateGroup5DIM(group5, messagingData.OverhangFrontInCM, messagingData.OverhangBackInCM, messagingData.OverhangLeftInCM, messagingData.OverhangRightInCM, messagingData.OverhangHeightInCM);

			GenerateGroup5SEL(group5, messagingData.SealNumber, SealingPartyCodedList.Unknown, false);
			// I've only put one but you can have up to 7. Think we only have one in Enterprise, one is fine.

			if (requiredGenerateVGM)
			{
				GenerateGroup5FTXGrossWeightVerificationType(group5,
					messagingData.GrossWeightVerifiedType,
					messagingData.GrossWeightVerifiedDateTime,
					messagingData.GrossWeightVerifiedByAddress,
					messagingData.GrossWeightVerifiedDeclarantContact,
					messagingData.GrossWeightVerifiedDeclarantSignature);
			}

			GenerateGroup5FTX(group5, messagingData.GoodsDescription);

			GenerateGroup6(group5, messagingData.ECNorCRN, messagingData.TerminalVBSBooking); // Reference Numbers

			foreach (ContainerMessagingDangerousGoods goods in messagingData.DangerousGoodsList)
			{
				GenerateGroup7(group5, goods.IMDGClass, goods.IMDGCodePage, goods.IMDGCodeVersion, goods.UNDGNumber,
					goods.FlashpointTemperatureInCelcius, goods.PackingGroup,
					goods.TechnicalName.Left(70), goods.TechnicalName.SubstringSafe(70, 70), "", "", "",
					goods.ContactName, goods.ContactEmailAddress, goods.ContactPhoneNumber, goods.ContactFaxNumber,
					goods.Weight > 0 && goods.Weight < 1 ? 1 : System.Convert.ToInt32(goods.Weight)); // Hazardous Goods
			}

			GenerateGroup10(group5, messagingData.TemperatureSettingFormatted, messagingData.IsTempControlled); // Temperature Setting - Watch Formatting!!
																												//MUST have 3 numbers and one decimal place with minus at the beginning if applicable.
																												// eg: -18.0, 12.0, -02.0 etc....

			GenerateGroup11(group5, "", messagingData.FlatRackID, messagingData.ReeferGeneratorID, messagingData.HasTynes); // Attachment Details
		}

		protected void GenerateGroup6(SegmentGroup5 group5, ZString customsExportClearanceNumber, ZString terminalVBSBookingNumber) // Reference Numbers
		{
			var group6 = group5.Group6.InstantiateAChildAndAddItToChildrenCollection();

			if (!customsExportClearanceNumber.IsEmpty)
			{
				var rFFCustoms = group6.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFFCustoms.Reference.ReferenceQualifier = ReferenceQualifierList.GoodsDeclarationNumber;
				rFFCustoms.Reference.ReferenceNumber = customsExportClearanceNumber;
			}

			if (!terminalVBSBookingNumber.IsEmpty || messagingData.Commodity1StopCode == "MTHZ")
			{
				var rFFTerminalBooking = group6.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFFTerminalBooking.Reference.ReferenceQualifier = ReferenceQualifierList.CarriersReferenceNumber;
				rFFTerminalBooking.Reference.ReferenceNumber = terminalVBSBookingNumber;
			}
		}

		protected void GenerateGroup7(SegmentGroup5 group5, ZString iMDGClass, ZString iMDGCodePage, ZString iMDGCodeVersion, ZString uNDGNumber, ZString flashPointInCelcius, ZString packingGroup, ZString hazardTechName1, ZString hazardTechName2, ZString hazardTechName3, ZString hazardTechName4, ZString hazardTechName5, ZString dGContactName, ZString dGEmail, ZString dGPhone, ZString dGFax, ZInt dGWeightInKG) // Hazardous Goods
		{
			var group7 = group5.Group7.InstantiateAChildAndAddItToChildrenCollection();

			var dGS = group7.DGS.InstantiateAChildAndAddItToChildrenCollection();
			dGS.DangerousGoodsRegulationsCoded = DangerousGoodsRegulationsCodedList.ImoImdgCode;
			dGS.HazardCode.HazardCodeIdentification = iMDGClass;
			dGS.HazardCode.HazardSubstanceItemPageNumber = iMDGCodePage;
			dGS.HazardCode.HazardCodeVersionNumber = iMDGCodeVersion;
			dGS.UndgInformation.UndgNumber = uNDGNumber;
			dGS.DangerousGoodsShipmentFlashpoint.ShipmentFlashpoint = flashPointInCelcius;
			dGS.DangerousGoodsShipmentFlashpoint.MeasureUnitQualifier = (flashPointInCelcius.IsEmpty ? "" : "CEL");
			dGS.PackingGroupCoded = PackingGroupCodedList.GetFromString(packingGroup);

			var fTX = group7.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTX.TextSubjectQualifier = TextSubjectQualifierList.DangerousGoodsTechnicalName;
			fTX.TextLiteral.FreeText1 = hazardTechName1;
			fTX.TextLiteral.FreeText2 = hazardTechName2;
			fTX.TextLiteral.FreeText3 = hazardTechName3;
			fTX.TextLiteral.FreeText4 = hazardTechName4;
			fTX.TextLiteral.FreeText5 = hazardTechName5;

			GenerateGroup8(group7, dGContactName, dGEmail, dGPhone, dGFax); // Hazardous Goods Contact
			GenerateGroup9(group7, dGWeightInKG); // Hazardous Goods Weight
		}

		protected void GenerateGroup8(SegmentGroup7 group7, ZString contactName, ZString email, ZString phone, ZString fax) // Hazardous Goods Contact
		{
			var group8 = group7.Group8.InstantiateAChildAndAddItToChildrenCollection();

			var cTA = group8.CTA.InstantiateAChildAndAddItToChildrenCollection();
			cTA.ContactFunctionCoded = ContactFunctionCodedList.DangerousGoodsContact;
			cTA.DepartmentOrEmployeeDetails.DepartmentOrEmployee = contactName;

			if (!email.IsEmpty)
			{
				var cOM = group8.COM.InstantiateAChildAndAddItToChildrenCollection();
				cOM.CommunicationContact.CommunicationChannelQualifier = CommunicationChannelQualifierList.ElectronicMail;
				cOM.CommunicationContact.CommunicationNumber = email;
			}

			if (!phone.IsEmpty)
			{
				var cOM = group8.COM.InstantiateAChildAndAddItToChildrenCollection();
				cOM.CommunicationContact.CommunicationChannelQualifier = CommunicationChannelQualifierList.Telephone;
				cOM.CommunicationContact.CommunicationNumber = phone;
			}

			if (!fax.IsEmpty)
			{
				var cOM = group8.COM.InstantiateAChildAndAddItToChildrenCollection();
				cOM.CommunicationContact.CommunicationChannelQualifier = CommunicationChannelQualifierList.Telefax;
				cOM.CommunicationContact.CommunicationNumber = fax;
			}
		}

		protected void GenerateGroup9(SegmentGroup7 group7, ZInt weightValue) // Hazardous Goods Weight
		{
			var group9 = group7.Group9.InstantiateAChildAndAddItToChildrenCollection();

			var mEA = group9.MEA.InstantiateAChildAndAddItToChildrenCollection();
			mEA.MeasurementPurposeQualifier = MeasurementPurposeQualifierList.Measurement;
			mEA.MeasurementDetails.PropertyMeasuredCoded = PropertyMeasuredCodedList.NetWeight;
			mEA.ValueRange.MeasureUnitQualifier = "KGM";
			mEA.ValueRange.MeasurementValue = weightValue.ToString();
		}

		protected void GenerateGroup10(SegmentGroup5 group5, ZString temperatureSetting, bool isTempControlled) // Temperature Details
		{
			var group10 = group5.Group10.InstantiateAChildAndAddItToChildrenCollection();

			if (isTempControlled)
			{
				var tMP = group10.TMP.InstantiateAChildAndAddItToChildrenCollection();
				tMP.TemperatureQualifier = TemperatureQualifierList.TransportControlTemperature;
				tMP.TemperatureSetting.TemperatureSetting = temperatureSetting;
				tMP.TemperatureSetting.MeasureUnitQualifier = "CEL";
			}
		}

		protected void GenerateGroup11(SegmentGroup5 group5, ZString railWagonNumber, ZString flatRackIdentifier, ZString reeferGeneratorNumber, ZBool hasForkedSupportOrTynes) // Attachment Details
		{
			var group11 = group5.Group11.InstantiateAChildAndAddItToChildrenCollection();

			if (!railWagonNumber.IsEmpty)
			{
				var eQA = group11.EQA.InstantiateAChildAndAddItToChildrenCollection();
				eQA.EquipmentQualifier = EquipmentQualifierList.RailCar;
				eQA.EquipmentIdentification.EquipmentIdentificationNumber = railWagonNumber;
			}
			if (hasForkedSupportOrTynes)
			{
				var eQA = group11.EQA.InstantiateAChildAndAddItToChildrenCollection();
				eQA.EquipmentQualifier = EquipmentQualifierList.ForkedSupport;
				eQA.EquipmentIdentification.EquipmentIdentificationNumber = "";
			}
			if (!flatRackIdentifier.IsEmpty)
			{
				var eQA = group11.EQA.InstantiateAChildAndAddItToChildrenCollection();
				eQA.EquipmentQualifier = EquipmentQualifierList.FlatRack;
				eQA.EquipmentIdentification.EquipmentIdentificationNumber = flatRackIdentifier;
			}
			if (!reeferGeneratorNumber.IsEmpty)
			{
				var eQA = group11.EQA.InstantiateAChildAndAddItToChildrenCollection();
				eQA.EquipmentQualifier = EquipmentQualifierList.ReeferGenerator;
				eQA.EquipmentIdentification.EquipmentIdentificationNumber = reeferGeneratorNumber;
			}
		}

		#endregion

		#region Group Specific Segment Generators

		protected void GenerateGroup4Inland(SegmentGroup4 group4, ZString inlandCarriersReference, ZBool isRail, ZString inlandCarriersABN, ZString truckRegoOrTrainNumber, ZString inlandOriginOneStopCode, ZString inlandDestinationOneStopCode, ZDateTime inlandDepartureTime, ZDateTime inlandArrivalTime)
		{
			var tDT = group4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tDT.TransportStageQualifier = TransportStageQualifierList.PreCarriageTransport;
			tDT.ConveyanceReferenceNumber = inlandCarriersReference;
			tDT.ModeOfTransport.ModeOfTransportCoded = (isRail ? "2" : "3");
			tDT.Carrier.CarrierIdentification = inlandCarriersABN;
			tDT.TransportIdentification.IdOfMeansOfTransportIdentification = truckRegoOrTrainNumber;

			if (!inlandOriginOneStopCode.IsEmpty)
			{
				var lOC = group4.LOC.InstantiateAChildAndAddItToChildrenCollection();
				lOC.PlaceLocationQualifier = PlaceLocationQualifierList.PlaceOfReceipt;
				lOC.RelatedLocationOneIdentification.RelatedPlaceLocationOneIdentification = inlandOriginOneStopCode;
			}

			if (!inlandDestinationOneStopCode.IsEmpty)
			{
				var lOC = group4.LOC.InstantiateAChildAndAddItToChildrenCollection();
				lOC.PlaceLocationQualifier = PlaceLocationQualifierList.PlaceOfDelivery;
				lOC.RelatedLocationOneIdentification.RelatedPlaceLocationOneIdentification = inlandDestinationOneStopCode;
			}

			if (!inlandDepartureTime.IsEmpty)
			{
				var dTM = group4.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateTimePeriodQualifier = DateTimePeriodQualifierList.DepartureDateTimeScheduled;
				dTM.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmddhhmm;
				dTM.DateTimePeriod.DateTimePeriod = inlandDepartureTime.ToString("yyyyMMddHHmm");
			}

			if (!inlandArrivalTime.IsEmpty)
			{
				var dTM = group4.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateTimePeriodQualifier = DateTimePeriodQualifierList.ArrivalDateTimeScheduled;
				dTM.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmddhhmm;
				dTM.DateTimePeriod.DateTimePeriod = inlandArrivalTime.ToString("yyyyMMddHHmm");
			}
		}

		protected void GenerateGroup4Maritime(SegmentGroup4 group4, ZString voyageNumber, ZString shippingLineOneStopCode, ZString lloydsNumber, ZString vesselName, ZString loadingTerminalOneStopCode, ZString portOfLoading, ZString portOfDischarge, ZString portOfFinalDestination)
		{
			var tDT = group4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tDT.TransportStageQualifier = TransportStageQualifierList.MainCarriageTransport;
			tDT.ConveyanceReferenceNumber = voyageNumber;
			tDT.ModeOfTransport.ModeOfTransportCoded = "1";
			tDT.Carrier.CarrierIdentification = shippingLineOneStopCode;
			tDT.TransportIdentification.IdOfMeansOfTransportIdentification = lloydsNumber;
			tDT.TransportIdentification.IdOfTheMeansOfTransport = vesselName;

			// BG - Not putting IF around this LOC as I always want it to be present, even if it's empty and rejects accordingly.
			var lOCLoading = group4.LOC.InstantiateAChildAndAddItToChildrenCollection();
			lOCLoading.PlaceLocationQualifier = PlaceLocationQualifierList.PlacePortOfLoading;
			lOCLoading.LocationIdentification.PlaceLocationIdentification = portOfLoading;
			lOCLoading.RelatedLocationOneIdentification.RelatedPlaceLocationOneIdentification = loadingTerminalOneStopCode;

			var lOCDischarge = group4.LOC.InstantiateAChildAndAddItToChildrenCollection();
			lOCDischarge.PlaceLocationQualifier = PlaceLocationQualifierList.PlacePortOfDischarge;
			lOCDischarge.LocationIdentification.PlaceLocationIdentification = portOfDischarge.IsEmpty ? portOfFinalDestination : portOfDischarge;

			var lOCFinalDest = group4.LOC.InstantiateAChildAndAddItToChildrenCollection();
			lOCFinalDest.PlaceLocationQualifier = PlaceLocationQualifierList.PlaceOfDelivery;
			lOCFinalDest.LocationIdentification.PlaceLocationIdentification = portOfFinalDestination;
		}

		protected void GenerateGroup5EQD(SegmentGroup5 group5, ZString containerNumber, ZString sMDGContainerSizeAndType, ZBool isRefrigerated, ZBool refrigeratorTurnedOn, ZBool isEmpty)
		{
			var eQD = group5.EQD.InstantiateAChildAndAddItToChildrenCollection();
			if (isRefrigerated)
			{
				if (refrigeratorTurnedOn)
				{
					eQD.EquipmentQualifier = EquipmentQualifierList.GetFromString("T75");
				}
				else
				{
					eQD.EquipmentQualifier = EquipmentQualifierList.GetFromString("T76");
				}
			}
			else
			{
				eQD.EquipmentQualifier = EquipmentQualifierList.Container;
			}
			eQD.EquipmentIdentification.EquipmentIdentificationNumber = containerNumber;
			eQD.EquipmentSizeAndType.EquipmentSizeAndTypeIdentification = EquipmentSizeAndTypeIdentificationList.GetFromString(sMDGContainerSizeAndType);
			eQD.EquipmentStatusCoded = EquipmentStatusCodedList.Export;
			eQD.FullEmptyIndicatorCoded = (isEmpty ? FullEmptyIndicatorCodedList.Empty : FullEmptyIndicatorCodedList.Full);
		}

		protected void GenerateGroup5HAN(SegmentGroup5 group5, ZString oneStopCommodityCode)
		{
			var hAN = group5.HAN.InstantiateAChildAndAddItToChildrenCollection();
			hAN.HandlingInstructions.HandlingInstructions = oneStopCommodityCode;
		}

		protected void GenerateGroup5NAD(SegmentGroup5 group5, ZString consignorName)
		{
			var nAD = group5.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyQualifier = PartyQualifierList.Consignor;
			nAD.PartyIdentificationDetails.PartyIdentification = consignorName;
		}

		protected void GenerateGroup5MEA(SegmentGroup5 group5, PropertyMeasuredCodedList weightType, ZDecimal weightValue)
		{
			if (!weightValue.IsEmpty || weightType.ToString() == PropertyMeasuredCodedList.GrossWeight.ToString())
			{
				var mEA = group5.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mEA.MeasurementPurposeQualifier = MeasurementPurposeQualifierList.Measurement;
				mEA.MeasurementDetails.PropertyMeasuredCoded = weightType;
				mEA.ValueRange.MeasureUnitQualifier = "KGM";
				mEA.ValueRange.MeasurementValue = weightValue.ToString(0);
			}
		}

		protected void GenerateGroup5SEL(SegmentGroup5 group5, ZString sealNumber, SealingPartyCodedList sealingParty, ZBool sealIsDamaged)
		{
			var sEL = group5.SEL.InstantiateAChildAndAddItToChildrenCollection();
			sEL.SealNumber = sealNumber;
			sEL.SealIssuer.SealingPartyCoded = sealingParty;
			sEL.SealConditionCoded = (sealIsDamaged ? SealConditionCodedList.InRightCondition : SealConditionCodedList.InRightCondition);
		}

		protected void GenerateGroup5FTXGrossWeightVerificationType(SegmentGroup5 group5, ZString grossWeightVerifiedType, ZDateTime grossWeightVerifiedDateTime, string grossWeightVerifiedByAddress, string grossWeightVerifiedDeclarant, string grossWeightVerifiedDeclarantSignature)
		{
			var fTX = group5.FTX.InstantiateAChildAndAddItToChildrenCollection();

			fTX.TextSubjectQualifier = TextSubjectQualifierList.GetFromString("AAY");
			fTX.TextReference.CodeListQualifier = CodeListQualifierList.VerifiedGrossWeight;
			fTX.TextReference.FreeTextIdentification = grossWeightVerifiedType;

			if (grossWeightVerifiedType != PRAConstants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal)
			{
				fTX.TextLiteral.FreeText1 = grossWeightVerifiedDateTime.ToDateTime().ToUniversalTime().ToString("yyyyMMddHHmmUTC", CultureInfo.InvariantCulture);
			}

			fTX.TextLiteral.FreeText2 = grossWeightVerifiedByAddress;
			fTX.TextLiteral.FreeText3 = grossWeightVerifiedDeclarant;
			fTX.TextLiteral.FreeText4 = grossWeightVerifiedDeclarantSignature;
		}

		protected void GenerateGroup5FTX(SegmentGroup5 group5, ZString goodsDescription)
		{
			if (!goodsDescription.IsEmpty)
			{
				var fTX = group5.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectQualifier = TextSubjectQualifierList.GetFromString("ZO1");
				fTX.TextLiteral.FreeText1 = goodsDescription.Left(26);
			}
		}

		protected void GenerateGroup5MEAHumidityAndVent(SegmentGroup5 group5, ZInt humidityPercentage, ZInt ventSettingValue, ZString ventSettingUnit)
		{
			if (!humidityPercentage.IsEmpty)
			{
				var mEA = group5.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mEA.MeasurementPurposeQualifier = MeasurementPurposeQualifierList.Measurement;
				mEA.MeasurementDetails.PropertyMeasuredCoded = PropertyMeasuredCodedList.Humidity;
				mEA.ValueRange.MeasureUnitQualifier = "P1"; // Percent
				mEA.ValueRange.MeasurementValue = humidityPercentage.ToString();
			}

			if (!ventSettingValue.IsEmpty)
			{
				var mEA = group5.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mEA.MeasurementPurposeQualifier = MeasurementPurposeQualifierList.Measurement;
				mEA.MeasurementDetails.PropertyMeasuredCoded = PropertyMeasuredCodedList.AirFlow;
				mEA.ValueRange.MeasureUnitQualifier = ventSettingUnit; // 2L-Cubic Feet/Minute, MQH-CubicMetres/Hour, P1-Percent
				mEA.ValueRange.MeasurementValue = ventSettingValue.ToString();
			}
		}

		protected void GenerateGroup5DIM(SegmentGroup5 group5, ZInt overhangFrontInCM, ZInt overhangBackInCM, ZInt overhangLeftInCM, ZInt overhangRightInCM, ZInt overhangHeightInCM)
		{
			if (!overhangFrontInCM.IsEmpty || !overhangBackInCM.IsEmpty || !overhangLeftInCM.IsEmpty || !overhangRightInCM.IsEmpty || !overhangHeightInCM.IsEmpty)
			{
				var dIM = group5.DIM.InstantiateAChildAndAddItToChildrenCollection();
				dIM.DimensionQualifier = DimensionQualifierList.OffStandardDimensionGeneral;
				dIM.Dimensions.MeasureUnitQualifier = "CMT";
				dIM.Dimensions.OverhangFront = overhangFrontInCM.ToString();
				dIM.Dimensions.OverhangLeft = overhangLeftInCM.ToString();
				dIM.Dimensions.OverhangHeight = overhangHeightInCM.ToString();
				dIM.Dimensions.OverhangBack = overhangBackInCM.ToString();
				dIM.Dimensions.OverhangRight = overhangRightInCM.ToString();
			}
		}

		#endregion

		#region Implementation

		protected IPRAMessagingData messagingData;
		protected PRAMessageTypeConstants.MessageType messageType;

		protected IFTERAMessage iFTERA;

		protected bool generated;
		protected void GenerateIfNotAlreadyGenerated()
		{
			if (!generated)
			{
				GenerateGroup0();
				generated = true;
			}
		}

		#endregion
	}
}
