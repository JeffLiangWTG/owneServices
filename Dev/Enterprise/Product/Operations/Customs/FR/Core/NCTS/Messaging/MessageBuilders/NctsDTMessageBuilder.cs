using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class NctsDTMessageBuilder : INctsNativeBuilder
	{
		public ZString NativeMessage<T>(T nctsHeader, NctsMessageFunctionSet messageFunction, ErrorCollector errorCollector)
		{
			var nativeMessage = ZString.Empty;

			var frNctsHeader = nctsHeader as NctsHeader;
			if (frNctsHeader == null)
			{
				return nativeMessage;
			}

			ZString schemaID;
			ZString partyID;
			if (messageFunction is FRNctsMessageFunctionSet.PrelodgeValidationMessage prelodgeValidationMessage)
			{
				nativeMessage = new CCF15AMessageBuilder(new CCF15AWrapper(frNctsHeader), prelodgeValidationMessage, errorCollector).GetXMLMessageWithoutNamespaces();
				schemaID = messageFunction.Code;
				partyID = GetPrincipalPartyId(frNctsHeader);
			}
			else if (messageFunction is NctsMessageFunctionSet.DeclarationDataMessage declarationDataMessage)
			{
				nativeMessage = new CC015BMessageBuilder(new CC015BWrapper(frNctsHeader), declarationDataMessage, errorCollector).GetXMLMessageWithoutNamespaces();
				schemaID = messageFunction.Code;
				partyID = GetPrincipalPartyId(frNctsHeader);
			}
			else if (messageFunction is NctsMessageFunctionSet.DeclarationCancellationRequestMessage declarationCancellationRequestMessage)
			{
				nativeMessage = new CC014AMessageBuilder(new CC014AWrapper(frNctsHeader), declarationCancellationRequestMessage, errorCollector).GetXMLMessageWithoutNamespaces();
				schemaID = messageFunction.Code;
				partyID = GetPrincipalPartyId(frNctsHeader);
			}
			else if (messageFunction is NctsMessageFunctionSet.ArrivalNotificationMessage arrivalNotificationMessage)
			{
				nativeMessage = new CC007AMessageBuilder(new CC007ADeclarationWrapper(frNctsHeader), arrivalNotificationMessage, errorCollector).GetXMLMessageWithoutNamespaces();
				schemaID = messageFunction.Code;
				partyID = GetDeclarantPartyId(frNctsHeader);
			}
			else if (messageFunction is NctsMessageFunctionSet.UnloadingRemarksMessage unloadingRemarksMessage)
			{
				nativeMessage = new CC044AMessageBuilder(new CC0044AWrapper(frNctsHeader), unloadingRemarksMessage, errorCollector).GetXMLMessageWithoutNamespaces();
				schemaID = messageFunction.Code;
				partyID = GetDeclarantPartyId(frNctsHeader);
			}
			else if (messageFunction is NctsMessageFunctionSet.DeclarationAmendmentMessage declarationAmendmentMessage)
			{
				nativeMessage = new CC013BMessageBuilder(new CC013BWrapper(frNctsHeader), declarationAmendmentMessage, errorCollector).GetXMLMessageWithoutNamespaces();
				schemaID = messageFunction.Code;
				partyID = GetPrincipalPartyId(frNctsHeader);
			}
			else if (messageFunction is NctsMessageFunctionSet.InformationAboutNonArrivedMovementMessage informationAboutNonArrivedMovementMessage)
			{
				nativeMessage = new CC141AMessageBuilder(new CC141AWrapper(frNctsHeader), informationAboutNonArrivedMovementMessage, errorCollector).GetXMLMessageWithoutNamespaces();
				schemaID = messageFunction.Code;
				partyID = GetPrincipalPartyId(frNctsHeader);
			}
			else
			{
				throw new System.NotImplementedException("CW1 doesn't yet support building message type ");
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Xml strings")]
			XElement PopulateMessageAndEnvelope(XmlDocument declaration)
			{
				var messageRoot = new XElement("Message");
				var enveloppe = new XElement("EnveloppeMessage");
				enveloppe.Add(
									new XElement("schemaID") { Value = schemaID },
									new XElement("schemaVersion") { Value = "1.0" },
									new XElement("partyId") { Value = partyID },
									new XElement("transactionId") { Value = frNctsHeader.BH_JobReference.KeepNumericCharacters().Right(10) },
									new XElement("numseq") { Value = GetSequenceNumber(frNctsHeader, schemaID) },
									new XElement("refdos") { Value = frNctsHeader.BH_JobReference }
				);
				messageRoot.Add(enveloppe);
				var dec = new XElement("Declaration");
				messageRoot.Add(dec);
				dec.Add(XElement.Parse(declaration.OuterXml));
				return messageRoot;
			}

			var docForDeclaration = new XmlDocument();
			docForDeclaration.LoadXml(nativeMessage);
			if (docForDeclaration.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
			{
				docForDeclaration.RemoveChild(docForDeclaration.FirstChild);
			}

			return PopulateMessageAndEnvelope(docForDeclaration).ToString();
		}

		string GetPrincipalPartyId(NctsHeader nctsHeader)
		{
			var result = "";
			var principal = nctsHeader.Principal?.Organisation;
			if (principal != null)
			{
				var account = principal.DeltaAgreementNumberCollection.Cast<OrgCusAccount>().FirstOrDefault(x => x.CZ_Code == OrgCusAccountCodeList.Codes.DTA && x.CZ_Type == OrgCusAccountDeltaTTypeList.Codes.TR);
				if (account != null)
				{
					result = account.CZ_RepresentativeID;
				}
			}
			return result;
		}

		string GetDeclarantPartyId(NctsHeader nctsHeader)
		{
			var result = "";
			var declarant = nctsHeader.Declarant?.Organisation;
			if (declarant != null)
			{
				var account = declarant.DeltaAgreementNumberCollection.Cast<OrgCusAccount>().FirstOrDefault(x => x.CZ_Code == OrgCusAccountCodeList.Codes.DTA && x.CZ_Type == OrgCusAccountDeltaTTypeList.Codes.TR);
				if (account != null)
				{
					result = account.CZ_RepresentativeID;
				}
			}
			return result;
		}

		string GetSequenceNumber(NctsHeader frNctsHeader, string schemaID)
		{
			var messageType = schemaID.Substring(2);
			if (schemaID == "IEF15")
			{
				messageType = "15F";
			}

			var isArrival = messageType == "007" || messageType == "044";
			var messageList = isArrival ? frNctsHeader.Messages.Cast<EDIMessage>().Where(m => m.EM_MessageSubType == "DT" && (m.EM_MessageType == "007" || m.EM_MessageType == "044")) : frNctsHeader.Messages.Cast<EDIMessage>().Where(m => m.EM_MessageSubType == "DT" && m.EM_MessageType != "007" && m.EM_MessageType != "044");

			return messageList.Count(m => m.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit && m.EM_Status == EDIMessage.Status.Acknowledged).ToString();
		}
	}
}
