using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class CertificateRequestMessageBuilder
	{
		public CertificateRequestMessageBuilder(ICertificateRequestData requestData)
		{
			this.requestData = requestData;
		}

		public RFPMessage GenerateMessage()
		{
			PopulateSANCRTMessage();
			var rfpMessage = requestData.Object.Factory.New<RFPMessage>();
			rfpMessage.EM_LinkedObject = requestData.Object;
			rfpMessage.EM_MessageType = EXDOCMessageTypeCodes.Descriptions.CRQ;
			rfpMessage.EM_MessageText = message.ToString(new UNOBCharacterSet());
			return rfpMessage;
		}

		#region Implementation

		void PopulateSANCRTMessage()
		{
			#region UNH

			EXDOCMessageUtilities.PopulateUNH(
				message.UNH.InstantiateAChildAndAddItToChildrenCollection(),
				MessageTypeList.InternationalMovementOfGoodsGovernmentalRegulatoryMessage,
				MessageVersionNumberList.DraftVersionUnEdifactDirectory,
				MessageReleaseNumberList.Release1997B,
				ControllingAgencyList.UnEceTradeWp4,
				"CR0801");

			#endregion

			#region Certificate Identification

			EXDOCMessageUtilities.PopulateBGM(
				message.BGM.InstantiateAChildAndAddItToChildrenCollection(),
				DocumentMessageNameCodedList.GetFromString(requestData.CommodityType),
				CodeListResponsibleAgencyCodedList.GetFromString("AQ"),
				string.Empty,
				string.Empty,
				MessageFunctionCodedList.GetFromString(EXDOCCertificateReasonCodes.Codes.CertificateRequest),
				ResponseTypeCodedList.GetFromString(string.Empty));

			#endregion

			#region Discharge Port

			if (!requestData.DischargePort.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateLOC(
					message.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					PlaceLocationQualifierList.PortOfDischarge,
					requestData.DischargePort);
			}

			#endregion

			#region Destination City

			EXDOCMessageUtilities.PopulateLOC(
				message.LOC.InstantiateAChildAndAddItToChildrenCollection(),
				PlaceLocationQualifierList.PlaceOfDestination,
				requestData.DestinationCity);

			#endregion

			#region Certificate Required Location

			EXDOCMessageUtilities.PopulateLOC(
				message.LOC.InstantiateAChildAndAddItToChildrenCollection(),
				PlaceLocationQualifierList.PlaceOfDocumentIssue,
				requestData.CertificateRequiredLocation);

			#endregion

			#region Exporter Certificate Reference

			EXDOCMessageUtilities.PopulateRFF(
				message.RFF.InstantiateAChildAndAddItToChildrenCollection(),
				ReferenceQualifierList.DeclarantsReferenceNumber,
				requestData.ExporterCertificateReference);

			#endregion

			#region Notify Party Text

			const int maxTextChars = 27225;

			if (!requestData.NotifyPartyText.IsEmpty)
			{
				const int maxNotifyElementLength = 55;

				EXDOCMessageUtilities.PopulateFTX(
					message,
					TextSubjectQualifierList.PartyInstructions,
					maxTextChars,
					maxNotifyElementLength,
					requestData.NotifyPartyText);
			}

			#endregion Letter of Credit Text

			#region Letter of Credit Text

			if (!requestData.LetterOfCreditText.IsEmpty)
			{
				const int maxLetterOfCreditElementLength = 70;

				EXDOCMessageUtilities.PopulateFTX(
					message,
					TextSubjectQualifierList.LetterOfCreditInformation,
					maxTextChars,
					maxLetterOfCreditElementLength,
					requestData.LetterOfCreditText);
			}

			#endregion

			#region Separate Certificate Container Indicator

			if (requestData.SeparateCertificateContainerInd)
			{
				EXDOCMessageUtilities.PopulateGIS(message, "Y", "SC");
			}

			#endregion

			#region Separate Certificate Marks Indicator

			if (requestData.SeparateCertificateMarksInd)
			{
				EXDOCMessageUtilities.PopulateGIS(message, "Y", "SM");
			}

			#endregion

			#region Separate Certificate Packer Indicator

			if (requestData.SeparateCertificatePackerInd)
			{
				EXDOCMessageUtilities.PopulateGIS(message, "Y", "SP");
			}

			#endregion

			#region Import Permits

			foreach (IImportPermit permit in requestData.ImportPermits)
			{
				SegmentGroup1 group1 = message.Group1.InstantiateAChildAndAddItToChildrenCollection();

				#region Import Permit Number

				EXDOCMessageUtilities.PopulateDOC(
					group1.DOC.InstantiateAChildAndAddItToChildrenCollection(),
					DocumentMessageNameCodedList.ImportLicence,
					permit.PermitNumber);

				#endregion

				#region Import Permit Date

				if (!permit.PermitDate.IsEmpty)
				{
					EXDOCMessageUtilities.PopulateDTM(
						group1.DTM.InstantiateAChildAndAddItToChildrenCollection(),
						DateTimePeriodQualifierList.DocumentMessageDateTime,
						permit.PermitDate.ToString("yyyyMMdd"),
						DateTimePeriodFormatQualifierList.Ccyymmdd);
				}

				#endregion
			}

			#endregion

			#region Owner Exporter Number

			SegmentGroup2 group2 = message.Group2.InstantiateAChildAndAddItToChildrenCollection();

			EXDOCMessageUtilities.PopulatePNA(
				group2.PNA.InstantiateAChildAndAddItToChildrenCollection(),
				PartyQualifierList.Exporter,
				requestData.OwnerExporterNumber);

			#endregion

			#region Consignee Details

			if (requestData.Consignee != null)
			{
				#region Consignee Name

				EXDOCMessageUtilities.PopulatePNA(
					group2.PNA.InstantiateAChildAndAddItToChildrenCollection(),
					PartyQualifierList.Consignee,
					requestData.Consignee.GetCustomsClientID(),
					NameComponentQualifierList.WholeName,
					requestData.Consignee.OH_FullNameTruncated);

				#endregion

				#region Consignee Address

				if (!requestData.Consignee.MainAddress.AddressAsASingleLineWithoutCompanyName.IsEmpty)
				{
					ZString combinedAddress = GetCombinedAddress(requestData.Consignee);

					EXDOCMessageUtilities.PopulateADR(
						group2.ADR.InstantiateAChildAndAddItToChildrenCollection(),
						AddressFormatCodedList.UnstructuredAddress,
						combinedAddress.SubstringSafe(0, 35),
						combinedAddress.SubstringSafe(35, 35),
						!requestData.Consignee.CityFallback.IsEmpty ? requestData.Consignee.CityFallback : (ZString)"UNKNOWN",
						requestData.Consignee.MainAddress.OA_PostCode,
						requestData.Consignee.UNLOCO != null ? requestData.Consignee.UNLOCO.RL_RN_NKCountryCode : ZString.Empty,
						requestData.Consignee.MainAddress.OA_State);
				}

				#endregion

				#region Consignee EU TRACES Approval ID

				ZString eUTracesApprovalID = requestData.Consignee.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.EUTracesID, Core.Constants.CountryCodes.Australia);
				if (!eUTracesApprovalID.IsEmpty)
				{
					EXDOCMessageUtilities.PopulateGroup2FTX(
						group2.FTX.InstantiateAChildAndAddItToChildrenCollection(),
						TextSubjectQualifierList.MutuallyDefined,
						eUTracesApprovalID);
				}

				#endregion

				#region Consignee Contact Details

				if (!requestData.Consignee.MainAddress.OA_Phone.IsEmpty)
				{
					SegmentGroup3 group3 = group2.Group3.InstantiateAChildAndAddItToChildrenCollection();

					EXDOCMessageUtilities.PopulateCTA(
						group3.CTA.InstantiateAChildAndAddItToChildrenCollection(),
						ContactFunctionCodedList.Consignee,
						string.Empty);

					EXDOCMessageUtilities.PopulateCOM(
						group3.COM.InstantiateAChildAndAddItToChildrenCollection(),
						CommunicationChannelQualifierList.Telephone,
						requestData.Consignee.MainAddress.OA_Phone);
				}

				#endregion
			}

			#endregion

			#region Consignee Representative Name

			if (requestData.Forwarder != null)
			{
				SegmentGroup3 group3 = group2.Group3.InstantiateAChildAndAddItToChildrenCollection();

				EXDOCMessageUtilities.PopulateCTARepName(
					group3.CTA.InstantiateAChildAndAddItToChildrenCollection(),
					ContactFunctionCodedList.Agent,
					requestData.Forwarder.OH_FullNameTruncated);
			}

			#endregion

			#region Transport Details

			SegmentGroup4 group4 = null;

			if (!requestData.TransportMode.IsEmpty)
			{
				group4 = message.Group4.InstantiateAChildAndAddItToChildrenCollection();

				EXDOCMessageUtilities.PopulateTDT(
					group4.TDT.InstantiateAChildAndAddItToChildrenCollection(),
					TransportStageQualifierList.AtDeparture,
					requestData.VoyageFlightNumber,
					requestData.TransportMode,
					requestData.CarrierName,
					requestData.VesselName);
			}

			#endregion

			#region Departure Date

			if (!requestData.DepartureDate.IsEmpty)
			{
				group4 = group4 ?? message.Group4.InstantiateAChildAndAddItToChildrenCollection();

				EXDOCMessageUtilities.PopulateDTM(
					group4.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.DepartureDateTime,
					requestData.DepartureDate.ToString("yyyyMMdd"),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}

			#endregion

			#region Certificate Lines

			foreach (ICertificateLine line in requestData.CertificateLines)
			{
				PoulateCertificateLine(line);
			}

			#endregion

			#region UNT

			SegmentGroup21 group21 = message.Group21.InstantiateAChildAndAddItToChildrenCollection();

			EXDOCMessageUtilities.PopulateUNT(
				group21.UNT.InstantiateAChildAndAddItToChildrenCollection(),
				message.CountIncludingUNT.ToString());

			#endregion
		}

		void PoulateCertificateLine(ICertificateLine certificateLine)
		{
			SegmentGroup11 group11 = message.Group11.InstantiateAChildAndAddItToChildrenCollection();

			#region Line Identifier

			EXDOCMessageUtilities.PopulateLIN(
				group11.LIN.InstantiateAChildAndAddItToChildrenCollection(),
				certificateLine.LineNumber.ToString());

			#endregion

			#region Line Net Quantity

			EXDOCMessageUtilities.PopulateMEA(
				group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
				MeasurementPurposeQualifierList.LineItemMeasurement,
				PropertyMeasuredCodedList.ShippedQuantity,
				certificateLine.NetQuantityUnit,
				certificateLine.NetQuantity.ToString());

			#endregion

			#region Product Code

			EXDOCMessageUtilities.PopulatePIA(
				group11.PIA.InstantiateAChildAndAddItToChildrenCollection(),
				ProductIdFunctionQualifierList.ProductIdentification,
				certificateLine.ProductCode,
				ItemNumberTypeCodedList.IndustryCommodityCode);

			#endregion

			#region Exporter Defined Product Description

			if (!certificateLine.ProductDescription.IsEmpty)
			{
				const string userDefinedCertificate = "UHC";

				EXDOCMessageUtilities.PopulateIMD(
					group11.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					userDefinedCertificate,
					certificateLine.ProductDescription);
			}

			#endregion

			#region Additional Product Description

			if (!certificateLine.AdditionalProductDescription.IsEmpty)
			{
				const string additional = "AD";

				EXDOCMessageUtilities.PopulateIMD(
					group11.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					additional,
					certificateLine.AdditionalProductDescription);
			}

			#endregion

			#region Extra Certificate

			if (certificateLine.ExtraCertificates.Length > 0)
			{
				SegmentGroup12 group12 = group11.Group12.InstantiateAChildAndAddItToChildrenCollection();

				foreach (ZString extraCertificate in certificateLine.ExtraCertificates)
				{
					EXDOCMessageUtilities.PopulateDOC(
						group12.DOC.InstantiateAChildAndAddItToChildrenCollection(),
						DocumentMessageNameCodedList.SanitaryCertificate,
						extraCertificate,
						string.Empty,
						DocumentMessageStatusCodedList.ToBePrinted);
				}
			}

			#endregion

			#region Packaging Details

			SegmentGroup15 group15 = group11.Group15.InstantiateAChildAndAddItToChildrenCollection();

			EXDOCMessageUtilities.PopulatePAC(
				group15.PAC.InstantiateAChildAndAddItToChildrenCollection(),
				certificateLine.PackQuantity.ToString(),
				PackagingLevelCodedList.Outer,
				certificateLine.PackType);

			#endregion

			#region RFP Numbers

			foreach (IRFPNumber rfpNumber in certificateLine.RFPNumbers)
			{
				PopulateRFPNumber(group11, rfpNumber);
			}

			#endregion
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		void PopulateRFPNumber(SegmentGroup11 group11, IRFPNumber rfpNumber)
		{
			SegmentGroup22 group22 = group11.Group22.InstantiateAChildAndAddItToChildrenCollection();

			#region RFP Number

			EXDOCMessageUtilities.PopulateRFF(
				group22.RFF.InstantiateAChildAndAddItToChildrenCollection(),
				ReferenceQualifierList.DocumentNumber,
				rfpNumber.RFPNumber);

			#endregion

			#region RFP Lines

			foreach (IRFPLine rfpLine in rfpNumber.RFPLines)
			{
				SegmentGroup23 group23 = group22.Group23.InstantiateAChildAndAddItToChildrenCollection();

				#region RFP Line Number

				LINSegment lin = group23.LIN.InstantiateAChildAndAddItToChildrenCollection();
				lin.LineItemNumber = rfpLine.RFPLineNumber.ToString();
				lin.SubLineInformation.SubLineIndicatorCoded = SubLineIndicatorCodedList.SubLineInformation;

				#endregion

				#region RFP Line Net Quantity

				EXDOCMessageUtilities.PopulateMEA(
					group23.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.LineItemMeasurement,
					PropertyMeasuredCodedList.ShippedQuantity,
					rfpLine.NetQuantityUnit,
					rfpLine.NetLineQuantity.ToString());

				#endregion

				#region RFP Line Packaging Details

				EXDOCMessageUtilities.PopulatePAC(
					group23.PAC.InstantiateAChildAndAddItToChildrenCollection(),
					rfpLine.PackQuantity.ToString(),
					PackagingLevelCodedList.Outer,
					rfpLine.PackType);

				#endregion

				#region Containers

				foreach (IRFPContainer container in rfpLine.Containers)
				{
					SegmentGroup24 group24 = group23.Group24.InstantiateAChildAndAddItToChildrenCollection();

					EXDOCMessageUtilities.PopulateEQD(
						group24.EQD.InstantiateAChildAndAddItToChildrenCollection(),
						EquipmentQualifierList.Container,
						container.ContainerNumber);

					if (!container.Seal.IsEmpty)
					{
						EXDOCMessageUtilities.PopulateSEL(
							group24.SEL.InstantiateAChildAndAddItToChildrenCollection(),
							container.Seal, "", "");
					}
				}

				#endregion
			}

			#endregion
		}

		ZString GetCombinedAddress(OrgHeader organization)
		{
			var combinedAddress = ZString.Empty;
			if (organization.MainAddress.OA_Address1.Length > 1 || !organization.MainAddress.OA_Address2.IsEmpty)
			{
				combinedAddress = organization.MainAddress.OA_Address1 + " " + requestData.Consignee.MainAddress.OA_Address2;
			}
			return combinedAddress.Trim();
		}

		readonly SANCRTMessage message = new SANCRTMessage();
		readonly ICertificateRequestData requestData;

		#endregion
	}
}
