using System;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName("http://www.cargowise.com/Schemas/Native#UniversalInterchange")]
	[SupportedSchemaName("http://www.cargowise.com/Schemas/Universal#UniversalInterchange")]
	[SupportedSchemaName("http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange")]
	class UniversalInterchangeHeaderHandler : XmlMessageHandler
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Is an XML Tag Name")]
		protected override string GetApplicationCode(string payloadTypeName)
		{
			if (Message.SenderID == "USCustomsEBond" || Message.SenderID == "USCustomsEBondTest")
			{
				return ApplicationCodeList.Codes.USeBond;
			}
			if (payloadTypeName == "ReferenceData" || payloadTypeName == "Native")
			{
				return ApplicationCodeList.Codes.NativeDataMessaging;
			}
			if (payloadTypeName == "ReadRexResponse" || payloadTypeName == "RexForwardOwnershipResponse" || payloadTypeName == "RexAcknowledgeOwnershipResponse" || payloadTypeName == "RexTransferOwnershipResponse" || payloadTypeName == "RexWithdrawOwnershipResponse")
			{
				return ApplicationCodeList.Codes.AUCustomsNEXDOC;
			}
			if (payloadTypeName == "CMD")
			{
				return ApplicationCodeList.Codes.SGCustomsCMD;
			}

			return ApplicationCodeList.Codes.UniversalDataMessaging;
		}

		protected override string GetInterchangeType()
		{
			return EDIInterchangeTypeList.Codes.XDC;
		}

		protected override string GetMessageType()
		{
			return EDIMessageTypeList.Codes.XDC;
		}

		protected override string GetPayloadSubType()
		{
			Message.MessageStream.SeekBegin();
			var payloadSubTypeNodesReader = new XPathReader(new XmlTextReader(Message.MessageStream), GetPayloadSubTypeNodes());

			string result = string.Empty;
			if (payloadSubTypeNodesReader.ReadUntilMatch())
			{
				result = payloadSubTypeNodesReader.LocalName;
			}
			Message.MessageStream.SeekBegin();
			return result;
		}

		XPathCollection GetPayloadSubTypeNodes()
		{
			if (payloadSubTypeNodes == null)
			{
				payloadSubTypeNodes = new XPathCollection();
				payloadSubTypeNodes.Add((NoResString)"/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='ReferenceData']/*[local-name()='Body']/*");
				payloadSubTypeNodes.Add((NoResString)"/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='Native']/*[local-name()='Body']/*");
				payloadSubTypeNodes.Add((NoResString)"/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='ReadRexResponse']/*");
				payloadSubTypeNodes.Add((NoResString)"/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='CMD']");
			}
			return payloadSubTypeNodes;
		}
		[ThreadStatic]
		static XPathCollection payloadSubTypeNodes;

		protected override string GetMessageSubType(string payloadTypeName, string payloadSubTypeName)
		{
			return (payloadTypeName == "CMD") ? payloadTypeName : base.GetMessageSubType(payloadTypeName, payloadSubTypeName);
		}

		protected override XPathCollection GetInterchangeHeaderTypeNodes()
		{
			if (interchangeHeaderTypeNodes == null)
			{
				interchangeHeaderTypeNodes = new XPathCollection();
				interchangeHeaderTypeNodes.Add((NoResString)"/*[local-name()='UniversalInterchange']/*[local-name()='Header']");
			}
			return interchangeHeaderTypeNodes;
		}
		[ThreadStatic]
		static XPathCollection interchangeHeaderTypeNodes;

		protected override XPathCollection GetPayloadTypeNodes()
		{
			if (payloadTypeNodes == null)
			{
				payloadTypeNodes = new XPathCollection();
				payloadTypeNodes.Add((NoResString)"/*[local-name()='UniversalInterchange']/*[local-name()='Body']");
			}
			return payloadTypeNodes;
		}
		[ThreadStatic]
		static XPathCollection payloadTypeNodes;

		protected override void ReadToMessageStartElement(XPathReader reader)
		{
		}

		protected override bool SupportSendingAcknowledgement
		{
			get
			{
				return true;
			}
		}

		protected override bool IsValidSyntax(out string errorMsg)
		{
			errorMsg = null;
			Message.MessageStream.SeekBegin();
			XPathCollection universalInterchangeNodes = new XPathCollection();
			universalInterchangeNodes.Add((NoResString)"/*[local-name()='UniversalInterchange']");
			var reader = new XPathReader(new XmlTextReader(Message.MessageStream), universalInterchangeNodes);
			if (!reader.ReadUntilMatch())
			{
				errorMsg = (NoResString)"Invalid SOAP Request: Universal Interchange is missing";
				return false;
			}
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "This is a bloody XPath")]
		internal const string InterchangeAcknowledgementXPath = "/*[local-name()='Header']/*[local-name()='Acknowledgement']";
	}
}
