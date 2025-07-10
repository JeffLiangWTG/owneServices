using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.AcknowledgementMessage;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BE.Business;

public static class MessageHelper
{
	public static (string, XmlDocument) ValidateMessageXML(ZString xml)
	{
		var errorMessage = string.Empty;
		XmlDocument xmlDocument = null;

		if (xml.IsEmpty)
		{
			errorMessage = (NoResString)"The Message XML is Empty";
		}
		else
		{
			try
			{
				xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(xml);
			}
			catch (XmlException)
			{
				errorMessage = (NoResString)"The Message XML is not a valid XML";
			}
		}

		return (errorMessage, xmlDocument);
	}

	public static ZString GetMessageTypeByXml(XmlDocument xmlDocument)
	{
		ZString documentElementName = xmlDocument?.DocumentElement?.LocalName ?? string.Empty;
		var messageType = new BEIncomingMessageSubTypes().ContainsCode(documentElementName.SubstringSafe(2, 3)) ? documentElementName.SubstringSafe(2, 3) : ZString.Empty;

		if (documentElementName == typeof(AcknowledgementMessage).GetCustomAttribute<XmlRootAttribute>().ElementName)
		{
			messageType = Constants.InterchangeMessageTypes.AcknowledgementMessage;
		}
		else if (documentElementName == Constants.InterchangeMessageTypes.CustomsServiceErrorUniversalEventResponseMessage)
		{
			messageType = BEIncomingMessageSubTypes.Codes.CustomsServiceErrorUniversalEvent;
		}
		return messageType;
	}

	public static CusEntryHeader LocateEntryHeaderByLRNFallbackToMRN(BusinessObjectFactory factory, IInboundProvider messageInboundDataProvider)
	{
		var lrn = messageInboundDataProvider.LRN;
		var mrn = messageInboundDataProvider.MRN;
		CusEntryHeader result = null;
		if (!lrn.IsNullOrEmpty())
		{
			result = LocateEntryHeaderByLRN(factory, messageInboundDataProvider);
		}

		if (result == null && !mrn.IsNullOrEmpty())
		{
			return LocateEntryHeaderByMRN(factory, messageInboundDataProvider);
		}

		return result;
	}

	public static CusEntryHeader LocateEntryHeaderByLRN(BusinessObjectFactory factory, IInboundProvider messageDataProvider)
	{
		var lrn = messageDataProvider.LRN;
		CusEntryHeader result = null;

		if (!lrn.IsNullOrEmpty())
		{
			var entryQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			entryQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, lrn);
			result = factory.LoadTop1<CusEntryHeader>(entryQuery);
		}

		return result;
	}

	public static CusEntryHeader LocateEntryHeaderByMRN(BusinessObjectFactory factory, IInboundProvider messageDataProvider)
	{
		var mrn = messageDataProvider.MRN;
		CusEntryHeader result = null;

		if (!mrn.IsNullOrEmpty())
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, mrn);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Belgium);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeader.Schema.TableName);
			query.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
			result = factory.LoadTop1<CusEntryHeader>(query);
		}

		return result;
	}

	public static BusinessObject LocateHeaderByEdiInterchange(EDIInterchange interchange)
	{
		var interchangeBySessionSubQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
		interchangeBySessionSubQuery.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Sent)
									.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
									.AddToFilter(EDIInterchangeSchema.EI_IsActive, true)
									.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.BECustoms)
									.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, interchange.EI_SessionGUID)
									.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + OrderByClause.Descending;

		var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
		messageQuery.AddSubQuery(EDIMessageSchema.EM_EI, interchangeBySessionSubQuery, JoinCondition.And);
		messageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
					.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent)
					.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Descending;

		var originalOutgoingEdiMessage = interchange.Factory.LoadTop1<EDIMessage>(messageQuery);
		return originalOutgoingEdiMessage?.EM_LinkedObject;
	}

	public static CusEntryHeader LocateEntryHeaderByLrnOrMRNFallbackToInterchange(BusinessObjectFactory factory, IInboundProvider messageDataProvider, EDIInterchange interchange)
	{
		var result = LocateEntryHeaderByLRNFallbackToMRN(factory, messageDataProvider);
		if (result == null && interchange != null)
		{
			result = LocateHeaderByEdiInterchange(interchange) as CusEntryHeader;
		}
		return result;
	}

	static ZString getNode(ZString messageText, ZString nodeName)
	{
		var nodeInnerXml = ZString.Empty;
		if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
		{
			var node = xmlDocument.GetElementsByTagName(nodeName, "*");
			if (node != null && node.Count == 1)
			{
				nodeInnerXml = node[0].InnerXml;
			}
		}
		return (nodeInnerXml);
	}

	public static ZString GetXmlBody(TextReader textReader)
	{
		var textString = textReader.ReadToEnd();
		if (textString.Contains(BodyText))
		{
			var nodeInnerXmlString = getNode(textString, BodyText);
			return nodeInnerXmlString;
		}
		else
		{
			return textString;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
	const string BodyText = "Body";
}
