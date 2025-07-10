
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.Business.MessageInterpretation;
	using Enterprise.Customs.Business.MessageProcessors;
	using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Edifact.D11B.Elements;
	using Enterprise.Freight.CFS.Business;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business.CustomValues;
	using Enterprise.ZArchitecture.Modules;

	[ApplicationIdentifier(ServiceOptions.Codes.HouseBillOfLading)]
	[WTG.StaticAnalysis.Annotation.CodeAlive("Is used by CA custom")]
	public class ManifestForwardHouseBill : ResponseMessageProcessor
	{
		public ManifestForwardHouseBill(LoggingInformation logger)
			: base(logger, MessageTypeList.Codes.ACIHouseBill, Res.GetString("FFC3F86B-EF1B-4277-9DFC-1A28E8336852", "Manifest Forward House Bill"))
		{
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			ediMessage.EM_MessageText = ediMessage.EM_MessageText.Replace("'CTA+AH'EQD+", "'EQD+").Replace("'CTA+AH'TDT+", "'TDT+").Replace("'CTA+AH'RCS+", "'RCS+");// todo: remove this when customs fix their sytax error

			var message = ediMessage.Factory.Load<ACIHouseBillMessage>(ediMessage.PK);
			if (message != null && message.GOVCBR != null)
			{
				var wrapper = new ManifestForwardHouseBillWrapper(message);
				var errorText = SetLinkedObject(wrapper);
				message.EM_MessageSubType = MessageTypeList.Codes.ManifestForwardHouse;
				message.EM_ApplicationReference = wrapper.HouseBillCCN.Replace(" ", "");
				var subject = Res.GetString("477E1329-F047-4B3B-B2AF-5D8A0972929F", "Manifest Forward Message for House Bill CCN {0}", wrapper.HouseBillCCN);
				var emailBuilder = new EmailDefBuilder(subject, message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.FreeFormResponse);
				emailBuilder.AddArgReplacement(EmailDefBuilder.GetJobLink(message, message.EM_MessageSubTypeDescription));
				emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, " " + MessageSender.TrimEnd());
				if (!string.IsNullOrEmpty(errorText))
				{
					emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, "<strong><large>" + errorText + "</strong></large>");
				}
				emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, GetMessageInterpretation(wrapper));
				message.EM_MessageInterpretation = emailBuilder.ToString();

				var notificationHelper = new ResponseNotificationHelper(ediMessage.Factory, wrapper);
				if (message.EM_LinkUniqueID.IsEmpty && notificationHelper.NotificationBranch != null)
				{
					message.EM_GB = notificationHelper.NotificationBranch.PK;
				}
				var groups = notificationHelper.NotificationEmailGroups;
				if (groups == null || !groups.Any())
				{
					Logger.LogWarning(string.Format("No notification group could be found for this Manifest Forward House Bill message with CCN {0} and SNP type {1}", wrapper.HouseBillCCN, wrapper.SNPType));
				}
				else
				{
					var email = emailBuilder.ToEmail();
					foreach (var group in groups)
					{
						CopyGroupToEmails(null, email, group);
					}
					SendReport(email);
				}
				message.PrimaryCCN = wrapper.PrimaryCCN;
				message.SetSystemDefinedValue(EDIMessage.Schema.SNPType, (ZString)wrapper.SNPType);
				message.SetSystemDefinedValue(EDIMessage.Schema.CBSAOffice, (ZString)wrapper.PortOfDestination);
				message.SetSystemDefinedValue(EDIMessage.Schema.SubLocation, (ZString)wrapper.SubLocationOfDestination);
			}
			else
			{
				throw new UnableToInterpretMessageException(ediMessage, this);
			}
			return EDIMessage.Status.Received;
		}

		string SetLinkedObject(ManifestForwardHouseBillWrapper wrapper)
		{
			var result = string.Empty;
			var houseCCN = wrapper.HouseBillCCN;
			if (!houseCCN.IsEmpty)
			{
				if (wrapper.SNPType == SecondaryNotifyPartyTypeList.Codes.CustomsBroker)
				{
					result = LinkToDeclaration(houseCCN, wrapper);
					if (!wrapper.WrappedMessage.EM_LinkUniqueID.IsValid)
					{
						result += LinkToShipment(houseCCN, wrapper);
					}
				}
				else
				{
					result = LinkToShipment(houseCCN, wrapper);
					if (!wrapper.WrappedMessage.EM_LinkUniqueID.IsValid)
					{
						result += LinkToDeclaration(houseCCN, wrapper);
					}
				}
			}
			else
			{
				result = Res.GetString("1d81d5fb-e7c0-4e53-ac4d-c61399e84b37", "No house CCN was included in this Manifest Forward message");
			}
			return result;
		}

		string LinkToDeclaration(string ccn, ManifestForwardHouseBillWrapper wrapper)
		{
			var result = string.Empty;
			var foundObjects = new ForwardedManifestSupporter(wrapper.WrappedMessage).GetMatchingDeclarations();
			if (foundObjects.Length > 0)
			{
				wrapper.WrappedMessage.EM_LinkedObject = foundObjects[0].GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease);
			}

			if (foundObjects.Length == 1)
			{
				result = Res.GetString("434E9796-3497-46AB-8327-1D78BAE45F25", "Matching declaration found for this CCN ({0}), with transaction number: {1}", ccn,
					DeclarationLink(foundObjects[0]));
			}
			else if (foundObjects.Length > 1)
			{
				result = Res.GetString("d75502ec-4137-447e-953e-33df4ca1f30b", "Multiple matching declarations found for this CCN ({0}), with transaction numbers: {1}", ccn,
					new ZStringBuilder(foundObjects.Select(x => DeclarationLink(x))).ToStringWithDelimiterBetweenAppends(","));
			}
			else if (wrapper.SNPType == SecondaryNotifyPartyTypeList.Codes.CustomsBroker)
			{
				result = Res.GetString("85e185bc-a18e-4d0a-ada5-16344d186b8b", "No matching declaration could be found for this Customs Broker Manifest Forward message with CCN ({0}). ", ccn);
			}

			return result;
		}

		string DeclarationLink(JobDeclaration declaration)
		{
			return EmailDefBuilder.GetJobLink(declaration, declaration.TransactionNumber.ToString());
		}

		string LinkToShipment(string ccn, ManifestForwardHouseBillWrapper wrapper)
		{
			var result = string.Empty;
			var foundObjects = new ForwardedManifestSupporter(wrapper.WrappedMessage).GetMatchingShipments();

			if (foundObjects.Length > 0)
			{
				wrapper.WrappedMessage.EM_LinkedObject = foundObjects[0];
			}

			if (foundObjects.Length == 1)
			{
				result = Res.GetString("1409FAD4-8363-498E-8EEF-82E23236CEED", "Matching shipment found for this CCN ({0}), with shipment number: {1}", ccn,
					ShipmentLink(foundObjects[0]));
			}
			else if (foundObjects.Length > 1)
			{
				result = Res.GetString("BADB0EB6-DA89-437A-B3B3-36D162916734", "Multiple matching shipments found for this CCN ({0}), with shipment numbers: {1}", ccn,
					new ZStringBuilder(foundObjects.Select(x => ShipmentLink(x))).ToStringWithDelimiterBetweenAppends(","));
			}
			else if (wrapper.SNPType != SecondaryNotifyPartyTypeList.Codes.CustomsBroker)
			{
				result = Res.GetString("36400F2D-E0B3-489D-99D7-C733CBFE1558", "No matching declaration or shipment could be found for this Non-Customs Broker Manifest Forward message with CCN ({0})", ccn);
			}

			return result;
		}

		string ShipmentLink(ForwardingShipment shipment)
		{
			IControllerIDProvider controllerIDProvider = shipment;
			if (shipment.JS_UniqueConsignRef.StartsWith("H"))
			{
				controllerIDProvider = (IControllerIDProvider)shipment.Factory.Load<CFSShipment>(shipment.PK) ?? shipment;
			}

			return EmailDefBuilder.GetJobLink(controllerIDProvider, shipment.JS_UniqueConsignRef);
		}

		static string GetMessageInterpretation(ManifestForwardHouseBillWrapper wrapper)
		{
			var interpretation = new MessageInterpretation(wrapper.WrappedMessage.GOVCBR, new CACharSet());

			if (wrapper.UNHSegment != null)
			{
				interpretation.AddUNHInterpretation(wrapper.UNHSegment, wrapper.UNHSegment.MessageReferenceNumber);
			}

			if (wrapper.BGMSegment != null)
			{
				var bgmInterpretation = interpretation.AddNewSegmentInterpretation(wrapper.BGMSegment);
				bgmInterpretation.AddElementInterpretation(() => new ServiceOptions().GetDescriptionFromCode(wrapper.DocumentName));
				bgmInterpretation.AddElementInterpretation(() => wrapper.HouseBillCCN);
				bgmInterpretation.AddElementInterpretation("Message Function", GetMessageFunctionDescription(wrapper.MessageFunction));
			}

			if (wrapper.RFFSNPSegment != null)
			{
				var rffInterpretation = interpretation.AddNewSegmentInterpretation(wrapper.RFFSNPSegment);
				rffInterpretation.AddElementInterpretation(() => wrapper.SnpIdentifier);
				rffInterpretation.AddElementInterpretation("Snp Type", new SecondaryNotifyPartyTypeList().GetDescriptionFromCode(wrapper.SNPType));
			}

			if (wrapper.RFFUCNSegment != null)
			{
				interpretation.AddNewSegmentInterpretation(wrapper.RFFUCNSegment, () => wrapper.UCN);
			}

			if (wrapper.DOCMovementTypeSegment != null)
			{
				interpretation.AddNewSegmentInterpretation(wrapper.DOCMovementTypeSegment, () => new eMHMovementTypeList().GetDescriptionFromCode(wrapper.MovementType));
			}

			if (wrapper.DOCPrimaryCCNSegment != null)
			{
				interpretation.AddNewSegmentInterpretation(wrapper.DOCPrimaryCCNSegment, () => wrapper.PrimaryCCN);
			}

			if (wrapper.Group13 != null)
			{
				if (wrapper.RCSG13Segment != null)
				{
					interpretation.AddNewSegmentInterpretation(wrapper.RCSG13Segment);
				}

				if (wrapper.FTXB2BSegment != null)
				{
					interpretation.AddNewSegmentInterpretation(wrapper.FTXB2BSegment, () => wrapper.B2BComments);
				}
			}

			if (wrapper.AJTSegment != null)
			{
				interpretation.AddNewSegmentInterpretation(wrapper.AJTSegment, () => wrapper.AmendmentReason);
			}

			if (wrapper.TDTSegment != null)
			{
				interpretation.AddNewSegmentInterpretation(wrapper.TDTSegment, () => new CBSATransportTypeList().GetCodeFromDescription(wrapper.ModeOfTransport));
			}

			if (wrapper.UNS1Segment != null)
			{
				interpretation.AddUNS1Interpretation(wrapper.UNS1Segment);
			}

			if (wrapper.HYNSegment != null)
			{
				interpretation.AddNewSegmentInterpretation(wrapper.HYNSegment);
			}

			if (wrapper.Group138 != null)
			{
				if (wrapper.CNISegment != null)
				{
					interpretation.AddNewSegmentInterpretation(wrapper.CNISegment);
				}

				if (wrapper.STSSegment != null)
				{
					interpretation.AddNewSegmentInterpretation(wrapper.STSSegment, () => wrapper.ConsolidationIndicator == "1" ? "Yes" : "No");
				}

				if (wrapper.MEASegment != null)
				{
					var meaInterpretation = interpretation.AddNewSegmentInterpretation(wrapper.MEASegment);
					meaInterpretation.AddElementInterpretation(() => wrapper.Volume);
					meaInterpretation.AddElementInterpretation(() => new CustomsUnitOfMeasureList().GetDescriptionFromCode(wrapper.VolumeUnits));
				}

				if (wrapper.HANSegment != null)
				{
					interpretation.AddNewSegmentInterpretation(wrapper.HANSegment, () => wrapper.SpecialInstructions);
				}

				foreach (INameAndAddress nameAndAddress in wrapper.NamesAndAddresses)
				{
					ProcessOneNameAndAddress(nameAndAddress, interpretation);
				}

				if (wrapper.LOCDestinationSegment != null)
				{
					var locDestinationInterpretation = interpretation.AddNewSegmentInterpretation(wrapper.LOCDestinationSegment);
					locDestinationInterpretation.AddElementInterpretation(() => wrapper.PortOfDestination);
					locDestinationInterpretation.AddElementInterpretation(() => wrapper.SubLocationOfDestination);
				}

				if (wrapper.LOCDischargeSegment != null)
				{
					var locDischargeInterpretation = interpretation.AddNewSegmentInterpretation(wrapper.LOCDischargeSegment);
					locDischargeInterpretation.AddElementInterpretation(() => wrapper.PortOfDischarge);
					locDischargeInterpretation.AddElementInterpretation(() => wrapper.SubLocationOfDischarge);
				}

				if (wrapper.Group142 != null)
				{
					if (wrapper.DOCTriggerSegment != null)
					{
						interpretation.AddNewSegmentInterpretation(wrapper.DOCTriggerSegment);
					}

					if (wrapper.Group143 != null)
					{
						var consolidator = wrapper.Consolidator;
						ProcessOneNameAndAddress(consolidator, interpretation);
					}
				}

				if (wrapper.Group146 != null)
				{
					if (wrapper.FTXDGSegment != null)
					{
						interpretation.AddNewSegmentInterpretation(wrapper.FTXDGSegment, () => wrapper.DangerousGoodsSpecialInstructions);
					}
				}

				foreach (IContainerAndSeals containerAndSeals in wrapper.ContainersAndSeals)
				{
					if (containerAndSeals.EQDSegment != null)
					{
						interpretation.AddNewSegmentInterpretation(containerAndSeals.EQDSegment, () => containerAndSeals.ContainerNumber);
					}

					if (containerAndSeals.SEQSegment != null)
					{
						interpretation.AddNewSegmentInterpretation(containerAndSeals.SEQSegment);
					}

					foreach (IContainerSeal containerSeal in containerAndSeals.ContainerSeals)
					{
						if (containerSeal.SELSegment != null)
						{
							interpretation.AddNewSegmentInterpretation(containerSeal.SELSegment, () => containerSeal.SealNumber);
						}

						if (containerSeal.SEQSegment != null)
						{
							interpretation.AddNewSegmentInterpretation(containerSeal.SEQSegment);
						}
					}
				}

				if (wrapper.Group161 != null)
				{
					if (wrapper.TDTTriggerSegment != null)
					{
						interpretation.AddNewSegmentInterpretation(wrapper.TDTTriggerSegment);
					}
				}

				foreach (IConsignmentLine consignmentLine in wrapper.ConsignmentLines)
				{
					if (consignmentLine.SEQG177Segment != null)
					{
						interpretation.AddNewSegmentInterpretation(consignmentLine.SEQG177Segment, "Consignment", "Pack Line");
					}

					if (consignmentLine.Group193 != null)
					{
						if (consignmentLine.PACSegment != null)
						{
							var pacdInterpretation = interpretation.AddNewSegmentInterpretation(consignmentLine.PACSegment);
							pacdInterpretation.AddElementInterpretation(() => consignmentLine.CargoQuantity);
							pacdInterpretation.AddElementInterpretation(() => new ACROSSPackageTypes().GetDescriptionFromCode(consignmentLine.CargoUnitOfMeasure));
						}

						if (consignmentLine.SEQG193Segment != null)
						{
							interpretation.AddNewSegmentInterpretation(consignmentLine.SEQG193Segment, "Consignment", "Marks");
						}

						if (consignmentLine.PCISegment != null)
						{
							interpretation.AddNewSegmentInterpretation(consignmentLine.PCISegment, () => consignmentLine.MarksAndNumbers);
						}
					}

					if (consignmentLine.Group200 != null)
					{
						if (consignmentLine.GIDSegment != null)
						{
							interpretation.AddNewSegmentInterpretation(consignmentLine.GIDSegment, () => consignmentLine.LineNumber);
						}

						if (consignmentLine.FTXSegment != null)
						{
							interpretation.AddNewSegmentInterpretation(consignmentLine.FTXSegment, () => consignmentLine.CargoDescription);
						}

						if (consignmentLine.TCCHSSegment != null)
						{
							interpretation.AddNewSegmentInterpretation(consignmentLine.TCCHSSegment, () => consignmentLine.HsCommodityCode);
						}

						foreach (IUNDGCode uNDGData in consignmentLine.UNDGCodes)
						{
							interpretation.AddNewSegmentInterpretation(uNDGData.TCCSegment, () => uNDGData.UNDGCode);
						}
					}
				}

				if (wrapper.UNS2Segment != null)
				{
					interpretation.AddUNS2Interpretation(wrapper.UNS2Segment);
				}

				if (wrapper.CNTSegment != null)
				{
					var meaInterpretation = interpretation.AddNewSegmentInterpretation(wrapper.CNTSegment);
					meaInterpretation.AddElementInterpretation(() => wrapper.CargoWeight);
					meaInterpretation.AddElementInterpretation(() => new EManifestUnitOfWeightList().GetDescriptionFromCode(wrapper.CargoWeightUnits));
				}

				if (wrapper.UNTSegment != null)
				{
					interpretation.AddUNTInterpretation(wrapper.UNTSegment, wrapper.UNTSegment.MessageReferenceNumber);
				}
			}

			return interpretation.ToHtml();
		}

		static void ProcessOneNameAndAddress(INameAndAddress nameAndAddress, MessageInterpretation interpretation)
		{
			if (nameAndAddress.NADSegment != null)
			{
				var nadInterpretation = interpretation.AddNewSegmentInterpretation(nameAndAddress.NADSegment);
				nadInterpretation.AddElementInterpretation(() => ForwardedManifestSupporter.GetNameAndAddressType(nameAndAddress.AddressType));
				nadInterpretation.AddElementInterpretation(() => nameAndAddress.Name);
				nadInterpretation.AddElementInterpretation(() => nameAndAddress.Address);
			}

			if (nameAndAddress.CTASegment != null)
			{
				interpretation.AddNewSegmentInterpretation(nameAndAddress.CTASegment, () => nameAndAddress.ContactName);
			}

			if (nameAndAddress.CTAAHSegment != null)
			{
				interpretation.AddNewSegmentInterpretation(nameAndAddress.CTAAHSegment);
			}

			if (nameAndAddress.COMSegment != null)
			{
				interpretation.AddNewSegmentInterpretation(nameAndAddress.COMSegment, () => nameAndAddress.Telephone);
			}
		}

		static ZString GetMessageFunctionDescription(MessageFunctionCodeList code)
		{
			if (code == MessageFunctionCodeList.Original)
			{
				return Res.GetString("7f457f4b-0825-4311-9c1d-a1ab70fcf810", "Original");
			}
			else if (code == MessageFunctionCodeList.Change)
			{
				return Res.GetString("ff5f2fdc-da4d-4158-9147-b742ce6608ed", "Change");
			}
			else if (code == MessageFunctionCodeList.Cancellation)
			{
				return Res.GetString("dc82bd3b-6895-41c9-a845-eb7b63c58cd9", "Withdrawn");
			}
			else if (code == MessageFunctionCodeList.ProposedAmendment)
			{
				return Res.GetString("39ff0f6c-b721-478e-9c49-949716688e81", "Post Arrival Change");
			}
			return ZString.Empty;
		}

		protected override BusinessObject EmailResponseLinkedObject
		{
			get { return null; }
		}

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { throw new System.NotImplementedException(); }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { throw new System.NotImplementedException(); }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { throw new System.NotImplementedException(); }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { throw new System.NotImplementedException(); }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { throw new System.NotImplementedException(); }
		}

		protected override ZString ErrorEmailMode
		{
			get { throw new System.NotImplementedException(); }
		}
	}
}
