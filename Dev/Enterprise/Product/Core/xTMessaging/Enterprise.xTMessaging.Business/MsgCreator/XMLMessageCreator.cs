using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.xTMessaging.Business
{
	public abstract class XMLMessageCreator : EDIMessageCreator
	{
		public XMLMessageCreator(EDIInterchange interchange, ILogger logger) : base(interchange, logger) { }

		protected override ZString CreateEDIMessagesForInterchangeCore(Stream payload, BusinessObjectFactory factory)
		{
			var debatchLogs = new ZStringBuilder();

			try
			{
				var xmlReader = new XPathReader(new XmlTextReader(payload), XPathsForPayload());

				if (xmlReader.ReadUntilMatch())
				{
					MoveToStartElement(xmlReader);

					if (xmlReader.ReadState != ReadState.EndOfFile)
					{
						for (; ; )
						{
							var subTreeStream = LargeMessageHelper.GetStreamFromNode(xmlReader);

							XElement currentNode = XElement.Load(subTreeStream);

							var payloadTypeName = currentNode.Name.LocalName;
							var applicationCode = GetApplicationCode(payloadTypeName);
							var payloadSubType = GetPayloadSubType(subTreeStream);
							var messageSubType = GetMessageSubType(payloadTypeName, payloadSubType);

							if (CheckSupportedType(applicationCode, payloadTypeName))
							{
								if (messageSubType == EDIMessageSubTypeList.Codes.Unknown)
								{
									var unkElementMsg = $"Unknown Message subType - Message of Application Code '{applicationCode}', Message Type '{payloadTypeName}' contains an unsupported element <{payloadSubType}>. Element will be ignored.";
									debatchLogs.Append(unkElementMsg);
									Logger.Warning(unkElementMsg);
								}
								else
								{
									var ediMessage = CreateEDIMessage(
										factory,
										() => applicationCode,
										() => GetMessageType(payloadTypeName, payloadSubType),
										() => messageSubType
										);

									ediMessage.SetEM_MessageTextOrDataSource(subTreeStream);
								}
							}
							else
							{
								var warningMessage = $"Unknown Payload - Message of ApplicationCode '{applicationCode}' contains an unsupported element <{payloadTypeName}>. Element will be ignored.";
								debatchLogs.Append(warningMessage);
								Logger.Warning(warningMessage);
							}
							LargeMessageHelper.ReadToNextElement(xmlReader);

							if (string.IsNullOrWhiteSpace(xmlReader.LocalName))
							{
								break;
							}
						}
					}
				}
				else
				{
					var unknownPayloadMsg = (NoResString)"Unable to Create EDIMessage - No Matching Payload is Found.";
					Interchange.EI_Status = EDIInterchange.Status.Error;
					Interchange.Notes.AddNew(true, MessageProcessNoteType, unknownPayloadMsg);
					throw new MsgProcessingException(unknownPayloadMsg);
				}
			}
			catch (XmlException ex)
			{
				var errorMessage = $"XML Error occurred when creating EDIMessage: {ex.Message}";
				debatchLogs.Append(errorMessage);
				Interchange.EI_Status = EDIInterchange.Status.Error;
				Interchange.Notes.AddNew(true, MessageProcessNoteType, debatchLogs.ToStringWithNewLineBetweenAppends());
				throw new MsgProcessingException(debatchLogs.ToStringWithNewLineBetweenAppends());
			}

			return debatchLogs.ToString();
		}

		#region abstract

		protected abstract XPathCollection XPathsForPayload();
		protected abstract XPathCollection XPathsForPayloadSubType();
		protected abstract string GetMessageSubType(string payloadTypeName, string payloadSubTypeName);
		protected abstract string GetMessageType(string payloadTypeName, string payloadSubTypeName);
		protected abstract string GetApplicationCode(string payloadTypeName);

		#endregion
		protected virtual void MoveToStartElement(XPathReader xmlReader)
		{
			LargeMessageHelper.ReadToNextElement(xmlReader);
		}

		protected virtual bool CheckSupportedType(string applicationCode, string incomingTag) => true;

		protected virtual string GetPayloadSubType(Stream payload)
		{
			string result = string.Empty;

			if (XPathsForPayloadSubType() != null)
			{
				var originalPos = payload.Position;
				payload.SeekBegin();
				var payloadSubTypeNodesReader = new XPathReader(XmlTextReader.Create(payload), XPathsForPayloadSubType());
				if (payloadSubTypeNodesReader.ReadUntilMatch())
				{
					result = payloadSubTypeNodesReader.LocalName;
				}
				payload.Position = originalPos;
			}

			return result;
		}

		protected override Type EDIMessageType => typeof(XmlEDIMessage);
	}
}
