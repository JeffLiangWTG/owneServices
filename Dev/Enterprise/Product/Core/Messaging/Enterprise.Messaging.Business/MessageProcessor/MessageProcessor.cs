using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.MessageProcessors
{
	public abstract class MessageProcessor
	{
		protected MessageProcessor(LoggingInformation logger, Func<EDIMessage, string> getMessageType, string messageFriendlyName)
		{
			this.Logger = logger;

			this.GetMessageTypeDelegate = getMessageType;
			this.MessageFriendlyName = messageFriendlyName;

			ErrorEmailSendCount = 0;
			AcknowledgementEmailSendCount = 0;
			StatusEmailSendCount = 0;
		}

		protected MessageProcessor(LoggingInformation logger, string messageType3CharCode, string messageFriendlyName)
			: this(logger, x => messageType3CharCode, messageFriendlyName)
		{
		}

		public void PreProcessMessage(EDIMessage message)
		{
			ProcessMessageCore(message, msg => DoPreProcessingReturningStatus(msg));
		}

		public void ProcessMessage(EDIMessage message)
		{
			ProcessMessageCore(message, msg =>
			{
				SetMessageTypes(msg);
				return DoProcessingReturningStatus(msg);
			});
		}

		protected virtual void SetMessageTypes(EDIMessage message)
		{
			var messageType = GetMessageTypeDelegate(message);
			message.EM_MessageType = messageType;
			//set the message subtype to the messages type so this in case the message sub type isn't
			//set in the processing, it won't be XXX
			message.EM_MessageSubType = messageType;
		}

		void ProcessMessageCore(EDIMessage message, Func<EDIMessage, ZString> processMessageFunc)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}
			FactorySavedAfterProcessing = message.Factory;
			try
			{
				message.EM_Status = processMessageFunc(message);
			}
			finally
			{
				FactorySavedAfterProcessing = null;
			}
		}

		protected BusinessObjectFactory FactorySavedAfterProcessing;
		public int ErrorEmailSendCount;
		public int AcknowledgementEmailSendCount;
		public int StatusEmailSendCount;

		public readonly string MessageFriendlyName;
		public readonly Func<EDIMessage, string> GetMessageTypeDelegate;

		#region Implementation

		protected virtual string DoPreProcessingReturningStatus(EDIMessage message) => EDIMessage.Status.PreProcessedOK;

		/// <summary>
		/// Process a given Edifact Message, returning a message status
		/// </summary>
		/// <param name="message">EDIMessage to process</param>
		/// <returns>The Status the Message should be set to after processing</returns>
		protected abstract string DoProcessingReturningStatus(EDIMessage message);

		protected readonly LoggingInformation Logger;

		protected Guid CompanyPK;

		protected ZString SafeGet(string[][] segment, int i, int j)
		{
			if (segment.Length > i && segment[i].Length > j)
			{
				return segment[i][j];
			}

			return null;
		}

		protected static string ConvertToNiceOutput(string message)
		{
			return UNOACharacterSet.FromUNOB(message).Replace("'", "'\r\n").Replace("?'\r\n", "?'");
		}

		protected internal void SendAcknowledgementReport(IBusiness parent, EmailDef email)
		{
			SendReport(email, parent, AcknowledgementEmailMode, AcknowledgementEmailGroup);
			AcknowledgementEmailSendCount++;
		}

		protected internal void SendImpedimentReport(IBusiness parent, EmailDef email)
		{
			SendReport(email, parent, ImpedimentEmailMode, ImpedimentEmailGroup);
			StatusEmailSendCount++;
		}

		protected internal void SendErrorReport(IBusiness parent, EmailDef email)
		{
			SendReport(email, parent, ErrorEmailMode, ErrorEmailGroup);
			ErrorEmailSendCount++;
		}

		protected void SendReport(EmailDef email, IBusiness parent, ZString emailMode, ZGuid emailGroup)
		{
			if (emailMode == Core.Constants.EmailTo.StaffMember || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
			{
				if (parent != null)
				{
					var userToNotify = GetUserToNotify(parent);
					if (userToNotify != null && !userToNotify.GS_EmailAddress.IsEmpty)
					{
						AddRecipientCore(email, userToNotify.GS_EmailAddress, RecipientDef.RecipientTypes.TO);
					}
				}
			}

			if (emailMode == Core.Constants.EmailTo.NominatedGroup || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup ||
							(ReportWhenNoParent && parent == null))
			{
				CopyGroupToEmails(parent, email, emailGroup);
			}

			SendReport(email);
		}

		protected virtual void AddRecipientCore(EmailDef emailDef, string email, RecipientDef.RecipientTypes type)
		{
			emailDef.AddRecipientForUserCommunication(email, type);
		}

		protected virtual bool ReportWhenNoParent
		{
			get { return false; }
		}

		protected virtual ZString ApplicationCodeForGetUserToNotify
		{
			get { return ZString.Empty; }
		}

		protected virtual void SendReport(EmailDef email)
		{
			if (email.Recipients.Count > 0 || email.CCRecipients.Count > 0 || email.BCCRecipients.Count > 0)
			{
				try
				{
					if (FactorySavedAfterProcessing != null)
					{
						Env.OutgoingMailManager.Create(FactorySavedAfterProcessing, email);
					}
					else // Just in case there's some error handling trying to report something when the factory has not been set.
					{
						Env.OutgoingMailManager.CreateAndSave(email);
					}
				}
				catch (EmailSendFailedException e)
				{
					Logger.Log("Couldn't send email: " + e.Message + ".  Here are the contents of the email that couln't be sent:\r\n\r\n" +
						"SUBJECT: " + email.Subject + "\r\n" +
						"BODY: " + email.Body + "\r\n");
				}
			}
		}

		protected void CopyGroupToEmails(IBusiness parent, EmailDef email, ZGuid groupToCopy)
		{
			if (!groupToCopy.IsEmpty)
			{
				GlbGroup group;
				if (parent != null)
				{
					group = (GlbGroup)parent.Factory.Load(typeof(GlbGroup), groupToCopy);
				}
				else
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					group = (GlbGroup)factory.Load(typeof(GlbGroup), groupToCopy);
				}

				if (group != null)
				{
					var emailGroupUtility = new EmailGroupUtility();

					foreach (GlbStaff staff in group.Staff)
					{
						if (!staff.GS_EmailAddress.IsEmpty && !emailGroupUtility.IsHostNotificationEmail(staff.GS_EmailAddress))
						{
							AddRecipientCore(email, staff.GS_EmailAddress, RecipientDef.RecipientTypes.CC);
						}
					}
				}
			}
		}

		protected virtual GlbStaff GetUserToNotify(IBusiness parent)
		{
			return GetLastNonBatchProcessorStaffToSendMessage(parent);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "is part of sql expression")]
		internal GlbStaff GetLastNonBatchProcessorStaffToSendMessage(IBusiness parent)
		{
			var rawSQLQuery =
				"SELECT TOP 1 " + EDIMessage.Schema.EM_SystemCreateUser +
				" FROM " + EDIMessageSchema.Constants.SqlSchemaName + "." + EDIMessageSchema.Constants.TableName +
				" WHERE " + EDIMessage.Schema.EM_SystemCreateUser + " != @BatchProcessorInitials" +
				" AND " + EDIMessage.Schema.EM_ReceiveTransmit + " = @TransmitCode" +
				" AND " + EDIMessage.Schema.EM_LinkUniqueID + " = @ParentPK" +
				(ApplicationCodeForGetUserToNotify.IsEmpty ? "" : " AND " + EDIMessage.Schema.EM_ApplicationCode + " = @AppCode") +
				" ORDER BY " + EDIMessage.Schema.EM_SystemCreateTimeUtc + " DESC";

			var @params = new ZSqlParameterCollection();
			@params.Add("@BatchProcessorInitials", User.ServiceUserCode, EDIMessageSchema.EM_SystemCreateUser);
			@params.Add("@TransmitCode", "TRX", EDIMessageSchema.EM_ReceiveTransmit);
			@params.Add("@ParentPK", parent.Identifier, EDIMessageSchema.EM_LinkUniqueID);
			if (!ApplicationCodeForGetUserToNotify.IsEmpty)
			{
				@params.Add("@AppCode", ApplicationCodeForGetUserToNotify, EDIMessageSchema.EM_ApplicationCode);
			}

			var dynamicCollection = new DynamicBusinessObjectCollection(parent.Factory);
			dynamicCollection.Load(rawSQLQuery, @params);

			if (dynamicCollection.Count == 1)
			{
				return parent.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)dynamicCollection[0][EDIMessage.Schema.EM_SystemCreateUser]);
			}
			else
			{
				return null;
			}
		}

		protected abstract ZGuid AcknowledgementEmailGroup
		{
			get;
		}

		protected abstract ZString AcknowledgementEmailMode
		{
			get;
		}

		protected abstract ZGuid ImpedimentEmailGroup
		{
			get;
		}

		protected abstract ZString ImpedimentEmailMode
		{
			get;
		}

		protected abstract ZGuid ErrorEmailGroup
		{
			get;
		}

		protected abstract ZString ErrorEmailMode
		{
			get;
		}

		#endregion
	}
}
