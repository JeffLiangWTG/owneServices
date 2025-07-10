using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public abstract class CAUniversalEventMessageProcessor
	{
		protected CAUniversalEventMessageProcessor(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, BusinessObject businessObject)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.universalEvent = Argument.NotNull(universalEvent, "universalEvent");
			this.message = Argument.NotNull(message, "message");
			this.businessObject = businessObject;
			this.factory = message.Factory;
		}

		protected readonly IXmlSessionTracker logger;
		protected readonly UniversalEvent universalEvent;
		protected readonly UniversalEventMessage message;
		protected readonly BusinessObject businessObject;
		protected readonly BusinessObjectFactory factory;

		public void Process()
		{
			SetApplicationReference();
			var result = ProcessCore();
			if (result)
			{
				UpdateMessageStatus();
			}
			SetMessageBranch();
			using (DisposableEnvironment.ForBranch(message.EM_GB.ToGuid()))
			{
				SendReport(GetEmailAndSetOnMessage(), NotifyEmailMode, NotifyEmailGroup);
			}
		}

		void SetApplicationReference()
		{
			ZString eventTime = message.RNSProcessingDate.ToLongTimeString();
			ZString interchangeNumber = message.EDIFACTInterchangeNumber.ToString().PadLeft(4, '0');
			ZString messageNumber = message.EDIFACTMessageNumber.ToString().PadLeft(4, '0');
			message.EM_ApplicationReference = eventTime + interchangeNumber + messageNumber;
		}

		void SetMessageBranch()
		{
			var lastSendMessage = GetLastSendMessage(businessObject);
			if (lastSendMessage != null)
			{
				message.EM_GB = lastSendMessage.EM_GB;
			}
			else
			{
				if (businessObject is CusEntryHeader entryHeader)
				{
					message.EM_GB = entryHeader.Branch.PK;
				}
				else if (businessObject is JobDeclaration declaration)
				{
					message.EM_GB = declaration.Branch.PK;
				}
				else if (businessObject is CusCAeMHMaster masterBill)
				{
					message.EM_GB = masterBill.Branch.PK;
				}
				else if (businessObject is CusCAeMHHouse houseBill)
				{
					message.EM_GB = houseBill.MasterBill.Branch.PK;
				}
				else
				{
					message.EM_GB = GlbBranch.CurrentBranch.PK;
				}
			}
		}

		protected EDIMessage GetLastSendMessage(BusinessObject parent)
		{
			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, parent.TableName);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, parent.PK);
			query.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc + " DESC";
			return factory.LoadTop1<EDIMessage>(query);
		}

		protected virtual bool ProcessCore()
		{
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			return true;
		}

		void UpdateMessageStatus()
		{
			var entryHeader = businessObject as CusEntryHeader;
			if (entryHeader != null)
			{
				if (ShouldUpdateMessageStatus(entryHeader))
				{
					var status = entryHeader.CH_Status;
					entryHeader.CH_Status = GetCalculatedMessageStatus(status);
				}

				UpdateEntryStatusIfNeeded(entryHeader);
			}
		}

		protected virtual ZBool ShouldUpdateMessageStatus(CusEntryHeader entryHeader)
		{
			return entryHeader.StatusCalculator.IsAwaitingReply(entryHeader.CH_Status);
		}

		protected virtual ZString GetCalculatedMessageStatus(ZString currentStatus) => ZString.Empty;

		protected virtual void UpdateEntryStatusIfNeeded(CusEntryHeader entryHeader)
		{
		}

		EmailDef GetEmailAndSetOnMessage()
		{
			var group = factory.Load<GlbGroup>(NotifyEmailGroup);

			var mailToPairList = factory.GetCachedValue("Enterprise.Customs.CA.Business.MessageProcessors.CAUniversalEventMessageProcessor|EmailToCodeDescriptionPairList", () => new CodeDescriptionPairList(OLookUpEditType.EmailTo));
			var modePair = mailToPairList[NotifyEmailMode];

			var emailBuilder = new EmailDefBuilder(GetMessageTypeDescription(), message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.IIDResponseWithNotifyInfo);
			emailBuilder.AddArgReplacementRange(GetEmailHeader(), GetAssociatedBusinessObjectDescription(), group == null ? ZString.Empty : $"{group.GG_Code} - {group.GG_Desc}", modePair == null ? ZString.Empty : $"{modePair.Code} - {modePair.Description}");
			var messageInterpretation = GetMessageInterpretation();
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, messageInterpretation);
			message.EM_MessageInterpretation = messageInterpretation;
			return emailBuilder.ToEmail();
		}

		#region Email Subject 

		protected abstract ZString GetMessageTypeDescription();

		#endregion

		#region Email Header 

		ZString GetEmailHeader()
		{
			var result = ZString.Empty;
			if (businessObject != null)
			{
				var jobLink = ZString.Empty;
				if (businessObject is CusEntryHeader entryHeader)
				{
					var obj = entryHeader.Declaration;
					if (obj != null)
					{
						jobLink = GetJobLinkFromDeclaration(obj);
					}
				}
				else if (businessObject is JobDeclaration declaration)
				{
					jobLink = GetJobLinkFromDeclaration(declaration);
				}
				else if (businessObject is ForwardingShipment shipment)
				{
					jobLink = EmailDefBuilder.GetJobLink(shipment, shipment.JS_UniqueConsignRef);
				}
				else if (businessObject is CusCAeMHHouse house)
				{
					var obj = house.MasterBill;
					if (obj != null)
					{
						jobLink = GetJobLinkFromMasterBill(obj);
					}
				}
				else if (businessObject is CusCAeMHMaster master)
				{
					jobLink = GetJobLinkFromMasterBill(master);
				}
				else if (businessObject is ForwardingConsol consol)
				{
					jobLink = EmailDefBuilder.GetJobLink(consol, consol.JK_UniqueConsignRef);
				}
				result = ZString.Format(@"Job Number : {0}", jobLink);
			}
			return result;
		}

		ZString GetJobLinkFromDeclaration(JobDeclaration declaration)
		{
			var result = ZString.Empty;
			if (declaration != null)
			{
				using (DisposableEnvironment.ForBranch(declaration.Branch.PK.ToGuid()))
				{
					result = EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference);
				}
			}
			return result;
		}

		ZString GetJobLinkFromMasterBill(CusCAeMHMaster masterBill)
		{
			return masterBill != null ? EmailDefBuilder.GetJobLink(masterBill, masterBill.BP_MessageReference) : string.Empty;
		}

		protected abstract ZString GetResponseTypeDescription();

		protected ZString GetAssociatedBusinessObjectDescription()
		{
			return ZString.Format(@"{0} has been received from the CBSA for a(n) {1}{2}.", GetResponseTypeDescription(), GetPrefixStringIfIsTest(), GetBusinessObjectType());
		}

		protected virtual ZString GetBusinessObjectType()
		{
			return "IID Declaration";
		}

		ZString GetPrefixStringIfIsTest()
		{
			var isTest = UniversalEventMessageProcessorHelper.GetContextValueByType(universalEvent.ContextCollection, UniversalEventMessageProcessorConstants.ContextType.IsTest);
			return isTest == UniversalEventMessageProcessorConstants.XmlBoolValues.False ? string.Empty : "Test ";
		}

		#endregion

		#region Email Body

		protected virtual ZString GetMessageInterpretation()
		{
			return new UniversalEventMessageInterpretationGenerator(factory, universalEvent).GetInterpretatedHTML();
		}

		#endregion

		#region SendReport

		protected abstract ZGuid NotifyEmailGroup { get; }

		protected abstract ZString NotifyEmailMode { get; }

		protected T GetFallBackValueFromRegistry<T>(Func<StronglyTypedRegistryItem<T>> registryItem)
		{
			var branch = message.Branch ?? GlbBranch.CurrentBranch;
			var companyPK = branch.GB_GC.ToGuid();
			var branchPK = branch.PK.ToGuid();

			return registryItem().GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		void SendReport(EmailDef email, ZString emailMode, ZGuid emailGroup)
		{
			if (emailMode == Core.Constants.EmailTo.StaffMember || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
			{
				if (businessObject != null && (businessObject is CusEntryHeader || businessObject is CusCAeMHMaster || businessObject is CusCAeMHHouse))
				{
					var userToNotify = GetUserToNotify(businessObject);
					if (userToNotify != null && !userToNotify.GS_EmailAddress.IsEmpty)
					{
						AddRecipientCore(email, userToNotify.GS_EmailAddress, RecipientDef.RecipientTypes.TO);
					}
				}
			}

			if (emailMode == Core.Constants.EmailTo.NominatedGroup || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
			{
				CopyGroupToEmails(email, emailGroup);
			}
			SendReport(email);
		}

		void SendReport(EmailDef email)
		{
			if (email.Recipients.Count > 0 || email.CCRecipients.Count > 0 || email.BCCRecipients.Count > 0)
			{
				try
				{
					if (factory != null)
					{
						Env.OutgoingMailManager.Create(factory, email);
					}
					else // Just in case there's some error handling trying to report something when the factory has not been set.
					{
						Env.OutgoingMailManager.CreateAndSave(email);
					}
				}
				catch (EmailSendFailedException e)
				{
					logger.LogBoth(Integration.LogType.Error, "Couldn't send email: " + e.Message + ".  Here are the contents of the email that couln't be sent:\r\n\r\n" +
						"SUBJECT: " + email.Subject + "\r\n" +
						"BODY: " + email.Body + "\r\n");
				}
			}
		}

		protected GlbStaff GetUserToNotify(BusinessObject parent)
		{
			var applicationCode = GetApplicationCodeForGetUserToNotify(parent);
			var applicationCodeClause = applicationCode.IsEmpty ? string.Empty : $@"		AND EM_ApplicationCode = '{applicationCode}'
";
			var staffQuery = new ZDBOnlyQuery(typeof(GlbStaff));
			var sql = $@"
	GS_Code IN
	(
		SELECT TOP 1 EM_SystemCreateUser
		FROM dbo.EDIMessage
		WHERE
		(
			EM_SystemCreateUser NOT IN ('{User.ServiceUserCode}', '{User.InterchangeUserCode}', '{User.WebUserCode}', '{User.UnKnownUserCode}')
		)
		AND EM_LinkUniqueID IN ( SELECT Value FROM @LinkUniqueIDs )
		AND EM_ReceiveTransmit = 'TRX' 
{applicationCodeClause}
		ORDER BY EM_SystemCreateTimeUtc DESC
	)";

			var sqlParams = new ZSqlParameterCollection();
			if (parent is CusEntryHeader || parent is CusCAeMHHouse)
			{
				sqlParams.Add(ZSqlParameter.New("@LinkUniqueIDs", new[] { parent.PK }, EDIMessageSchema.EM_LinkUniqueID, isTableValued: true));
			}
			else if (parent is CusCAeMHMaster master)
			{
				var uniqueIDs = new List<ZGuid>();
				uniqueIDs.Add(parent.PK);
				uniqueIDs.AddRange(master.HouseBills.Select(x => x.PK));
				sqlParams.Add(ZSqlParameter.New("@LinkUniqueIDs", uniqueIDs.ToArray(), EDIMessageSchema.EM_LinkUniqueID, isTableValued: true));
			}

			staffQuery.AddFilterAndZSQLParameterCollection(sql, sqlParams);
			staffQuery.AddToFilter(GlbStaffSchema.GS_EmailAddress, SQLComparisonOperator.NotEqual, ZString.Empty);
			var result = parent.Factory.LoadTop1<GlbStaff>(staffQuery);
			if (result == null && parent is CusEntryHeader entryHeader)
			{
				var declaration = entryHeader.Declaration;
				if (declaration != null)
				{
					result = declaration.CusAgent;
					if (result == null || result.GS_EmailAddress.IsEmpty)
					{
						result = parent.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, declaration.JE_SystemCreateUser);
					}
				}
			}
			return result;
		}

		ZString GetApplicationCodeForGetUserToNotify(BusinessObject parent)
		{
			var applicationCode = ZString.Empty;
			if (parent is CusEntryHeader)
			{
				applicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CAIMP;
			}
			else if (parent is CusCAeMHHouse || parent is CusCAeMHMaster)
			{
				applicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CAACI;
			}
			return applicationCode;
		}

		void CopyGroupToEmails(EmailDef email, ZGuid groupToCopy)
		{
			if (!groupToCopy.IsEmpty)
			{
				var group = factory.Load<GlbGroup>(groupToCopy);

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

		void AddRecipientCore(EmailDef emailDef, ZString email, RecipientDef.RecipientTypes type)
		{
			emailDef.AddRecipientForSystemCommunication(email, type);
		}

		#endregion
	}
}
