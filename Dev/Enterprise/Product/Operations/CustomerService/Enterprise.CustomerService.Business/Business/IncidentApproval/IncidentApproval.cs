using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.CustomerService.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business
{
	[CodeProperty(Schema.IA_ClientReference)]
	[DescriptionProperty(Schema.IA_IncidentSummary)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class IncidentApproval : AutoIncidentApproval, IDocManagerSupport, IIncidentApproval
	{
		public IncidentApproval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (HasBeenSentNoIncidentNumberAssigned)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		#region Approve

		public bool Approve()
		{
			bool success = false;

			IA_GS_NKApprovingStaff = GlbStaff.CurrentUser.GS_Code;
			ZString originalStatus = IA_Status;
			IA_Status = (IA_Criticality == Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest) ?
					IncidentApprovalLookups.StatusCodes.DevelopmentEstimateRequested : IncidentApprovalLookups.StatusCodes.ApprovedAndSent;

			Xsd.CustomerServiceRequest request = GetPopulatedXsd(IncidentApprovalLookups.Actions.Add);
			Send(request);

			try
			{
				SaveInternal();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				IA_Status = originalStatus;
				throw;
			}
			success = true;

			return success;
		}

		protected virtual void SaveInternal()
		{
			Factory.Save();
		}

		#endregion

		#region Properties

		#region Can Approve

		static public bool CanApprove
		{
			get { return Env.Security.IncidentApprovalApprove.IsAllowed; }
		}

		#endregion

		#region Has Been Sent

		public bool HasBeenSent
		{
			get { return !IA_Status.IsEmpty && IA_Status != IncidentApprovalLookups.StatusCodes.New; }
		}

		#endregion

		#region Has Been Sent No Incident Number Assigned

		public bool HasBeenSentNoIncidentNumberAssigned
		{
			get { return HasBeenSent && IA_IncidentNumber.IsEmpty; }
		}

		#endregion

		#region IA_Status

		[ReadOnly(true)]
		public override ZString IA_Status
		{
			get { return base.IA_Status; }
			set { base.IA_Status = value; }
		}

		#endregion

		#region IA_ClientReference

		[ReadOnly(true)]
		public override ZString IA_ClientReference
		{
			get { return base.IA_ClientReference; }
			set { base.IA_ClientReference = value; }
		}

		#endregion

		#region IA_IncidentNumber

		[ReadOnly(true)]
		public override ZString IA_IncidentNumber
		{
			get { return base.IA_IncidentNumber; }
			set { base.IA_IncidentNumber = value; }
		}

		#endregion

		#region IA_GS_NKApprovingStaff

		public bool IA_GS_NKApprovingStaff_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region IA_GS_NKReportingStaff

		public override ZPropertyInfo IA_GS_NKReportingStaffInfo
		{
			get
			{
				return GetZPropertyInfo(IncidentApprovalSchema.Constants.IA_GS_NKReportingStaff, Res.GetString("545b432e-72cb-4ef9-8074-b69ea6114c83", "Third Party Notify"));
			}
		}

		#endregion

		#region IA_Module

		public bool IA_Module_ReadOnly
		{
			get { return HasBeenSent; }
		}

		public ModuleListType ModuleType
		{
			get { return IncidentApprovalLookups.GetModuleListType(IA_Criticality); }
		}

		#endregion

		#region IA_ModuleDescription

		public ZString IA_ModuleDescription
		{
			get
			{
				switch (ModuleType)
				{
					case ModuleListType.MenuSection:
						return MenuSectionDescription;
					case ModuleListType.Cr8:
						return Cr8ModuleDescription;
					case ModuleListType.Cr9:
						return Cr9ModuleDescription;
					default:
						return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo IA_ModuleDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(IA_ModuleDescription)); }
		}

		#endregion

		#region MenuSection

		[List("Lookups.MenuSectionList")]
		[ReadOnlyMember(nameof(IA_Module_ReadOnly))]
		public ZString MenuSection
		{
			get { return IA_Module; }
			set { IA_Module = value; }
		}

		public ZWrappedPropertyInfo MenuSectionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(MenuSection), x => IA_ModuleInfo); }
		}

		public ZString MenuSectionDescription
		{
			get { return Lookups.MenuSectionListIncludingHidden.GetDescriptionFromCode(MenuSection); }
		}

		#endregion

		#region CR8 Module

		[List("Lookups.Cr8ModuleList")]
		[ReadOnlyMember(nameof(IA_Module_ReadOnly))]
		public ZString Cr8Module
		{
			get { return IA_Module; }
			set { IA_Module = value; }
		}

		public ZWrappedPropertyInfo Cr8ModuleInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Cr8Module), x => IA_ModuleInfo); }
		}

		public ZString Cr8ModuleDescription
		{
			get { return Lookups.Cr8ModuleList.GetDescriptionFromCode(Cr8Module); }
		}

		#endregion

		#region CR9 Module

		[List("Lookups.Cr9ModuleList")]
		[ReadOnlyMember(nameof(IA_Module_ReadOnly))]
		public ZString Cr9Module
		{
			get { return IA_Module; }
			set { IA_Module = value; }
		}

		public ZWrappedPropertyInfo Cr9ModuleInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Cr9Module), x => IA_ModuleInfo); }
		}

		public ZString Cr9ModuleDescription
		{
			get { return Lookups.Cr9ModuleList.GetDescriptionFromCode(Cr9Module); }
		}

		#endregion

		#region IA_LicenceCode

		public bool IA_LicenceCode_ReadOnly
		{
			get { return HasBeenSent; }
		}

		#endregion

		#region Quote Accepted Date

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant log reference")]
		const string QuoteAcceptedLogReference = "Formal Quotation Accepted";

		ZDateTime QuoteAcceptedDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				ZQuery query = new ZQuery(StmALogSchema.SL_Reference, QuoteAcceptedLogReference);
				query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
				StmALog[] logs = Logs.Find(query);
				if (logs.Length > 0)
				{
					result = logs[0].SL_EventTime;
				}

				return result;
			}
		}

		public ZString QuoteAcceptedDateAsText
		{
			get { return QuoteAcceptedDate.IsEmpty ? "" : Res.GetString("1784d65c-a980-4dc4-b075-b349f239059e", "Quotation accepted on: {0}", QuoteAcceptedDate.ToShortDateString()); }
		}

		#endregion

		#region Incident Needs To Confirm Criticality

		public bool NeedConfirmCriticality
		{
			get
			{
				return !HasBeenSent
					&& (IA_Criticality == Constants.CustomerService.CriticalityCodes.CR2_ModuleDown
					|| IA_Criticality == Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround
					|| IA_Criticality == Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround);
			}
		}

		#endregion

		#region Reported / Created By Staff

		public ZString ReportedByStaffCode
		{
			get
			{
				ZString result = ZString.Empty;

				if (!IsInDatabase)
				{
					result = GlbStaff.CurrentUser.GS_Code;
				}
				else
				{
					result = IA_SystemCreateUser == User.ServiceUserCode ? IA_GS_NKApprovingStaff : IA_SystemCreateUser;
				}

				return result;
			}
		}

		public GlbStaff ReportedByStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ReportedByStaffCode); }
		}

		#endregion

		#region Notification To

		public ZString NotificationTo
		{
			get
			{
				List<ZString> notificationToParties = new List<ZString>(3);
				StringBuilder builder = new StringBuilder();

				if ((SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.Value == IncidentApprovalLookups.EmailRecipientSettings.ReportedByStaff
					|| SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.Value == IncidentApprovalLookups.EmailRecipientSettings.AllParties)
					&& !ReportedByStaffCode.IsEmpty)
				{
					notificationToParties.Add(ReportedByStaffCode);
					builder.Append(ReportedByStaffCode);
					builder.Append(", ");
				}

				if ((SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.Value == IncidentApprovalLookups.EmailRecipientSettings.ApprovingStaff
					|| SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.Value == IncidentApprovalLookups.EmailRecipientSettings.AllParties)
					&& !IA_GS_NKApprovingStaff.IsEmpty && !notificationToParties.Contains(IA_GS_NKApprovingStaff))
				{
					notificationToParties.Add(IA_GS_NKApprovingStaff);
					builder.Append(IA_GS_NKApprovingStaff);
					builder.Append(", ");
				}

				if (!IA_GS_NKReportingStaff.IsEmpty && !notificationToParties.Contains(IA_GS_NKReportingStaff))
				{
					notificationToParties.Add(IA_GS_NKReportingStaff);
					builder.Append(IA_GS_NKReportingStaff);
					builder.Append(", ");
				}

				return Res.GetString("5e5d3a01-c3b0-45f4-972a-b7397fe3f841", "Notification To:") + " " + builder.ToString().TrimEnd(' ', ',');
			}
		}

		#endregion

		#region Is Updating By Response Processor

		public bool IsUpdatingByResponseProcessor { get; set; }

		#endregion

		#region Incident Key

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant log reference")]
		public const string IncidentKeyLogReference = "Incident Key";

		public string IncidentKey
		{
			get
			{
				string result;

				StmALog[] keyLogs = Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, IncidentKeyLogReference));
				if (keyLogs.Length > 0)
				{
					result = keyLogs[0].SL_Reference.Substring(IncidentKeyLogReference.Length + 1);
				}
				else
				{
					result = PK.ToString();
				}

				return result;
			}
		}

		#endregion

		#region Last Sync Time

		public ZDateTime LastSyncTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				var logs = Logs.Find(GetQueryForLastSyncTimeLog());
				if (logs.Length > 0 && logs[0].SL_Reference.Length > LastSyncTimeLogReference.Length)
				{
					string lastSyncTimeAsText = logs[0].SL_Reference.Substring(LastSyncTimeLogReference.Length + 1);
					result = ZDateTimeConversionHelper.ParseRoundTripFormatString(lastSyncTimeAsText);
				}
				return result;
			}
			set
			{
				var logs = Logs.Find(GetQueryForLastSyncTimeLog());
				string newReference = string.Format("{0} {1}", LastSyncTimeLogReference, value.ToString("o"));  // Standard datetime format as "yyyy-MM-ddTHH:mm:ss.fffffff"
				if (logs.Length > 0)
				{
					logs[0].UpdateReference(newReference);
				}
				else
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(AutoEvents.EditedARecord, newReference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		static ZQuery GetQueryForLastSyncTimeLog()
		{
			ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, LastSyncTimeLogReference);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			return query;
		}

		const string LastSyncTimeLogReference = "LastSyncTime";

		#endregion

		#region Closed Within 7 Days

		public ZBool IsClosedWithin7Days
		{
			get
			{
				return IA_Status == IncidentApprovalLookups.StatusCodes.Closed && LastCloseTime > ZDateTime.UtcNow.AddDays(-7);
			}
		}

		ZDateTime LastCloseTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				ZQuery query = new ZQuery(StmALogSchema.SL_Reference, LastCloseTimeLogReference);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusChangeCode);
				query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
				var logs = Logs.Find(query);
				if (logs.Length > 0)
				{
					result = logs[0].SL_PostedTimeUtc;
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log reference")]
		const string LastCloseTimeLogReference = "Incident Closed";

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var name = new IncidentApprovalData().HumanReadableName;
				var id = !IA_IncidentNumber.IsEmpty ? IA_IncidentNumber : IA_ClientReference;
				return name + " " + id;
			}
		}

		protected override ZString HumanReadableShortcutNameCore => HumanReadableNameCore + " - " + IA_IncidentSummary;

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			IA_LicenceCode = Env.CurrentCompany.GetLicenceCode();

			if (!SystemDataRegistry.Instance.CustomerStatuses.Value.ContainsCode(IA_ClientSpecifiedStatus))
			{
				IA_ClientSpecifiedStatus = ZString.Empty;
			}

			IA_ActiveModuleId = NotAvailableActiveModuleID;
		}

		public static readonly ZString NotAvailableActiveModuleID = "N/A";
		public static string DefaultClientSpecificMenuSection { get { return MandatoryCustomerServiceMenuSectionList.Codes.Other; } }

		public void SetCurrentModule(string moduleId, string menuSectionCode)
		{
			IA_ActiveModuleId = moduleId;
			MenuSectionCode = menuSectionCode;
		}

		public string MenuSectionCode { get; private set; }

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public void UpdateLastEditDetails()
		{
			IA_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			IA_SystemLastEditUser = Env.CurrentUser.Initials;
		}

		#endregion

		#region Log Incident Criticality Change

		public void LogIncidentCriticalityChange(ZString userMessage)
		{
			if (!userMessage.IsEmpty)
			{
				AddUserMessageToEConversation(userMessage);
			}
			string logText = Res.GetString("45479ed7-bd48-4606-84fc-7afd0bbc96a5", "Criticality changed from {0} to {1}", IA_CriticalityInfo.OriginalValue, IA_Criticality);
			AddSystemLogToEConversation(logText);
		}

		#endregion

		#region Add Incident Key

		public void AddIncidentKey(string key)
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Logs.AddNew(AutoEvents.EditedARecord, IncidentKeyLogReference + " " + key);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		#endregion

		#region Populate Xsd

		internal Xsd.CustomerServiceRequest GetPopulatedXsd(string action)
		{
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.SentTimeUtc = ZDateTime.UtcNow.ToString("o");
			request.Action = action;
			PopulateIncidentDetails(request);
			return request;
		}

		void PopulateIncidentDetails(Xsd.CustomerServiceRequest request)
		{
			request.PK = PK.ToString();
			request.ClientReferenceNumber = IA_ClientReference;
			request.IncidentNumber = IA_IncidentNumber;
			request.Criticality = IA_Criticality;
			request.Status = IA_Status;
			request.Product = IncidentApprovalLookups.EnterpriseProductCode;
			request.Module = IA_Module;
			request.ActiveModuleId = IA_ActiveModuleId;
			request.IncidentSummary = IA_IncidentSummary;
			request.IncidentDetails = IA_IncidentDetails;
			request.LicenceCode = IA_LicenceCode;

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			request.DatabaseNumber = registrationKey.DatabaseNumber;
			request.Password = registrationKey.Password;

			var companyCode = IA_LicenceCode.SubstringSafe(3, 3);
			var selectedCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
			if (selectedCompany != null)
			{
				request.CompanyCode = selectedCompany.GC_Code;
				request.CompanyName = selectedCompany.GC_Name;
				request.CompanyCountry = selectedCompany.GC_RN_NKCountryCode;
			}

			if (ApprovingStaff != null)
			{
				request.ApprovingUser = StaffContactValueObjectHelper.GetStaffNameForXsd(ApprovingStaff);
				request.ApprovingUserEmail = ApprovingStaff.GS_EmailAddress;
				request.Language = ApprovingStaff.GS_WorkingLanguage;
				if (request.Action == IncidentApprovalLookups.Actions.Add)
				{
					var staffItem = StaffContactValueObjectHelper.StaffToContact(ApprovingStaff);
					request.Staff.Add(staffItem);
				}
			}

			if (ReportedByStaff != null)
			{
				request.ReportingStaffMemberName = StaffContactValueObjectHelper.GetStaffNameForXsd(ReportedByStaff);
				request.ReportingStaffEmail = ReportedByStaff.GS_EmailAddress;
			}

			PopulateEDocsUpdate(request);
			PopulateEConversationUpdate(request);
			PopulateEConversationMessageRatingUpdate(request);
		}

		void PopulateEDocsUpdate(Xsd.CustomerServiceRequest request)
		{
			// Get all attachments if this hasn't been sent, or just new attachments if it has.
			bool attachAll = request.Action == IncidentApprovalLookups.Actions.Add;
			GetNewAttachments(attachAll);

			if (newPublishedAttachments != null && newPublishedAttachments.Count > 0)
			{
				request.Attachments = newPublishedAttachments;
				newPublishedAttachments = null;
			}
		}

		void PopulateEConversationUpdate(Xsd.CustomerServiceRequest request)
		{
			newLocalPublishedMessages = new Xsd.ConversationMessageCollection();
			EConversation.GetNewLocalPublishedMessages(newLocalPublishedMessages);

			if (newLocalPublishedMessages.Count > 0)
			{
				request.IncidentConversationUpdate = newLocalPublishedMessages;
				newLocalPublishedMessages = null;
			}
		}

		void PopulateEConversationMessageRatingUpdate(Xsd.CustomerServiceRequest request)
		{
			var ratingUpdates = new Xsd.ConversationMessageRatingCollection();
			var messages = EConversation.GetTimeOrderedMessages();
			foreach (var message in messages)
			{
				if (message.HasRatingChanged)
				{
					var rating = ratingUpdates.AddNew();
					rating.Id = message.Id.ToString();
					rating.UserCode = GlbStaff.CurrentUser.GS_Code;
					rating.UserName = GlbStaff.CurrentUser.GS_FullName;
					rating.Rating = message.Rating;
				}
			}

			request.ConversationMessageRatingUpdate = ratingUpdates;
		}

		Xsd.ConversationMessageCollection newLocalPublishedMessages;

		#endregion

		#region Send

		internal void Send(Xsd.CustomerServiceRequest request)
		{
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));
			var interchange = SystemMessage.CreateSecureInterchange(Factory, SystemMessageList.Descriptions.CustomerServiceRequest, serializer, request);
			var company = Env.CurrentCompany;
			Logs.AddNew(AutoEvents.Transferred, "Customer Service Request from " + company.Name + " - " + company.Code);
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();

			PopulateFieldsOnSaving();
			LogQuoteAcceptedDate();
			LogIncidentCloseDate();
		}

		readonly List<StmALog> logsToDeleteIfSaveFails = new List<StmALog>();

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					IA_ClientReference = ZString.Empty;
				}

				foreach (var log in logsToDeleteIfSaveFails)
				{
					log.Delete();
				}
			}

			logsToDeleteIfSaveFails.Clear();
		}

		void LogQuoteAcceptedDate()
		{
			if (IA_StatusInfo.HasChanges && !IsUpdatingByResponseProcessor && IA_Status == IncidentApprovalLookups.StatusCodes.FormalQuotationAccepted)
			{
				var log = Logs.AddNew(AutoEvents.QuotationAccepted, QuoteAcceptedLogReference, ZDateTimeOffset.Now);
				logsToDeleteIfSaveFails.Add(log);
			}
		}

		void LogIncidentCloseDate()
		{
			if (IA_StatusInfo.HasChanges
				&& (IA_Status == IncidentApprovalLookups.StatusCodes.Closed || IA_Status == IncidentApprovalLookups.StatusCodes.ClosedAwaitingResponse)
				&& (ZString)IA_StatusInfo.OriginalValue != IncidentApprovalLookups.StatusCodes.ClosedAwaitingResponse)
			{
				var log = Logs.AddNew(AutoEvents.StatusChange, LastCloseTimeLogReference);
				logsToDeleteIfSaveFails.Add(log);
			}
		}

		void PopulateFieldsOnSaving()
		{
			if (!IsInDatabase)
			{
				IA_ClientReference = Env.NumberFountains.IncidentApprovalClientRef.GetNextFormatted(Factory);

				if (IA_Status.IsEmpty)
				{
					IA_Status = IncidentApprovalLookups.StatusCodes.New;
				}
			}
		}

		#endregion

		#region Modifiers

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			if (IA_Criticality == Constants.CustomerService.CriticalityCodes.CR1_SystemDown)
			{
				return property.Name != Schema.IA_ClientSpecifiedStatus;
			}
			else
			{
				return MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			}
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.IncidentApproval);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		public IncidentApprovalAttachmentCollection AttachedEDocs
		{
			get
			{
				if (attachedEDocs == null)
				{
					attachedEDocs = new IncidentApprovalAttachmentCollection(this);
					attachedEDocs.Load();
				}
				return attachedEDocs;
			}
		}
		IncidentApprovalAttachmentCollection attachedEDocs;

		#endregion

		#region Attachments

		void GetNewAttachments(bool attachAll)
		{
			newPublishedAttachments = new Xsd.CustomerServiceRequestAttachmentCollection();
			PopulateAttachments(DocManagerInfo.Files, attachAll);
			PopulateAttachments(DocManagerInfo.Documents, attachAll);

			if (!IsUpdatingByResponseProcessor && !attachAll && NewPublishedAttachments.Count > 0)
			{
				StringBuilder builder = new StringBuilder(Res.GetString("5b3da04a-47ff-47a2-afbd-91be10e922a9", "Attached to eDocs:") + " ");
				foreach (Xsd.CustomerServiceRequestAttachment attachment in NewPublishedAttachments)
				{
					builder.Append(attachment.FileName);
					builder.Append(", ");
				}
				string message = builder.ToString().TrimEnd(' ', ',');

				if (!EConversation.AnyNewLocalMessageContains(message))
				{
					AddSystemLogToEConversation(message);
				}
			}
		}

		void PopulateAttachments(IStorageDocsBaseCollection collection, bool attachAll)
		{
			foreach (IeDoc eDoc in collection)
			{
				if (attachAll || ((BusinessObject)eDoc).HasChanges)
				{
					int maxFileNameLength = StorageDocsSchema.SC_FileName.MaxLength;
					Xsd.CustomerServiceRequestAttachment attachmentItem = NewPublishedAttachments.AddNew();
					ZString fileName = eDoc.FileName;
					if (fileName.Length > maxFileNameLength)
					{
						string extension = Path.GetExtension(fileName);
						fileName = fileName.Substring(0, maxFileNameLength - extension.Length).TrimEnd() + extension;
					}

					attachmentItem.FileName = fileName;
					attachmentItem.Data = eDoc.ImageData;
				}
			}
		}

		Xsd.CustomerServiceRequestAttachmentCollection NewPublishedAttachments
		{
			get { return newPublishedAttachments ?? (newPublishedAttachments = new Xsd.CustomerServiceRequestAttachmentCollection()); }
		}
		Xsd.CustomerServiceRequestAttachmentCollection newPublishedAttachments;

		#endregion

		#region eConversation

		public void AddUserMessageToEConversation(ZString comment)
		{
			if (!string.IsNullOrWhiteSpace(comment))
			{
				EConversation.AddMessageFromLocalUser(comment);
				UpdateLastEditDetails();
			}
		}

		public void AddSystemLogToEConversation(ZString comment)
		{
			if (!string.IsNullOrWhiteSpace(comment))
			{
				EConversation.AddSystemLogFromLocalUser(comment);
			}
		}

		public EConversation EConversation
		{
			get
			{
				if (eConversation == null)
				{
					eConversation = new EConversation(this);
				}
				return eConversation;
			}
		}
		EConversation eConversation;

		public void ReloadEConversation()
		{
			NotesFactory.ClearQueryCache(StmNoteSchema.Constants.TableName);
			EConversation.Reload();
		}

		#endregion
	}
}
