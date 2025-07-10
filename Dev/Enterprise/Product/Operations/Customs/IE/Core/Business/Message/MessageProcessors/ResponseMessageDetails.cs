using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.AES;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Business.AIS.UCC5;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class ResponseMessageDetails : IXmlRootNameMappingProvider
	{
		public static ResponseDetail GetResponseDetail(string applicationCode, string messageType, string messageSubType, string messageText = null)
		{
			ResponseDetail result;
			switch (messageSubType)
			{
				case CommonInterchangeTypeList.Codes.MessageAcknowledge:
					switch (applicationCode)
					{
						case EDIMessage.ApplicationCodes.IECustomsEMCS:
							result = EMCSResponseMessageDetails.AcknowledgementResponseDetail;
							break;
						case EDIMessage.ApplicationCodes.IECustomsNCTS:
							result = NCTSResponseMessageDetails.AcknowledgementResponseDetail;
							break;
						default:
							result = new ResponseDetail(xmlObjectType: null, processorType: typeof(MessageAcknowledgementProcessor));
							break;
					}
					break;
				case EDIMessage.Status.Error:
					result = new ResponseDetail(xmlObjectType: null, processorType: typeof(ErrorMessageProcessor));
					break;
				default:
					switch (applicationCode)
					{
						case EDIMessage.ApplicationCodes.IECustomsExport:
							result = AESResponseMessageDetails.GetResponseDetail(messageType);
							break;
						case EDIMessage.ApplicationCodes.IECustomsImport:
							result = AISResponseMessageDetails.GetResponseDetail(messageType, messageText);
							break;
						case EDIMessage.ApplicationCodes.IECustomsUCC5Import:
							result = AISUCC5ResponseMessageDetails.GetResponseDetail(messageType);
							break;
						case EDIMessage.ApplicationCodes.IECustomsEMCS:
							result = EMCSResponseMessageDetails.GetResponseDetail(messageType, messageText);
							break;
						case EDIMessage.ApplicationCodes.IECustomsNCTS:
							result = NCTSResponseMessageDetails.GetResponseDetail(messageType);
							break;
						case EDIMessage.ApplicationCodes.IECustomsAndExcise:
							result = CustomsAndExciseReportResponseMessageDetails.GetResponseDetail(messageType, messageText);
							break;
						case EDIMessage.ApplicationCodes.IECustomsPBN:
							result = PBNResponseMessageDetails.GetResponseDetail(messageType, messageText);
							break;
						default:
							result = null;
							break;
					}
					break;
			}
			return result;
		}

		public IReadOnlyDictionary<ZString, (ZString ApplicationCode, ZString MessageType)> GetXmlRootNameMapping()
		{
			var result = new Dictionary<ZString, (ZString applicationCode, ZString messageType)>();
			AddXmlRootNameMapping(result, EDIMessage.ApplicationCodes.IECustomsExport, AESResponseMessageDetails.ResponseDetails);
			AddXmlRootNameMapping(result, EDIMessage.ApplicationCodes.IECustomsImport, AISResponseMessageDetails.ResponseDetails);
			AddXmlRootNameMapping(result, EDIMessage.ApplicationCodes.IECustomsImport, AISResponseMessageDetails.H7ResponseDetails);
			AddXmlRootNameMapping(result, EDIMessage.ApplicationCodes.IECustomsUCC5Import, AISUCC5ResponseMessageDetails.ResponseDetails);
			AddXmlRootNameMapping(result, EDIMessage.ApplicationCodes.IECustomsEMCS, EMCSResponseMessageDetails.ResponseDetails);
			AddXmlRootNameMapping(result, EDIMessage.ApplicationCodes.IECustomsNCTS, NCTSResponseMessageDetails.ResponseDetails);
			return result;
		}

		void AddXmlRootNameMapping(Dictionary<ZString, (ZString applicationCode, ZString messageType)> dictionary, ZString applicationCode, ImmutableDictionary<string, ResponseDetail> responseDetails)
		{
			foreach (var responseDetailPair in responseDetails)
			{
				AddXmlRootNameMapping(dictionary, applicationCode, responseDetailPair.Key, responseDetailPair.Value);
			}
		}

		void AddXmlRootNameMapping(Dictionary<ZString, (ZString applicationCode, ZString messageType)> dictionary, ZString applicationCode, ImmutableDictionary<string, ResponseDetail[]> responseDetails)
		{
			foreach (var responseDetailPair in responseDetails)
			{
				responseDetailPair.Value.ForEach(responseDetail => AddXmlRootNameMapping(dictionary, applicationCode, responseDetailPair.Key, responseDetail));
			}
		}

		void AddXmlRootNameMapping(Dictionary<ZString, (ZString applicationCode, ZString messageType)> dictionary, ZString applicationCode, string messageType, ResponseDetail responseDetail)
		{
			if (Attribute.GetCustomAttribute(responseDetail.XmlObjectType, typeof(XmlRootAttribute)) is XmlRootAttribute xmlRootAttribute)
			{
				dictionary.Add(xmlRootAttribute.Namespace + xmlRootAttribute.ElementName, (applicationCode, messageType));
			}
		}

		public static IEMCSResponseMessageDetails EMCSResponseMessageDetails => emcsResponseMessageDetails ?? (emcsResponseMessageDetails = ObjectFactory.Get<IEMCSResponseMessageDetails>());

		[ThreadStatic]
		static IEMCSResponseMessageDetails emcsResponseMessageDetails;

		public static INCTSResponseMessageDetails NCTSResponseMessageDetails => nctsResponseMessageDetails ?? (nctsResponseMessageDetails = ObjectFactory.Get<INCTSResponseMessageDetails>());

		[ThreadStatic]
		static INCTSResponseMessageDetails nctsResponseMessageDetails;

		public static IPBNResponseMessageDetails PBNResponseMessageDetails => pbnResponseMessageDetails ?? (pbnResponseMessageDetails = ObjectFactory.Get<IPBNResponseMessageDetails>());

		[ThreadStatic]
		static IPBNResponseMessageDetails pbnResponseMessageDetails;
	}
}
