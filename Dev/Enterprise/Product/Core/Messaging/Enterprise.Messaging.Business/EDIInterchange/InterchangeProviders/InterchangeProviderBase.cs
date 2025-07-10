using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Messaging.InterchangeProviders
{
	public abstract class InterchangeProviderBase : IDisposable
	{
		protected InterchangeProviderBase(NonDependentEDIMessageCollection messages)
		{
			CollatedMessages = new ArrayList();
			BuildInterchangeBatches(messages);
		}
		public void Dispose() => DisposeCore();

		public EDIInterchange[] Interchanges
		{
			get
			{
				if (interchanges == null)
				{
					PackCollatedMessagesIntoInterchanges();
				}

				return interchanges;
			}
		}
		EDIInterchange[] interchanges;

		protected abstract void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange);

		protected void AddMessageCollection(NonDependentEDIMessageCollection messages)
		{
			CollatedMessages.Add(messages);
		}

		protected void AddMessageCollection(NonDependentEDIMessageCollection[] messages)
		{
			CollatedMessages.AddRange(messages);
		}

		protected virtual UNCharacterSet CurrentUNCharacterSet
		{
			get { return new UNOACharacterSet(); }
		}

		protected virtual ZString GetInterchangeFooter(int messageCount)
		{
			return GetUNZString(messageCount.ToString());
		}

		protected virtual ZString GetInterchangeFooter(int messageCount, EDIInterchange interchange)
		{
			return GetInterchangeFooter(messageCount);
		}

		protected virtual ZString GetInterchangeFooterWithUNTsInBody(int messageCount, int uneCount)
		{
			return GetInterchangeFooter(messageCount);
		}

		protected virtual Type InterchangeType
		{
			get { return typeof(EDIInterchange); }
		}

		#region Implementation

		protected virtual void DisposeCore() { }

		#region BuildInterchangeBatches

		void BuildInterchangeBatches(NonDependentEDIMessageCollection messages)
		{
			if (messages.Count > 0)
			{
				BuildInterchangeBatchesCore(messages);
			}
			else
			{
				interchanges = Array.Empty<EDIInterchange>();
			}
		}

		protected virtual void BuildInterchangeBatchesCore(NonDependentEDIMessageCollection messages)
		{
			NonDependentEDIMessageCollection[] newCollatedMessages = Collate(messages);
			CollatedMessages.AddRange(newCollatedMessages);
		}

		#region Collate

		protected NonDependentEDIMessageCollection[] Collate(NonDependentEDIMessageCollection messages)
		{
			ArrayList result = new ArrayList();
			Hashtable messageCollections = new Hashtable();

			foreach (EDIMessage message in messages)
			{
				string collationKey = GetCollationKey(message);
				NonDependentEDIMessageCollection collectionForKey;

				if (collationKey != DoNotCollateType)
				{
					collectionForKey = (NonDependentEDIMessageCollection)messageCollections[collationKey];
					if (collectionForKey == null)
					{
						collectionForKey = new NonDependentEDIMessageCollection(messages.Factory);
						messageCollections[collationKey] = collectionForKey;
					}
				}
				else
				{
					collectionForKey = new NonDependentEDIMessageCollection(message.Factory);
					result.Add(collectionForKey);
				}

				collectionForKey.Add(message);
			}

			ArrayList keys = new ArrayList(messageCollections.Keys);
			keys.Sort();

			foreach (string key in keys)
			{
				result.Add(messageCollections[key]);
			}

			return (NonDependentEDIMessageCollection[])result.ToArray(typeof(NonDependentEDIMessageCollection));
		}

		protected virtual string GetCollationKey(EDIMessage message)
		{
			return message.CollationKey;
		}

		#endregion

		#endregion

		public void PackCollatedMessagesIntoInterchanges()
		{
			var interchanges = new List<EDIInterchange>();

			foreach (NonDependentEDIMessageCollection messages in CollatedMessages)
			{
				var interchange = GetNewInterchange(messages.Factory);
				PreparedTime = ZDateTimeOffset.Now;

				PopulateInterchange(messages, interchange);

				if (!interchange.IsDeleted)
				{
					if (messages.Count == 0) // all messages deleted
					{
						interchange.Delete();
					}
					else
					{
						interchange.EI_FooterText = GetFooterText(interchange, messages);
						interchanges.Add(interchange);
						LogInterchangeReady(messages, PreparedTime);
					}
				}
			}

			this.interchanges = interchanges.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is EDIFACT text")]
		protected virtual ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages)
		{
			var unes = interchange.EI_BodyText.Occurrences("'UNE+");
			return unes > 0 ? GetInterchangeFooterWithUNTsInBody(messages.Count, unes) : GetInterchangeFooter(messages.Count, interchange);
		}

		public ZString GetErrorsOnDiscardedMessages()
		{
			var result = new ZStringBuilder();
			foreach (NonDependentEDIMessageCollection messages in CollatedMessages)
			{
				foreach (EDIMessage message in messages)
				{
					if (message.EM_Status == EDIMessage.Status.Discarded)
					{
						var collection = message.GetNotes().GetAllNotes();
						var note = collection.OfType<StmNote>().Where(stmNote => !stmNote.IsInDatabase && stmNote.ST_Description == ProcessingLogDescription).OrderByDescending(stmNote => stmNote.ST_CreatedDateUtc).FirstOrDefault()?.ST_NoteDataAsText ?? ZString.Empty;
						if (!note.IsEmpty)
						{
							result.AppendLine(note);
						}
					}
				}
			}
			return result.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Description")]
		public const string ProcessingLogDescription = "Processing Log";

		#region SetInterchangeValuesForTransmit

		protected virtual int MaximumInterchangeSize
		{
			get { return 0; }
		}

		protected internal void SetInterchangeValuesForTransmit(EDIInterchange interchange, NonDependentEDIMessageCollection messages, ZString interchangeType, ZString to, ZString from)
		{
			using (interchange.SuspendeHubQueueStatusCalculation())
			{
				interchange.EI_InterchangeNum = ZString.Empty;//populate in OnSave()
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				interchange.EI_Priority = EDIInterchangePriorityList.Codes.High;
				interchange.EI_IsActive = true;
				interchange.EI_Status = QueuedInterchangeStatusCode(interchange);
				interchange.EI_InterchangeType = interchangeType;
				interchange.EI_To = to;
				interchange.EI_From = from;

				if (interchange.EI_To.IsEmpty || interchange.EI_From.IsEmpty)
				{
					MarkMessagesInThisInterchangeAsFailed(messages, interchange);
				}
				else
				{
					if (messages.Count > 0)
					{
						interchange.EI_BodyText = MessageBody(messages, interchange).ToString();
					}
				}
			}
		}

		protected virtual void MarkMessagesInThisInterchangeAsFailed(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (interchange.EI_To.IsEmpty)
			{
				ReportMissingCustomsInterchangeRecipientIdError();
			}

			if (interchange.EI_From.IsEmpty)
			{
				ReportMissingCustomsInterchangeSenderIdError();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception messsage")]
		protected virtual void ReportMissingCustomsInterchangeSenderIdError()
		{
			ZStringBuilder message = new ZStringBuilder();
			message.Append(string.Format(CultureInfo.InvariantCulture, "There is no Customs interchange sender id set up," + "\r\n"));
			message.Append(string.Format(CultureInfo.InvariantCulture, "for Company - {0}, Branch - {1}", GlbCompany.CurrentCompany.GC_Code, GlbBranch.CurrentBranch.GB_Code) + "\r\n");
			if (!string.IsNullOrEmpty(InstructionHowToSetInterchangeSenderID))
			{
				message.Append(string.Format(CultureInfo.InvariantCulture, "Please follow the instruction here to set it up." + "\r\n"));
				message.Append(InstructionHowToSetInterchangeSenderID);
			}
			throw new MessageProcessingException(string.Format(CultureInfo.InvariantCulture, message.ToString()), "", true, false);
		}

		protected virtual void ReportMissingCustomsInterchangeRecipientIdError()
		{
			throw new MessageProcessingException(string.Format(CultureInfo.InvariantCulture, "There is no Customs interchange recipient id passed."), "", true, false);
		}

		protected virtual ZString QueuedInterchangeStatusCode(EDIInterchange interchange)
		{
			return EDIInterchange.Status.Queued;
		}

		protected virtual StringBuilder MessageBody(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var result = new StringBuilder();
			var maximumInterchangeSize = MaximumInterchangeSize;
			bool interchangeMaxSizeRestrictionApplies = maximumInterchangeSize > 0;
			var messageTextList = new List<ZString>();

			foreach (EDIMessage message in messages.ToArray())
			{
				if (interchangeMaxSizeRestrictionApplies)
				{
					maximumInterchangeSize -= message.EM_MessageText.Length;
				}

				if (!interchangeMaxSizeRestrictionApplies || maximumInterchangeSize >= 0 || messageTextList.Count == 0)
				{
					if (interchange.EI_ApplicationCode.IsEmpty)
					{
						interchange.EI_ApplicationCode = message.EM_ApplicationCode;
					}
					if (interchange.EI_GP.IsEmpty && message.EM_GP.IsValid)
					{
						interchange.EI_GP = message.EM_GP;
					}
					interchange.ContainedMessages.Add(message);

					messageTextList.Add(GetMessageTextToMessageBody(interchange, message));
					message.EM_EI = interchange.PK;
					message.EM_Status = ProcessedMessageStatusCode(interchange);
				}
				else
				{
					messages.Remove(message.PK);
					message.ReloadSafe();
				}
			}

			AppendMessageTextToMessageBody(result, messageTextList, interchange);

			return result;
		}

		protected virtual ZString GetMessageTextToMessageBody(EDIInterchange interchange, EDIMessage message)
		{
			return message.EM_MessageText;
		}

		protected virtual ZString ProcessedMessageStatusCode(EDIInterchange interchange)
		{
			return EDIMessage.Status.Sent;
		}

		protected virtual void AppendMessageTextToMessageBody(StringBuilder stringBuilder, IEnumerable<ZString> messageTextList, EDIInterchange interchange)
		{
			foreach (var messageText in messageTextList)
			{
				stringBuilder.Append(messageText);
			}
		}

		protected abstract internal string InstructionHowToSetInterchangeSenderID
		{
			get;
		}

		#endregion

		#region GetUNB

		protected UNBSegment GetUNB(ZDateTime timeOfPreparation, ZString to, ZString toQualifier, ZString from, ZString fromQualifier, ZString addressForReverseRouting, ZString syntax, ZString syntaxVersion, ZString applicationReference, bool ackRequest, bool testIndicator, ZString recipientsReferencePassword, ZString communicationsAgreement)
		{
			UNBSegment uNB = new UNBSegment();

			uNB.DateTimeOfPreparation.Date = GetDateString(timeOfPreparation);
			uNB.DateTimeOfPreparation.Time = GetTimeString(timeOfPreparation.ToDateTime());
			uNB.InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder;
			uNB.InterchangeRecipient.RecipientIdentification = to;
			uNB.InterchangeRecipient.PartnerIdentificationCodeQualifier = toQualifier;

			if (!addressForReverseRouting.IsEmpty)
			{
				uNB.InterchangeSender.InterchangeSenderInternalIdentification = addressForReverseRouting;
			}

			uNB.InterchangeSender.SenderIdentification = from;
			uNB.InterchangeSender.PartnerIdentificationCodeQualifier = fromQualifier;
			uNB.SyntaxIdentifier.SyntaxIdentifier = syntax;
			uNB.SyntaxIdentifier.SyntaxVersionNumber = syntaxVersion;
			uNB.RecipientsReferencePassword.RecipientsReferencePassword = recipientsReferencePassword;
			uNB.CommunicationsAgreementId = communicationsAgreement;
			if (!applicationReference.IsEmpty)
			{
				uNB.ApplicationReference = applicationReference;
			}

			if (ackRequest)
			{
				uNB.AcknowlegementRequest = "1";
			}

			if (testIndicator)
			{
				uNB.TestIndicator = "1";
			}

			return uNB;
		}

		protected UNBSegment GetUNB(ZDateTime timeOfPreparation, ZString to, ZString toQualifier, ZString from, ZString fromQualifier, ZString addressForReverseRouting, ZString syntax, ZString syntaxVersion, ZString applicationReference, bool ackRequest, bool testIndicator, ZString recipientsReferencePassword)
		{
			return GetUNB(timeOfPreparation, to, toQualifier, from, fromQualifier, addressForReverseRouting, syntax, syntaxVersion, applicationReference, ackRequest, testIndicator, recipientsReferencePassword, ZString.Empty);
		}

		#endregion

		#region GetUNG

		protected UNGSegment GetUNG(ZDateTime timeOfPreparation, ZString messageType, ZString senderID, ZString senderQualifier, ZString recipientID,
ZString controllingAgency, ZString messageversion, ZString messageRelease, ZString assignedCode, ZString applicationPassword, string functionalGroupReferenceNumber = "")
		{
			UNGSegment uNG = new UNGSegment();
			uNG.FunctionalGroupIdentification = messageType;
			uNG.ApplicationSenderIdentification.SenderIdentification = senderID;
			uNG.ApplicationSenderIdentification.SenderIDQualifier = senderQualifier;
			uNG.ApplicationRecipientIdentification.RecipientsIdentification = recipientID;
			uNG.DateTimeOfPreparation.Date = GetDateString(timeOfPreparation);
			uNG.DateTimeOfPreparation.Time = GetTimeString(timeOfPreparation.ToDateTime());
			uNG.FunctionalGroupReferenceNumber = string.IsNullOrEmpty(functionalGroupReferenceNumber) ? EDIInterchange.FunctionalGroupNumberPlaceHolder : functionalGroupReferenceNumber;
			uNG.ControllingAgency = controllingAgency;
			uNG.MessageVersion.MessageVersionNumber = messageversion;
			uNG.MessageVersion.MessageReleaseNumber = messageRelease;
			uNG.MessageVersion.AssociationAssignedCode = assignedCode;
			uNG.ApplicationPassword = applicationPassword;
			return uNG;
		}

		#endregion

		#region GetUNZString

		protected ZString GetUNZString(ZString interchangeControlCount)
		{
			UNZSegment uNZ = new UNZSegment();
			uNZ.InterchangeControlCount = interchangeControlCount;
			uNZ.InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder;
			return uNZ.ToString(CurrentUNCharacterSet);
		}

		#endregion

		#region GetUNEString

		protected ZString GetUNEString(ZString groupControlCount, string functionalGroupReferenceNumber = "")
		{
			UNESegment uNE = new UNESegment();
			uNE.NumberOfMessages = groupControlCount;
			uNE.FunctionalGroupReference = string.IsNullOrEmpty(functionalGroupReferenceNumber) ? EDIInterchange.FunctionalGroupNumberPlaceHolder : functionalGroupReferenceNumber;
			return uNE.ToString(CurrentUNCharacterSet);
		}

		#endregion

		#region LogInterchangeReady

		void LogInterchangeReady(NonDependentEDIMessageCollection messages, ZDateTimeOffset now)
		{
			foreach (EDIMessage message in messages)
			{
				message.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.InterchangeReady, now);
			}
		}

		#endregion

		#region Get Date/Time String

		protected virtual string GetDateString(ZDateTime date)
		{
			return date.ToString("yyMMdd");
		}

		public string GetTimeString(DateTime time)
		{
			string result = time.Minute.ToString();
			if (result.Length == 1)
			{
				result = time.Hour.ToString() + "0" + result;
			}
			else
			{
				result = time.Hour.ToString() + result;
			}

			if (result.Length == 3)
			{
				result = "0" + result;
			}

			return result;
		}

		#endregion

		EDIInterchange GetNewInterchange(BusinessObjectFactory factory)
		{
			return (EDIInterchange)factory.New(InterchangeType);
		}

		readonly ArrayList CollatedMessages;
		protected ZDateTimeOffset PreparedTime;
		protected const string DoNotCollateType = "DONOTCOLLATE";

		#endregion
	}
}
