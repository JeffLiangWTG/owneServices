using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Customs.IE.MessageContracts.NCTS;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC013C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC015C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Microsoft.XmlDiffPatch;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public static class MessageStaticHelper
	{
		public static bool PreviouslySentPlaceOfLoading(NctsHeader nctsHeader)
		{
			var result = false;
			if (GetLastIE015OrIE013Message(nctsHeader) is NCTSOutboundEDIMessage previousMessage)
			{
				if (previousMessage.EM_MessageType.EqualsIgnoringCase(NCTSOutgoingMessageTypeList.Codes.DeclarationData))
				{
					result = HasPlaceOfLoading(previousMessage.GetDataProvider<Cc015CType>());
				}
				else
				{
					result = HasPlaceOfLoading(previousMessage.GetDataProvider<Cc013CType>());
				}
			}

			return result;
		}

		static bool HasPlaceOfLoading(Cc015CType messageData)
		{
			return messageData != null && messageData.Consignment?.PlaceOfLoading != null;
		}

		static bool HasPlaceOfLoading(Cc013CType messageData)
		{
			return messageData != null && messageData.Consignment?.PlaceOfLoading != null;
		}

		public static bool CanAmendGuarantees(NctsHeader nctsHeader)
		{
			var result = true;
			var lastIE015orIE013Message = GetLastIE015OrIE013Message(nctsHeader);
			if (lastIE015orIE013Message != null)
			{
				var lastIE015orIE013MessageText = lastIE015orIE013Message.EM_MessageText;
				var generatedIE013Message = GetGeneratedIE013Message(nctsHeader);

				var lastIE015orIE013MessageXmlDoc = GetAndLoadXmlDocument(lastIE015orIE013MessageText);
				var nextIE013MessageXmlDoc = GetAndLoadXmlDocument(generatedIE013Message);

				RemoveUndesiredElementsFromComparison(lastIE015orIE013MessageXmlDoc);
				RemoveUndesiredElementsFromComparison(nextIE013MessageXmlDoc);

				var (lastIE015orIE013MessageGuarantees, lastIE015orIE013MessageOtherElements) = SplitGuaranteesAndOtherElementXmlDocs(lastIE015orIE013MessageXmlDoc);
				var (nextIE013MessageGuarantees, nextIE013MessageOtherElements) = SplitGuaranteesAndOtherElementXmlDocs(nextIE013MessageXmlDoc);

				var guaranteesChanged = HaveChanges(lastIE015orIE013MessageGuarantees, nextIE013MessageGuarantees);
				var otherElementsChanged = HaveChanges(lastIE015orIE013MessageOtherElements, nextIE013MessageOtherElements);

				result = !(guaranteesChanged && otherElementsChanged);
			}

			return result;
		}

		static NCTSOutboundEDIMessage GetLastIE015OrIE013Message(NctsHeader nctsHeader)
		{
			var messages = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader.Messages : nctsHeader.Messages;
			return messages.GetMatchingMessages(
				applicationCode: EDIInterchange.ApplicationCodes.IECustomsNCTS,
				messageTypes: new ZString[] { NCTSOutgoingMessageTypeList.Codes.DeclarationData, NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment },
				direction: EDIInterchange.Direction.Transmit
			).Cast<NCTSOutboundEDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
		}

		static string GetGeneratedIE013Message(NctsHeader nctsHeader)
		{
			IXmlMessageBuilder messageBuilder = new IE013MessageBuilder(new IE013MessageProvider(nctsHeader));
			return messageBuilder.GenerateXmlMessage().GetSerializedString();
		}

		static XmlDocument GetAndLoadXmlDocument(ZString message)
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(message);
			return xmlDocument;
		}

		static void RemoveUndesiredElementsFromComparison(XmlDocument xmlDoc)
		{
			// replace the root element which is not relevant for comparison (CC015C or CC013C)
			var newRootElement = xmlDoc.CreateElement((NoResString)"root");
			foreach (XmlNode childNode in xmlDoc.DocumentElement.ChildNodes)
			{
				var importedNode = xmlDoc.ImportNode(childNode, deep: true);
				newRootElement.AppendChild(importedNode);
			}
			xmlDoc.ReplaceChild(newRootElement, xmlDoc.DocumentElement);

			// removes elements that are not relevant for the comparison
			string[] elements = {
				GetSerializedFieldElementName(typeof(Cc015CType), nameof(Cc015CType.MessageSender)),
				GetSerializedFieldElementName(typeof(Cc015CType), nameof(Cc015CType.MessageRecipient)),
				GetSerializedFieldElementName(typeof(Cc015CType), nameof(Cc015CType.PreparationDateAndTime)),
				GetSerializedFieldElementName(typeof(Cc015CType), nameof(Cc015CType.MessageIdentification)),
				GetSerializedFieldElementName(typeof(Cc015CType), nameof(Cc015CType.MessageType)),
				GetSerializedFieldElementName(typeof(TransitOperationType04), nameof(TransitOperationType04.AmendmentTypeFlag)),
				GetSerializedFieldElementName(typeof(TransitOperationType04), nameof(TransitOperationType04.Mrn)),
				GetSerializedFieldElementName(typeof(TransitOperationType06), nameof(TransitOperationType06.Lrn)),
			};

			foreach (var element in elements)
			{
				var nodes = xmlDoc.GetElementsByTagName(element);
				var node = nodes[0];
				if (node != null)
				{
					node.ParentNode.RemoveChild(node);
				}
			}

			string GetSerializedFieldElementName(Type type, string field)
			{
				var property = type.GetProperty(field);
				var v = (XmlElementAttribute)property.GetCustomAttributes(typeof(XmlElementAttribute), false).First();
				return v?.ElementName;
			}
		}

		static (XmlDocument guaranteesXmlDoc, XmlDocument otherElementsXmlDoc) SplitGuaranteesAndOtherElementXmlDocs(XmlDocument fullXmlDoc)
		{
			var otherElementsXmlDoc = fullXmlDoc;
			var guaranteesXmlDoc = new XmlDocument();
			var guaranteesRootElement = guaranteesXmlDoc.CreateElement("GuaranteesRoot");
			guaranteesXmlDoc.AppendChild(guaranteesRootElement);
			foreach (XmlNode guarantee in otherElementsXmlDoc.SelectNodes($"//Guarantee"))
			{
				var importedNode = guaranteesXmlDoc.ImportNode(guarantee, deep: true);
				guaranteesRootElement.AppendChild(importedNode);
				guarantee.ParentNode.RemoveChild(guarantee);
			}
			return (guaranteesXmlDoc, otherElementsXmlDoc);
		}

		static bool HaveChanges(XmlDocument lastIE015orIE013MessageSentXmlDoc, XmlDocument nextIE013MessageToSendXmlDoc)
		{
			var xmldiff = new XmlDiff();
			xmldiff.Algorithm = XmlDiffAlgorithm.Fast;
			return !xmldiff.Compare(lastIE015orIE013MessageSentXmlDoc, nextIE013MessageToSendXmlDoc);
		}

		internal static bool IsGuaranteeTypeWithReferences(string guaranteeType)
		{
			return guaranteeType.In(new[]
			{
				EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver,
				EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee,
				EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor,
				EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee,
				EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher,
				EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur,
				EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage
			});
		}
	}
}
