using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.XMS)]
	class XmlInterchangeHandler : XmlMessageHandler
	{
		protected override string GetApplicationCode(string payloadTypeName)
		{
			return ApplicationCodeList.Codes.XMS;
		}

		protected override string GetInterchangeType()
		{
			return EDIInterchangeTypeList.Codes.XMS;
		}

		protected override string GetMessageType()
		{
			return EDIMessageTypeList.Codes.XMS;
		}

		protected override string GetPayloadSubType()
		{
			Message.MessageStream.SeekBegin();
			var payloadSubTypeNodesReader = new XPathReader(new XmlTextReader(Message.MessageStream), GetPayloadSubTypeNodes());

			string result = string.Empty;
			if (payloadSubTypeNodesReader.ReadUntilMatch())
			{
				payloadSubTypeNodesReader.Read();
				result = payloadSubTypeNodesReader.Value;
			}
			Message.MessageStream.SeekBegin();
			return result;
		}

		XPathCollection GetPayloadSubTypeNodes()
		{
			if (payloadSubTypeNodes == null)
			{
				payloadSubTypeNodes = new XPathCollection();
				payloadSubTypeNodes.Add((NoResString)"/*[local-name()='XmlInterchange' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='InterchangeInfo' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Target' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Type' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']");
			}
			return payloadSubTypeNodes;
		}
		[ThreadStatic]
		static XPathCollection payloadSubTypeNodes;

		protected override XPathCollection GetInterchangeHeaderTypeNodes()
		{
			if (interchangeHeaderTypeNodes == null)
			{
				interchangeHeaderTypeNodes = new XPathCollection();
				interchangeHeaderTypeNodes.Add((NoResString)"/*[local-name()='XmlInterchange' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='InterchangeInfo' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']");
			}
			return interchangeHeaderTypeNodes;
		}
		[ThreadStatic]
		static XPathCollection interchangeHeaderTypeNodes;

		protected override XPathCollection GetPayloadTypeNodes()
		{
			if (payloadTargetNodes == null)
			{
				payloadTargetNodes = new XPathCollection();
				payloadTargetNodes.Add((NoResString)"/*[local-name()='XmlInterchange' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Payload' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']");
			}
			return payloadTargetNodes;
		}
		[ThreadStatic]
		static XPathCollection payloadTargetNodes;

		protected override void ReadToMessageStartElement(XPathReader reader)
		{
			LargeMessageHelper.ReadToNextElement(reader);
		}

		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			var failureLog = new List<string>();
			try
			{
				if (interchange.EI_Status != EDIInterchange.Status.Failed && interchange.EI_HeaderText.IsEmpty)
				{
					using (var bodyStream = interchange.GetEI_BodyTextReader().CopyWithoutDispose())
					{
						interchange.UpdateInterchangeHeaderTextUsingXPathReaderAndDisposeStream(bodyStream);
					}
				}
			}
			catch (XmlException exception)
			{
				failureLog.Add(Res.GetString("3782d121-da5e-4b20-bb98-90a764db010c", "Message has invalid XML exception."));
				interchange.SetEI_BodyTextOrDataSource(Message.MessageStream);
				UpdateStatusToFailed(interchange);

				failureLog.Add(exception.ToString());
				interchange.Notes.AddNew(true, NoteDescriptionFailureLog, string.Join("\r\n", failureLog.ToArray()));
			}

			return interchange;
		}

		protected override bool SupportSendingAcknowledgement
		{
			get
			{
				return true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "This is a bloody XPath")]
		internal const string InterchangeAcknowledgementXPath = "/*[local-name()='InterchangeInfo']/*[local-name()='Acknowledgement']";
	}
}
