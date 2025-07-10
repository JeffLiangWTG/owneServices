using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CONTRL;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CONTRLMessageProcessor : Messaging.MessageProcessors.CustomsMessageProcessor
	{
		public CONTRLMessageProcessor(LoggingInformation logger)
			: base(logger, "CTL", "Control(CONTRL) message")
		{
		}

		#region Implementation
		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			CONTRLMessage myMessage = (CONTRLMessage)message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet());
			UCISegment myUCI = GetUCISegment(myMessage);
			if (myUCI == null)
			{
				Logger.ContinueDebugLog("Message did not include mandatory UCI segment.");
				return EDIMessage.Status.Error;
			}

			string controlReferenceNumber = myUCI.InterchangeControlReference;

			InterchangeSenderElements senderElements = myUCI.InterchangeSender;
			if (senderElements == null)
			{
				Logger.ContinueDebugLog("Message did not contain mandatory sender information.");
				return EDIMessage.Status.Error;
			}
			string creator = senderElements.SenderIdentification;

			InterchangeRecipientElements recipientElements = myUCI.InterchangeRecipient;
			if (senderElements == null)
			{
				Logger.ContinueDebugLog("Message did not contain mandatory recipient information.");
				return EDIMessage.Status.Error;
			}
			string recipient = recipientElements.RecipientIdentification;

			string actionCoded = myUCI.ActionCoded;
			string syntaxErrorCoded = myUCI.SyntaxErrorCoded;

			Logger.ContinueDebugLog("Control Reference Number: " + controlReferenceNumber);

			ZQuery interchangeFilter = new ZQuery();
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_From, creator);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_To, recipient);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, controlReferenceNumber);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIMessage.ApplicationCodes.CMR);

			CMRInterchange interchange = message.Factory.LoadTop1<CMRInterchange>(interchangeFilter);
			if (interchange == null)
			{
				Logger.ContinueDebugLog("Could not find the interchange being referred to by the CONTRL message.");
				return EDIMessage.Status.Error;
			}
			message.EM_LinkedObject = interchange;

			switch (actionCoded)
			{
				case "4":
					ProcessInterchangeRejection(message, syntaxErrorCoded, interchange, myMessage);
					break;
				case "7":
				case "8":
					#region Interchange Acknowledged Code
					Logger.ContinueDebugLog("Message Subtype: Interchange Acknowledgement");
					ProcessInterchangeAcknowledged(interchange, message, myMessage);
					#endregion
					break;
				default:
					{
						Logger.ContinueDebugLog("CONTRL message ActionCoded value " + actionCoded + " not recognised");
						return EDIMessage.Status.Error;
					}
			}

			return EDIMessage.Status.Received;
		}

		protected void ProcessInterchangeRejection(EDIMessage message, ZString syntaxErrorCoded, CMRInterchange interchange, CONTRLMessage cONTRLMessage)
		{
			Logger.ContinueDebugLog("Message Subtype: Interchange Rejection");
			message.EM_MessageSubType = nameof(Core.Constants.AUCTLMessageSubType.REJ);
			CancelLinkedPendingOutturnMessagesIfExisting(message, interchange);
			if (syntaxErrorCoded == "26")
			{
				Logger.ContinueDebugLog("Duplicate interchange reported.  Treating this as acknowledgement.");
				ProcessInterchangeAcknowledged(interchange, message, cONTRLMessage);
				return;
			}

			ZQuery filter = new ZQuery();
			filter.AddToFilter(EDIMessageSchema.EM_EI, interchange.PK);
			EDIMessage[] outgoingMessages = (EDIMessage[])message.Factory.Load(typeof(EDIMessage), filter);
			foreach (EDIMessage outgoingMessage in outgoingMessages)
			{
				switch (syntaxErrorCoded)
				{
					case "23":
					case "023":
						Logger.ContinueDebugLog("Messge Signatory Unknown (may not be registered with Customs)");
						break;
					default:
						ProcessMessageRejection(message, outgoingMessage);
						break;
				}
			}
		}

		public void CancelLinkedPendingOutturnMessagesIfExisting(EDIMessage message, CMRInterchange interchange)
		{
			var outgoingMessageQuery = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, new ZString[] { CMRMessage.CMRMessageTypes.AIROUT, CMRMessage.CMRMessageTypes.SEAOUT });
			outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationReference, SQLComparisonOperator.StartsWith, CMRCUSRESMessage.splitMessageIdentifier);
			outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, interchange.EI_SystemCreateTimeUtc);
			var outgoingLinkedMessage = message.Factory.LoadTop1<EDIMessage>(outgoingMessageQuery);
			if (outgoingLinkedMessage != null)
			{
				var pendingMessagesFilter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, outgoingLinkedMessage.EM_LinkUniqueID);
				pendingMessagesFilter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Pending);
				pendingMessagesFilter.AddToFilter(EDIMessageSchema.EM_MessageType, outgoingLinkedMessage.EM_MessageType);
				pendingMessagesFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				pendingMessagesFilter.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, outgoingLinkedMessage.EM_SystemCreateTimeUtc);
				const string indexName = "NR_RX__EM_LinkUniqueID"; // there is no existing constant for this index name
				pendingMessagesFilter.TableIndexHints.Add(new TableIndexHint(indexName));
				EDIMessage[] linkedPendingMessages = message.Factory.Load<EDIMessage>(pendingMessagesFilter);
				foreach (EDIMessage pendingMessage in linkedPendingMessages)
				{
					pendingMessage.EM_Status = EDIMessage.Status.Cancelled;
				}

				CMRCUSRESMessage.AddSplitMessageLog(outgoingLinkedMessage);
			}
		}

		protected void ProcessInterchangeAcknowledged(CMRInterchange interchange, EDIMessage message, CONTRLMessage cONTRLMessage)
		{
			var outgoingMessagesFilter = new ZQuery();
			outgoingMessagesFilter.AddToFilter(EDIMessageSchema.EM_EI, interchange.PK);
			var outgoingMessages2 = (EDIMessage[])message.Factory.Load(typeof(EDIMessage), outgoingMessagesFilter);

			foreach (SegmentGroup1 group1 in cONTRLMessage.Group1)
			{
				if (group1.UCM.Count > 0)
				{
					var uCM = group1.UCM[0];
					if (uCM.ActionCoded == "4")
					{
						var messageInError = uCM.MessageReferenceNumber;
						foreach (var outgoingMessage in outgoingMessages2)
						{
							if (outgoingMessage.EM_MessageNum == messageInError)
							{
								ProcessMessageRejection(message, outgoingMessage);
								break;
							}
						}
					}
				}
			}

			message.EM_MessageSubType = nameof(Core.Constants.AUCTLMessageSubType.ACK);

			foreach (var outgoingMessage in outgoingMessages2)
			{
				var eventTime = ZDateTimeOffset.Now;

				var log = outgoingMessage.Logs.AddNew(AutoEvents.InterchangeAcknowledgedAsSent, eventTime);
				log.SL_GS_NKUser = GlbStaff.CurrentUser.GS_Code;
			}
		}

		internal void ProcessMessageRejection(EDIMessage incomingMessage, EDIMessage outgoingMessage)
		{
			outgoingMessage.EM_Status = EDIMessage.Status.Rejected;
			if (outgoingMessage.EM_LinkedObject is ICMRControlMessageRespondee messageRespondee)
			{
				ZString logText = messageRespondee.UpdateStatusWhenControlMessageSyntaxError(incomingMessage, outgoingMessage);
				if (logText != ZString.Empty)
				{
					Logger.ContinueDebugLog("Updating status of " + logText);
				}
				var clonedMessage = (EDIMessage)incomingMessage.Clone();
				clonedMessage.EM_Status = EDIMessage.Status.Received;
				clonedMessage.EM_MessageSubType = EDIMessage.Status.Rejected;
				messageRespondee.Messages.Add(clonedMessage);
			}
		}

		UCISegment GetUCISegment(CONTRLMessage myMessage)
		{
			if (myMessage.UCI.Count > 0)
			{
				return myMessage.UCI[0];
			}
			else
			{
				return null;
			}
		}

		protected override ZGuid AcknowledgementEmailGroup { get { return ZGuid.Empty; } }
		protected override ZString AcknowledgementEmailMode { get { return ZString.Empty; } }
		protected override ZGuid ImpedimentEmailGroup { get { return ZGuid.Empty; } }
		protected override ZString ImpedimentEmailMode { get { return ZString.Empty; } }
		protected override ZGuid ErrorEmailGroup { get { return ZGuid.Empty; } }
		protected override ZString ErrorEmailMode { get { return Core.Constants.EmailTo.StaffMemberAndNominatedGroup; } }

		#endregion
	}
}
