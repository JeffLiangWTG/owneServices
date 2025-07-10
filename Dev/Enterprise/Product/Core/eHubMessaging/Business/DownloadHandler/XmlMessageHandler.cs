using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	abstract class XmlMessageHandler : MessageHandler
	{
		protected abstract string GetApplicationCode(string payloadTypeName);

		protected abstract string GetInterchangeType();

		protected abstract string GetMessageType();

		protected abstract XPathCollection GetPayloadTypeNodes();

		protected virtual XPathCollection GetInterchangeHeaderTypeNodes()
		{
			return null;
		}

		protected virtual bool IsValidSyntax(out string errorMsg)
		{
			errorMsg = null;
			return true;
		}

		protected abstract string GetPayloadSubType();

		protected abstract void ReadToMessageStartElement(XPathReader reader);

		protected virtual string GetMessageSubType(string payloadTypeName, string payloadSubTypeName)
		{
			return XmlMessageHelper.GetMessageSubType(payloadTypeName, payloadSubTypeName);
		}

		class MessageInfo
		{
			public MessageInfo(XmlMessageHandler handler, string payloadTypeName, string payloadSubTypeName)
			{
				this.ApplicationCode = handler.GetApplicationCode(payloadTypeName);
				this.MessageTypeCode = handler.GetMessageType();
				this.MessageSubTypeCode = handler.GetMessageSubType(payloadTypeName, payloadSubTypeName);
			}

			public readonly string ApplicationCode;
			public readonly string MessageTypeCode;
			public readonly string MessageSubTypeCode;
		}

		class MessageOutcome
		{
			public MessageOutcome()
			{
				_failedMessageInfoStack = new Stack<MessageInfo>();
				_successMessageInfoStack = new Stack<MessageInfo>();
			}

			readonly Stack<MessageInfo> _successMessageInfoStack;
			readonly Stack<MessageInfo> _failedMessageInfoStack;

			internal void AddSuccessfulOutcome(MessageInfo messageInfo) => _successMessageInfoStack.Push(messageInfo);
			internal void AddFailedOutcome(MessageInfo messageInfo) => _failedMessageInfoStack.Push(messageInfo);

			internal bool SuccessfulOverall() => _failedMessageInfoStack.IsNullOrEmpty() || (_failedMessageInfoStack.Any() && _successMessageInfoStack.Any());
		}

		protected override void SendAcknowledgement(EDIInterchange interchange, BusinessObjectFactory factory, INotifications notifier)
		{
			base.SendAcknowledgement(interchange, factory, notifier);

			if (interchange.EI_Status == XmlEDIInterchange.Status.Failed)
			{
				var failureLogNote = interchange.Notes.FindByDescription(NoteDescriptionFailureLog).MaxBySafe(stmNote => stmNote.ST_CreatedDateUtc);
				if (failureLogNote != null)
				{
					new InterchangeAcknowledgement().Send(factory, notifier, interchange, failureLogNote.ST_NoteDataAsText, null, InterchangeAcknowledgementType.Failed);
				}
			}
		}

		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = GetApplicationCode(null);
			interchange.EI_InterchangeType = GetInterchangeType();

			var failureLog = new List<string>();
			bool failed = false;
			bool rejected = false;

			try
			{
				failureLog.Add(Res.GetString("b10d5606-e255-4846-8750-bffac0de10e1", "Length of the message content is {0} before Get Payload Sub Type", Message.MessageStream.Length));
				string payloadSubTypeName = GetPayloadSubType();
				failureLog.Add(Res.GetString("1fabe80f-492b-491e-b3d7-f234c2dc3c66", "Got Payload Sub Type Name of [{0}]", payloadSubTypeName ?? Res.GetString("4f19fc8d-b6ef-4b53-a0ce-75bff9545590", "{null}")));

				var reader = new XPathReader(new XmlTextReader(Message.MessageStream), GetPayloadTypeNodes());
				if (reader.ReadUntilMatch())
				{
					failureLog.Add(Res.GetString("7d092a79-3215-4baa-821f-a75c0be548ba", "Found a Payload Match."));
					failed = !ReadMessages(reader, interchange, payloadSubTypeName, failureLog);
				}
				else
				{
					string errorMsg;
					rejected = !IsValidSyntax(out errorMsg);
					if (errorMsg != null)
					{
						failureLog.Add(Res.GetString("7A968B8A-3050-496D-A8B6-B4C77A2302A6", "{0}", errorMsg));
					}
				}

				if (failed)
				{
					UpdateStatusToFailed(interchange);
				}
				else if (rejected)
				{
					UpdateStatusToRejected(interchange);
				}
				else
				{
					failureLog.Add(Res.GetString("6e62225e-4430-406a-9bd5-27d516adcd9f", "Finished parsing messages. Setting Interchange Body Text."));
					SetInterchangeText(interchange);
				}
			}
			catch (Exception exception) when (!(exception.IsCriticalException()) && !exception.IsOutOfDiskSpaceException() && !exception.IsUnableToCreateTempFileException())
			{
				interchange.SetEI_BodyTextOrDataSource(Message.MessageStream);
				UpdateStatusToFailed(interchange);

				failureLog.Add(Res.GetString("bfef50d5-a5d2-4c04-bf2a-cd888ea28c8b", "An error occurred: {0}", exception.Message));
			}

			if (interchange.EI_Status == XmlEDIInterchange.Status.Failed || failed || rejected)
			{
				interchange.Notes.AddNew(true, NoteDescriptionFailureLog, string.Join("\r\n", failureLog.ToArray()));
				TryExtractHeader(interchange);
			}

			return interchange;
		}

		void TryExtractHeader(EDIInterchange interchange)
		{
			try
			{
				Message.MessageStream.Position = 0;
				SetInterchangeHeaderText(interchange);
			}
			catch (Exception exception) when (!exception.IsCriticalException() && !exception.IsOutOfDiskSpaceException() && !exception.IsUnableToCreateTempFileException())
			{
			}
		}

		void SetInterchangeText(EDIInterchange interchange)
		{
			Message.MessageStream.SeekBegin();

			if (GetInterchangeHeaderTypeNodes() == null)
			{
				var interchangeStream = new VirtualMemoryStream();
				interchangeStream.AddStream(Message.MessageStream);
				interchange.SetEI_BodyTextOrDataSource(interchangeStream);
			}
			else
			{
				SetInterchangeHeaderText(interchange);
				Message.MessageStream.SeekBegin();
				var xmlBodyReader = new XmlTextReader(Message.MessageStream);
				var bodyReader = new XPathReader(xmlBodyReader, GetPayloadTypeNodes());
				if (bodyReader.ReadUntilMatch())
				{
					interchange.SetEI_BodyTextOrDataSource(LargeMessageHelper.GetStreamFromNode(bodyReader));
				}
			}
		}

		void SetInterchangeHeaderText(EDIInterchange interchange)
		{
			var readerSettings = new XmlReaderSettings() { CloseInput = false };
			using (var xmlReader = XmlReader.Create(Message.MessageStream, readerSettings))
			using (var headerReader = new XPathReader(xmlReader, GetInterchangeHeaderTypeNodes()))
			{
				if (headerReader.ReadUntilMatch())
				{
					using (var stream = LargeMessageHelper.GetStreamFromNode(headerReader))
					{
						interchange.EI_HeaderText = stream.ReadToEnd();
					}
				}
			}
		}

		EDIMessage SaveEDIMessage(EDIInterchange interchange, MessageInfo messageInfo, XmlReader reader, bool markAsFail = false)
		{
			var message = CreateEDIMessage(interchange);
			message.EM_ApplicationCode = messageInfo.ApplicationCode;
			message.EM_MessageType = messageInfo.MessageTypeCode;
			message.EM_MessageSubType = messageInfo.MessageSubTypeCode;
			if (markAsFail)
			{
				message.EM_Status = EDIMessage.Status.Failed;
			}

			var xmlReaderWrapper = new XmlReaderWithExternalReferenceNumberExtraction(reader);
			message.SetEM_MessageTextOrDataSource(LargeMessageHelper.GetStreamFromNode(xmlReaderWrapper));
			message.EM_ExternalReferenceNumber = xmlReaderWrapper.ExternalReferenceNumber;
			return message;
		}

		internal void UpdateStatusToFailed(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.Failed;

			interchange.ContainedMessages.DeleteAll();
		}

		internal void UpdateStatusToRejected(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.SyntaxRejected;

			interchange.ContainedMessages.DeleteAll();
		}

		bool ReadMessages(XPathReader reader, EDIInterchange interchange, string payloadSubTypeName, List<string> failureLog)
		{
			var processOutcome = new MessageOutcome();
			LargeMessageHelper.ReadToNextElement(reader);
			for (; ; )
			{
				var messageInfo = new MessageInfo(this, reader.LocalName, payloadSubTypeName);
				interchange.EI_ApplicationCode = messageInfo.ApplicationCode;

				if (!CheckSupportedType(interchange, reader.LocalName, messageInfo))
				{
					interchange.AddMessageProcessWarning(Res.GetString("28B75F03-B457-41FE-B933-C95A26788FAC", "Message contains an unsupported element {0}", reader.LocalName));
					processOutcome.AddFailedOutcome(messageInfo);
					SaveEDIMessage(interchange, messageInfo, reader, true).Delete(); //creating ediMessage to move reader to next item
					LargeMessageHelper.ReadToNextElement(reader);
				}
				else
				{
					failureLog.Add(Res.GetString("4f5588dd-4488-4a0f-bdf2-4957208e9d96",
						"Got an Application Code of [{0}]",
						messageInfo.ApplicationCode ??
						Res.GetString("4f19fc8d-b6ef-4b53-a0ce-75bff9545590", "{null}")));
					if (messageInfo.MessageSubTypeCode == EDIMessageSubTypeList.Codes.Unknown)
					{
						processOutcome.AddFailedOutcome(messageInfo);
						failureLog.Add(Res.GetString("26c0b7c5-092c-4eaf-b262-800050b98f7b",
							"Saving Message using Sub Type [Unknown]..."));
						SaveEDIMessage(interchange, messageInfo, reader, true);

						LargeMessageHelper.ReadToNextElement(reader);
					}
					else
					{
						ReadToMessageStartElement(reader);
						var name = reader.LocalName;
						while (!string.IsNullOrEmpty(name) && reader.LocalName == name)
						{
							failureLog.Add(Res.GetString("843d53a4-7bd7-49bf-b071-0ecfbfba62fd",
								"Saving Message using Sub Type [{0}]...",
								messageInfo.MessageSubTypeCode ??
								Res.GetString("4f19fc8d-b6ef-4b53-a0ce-75bff9545590", "{null}")));
							SaveEDIMessage(interchange, messageInfo, reader);
							processOutcome.AddSuccessfulOutcome(messageInfo);
							LargeMessageHelper.ReadToNextElement(reader);
						}
					}
				}
				if (string.IsNullOrWhiteSpace(reader.LocalName))
				{
					break;
				}
			}

			return processOutcome.SuccessfulOverall();
		}

		bool CheckSupportedType(EDIInterchange interchange, string incomingTag, MessageInfo messageInfo)
		{
			if (messageInfo.ApplicationCode == EDIInterchange.ApplicationCodes.UniversalDataMessaging)
			{
				return !EdiMessageTags.UniversalMessage.RootTags.IsRequestTag(incomingTag);
			}

			return true;
		}
	}
}
