using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Integration;
using CargoWise.Shared;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentScanning.Business;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;
using static Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLookups;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	//TODO:missing test - need to have test derived from Enterprise.ProcessManagement.Business.Test.WorkTaskRelatedItemTestCase<T>
	// C:\git\wtg\CargoWise\Dev\Enterprise\Product\Operations\ProcessManagement\Business.Test\RelatedItem\WorkTaskRelatedItemTestCase.cs
	// similarly to IncidentManagementGroupWorkTaskRelatedItemTest or SupportIncidentRelatedItemTest (both are in this solution)

	[UserDefinedValues]
	[CodeProperty(IncidentMainSchema.Constants.IM_IncidentNumber), DescriptionProperty(IncidentMainSchema.Constants.IM_Description)]
	[UniversalCopyWithExtendedEntities]
	public class SupportIncident :
		IncidentMainBase,
		IAllowAttachEmailsToEDocs,
		IClientOrgLicenceProvider,
		IJobInvoicingPlugIn,
		IEDocsProvider,
		IIncidentEventConsumer,
		IWorkItemRelatedItem,
		ISendEmailSource,
		IWorkTaskRelatedItemSource,
		IARInvoiceSavingNotificationSubscriber,
		IRelatableActivity,
		ICustomFieldProvider,
		IEDocsPluginHostDecider,
		IConversationProvider,
		IConversationBroadcastRecipient,
		IWorkTaskTreeNode,
		IImportParentRelatedActivityInfoOnNew,
		IImportChildRelatedActivityInfoOnAttach,
		IImportChildRelatedActivityInfoOnDetach,
		IIncidentDetailsSource,
		IEDIEmailTriggeringRulesProvider,
		ITriageAssistParent
	{
		public SupportIncident(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (factory?.ServiceContainer.GetAfterOnSavingService<SupportIncidentAfterOnSavingBOProcessingService>() == null)
			{
				factory?.ServiceContainer.AddAfterOnSavingService(new SupportIncidentAfterOnSavingBOProcessingService());
			}

			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(IM_Category), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(IM_Status), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(IM_ResolutionCode), ConcurrencyPolicy.Strict);
		}

		#region Default Values and Loaded

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IM_OA_BranchAddress_ZAddress.DefaultAddressType = AddressType.OFC;
			IM_IncidentType = IncidentConstants.IncidentType.SupportIncident;
			IM_Category = SupportIncidentCategoriesList.Codes.Support;
			IM_Status = SupportIncidentLookups.Status.Open;
			IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			IM_SourceModuleId = IncidentApproval.NotAvailableActiveModuleID;
			IM_Language = GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_WorkingLanguage : (ZString)Core.SharedConstants.Languages.English;
		}

		protected override sealed EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new SupportIncidentFetchStrategy(this);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			OriginalIM_GS_NKCustServiceContact = IM_GS_NKCustServiceContact;
			OriginalIM_GS_NKAssignedToCurrent = IM_GS_NKAssignedToCurrent;

			using (SuspendSettingHasChanges())
			{
				OriginalSnapShot.TakeSnapShotLazyLoading(this, false);
				IsNewIncident = false;
			}
		}

		#region from old EDIWorkTask base class

		#region Product Area Lookup

		public ZString GetRecalculatedProductArea()
		{
			return IncidentDetailsHelper.FindProductArea(this);
		}

		#endregion

		#region Lookups

		protected override ReadOnlyCodeDescriptionPairList Statuses
		{
			get { return Lookups.StatusList; }
		}

		#endregion

		#region Business Object Overrides

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		protected override ZString HumanReadableNameCore => HumanReadableShortcutNameCore;

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = IM_IncidentNumber;

				if (Client != null)
				{
					result += " - " + Client.OH_Code;
				}

				if (!string.IsNullOrEmpty(IM_Description))
				{
					result += " - " + IM_Description;
				}

				return result;
			}
		}

		protected ZString OriginalIM_GS_NKCustServiceContact;
		protected ZString OriginalIM_GS_NKAssignedToCurrent;

		#endregion

		#region Properties

		#region Selection State

		public bool IsSelectingClient
		{
			get { return selectionState == SelectionStates.Client; }
		}

		protected enum SelectionStates
		{
			None,
			Licence,
			Client
		}

		protected SelectionStates selectionState;

		#endregion

		bool IsFromPerformAction;

		#region IM_IncidentNumber

		public bool IM_IncidentNumber_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Contact Name

		public ZString ContactName
		{
			get { return Contact?.OC_ContactName ?? ZString.Empty; }
		}

		public ZPropertyInfo ContactNameInfo
		{
			get { return GetZPropertyInfo(nameof(ContactName)); }
		}

		#endregion

		#region Contact Phone

		const string DirectContactPhoneLabel = "Dir: ";
		const string BranchOfficePhoneLabel = "Off: ";

		public ZString ContactPhoneForDisplay
		{
			get { return GetPhoneWithFallbackForDisplay(Contact); }
		}

		public ZString ContactPhone
		{
			get { return GetPhoneWithFallback(Contact); }
		}

		public ZPropertyInfo ContactPhoneInfo
		{
			get { return GetZPropertyInfo(nameof(ContactPhone)); }
		}

		protected ZString GetPhoneWithFallbackForDisplay(OrgContact contact)
		{
			ZString result = GetPhoneWithFallback(contact);
			if (!result.IsEmpty)
			{
				string phoneType = !contact.OC_Phone.IsEmpty ? DirectContactPhoneLabel : BranchOfficePhoneLabel;
				result = string.Concat(phoneType, result);
			}
			else
			{
				result = "No Contact Number";
			}
			return result;
		}

		protected ZString GetPhoneWithFallback(OrgContact contact)
		{
			ZString result = ZString.Empty;
			if (contact != null)
			{
				result = !contact.OC_Phone.IsEmpty
					? contact.OC_Phone
					: GetFirstNotEmptyPhoneFromAddress(contact.BranchAddress, BranchAddress, contact.Header.MainAddress);
			}
			return result;
		}

		ZString GetFirstNotEmptyPhoneFromAddress(params OrgAddress[] addresses)
		{
			var addressWithPhone = addresses.FirstOrDefault(address => address != null && !address.OA_Phone.IsEmpty);
			return addressWithPhone != null ? addressWithPhone.OA_Phone : ZString.Empty;
		}

		#endregion

		#region Contact Email

		IEDIEmailTriggeringRules IEDIEmailTriggeringRulesProvider.TriggeringRules
		{
			get
			{
				if (triggeringRules == null)
				{
					triggeringRules = new SupportIncidentEmailTriggeringRules(this);
				}

				return triggeringRules;
			}
		}
		SupportIncidentEmailTriggeringRules triggeringRules;

		public ZString ContactEmail
		{
			get { return GetEmailWithFallback(Contact); }
		}

		public ZPropertyInfo ContactEmailInfo
		{
			get { return GetZPropertyInfo(nameof(ContactEmail)); }
		}

		protected ZString GetEmailWithFallback(OrgContact contact)
		{
			ZString result = "";

			if (contact != null)
			{
				if (!contact.OC_Email.IsEmpty)
				{
					result = contact.OC_Email;
				}
				else if (contact.Header != null)
				{
					result = contact.Header.MainAddress.OA_Email;
				}
			}

			return result;
		}

		#endregion

		#region Log

		public ZString LogText
		{
			get
			{
				return LogNote.Text;
			}
		}

		internal void SetLogTextForTest(ZString text)
		{
			LogNote.Text = text;
		}

		public ZPropertyInfo LogTextInfo
		{
			get { return GetZPropertyInfo(nameof(LogText)); }
		}

		public int LogText_MaxLength
		{
			get { return EDIPredefinedNoteTypes.Instance.IncidentLog.TextOnlyMaxLength; }
		}

		protected UniqueNote LogNote
		{
			get
			{
				if (fLogNote == null)
				{
					fLogNote = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentLog);
				}
				return fLogNote;
			}
		}

		UniqueNote fLogNote;

		#endregion

		#region IM_ActualHoursWorked

		public bool IM_ActualHoursWorked_ReadOnly
		{
			get { return ReadOnlyStateForChargeableFields; }
		}

		#endregion

		#region IM_QuoteAmount

		public bool IM_QuoteAmount_ReadOnly
		{
			get { return ReadOnlyStateForChargeableFields; }
		}

		#endregion

		#region IM_RX_NKQuoteCurrency_ReadOnly

		protected bool IM_RX_NKQuoteCurrency_ReadOnly
		{
			get { return ReadOnlyStateForChargeableFields; }
		}

		#endregion

		#region Correspondence Email Body

		public ZString CorrespondenceBody
		{
			get { return fCorrespondenceBody; }
			set { fCorrespondenceBody = value; }
		}

		ZString fCorrespondenceBody;

		#endregion

		#region Product Description

		public ZString ProductDescription
		{
			get { return Lookups.ProductList.GetDescriptionFromCode(IM_Product); }
		}

		public ZPropertyInfo ProductDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ProductDescription)); }
		}

		#endregion

		#region Module Description

		public ZString ModuleDescription
		{
			get { return Lookups.GetModuleList(ModuleType, IM_Product, ZString.Empty).GetDescriptionFromCode(IM_Module); }
		}

		public ZPropertyInfo ModuleDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ModuleDescription)); }
		}

		#endregion

		#region Is Current Module Enabled

		public bool IsCurrentModuleEnabled
		{
			get
			{
				var moduleMapping = IncidentDetailsHelper.GetProductAreaModuleMapping(IM_Product, IM_Module, IM_Priority);
				if (moduleMapping == null)
				{
					return false;
				}

				return moduleMapping.IsEnabled;
			}
		}

		#endregion

		#region Current Version

		public ReleaseBuild ClientReportedOnVersion
		{
			get { return Factory.Load<ReleaseBuild>(IM_HL_ClientReportedOnVersion); }
		}

		#endregion

		#region Detail Note

		protected bool DetailNoteTextLoaded { get; private set; }

		public virtual ZString DetailNoteText
		{
			get
			{
				if (!DetailNoteTextLoaded)
				{
					DetailNoteTextLoaded = true;
					OriginalSnapShot.IncidentDetails = DetailNote.Text;
				}

				return DetailNote.Text;
			}
			set
			{
				DetailNote.Text = value;
				DetailNoteTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DetailNoteTextInfo
		{
			get { return GetZPropertyInfo(nameof(DetailNoteText)); }
		}

		public int DetailNoteText_MaxLength
		{
			get { return EDIPredefinedNoteTypes.Instance.IncidentDetail.TextOnlyMaxLength; }
		}

		UniqueNote DetailNote
		{
			get
			{
				if (detailNote == null)
				{
					detailNote = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentDetail);
				}
				return detailNote;
			}
		}

		UniqueNote detailNote;

		public void SetLongDetailNoteText(string noteText)
		{
			var maxNoteTextLen = DetailNoteText_MaxLength;

			if (noteText.Length <= maxNoteTextLen)
			{
				DetailNoteText = noteText;
			}
			else
			{
				const string footerText = "\r\nContinued on the Notes (Extra Incident Detail).";
				DetailNoteText = noteText.Substring(0, maxNoteTextLen - footerText.Length) + footerText;

				var startIndex = maxNoteTextLen - footerText.Length;
				var totalNotes = (int)Math.Ceiling((decimal)(noteText.Length - startIndex) / maxNoteTextLen);
				for (var currentNoteIndex = 1; currentNoteIndex <= totalNotes; currentNoteIndex++)
				{
					Notes.AddNew(true, $"Extra Incident Detail {currentNoteIndex}/{totalNotes}", noteText.Substring(startIndex, Math.Min(maxNoteTextLen, noteText.Length - startIndex)));
					startIndex += maxNoteTextLen;
				}
			}
		}

		#endregion Detail note

		#endregion

		#region Notes

		protected override void RunPreSaveValidationCore()
		{
			SynchroniseNotes();
			base.RunPreSaveValidationCore();
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var objects = new List<BusinessObject>();
				objects.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				objects.Add(Request);
				objects.AddRange(Notes.GetAllNotes());
				return objects.ToArray();
			}
		}

		#endregion

		#region Saved

		public override sealed void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				CheckAndCloseTasks();
				OnSaveSucceeded();
				NotifyInternalSubscribersForInternalLog = false;
				ValidateTimestamps();
			}
			else
			{
				if (!IsInDatabase)
				{
					IM_IncidentNumber = "";
				}
			}

			Logs.RemoveAddedLogs();
		}

		#endregion

		void CheckAndCloseTasks()
		{
			if (IsAlreadySyncingStatus || IsDeleted)
			{
				return;
			}

			using (SettingStatusSuspender.Suspend())
			{
				if (IM_Status == SupportIncidentLookups.Status.Closed)
				{
					using (SuspendCalculateStatusAndDisposition())
					{
						WorkflowItems.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
						Reload();
						var existOpenTask = WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().Any(task => task.P9_Status != "CLS" && task.P9_Status != "CAN");
						if (existOpenTask)
						{
							CloseTasks(IM_ResolutionCode, ClosingAsIsAwaitingClient);
							try
							{
								Factory.Save();
							}
							catch (ZDataConcurrencyException)
							{
							}
						}
					}
				}
			}
		}

		bool IsAlreadySyncingStatus => SettingStatusSuspender.IsSuspended;

		readonly ActionSuspender SettingStatusSuspender = new ActionSuspender();

		#region Email

		#region Assigned Staff Changed Email

		void SendEmailToAssignedStaff()
		{
			if (ShouldSendEmailToAssignedStaff)
			{
				SendEmailToChangedStaff(AssignedToCurrent, OriginalIM_GS_NKAssignedToCurrent);
			}
			OriginalIM_GS_NKAssignedToCurrent = IM_GS_NKAssignedToCurrent;
		}

		void SendEmailToCustomerServiceContact()
		{
			if (ShouldSendEmailToCustomerServiceContact)
			{
				SendEmailToChangedStaff(CustServiceContact, OriginalIM_GS_NKCustServiceContact);
			}
			OriginalIM_GS_NKCustServiceContact = IM_GS_NKCustServiceContact;
		}

		void SendEmailToChangedStaff(GlbStaff staff, ZString originalStaffNK)
		{
			if (CheckShouldSendEmailToChangedStaff(staff, originalStaffNK))
			{
				if (!staff.GS_EmailAddress.IsEmpty)
				{
					var emailContents = new AssignedStaffNotificationEmailContentBuilder(this);
					var email = EDIEmailBuilder.GetInstance(this).BuildHtmlEmailDefByTemplate(emailContents);
					if (email != null)
					{
						email.FromDisplayName = MailFromName;
						email.FromAddress = MailFromAddress;
						email.ReplyTo = MailFromAddress;
						email.AddRecipientForUserCommunication(staff.GS_EmailAddress);

						CustomerServiceEmailUniqueIdUtil.AppendMark(email, this);
						Env.OutgoingMailManager.CreateAndSave(email);
					}
				}

				StatusChanged = false;
			}
		}

		protected bool CheckShouldSendEmailToChangedStaff(GlbStaff staff, ZString originalStaffNK)
		{
			bool result = false;

			if (EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification.Value && staff != null)
			{
				bool hasAssignedToStaffChanged = (GlbStaff.CurrentUser.PK != staff.PK && originalStaffNK != staff.GS_Code);
				bool hasStatusChangedToOPEN = StatusChanged && IM_Status == SupportIncidentLookups.Status.Open;

				if (hasAssignedToStaffChanged || hasStatusChangedToOPEN)
				{
					result = true;
				}
			}

			return result;
		}

		public const string MailFromName = "ediProd System";
		public const string MailFromAddress = IncidentConstants.ManagerEmailAddress;

		#endregion

		#region Email Precondition Failure Event

		public event EventHandler<EmailPreconditionFailureArgs> EmailPreconditionFailure;

		public class EmailPreconditionFailureArgs : EventArgs
		{
			public EmailPreconditionFailureArgs(string failureReason)
			{
				this.FailureReason = failureReason;
			}

			public readonly string FailureReason;
		}

		void OnEmailPreconditionFailure(string failureReason)
		{
			if (EmailPreconditionFailure != null)
			{
				EmailPreconditionFailure(this, new EmailPreconditionFailureArgs(failureReason));
			}
		}

		protected bool CheckIsInValidStateToSendEmail()
		{
			bool valid = true;
			if (HasChanges)
			{
				valid = false;
				OnEmailPreconditionFailure("Changes have been made to this record. You must save before sending an Email.");
			}
			else
			{
				if (Client == null)
				{
					valid = false;
					OnEmailPreconditionFailure("Please specify a Client before sending an Email.");
				}
				else if (Contact == null)
				{
					valid = false;
					OnEmailPreconditionFailure("Please specify a Contact before sending an Email.");
				}
			}

			return valid;
		}

		#endregion

		#endregion

		#region Logging

		public new IncidentLogs Logs
		{
			get { return (IncidentLogs)base.Logs; }
		}

		protected override Logs GetNewLogs()
		{
			return new IncidentLogs(this);
		}

		protected override void SetJobNumberFieldOnSaving()
		{
			SetIncidentNumberIfRequired();
		}

		public SupportIncidentFieldChangedEventLogger FieldChangedEventLogger
		{
			get
			{
				if (fieldChangedEventLogger == null)
				{
					fieldChangedEventLogger = new SupportIncidentFieldChangedEventLogger(this);
				}

				return fieldChangedEventLogger;
			}
		}
		SupportIncidentFieldChangedEventLogger fieldChangedEventLogger;

		[LoggingValueChanges(ChangedFieldDescription.IM_Status)]
		public override ZString IM_Status { get => base.IM_Status; set => base.IM_Status = value; }

		[LoggingValueChanges(ChangedFieldDescription.IM_GS_NKAssignedToCurrent)]
		public override ZString IM_GS_NKAssignedToCurrent { get => base.IM_GS_NKAssignedToCurrent; set => base.IM_GS_NKAssignedToCurrent = value; }

		[LoggingValueChanges(ChangedFieldDescription.IM_GS_NKCustServiceContact)]
		public override ZString IM_GS_NKCustServiceContact { get => base.IM_GS_NKCustServiceContact; set => base.IM_GS_NKCustServiceContact = value; }

		[LoggingValueChanges(ChangedFieldDescription.IM_ClosureResolution)]
		public override ZString IM_ClosureResolution
		{
			get => base.IM_ClosureResolution;
			set => base.IM_ClosureResolution = value;
		}

		public static class ChangedFieldDescription
		{
			public const string IM_Status = "Status";

			public const string IM_ResolutionCode = "Disposition";

			public const string IM_SourceModuleId = "Menu Item";

			public const string IM_Category = "Stage";

			public const string IM_GS_NKAssignedToCurrent = "Assigned User";

			public const string IM_GS_NKCustServiceContact = "Cust Svc";

			public const string IM_ClosureResolution = "Resolution Method";
		}

		GlbStaff OriginalAssignedUser
		{
			get { return !IM_GS_NKAssignedToCurrentInfo.OriginalValue.IsEmpty ? Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)IM_GS_NKAssignedToCurrentInfo.OriginalValue) : null; }
		}

		GlbStaff OriginalCustomerServiceContact
		{
			get { return !IM_GS_NKCustServiceContactInfo.OriginalValue.IsEmpty ? Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)IM_GS_NKCustServiceContactInfo.OriginalValue) : null; }
		}

		#endregion

		#region Tasks

		public ProcessTask CurrentTask
		{
			get
			{
				if (currentTask == null)
				{
					currentTask = new CachedProperty<ProcessTask>(Factory, delegate
					{
						return TaskFinder.FindCurrentStartableTask();
					}
					);
				}
				return currentTask.Value;
			}
		}
		CachedProperty<ProcessTask> currentTask;

		public SupportIncidentProcessTask CurrentOrNextTask
		{
			get
			{
				if (currentOrNextTask == null)
				{
					currentOrNextTask = new CachedProperty<SupportIncidentProcessTask>(Factory, delegate
					{
						return TaskFinder.FindCurrentOrNextStartableTask() as SupportIncidentProcessTask;
					}
					);
				}
				return currentOrNextTask.Value;
			}
		}

		CachedProperty<SupportIncidentProcessTask> currentOrNextTask;

		CurrentTaskFinder TaskFinder => taskFinder ?? (taskFinder = new CurrentTaskFinder(this));
		CurrentTaskFinder taskFinder;

		#endregion

		#region IAllowAttachEmailsToEDocs Members

		string IAllowAttachEmailsToEDocs.ReferenceNumber
		{
			get { return IM_IncidentNumber; }
		}

		#endregion

		#region IClientOrgLicenceProvider Members

		EDIOrgHeader IClientOrgLicenceProvider.LicenceOrganisation
		{
			get { return (EDIOrgHeader)Client; }
		}

		ZString IClientOrgLicenceProvider.ReferenceNumber
		{
			get { return IM_IncidentNumber; }
		}

		#endregion

		#endregion

		#region Original Values

		bool isNewIncident = true;
		public bool IsNewIncident
		{
			get { return isNewIncident; }
			private set { isNewIncident = value; }
		}

		public SupportIncidentStatusSnapShot OriginalSnapShot
		{
			get { return originalSnapShot ?? (originalSnapShot = new SupportIncidentStatusSnapShot()); }
			private set { originalSnapShot = value; }
		}
		SupportIncidentStatusSnapShot originalSnapShot;

		#endregion

		#region OnFactorySaving

		bool StatusChanged;

		protected override void OnFactorySaving()
		{
			SendDelayedMessage();
			SendMessageIfEmailNotificationSwitchStatusChanged();

			StatusChanged = IM_StatusInfo.HasChanges;
			base.OnFactorySaving();
			SynchroniseNotes();

			if (HasChanges || !IsInDatabase)
			{
				RemoveNonPersistentTasks();

				CalculateWorkItemDependentStatusAndDisposition();
				CalculateWorkflowDependentProperties();

				AttachOrDetachDefectCausedByWorkItem();
				SetIncidentClosedDate();
			}

			if (!inOnSaveSucceeded)
			{
				HandleEDocsChangesAndAddConversationMessage();

				if (EConversationHasChanges || EDocsHasChanges)
				{
					//Force this BusinessObject to have row changes so OnSaved() gets called
					HasChanges = true;
					IM_SystemLastEditTimeUtc = ZDateTime.UtcNow;
					Request.INC_SystemLastEditTimeUtc = ZDateTime.UtcNow;
				}

				AddLogsForCriticalFieldsChange();
			}
		}

		void HandleEDocsChangesAndAddConversationMessage()
		{
			// Ignore eDocs coming from the customer using a remote CW1 via the eRequest web service.
			// The remote CW1 will have added the eConversation messages.
			if (GlbStaff.CurrentUser.GS_Code != User.WebUserCode)
			{
				// Note: this needs to come before calculating eConversation changes
				// since it can add messages that need sending to the client
				eDocsToPublish = GetChangedPublishedEDocsAndLogToEConversation();
			}
		}

		void AddLogsForCriticalFieldsChange()
		{
			AddLogIfChanged(OriginalSnapShot.ContactName, Contact?.OC_ContactName ?? ZString.Empty, nameof(OriginalSnapShot.ContactName));
			AddLogIfChanged(OriginalSnapShot.Database, DatabaseServerCode, nameof(OriginalSnapShot.Database));
			AddLogIfChanged(OriginalSnapShot.ClientCompanyCode, ClientCompanyCode, nameof(OriginalSnapShot.ClientCompanyCode));
			AddLogIfChanged(OriginalSnapShot.ClientCode, ClientCode, nameof(OriginalSnapShot.ClientCode));
			AddLogIfChanged(OriginalSnapShot.EnterpriseID, LicEnterprise?.LE_EnterpriseID ?? ZString.Empty, nameof(OriginalSnapShot.EnterpriseID));
			AddLogIfChanged(OriginalSnapShot.EnterpriseCode, EnterpriseCode, nameof(OriginalSnapShot.EnterpriseCode));
		}

		bool EDocsHasChanges => DocManagerInfo.EDocsHasChanges;

		List<StorageDocsBase> GetChangedPublishedEDocsAndLogToEConversation()
		{
			var result = new List<StorageDocsBase>();

			StringBuilder newNonEmailFileNames = new StringBuilder();
			StringBuilder modifiedNonEmailFileNames = new StringBuilder();

			if (DocManagerInfo.EDocsView.Count > 0)
			{
				for (int listIndex = 0; listIndex < 2; ++listIndex)
				{
					foreach (StorageDocsBase doc in (listIndex == 0 ? this.DocManagerInfo.Files : this.DocManagerInfo.Documents))
					{
						if (doc.SC_IsPublished && doc.HasChanges)
						{
							result.Add(doc);
							var fileName = doc.SC_FileNameWithExtension;

							if (fileName != "Incident Email.txt" && !fileName.StartsWith("Incident Email[", StringComparison.Ordinal))
							{
								StringBuilder builder = (doc.IsInDatabase ? modifiedNonEmailFileNames : newNonEmailFileNames);
								if (builder.Length > 0)
								{
									builder.Append(", ");
								}
								builder.Append(fileName);
							}
						}
					}
				}
			}

			if (newNonEmailFileNames.Length > 0)
			{
				AddPublicSystemLogMessage("Attached to eDocs: " + newNonEmailFileNames);
			}

			if (modifiedNonEmailFileNames.Length > 0)
			{
				AddPublicSystemLogMessage("Modified in eDocs: " + modifiedNonEmailFileNames);
			}

			return result;
		}

		public void SetIncidentClosedDate()
		{
			if (IM_Status == SupportIncidentLookups.Status.Closed)
			{
				if (IM_CloseTimeUtc.IsEmpty)
				{
					IM_CloseTimeUtc = ZDateTime.UtcNow;
				}
			}
			else
			{
				IM_CloseTimeUtc = ZDateTime.Empty;
			}
		}

		#endregion

		#region Setup For Incidents

		public void SetupForNewCreatedDefect()
		{
			SetupForInternalReportedIncident(SupportIncidentCategoriesList.Codes.Defect, "", null);
		}

		public void SetupForNewCreatedFeatureRequest()
		{
			SetupForInternalReportedIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, "", null);
			IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
		}

		public void SetupForProjectFeatureRequest()
		{
			SetupForInternalReportedIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, "Project Feature Request", null);
			IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			IM_Status = SupportIncidentLookups.Status.Open;
			IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			IM_Source = SupportIncidentLookups.SourceListConstants.CreatedFromProject;
		}

		public void SetupForInternalReportedIncident(ZString escalationStage, ZString resolutionComment, LicenceHeader licence)
		{
			IM_Source = SupportIncidentLookups.SourceListConstants.WTGInternalViaEdiProd;

			if (escalationStage == SupportIncidentCategoriesList.Codes.FeatureRequest || escalationStage == SupportIncidentCategoriesList.Codes.Defect)
			{
				Escalate(escalationStage, resolutionComment);
			}
			else
			{
				AddInternalSystemLogMessage(resolutionComment);
			}

			PopulateClientInfoFromLicence(licence);
		}

		void PopulateClientInfoFromLicence(LicenceHeader licence)
		{
			if (licence != null && !inClientLicenceUpdate)
			{
				inClientLicenceUpdate = true;
				try
				{
					PopulateClientInfoFromLicenceCore(licence);
				}
				finally
				{
					inClientLicenceUpdate = false;
				}
			}
		}

		void PopulateClientInfoFromLicenceCore(LicenceHeader licence)
		{
			var clientCompany = licence.ClientCompany;
			if (clientCompany != null)
			{
				IM_OH_Client = clientCompany.LCC_OH;
				IM_LD = clientCompany.LCC_LD;
				IM_LCC = clientCompany.PK;
				EnterprisePK = clientCompany.Database.LicEnterprise.PK;
				DatabaseServerCode = clientCompany.Database.LD_ServerCode;
				clientCompanyCode = clientCompany.LCC_Code;

				if (Client != null)
				{
					IM_OC_Contact = Client.Contacts.FindOrCreateFromStaff(GlbStaff.CurrentUser).PK;
				}
			}
		}

		#endregion

		public bool HasDispositionChangedToEventLog(string dispostion)
		{
			var query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Disposition -");
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.EndsWith, "to " + dispostion);
			return Logs.Find(query).Length > 0;
		}

		public string GetLastServiceLog()
		{
			var query = GetServiceLogsQuery();
			query.OrderBy = $"{StmALogSchema.Constants.SL_EventTime} DESC";

			var lastFilterLog = Factory.LoadTop1<StmALog>(query);
			return lastFilterLog?.SL_SE_NKEvent;
		}

		public void CancelAllServiceLogs()
		{
			var serviceLogs = GetAllServiceLogs();
			foreach (var serviceLog in serviceLogs)
			{
				serviceLog.Cancel();
			}
			ServiceStatus = "";
			OutageDuration = "";
		}

		public StmALog[] GetAllServiceLogs()
		{
			var query = GetServiceLogsQuery();
			return Factory.Load<StmALog>(query);
		}

		ZQuery GetServiceLogsQuery()
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, new string[] { AutoEvents.ServiceSuspendedCode, AutoEvents.ServiceCommencedCode });
			query.AddToFilter(StmALogSchema.SL_Parent, PK);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			return query;
		}

		public void RecalculateServiceStatus()
		{
			var lastServiceLog = GetLastServiceLog();
			if (string.IsNullOrEmpty(lastServiceLog))
			{
				base.IM_ServiceStatus = "";
			}
			else
			{
				base.IM_ServiceStatus = lastServiceLog;
			}
		}

		public void RecalculateOutageDuration()
		{
			var serviceLogs = GetAllServiceLogs().OrderBy(log => log.SL_EventTime).ToArray();
			if (serviceLogs.Length == 0)
			{
				OutageDuration = "";
				return;
			}

			var lastSVSTime = ZDateTime.MinSmallDateTimeValue;
			var totalInterval = TimeSpan.Zero;

			foreach (StmALog serviceLog in serviceLogs)
			{
				if (serviceLog.SL_SE_NKEvent == AutoEvents.ServiceSuspendedCode)
				{
					lastSVSTime = serviceLog.SL_EventTime;
				}
				else if (serviceLog.SL_SE_NKEvent == AutoEvents.ServiceCommencedCode && lastSVSTime != ZDateTime.MinSmallDateTimeValue)
				{
					totalInterval += serviceLog.SL_EventTime - lastSVSTime;
				}
			}

			if (serviceLogs[serviceLogs.Length - 1].SL_SE_NKEvent == AutoEvents.ServiceSuspendedCode)
			{
				totalInterval += ZDateTime.Now - lastSVSTime;
			}

			OutageDuration = $"{totalInterval.Days * 24 + totalInterval.Hours:D2}:{totalInterval.Minutes:D2}:{totalInterval.Seconds:D2}";
		}

		public bool CanCloseAwaitingResponse
		{
			get
			{
				return IM_ResolutionCode != SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated
					&& IM_ResolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade
					&& IM_ResolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed;
			}
		}

		public bool ShouldKeepCurrentDispositionWhileAwaitingResponse
		{
			get { return WorkflowExcludeDispositionSet.Contains(IM_ResolutionCode); }
		}

		#endregion

		#region Lookups

		protected override IncidentMainLookups GetNewLookups()
		{
			return new SupportIncidentLookups(this);
		}

		public new SupportIncidentLookups Lookups
		{
			get { return (SupportIncidentLookups)base.Lookups; }
		}

		#endregion

		#region Validation

		public new SupportIncidentValidation Validation
		{
			get { return (SupportIncidentValidation)base.Validation; }
		}

		protected override IncidentMainValidation GetNewValidation()
		{
			return new SupportIncidentValidation(this);
		}

		#endregion

		#region Readonly

		public bool IM_GS_NKCustDefectCausedBy_ReadOnly
		{
			get { return DefectFieldsReadonly; }
		}

		public bool DefectCausedByWorkItemPK_ReadOnly
		{
			get { return DefectFieldsReadonly; }
		}

		public bool IM_GG_Team_ReadOnly
		{
			get { return DefectFieldsReadonly; }
		}

		public bool IM_ClientBugSeverity_ReadOnly
		{
			get { return DefectFieldsReadonly; }
		}

		public bool FeatureSoftwareChangeNoteText_ReadOnly
		{
			get { return FeatureRequestFieldsReadonly; }
		}

		public bool FeatureBusinessProblemNoteText_ReadOnly
		{
			get { return FeatureRequestFieldsReadonly; }
		}

		public bool IM_FeatureRequestIndustryValue_ReadOnly
		{
			get { return FeatureRequestFieldsReadonly; }
		}

		bool DefectFieldsReadonly
		{
			get
			{
				return IM_Category != SupportIncidentCategoriesList.Codes.Defect
					|| IM_Status == SupportIncidentLookups.Status.Closed
					|| !EDISecurityCheckpoints.CustomerServiceIncidentEditDefectManagement.IsAllowed;
			}
		}

		bool FeatureRequestFieldsReadonly
		{
			get
			{
				return IM_Category != SupportIncidentCategoriesList.Codes.FeatureRequest
					|| IM_Status == SupportIncidentLookups.Status.Closed
					|| !EDISecurityCheckpoints.CustomerServiceIncidentEditFeatureManagement.IsAllowed;
			}
		}

		public bool SupportFieldsReadonly
		{
			get
			{
				return IM_Category != SupportIncidentCategoriesList.Codes.Support;
			}
		}

		[ReadOnlyMember(nameof(SupportFieldsReadonly))]
		public override ZBool IM_ChargableWork
		{
			get { return base.IM_ChargableWork; }
			set { base.IM_ChargableWork = value; }
		}

		#endregion

		#region Enterprise, Database and Company

		#region Enterprise

		public static ZGuid GetEnterprisePK(LicenceDatabase database, EDIOrgHeader orgHeader, bool databaseOnly = false)
		{
			if (database != null)
			{
				if (databaseOnly)
				{
					return (ZGuid)database?.LD_LEInfo.OriginalValue;
				}
				return database?.LD_LE ?? ZGuid.Empty;
			}
			else if (orgHeader != null)
			{
				var licCompany = orgHeader.LicCompany;
				if (licCompany != null)
				{
					if (databaseOnly)
					{
						return (ZGuid)licCompany?.LC_LEInfo.OriginalValue;
					}
					return licCompany?.LC_LE ?? ZGuid.Empty;
				}
			}
			return ZGuid.Empty;
		}

		[List("Lookups.EnterpriseList")]
		public ZGuid EnterprisePK
		{
			get
			{
				if (!isEnterprisePKInitialised)
				{
					enterprisePK = GetEnterprisePK(Database, (EDIOrgHeader)Client);
					isEnterprisePKInitialised = true;
					enterpriseCode = LicEnterprise?.LE_EnterpriseCode ?? "";
				}
				return enterprisePK;
			}
			set
			{
				SetNonPersistentPropertyValue(EnterprisePKInfo, ref enterprisePK, value);
				isEnterprisePKInitialised = true;
				enterpriseCode = LicEnterprise?.LE_EnterpriseCode ?? "";
				if (!IsValidationSuspended)
				{
					Validation.ValidateEnterprisePK();
					Validation.ValidateEnterpriseCode();
				}
				EnterpriseCodeInfo.RefreshBinding();
			}
		}
		ZGuid enterprisePK;
		internal bool isEnterprisePKInitialised;

		public ZPropertyInfo EnterprisePKInfo
		{
			get { return GetZPropertyInfo(nameof(EnterprisePK)); }
		}

		public bool EnterprisePK_ReadOnly
		{
			get { return IM_LD_ReadOnly; }
		}

		[List("Lookups.EnterpriseCodeList"), MaxLength(LicenceEnterprise.Schema.LE_EnterpriseCodeMaxLength)]
		public ZString EnterpriseCode
		{
			get => enterpriseCode;

			set
			{
				SetNonPersistentPropertyValue(EnterpriseCodeInfo, ref enterpriseCode, value);
				var pk = value.IsEmpty ? ZGuid.Empty : Factory.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, value)).FirstOrDefault()?.PK ?? ZGuid.Empty;
				SetNonPersistentPropertyValue(EnterprisePKInfo, ref enterprisePK, pk);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEnterprisePK();
					Validation.ValidateEnterpriseCode();
				}
			}
		}

		public ZPropertyInfo EnterpriseCodeInfo
		{
			get { return GetZPropertyInfo(nameof(EnterpriseCode)); }
		}

		public bool EnterpriseCode_ReadOnly => EnterprisePK_ReadOnly;

		ZString enterpriseCode;

		LicenceEnterprise LicEnterprise => Factory.Load<LicenceEnterprise>(EnterprisePK);

		#endregion

		#region Database

		public override ZGuid IM_LD
		{
			get { return base.IM_LD; }
			set
			{
				if (base.IM_LD != value)
				{
					base.IM_LD = value;

					if (Database != null)
					{
						IM_HL_ClientReportedOnVersion = Database.LD_HL_CurrentRunningVersion;
					}
					else
					{
						IM_HL_ClientReportedOnVersion = ZGuid.Empty;
					}

					UpdateClientLicenceFromDatabasePk();
					UpdateClientFromDatabase();
				}
			}
		}

		void UpdateClientFromDatabase()
		{
			if (!inClientLicenceUpdate)
			{
				inClientLicenceUpdate = true;
				try
				{
					var db = Database;
					if (db != null && db.WebAccessOrg != null)
					{
						SetClientOnly(db.LD_OH_WebAccessOrg);
					}
				}
				finally
				{
					inClientLicenceUpdate = false;
				}
			}
		}

		public void SetDatabaseOnly(LicenceDatabase db)
		{
			if (inClientLicenceUpdate)
			{
				IM_LD = db.PK;
			}
			else
			{
				inClientLicenceUpdate = true;
				try
				{
					IM_LD = db.PK;
				}
				finally
				{
					inClientLicenceUpdate = false;
				}
			}
		}

		public bool IM_LD_ReadOnly
		{
			get
			{
				return IsInDatabase
					&& !IM_LDInfo.OriginalValue.IsEmpty
					&& !IM_LDInfo.HasChanges
					&& IncidentCustomerNotifierFactory.IsLinkedToERequestInCustomerDatabase(this);
			}
		}

		public LicenceDatabase Database
		{
			get { return Factory.Load<LicenceDatabase>(IM_LD); }
		}

		[List("Lookups.DatabaseCodeDescriptionPairList")]
		public ZString DatabaseServerCode
		{
			get
			{
				if (databaseServerCode.IsEmpty)
				{
					var db = Database;
					if (db != null)
					{
						databaseServerCode = db.LD_ServerCode;
					}
				}
				return databaseServerCode;
			}
			set
			{
				SetNonPersistentPropertyValue(DatabaseServerCodeInfo, ref databaseServerCode, value);
				UpdateClientLicenceFromServerCode();

				if (!IsValidationSuspended)
				{
					Validation.ValidateDatabaseServerCode();
				}
			}
		}
		ZString databaseServerCode;

		public ZPropertyInfo DatabaseServerCodeInfo
		{
			get { return GetZPropertyInfo(nameof(DatabaseServerCode)); }
		}

		public bool DatabaseServerCode_ReadOnly
		{
			get { return IM_LD_ReadOnly; }
		}

		public ZString DatabaseCurrentVersion
		{
			get
			{
				var version = Database?.CurrentVersion?.VersionNumber;
				return version != null ? (ZString)version.ToString() : ZString.Empty;
			}
		}

		public LicenceDatabase DatabaseOrPrimaryProductionSystem
		{
			get
			{
				if (Database != null)
				{
					return Database;
				}
				else
				{
					var query = new ZQuery();
					query.AddToFilter(LicenceDatabaseSchema.LD_OH_WebAccessOrg, IM_OH_Client);
					query.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, DatabaseTypes.Codes.Production);
					query.AddToFilter(LicenceDatabaseSchema.LD_Product, IM_Product);

					return Factory.LoadTop1<LicenceDatabase>(query);
				}
			}
		}

		#endregion

		#region Company

		public override ZGuid IM_LCC
		{
			get { return base.IM_LCC; }
			set
			{
				if (base.IM_LCC != value)
				{
					base.IM_LCC = value;

					if (!value.IsEmpty && ClientCompany != null)
					{
						if (IM_RN_NKCountry.IsEmpty)
						{
							IM_RN_NKCountry = ClientCompany.LCC_RN_NKCountryCode;
						}
					}
					else
					{
						IM_RN_NKCountry = ZString.Empty;
					}
				}
			}
		}

		public void SetClientCompanyOnly(ClientCompany co)
		{
			if (inClientLicenceUpdate)
			{
				IM_LCC = co.PK;
				IM_RN_NKCountry = co.LCC_RN_NKCountryCode;
			}
			else
			{
				inClientLicenceUpdate = true;
				try
				{
					IM_LCC = co.PK;
					IM_RN_NKCountry = co.LCC_RN_NKCountryCode;
				}
				finally
				{
					inClientLicenceUpdate = false;
				}
			}
		}

		public bool IM_LCC_ReadOnly
		{
			get { return IM_LD_ReadOnly; }
		}

		public ClientCompany ClientCompany
		{
			get { return Factory.Load<ClientCompany>(IM_LCC); }
		}

		[List("Lookups.ClientCompanyCodeDescriptionPairList")]
		public ZString ClientCompanyCode
		{
			get
			{
				if (clientCompanyCode.IsEmpty)
				{
					var company = ClientCompany;
					if (company != null)
					{
						clientCompanyCode = company.LCC_Code;
					}
				}
				return clientCompanyCode;
			}
			set
			{
				SetNonPersistentPropertyValue(ClientCompanyCodeInfo, ref clientCompanyCode, value);
				UpdateClientLicenceFromCompanyCode();

				if (!IsValidationSuspended)
				{
					Validation.ValidateClientCompanyCode();
				}
			}
		}
		ZString clientCompanyCode;

		public ZPropertyInfo ClientCompanyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ClientCompanyCode)); }
		}

		public bool ClientCompanyCode_ReadOnly
		{
			get { return IM_LCC_ReadOnly && Lookups.ActiveClientCompanyCodeDescriptionPairList.ContainsCode(ClientCompanyCode) && !IM_LCCInfo.HasChanges; }
		}

		public bool ClientCompanyCode_Visible
		{
			get => Database?.IsEnterpriseFamilyDatabase ?? false;
		}

		#endregion

		#endregion

		#region Client

		public override ZGuid IM_OC_Contact
		{
			get { return base.IM_OC_Contact; }
			set
			{
				if (value != base.IM_OC_Contact)
				{
					base.IM_OC_Contact = value;
					foreach (SupportIncidentProcessTask task in WorkflowItems)
					{
						task.P9_OC = base.IM_OC_Contact;
					}

					SetBranchAddress();
					if (IsFeatureRequest && FeatureRequestContactPK.IsEmpty)
					{
						FeatureRequestContactPK = value;
					}
				}
			}
		}

		void SetBranchAddress()
		{
			if (Contact == null && Client == null)
			{
				return;
			}

			if (Contact != null)
			{
				IM_OA_BranchAddress = Contact.WorkingAddressPK;
				if (IM_OA_BranchAddress.IsEmpty && Client != null)
				{
					IM_OA_BranchAddress = Client.MainAddress.PK;
				}
			}
			else
			{
				IM_OA_BranchAddress = Client.MainAddress.PK;
			}
		}

		void AddContactToRelatedParty()
		{
			if (Contact == null || EConversation.Conversation.RelatedParties.HasParticipant(Contact))
			{
				return;
			}
			EConversation.Conversation.RelatedParties.AddNewParticipant(Contact);
		}

		public ZString ContactPrimaryWorkplace
		{
			get
			{
				return GetPrimaryWorkplace(Contact, Client);
			}
		}

		ZString GetPrimaryWorkplace(OrgContact contact, OrgHeader org)
		{
			if (contact == null || org == null)
			{
				return ZString.Empty;
			}

			if (contact.WorkingAddressPK.IsEmpty)
			{
				return org.OH_FullName;
			}

			var address = org.Addresses.Cast<OrgAddress>().FirstOrDefault(a => a.PK == contact.WorkingAddressPK);

			if (address == null)
			{
				return org.OH_FullName;
			}

			return address.OA_CompanyNameOverride.IsEmpty ? org.OH_FullName : address.OA_CompanyNameOverride;
		}

		public override ZGuid IM_OH_Client
		{
			get { return base.IM_OH_Client; }
			set
			{
				if (value != base.IM_OH_Client)
				{
					base.IM_OH_Client = value;
					IM_OC_Contact = ZGuid.Empty;

					if (IsFeatureRequest && FeatureRequestClientPK.IsEmpty)
					{
						FeatureRequestClientPK = value;
					}
					var org = Client;
					if (org != null)
					{
						if (!org.ARSettlementGroupPK.IsEmpty)
						{
							Job.Loader loader = new Job.Loader(this);
							Job job = loader.Load();
							if (job != null)
							{
								job.LocalChargesPK = org.ARSettlementGroupPK;
							}
						}
					}
					else
					{
						IM_OA_BranchAddress = ZGuid.Empty;
					}

					UpdateLicenceFromClient();
				}
			}
		}

		public void SetClientOnly(OrgHeader org)
		{
			SetClientOnly(org.PK);
		}

		public void SetClientOnly(ZGuid orgPk)
		{
			if (inClientLicenceUpdate)
			{
				IM_OH_Client = orgPk;
			}
			else
			{
				inClientLicenceUpdate = true;
				try
				{
					IM_OH_Client = orgPk;
				}
				finally
				{
					inClientLicenceUpdate = false;
				}
			}
		}

		public bool IM_OH_Client_ReadOnly
		{
			get { return IM_LD_ReadOnly; }
		}

		bool inClientLicenceUpdate;

		public void UpdateLicenceFromClient(bool populateClientCompany = true)
		{
			if (!inClientLicenceUpdate)
			{
				inClientLicenceUpdate = true;
				try
				{
					PopulateLicenceFromClient(populateClientCompany);
				}
				finally
				{
					inClientLicenceUpdate = false;
				}
			}
		}

		public static bool ShouldProductPopulateLicence(string productCode)
		{
			return ProductTypes.IsEnterpriseFamily(productCode) || productCode == ProductTypes.Codes.Sapphire;
		}

		void PopulateLicenceFromClient(bool populateClientCompany)
		{
			var org = ((EDIOrgHeader)Client);
			if (org == null)
			{
				return;
			}

			if (IM_Product.IsEmpty || ShouldProductPopulateLicence(IM_Product))
			{
				IM_LD = ZGuid.Empty;
				IM_LCC = ZGuid.Empty;
				EnterprisePK = ZGuid.Empty;
				DatabaseServerCode = ZString.Empty;
				ClientCompanyCode = ZString.Empty;

				if (IM_Product.IsEmpty || IsInDatabase)
				{
					if (!PopulateLicenceIfSingleClientCompanyMatches())
					{
						if (PopulateEnterpriseFromClient())
						{
							PopulateDatabaseIfSingleDatabaseMatches();

							if (populateClientCompany)
							{
								PopulateClientCompanyFromDatabase(matchCountryIfExists: false);
							}
						}
					}
				}
				else
				{
					// product is CW1 family
					PopulateEnterpriseFromClient();
					PopulateDatabaseIfAnyMatchProduct();
					if (populateClientCompany)
					{
						PopulateClientCompanyFromDatabase(matchCountryIfExists: true);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool PopulateDatabaseIfAnyMatchProduct()
		{
			var recentHeartBeatLimit = ZDateTime.Today.AddDays(-30);

			var firstOrgLicence = ((EDIOrgHeader)(Client))?.LicCompany?.LicHeadersForAllDatabases
				.Cast<LicenceHeader>()
				.Where(x => x.LA_IsActive && x.Database.LD_IsActive && IsMatchingDatabaseProduct(x.Database))
				.OrderBy(x => ProductMatchRank(x.Database))
				.ThenBy(x => x.Database.LD_LicenceType == DatabaseTypes.Codes.Production ? 0 : 1)
				.ThenBy(x => !x.Database.LD_LastHeartbeat.IsEmpty && x.Database.LD_LastHeartbeat > recentHeartBeatLimit ? 0 : 1)
				.ThenBy(x => !x.LA_AgreedLiveDate.IsEmpty ? 0 : 1)
				.ThenByDescending(x => x.Database.CurrentVersion?.VersionNumber ?? new VersionNumber(0, 0, 0, 0))
				.ThenBy(x => x.LA_AgreedLiveDate)
				.FirstOrDefault();

			if (firstOrgLicence != null)
			{
				IM_LD = firstOrgLicence.LA_LD;
			}
			else
			{
				var firstDb = Lookups.DatabaseList.Cast<LicenceDatabase>()
					.Where(x => IsMatchingDatabaseProduct(x))
					.OrderBy(x => x.LD_IsActive ? 0 : 1)
					.ThenBy(x => x.LD_LicenceType == DatabaseTypes.Codes.Production ? 0 : 1)
					.ThenBy(x => !x.LD_LastHeartbeat.IsEmpty && x.LD_LastHeartbeat > recentHeartBeatLimit ? 0 : 1)
					.FirstOrDefault();

				if (firstDb != null)
				{
					IM_LD = firstDb.PK;
				}
			}

			return !IM_LD.IsEmpty;
		}

		bool PopulateDatabaseIfSingleDatabaseMatches()
		{
			var productionDatabases = Lookups.DatabaseList.Cast<LicenceDatabase>()
				.Where(x => x.LD_LicenceType == DatabaseTypes.Codes.Production && IsMatchingDatabaseProduct(x));

			LicenceDatabase database = null;
			if (productionDatabases.Count() == 1)
			{
				database = productionDatabases.First();
			}
			else
			{
				var activeProductionDatabases = productionDatabases.Where(x => x.LD_IsActive);
				if (activeProductionDatabases.Count() == 1)
				{
					database = activeProductionDatabases.First();
				}
			}

			if (database != null)
			{
				IM_LD = database.PK;
				DatabaseServerCode = database.LD_ServerCode;
				return true;
			}

			return false;
		}

		bool PopulateLicenceIfSingleClientCompanyMatches()
		{
			var query = new ZQuery(ClientCompanySchema.LCC_OH, IM_OH_Client);
			if (!IM_RN_NKCountry.IsEmpty)
			{
				query.AddToFilter(ClientCompanySchema.LCC_RN_NKCountryCode, IM_RN_NKCountry);
			}
			var clientCompanies = Factory.Load<ClientCompany>(query);
			if (clientCompanies.Length == 1)
			{
				var clientCompany = clientCompanies[0];
				var database = clientCompany.Database;

				if (SupportIncidentLookups.GetCachedDatabaseListForEnterprise(Factory, database.LD_LE)
					.Cast<LicenceDatabase>()
					.Any(x => x.PK == database.PK))
				{
					EnterprisePK = database.LD_LE;
					IM_LD = database.PK;
					IM_LCC = clientCompany.PK;
					DatabaseServerCode = database.LD_ServerCode;
					ClientCompanyCode = clientCompany.LCC_Code;
					return true;
				}
			}

			return false;
		}

		bool PopulateEnterpriseFromClient()
		{
			var org = ((EDIOrgHeader)Client);
			if (EnterprisePK.IsEmpty && org != null)
			{
				var licenceCompany = org.LicCompany;
				if (licenceCompany != null)
				{
					EnterprisePK = licenceCompany.LC_LE;
				}
				else
				{
					var enterprises = Factory.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_OH, IM_OH_Client));
					if (enterprises.Length == 1)
					{
						EnterprisePK = enterprises[0].PK;
					}
				}
			}

			return !EnterprisePK.IsEmpty;
		}

		bool IsMatchingDatabaseProduct(LicenceDatabase db)
		{
			return IM_Product.IsEmpty || ShouldProductPopulateLicence(IM_Product) && ShouldProductPopulateLicence(db.LD_Product);
		}

		int ProductMatchRank(LicenceDatabase db)
		{
			if (IM_Product.IsEmpty)
			{
				return 0;
			}
			else if (ShouldProductPopulateLicence(db.LD_Product) && ShouldProductPopulateLicence(IM_Product))
			{
				// Incidents use product code ENT for either database codes ENT or CW1 or CWN or CGW
				if (IM_Product == ProductTypes.Codes.Enterprise &&
					(db.LD_Product == ProductTypes.Codes.Enterprise || db.LD_Product == ProductTypes.Codes.CargoWiseOne || db.LD_Product == ProductTypes.Codes.CargoWiseNext || db.LD_Product == ProductTypes.Codes.CargoWise))
				{
					return 1;
				}
				else if (IM_Product == db.LD_Product)
				{
					return 1;
				}
				else
				{
					return 2;
				}
			}
			else if (IM_Product == db.LD_Product)
			{
				return 1;
			}
			else
			{
				return -1;
			}
		}

		void UpdateClientLicenceFromDatabasePk()
		{
			if (!inClientLicenceUpdate)
			{
				inClientLicenceUpdate = true;
				try
				{
					PopulateClientCompanyFromDatabase(matchCountryIfExists: false);
				}
				finally
				{
					inClientLicenceUpdate = false;
				}
			}
		}

		void PopulateClientCompanyFromDatabase(bool matchCountryIfExists)
		{
			var newLCC = ZGuid.Empty;
			var newClientCompanyCode = ZString.Empty;

			if (Client != null)
			{
				var companyList = Lookups.ActiveClientCompanyList.Cast<ClientCompany>();
				ClientCompany matchingClientCompany = null;

				if (matchCountryIfExists && !IM_RN_NKCountry.IsEmpty)
				{
					matchingClientCompany = companyList
						.Where(x => x.LCC_RN_NKCountryCode == IM_RN_NKCountry)
						.OrderBy(x => x.LCC_OH == IM_OH_Client ? 0 : 1)
						.ThenBy(x => x.LCC_OH.IsEmpty ? 0 : 1)
						.FirstOrDefault();
				}

				if (matchingClientCompany == null)
				{
					matchingClientCompany = companyList.FirstOrDefault(x => x.LCC_OH == IM_OH_Client);
				}

				if (matchingClientCompany != null)
				{
					newLCC = matchingClientCompany.PK;
					newClientCompanyCode = matchingClientCompany.LCC_Code;
				}
			}

			IM_LCC = newLCC;
			ClientCompanyCode = newClientCompanyCode;
		}

		void UpdateClientLicenceFromServerCode()
		{
			if (!inClientLicenceUpdate)
			{
				inClientLicenceUpdate = true;
				try
				{
					PopulateDatabasePkFromServerCode();
					PopulateClientCompanyFromDatabase(matchCountryIfExists: false);
				}
				finally
				{
					inClientLicenceUpdate = false;
				}
			}
		}

		void PopulateDatabasePkFromServerCode()
		{
			var selectedDb = Lookups.DatabaseList.Cast<LicenceDatabase>().FirstOrDefault(x => x.LD_ServerCode == databaseServerCode);
			if (selectedDb != null)
			{
				IM_LD = selectedDb.PK;
			}
			else
			{
				IM_LD = ZGuid.Empty;
			}
		}

		void UpdateClientLicenceFromCompanyCode()
		{
			if (!inClientLicenceUpdate)
			{
				inClientLicenceUpdate = true;
				try
				{
					PopulateClientCompanyPkFromCompanyCode();

					if (IM_OH_Client.IsEmpty)
					{
						PopulateClientFromCompany();
					}
				}
				finally
				{
					inClientLicenceUpdate = false;
				}
			}
		}

		void PopulateClientCompanyPkFromCompanyCode()
		{
			var selectedCompany = Lookups.ActiveClientCompanyList.Cast<ClientCompany>().FirstOrDefault(x => x.LCC_Code == clientCompanyCode);
			if (selectedCompany != null)
			{
				IM_LCC = selectedCompany.PK;
			}
			else
			{
				IM_LCC = ZGuid.Empty;
			}
		}

		void PopulateClientFromCompany()
		{
			var clientCo = ClientCompany;
			if (clientCo == null)
			{
				return;
			}

			if (!clientCo.LCC_OH.IsEmpty && clientCo.Org != null)
			{
				IM_OH_Client = clientCo.Org.PK;
			}
			else
			{
				var licence = clientCo.UsageOwnerLicence;
				if (licence != null)
				{
					IM_OH_Client = licence.Company.LC_OH;
				}
				else
				{
					IM_OH_Client = clientCo.Database.LicEnterprise.Organisation.PK;
				}
			}
		}

		public override ZGuid IM_OA_BranchAddress
		{
			get
			{
				return base.IM_OA_BranchAddress;
			}
			set
			{
				if (value != base.IM_OA_BranchAddress)
				{
					base.IM_OA_BranchAddress = value;

					foreach (SupportIncidentProcessTask task in WorkflowItems)
					{
						task.P9_OA = base.IM_OA_BranchAddress;
					}

					ClientLocalTimeInfo.RefreshBinding();
					ClientLocalTimeStringInfo.RefreshBinding();
				}

				if (BranchAddress != null && BranchAddress.Header != null && !BranchAddress.Header.ARSettlementGroupPK.IsEmpty)
				{
					Job.Loader loader = new Job.Loader(this);
					Job job = loader.Load();
					if (job != null)
					{
						job.LocalChargesPK = BranchAddress.Header.ARSettlementGroupPK;
					}
				}

				PropagateClientToFeatureRequestIfRequired();
			}
		}

		void PropagateClientToFeatureRequestIfRequired(bool setContact = false)
		{
			if (IsFeatureRequest)
			{
				if (FeatureRequestClientAddressPK.IsEmpty && !IM_OA_BranchAddress.IsEmpty)
				{
					FeatureRequestClientAddressPK = IM_OA_BranchAddress;

					if (setContact && FeatureRequestContactPK.IsEmpty && !IM_OC_Contact.IsEmpty)
					{
						FeatureRequestContactPK = IM_OC_Contact;
					}
				}
			}
		}

		[List("Lookups.Languages")]
		public override ZString IM_Language
		{
			get { return base.IM_Language; }
			set
			{
				if (base.IM_Language != value)
				{
					base.IM_Language = value;
					HandleWorflowPropertyChange();
				}
			}
		}

		#endregion

		#region Properties

		#region Upgrade Deploy Date

		public ZString UpgradeDeployDate
		{
			get { return IM_BugFixDeployed.IsValid && !IM_BugFixDeployed.IsEmpty ? IM_BugFixDeployed.ToShortDateString() : string.Empty; }
		}

		public ZPropertyInfo UpgradeDeployDateInfo
		{
			get { return GetZPropertyInfo(nameof(UpgradeDeployDate)); }
		}

		#endregion

		public bool IsCreatedFromIssue
		{
			get { return (IM_Source == SupportIncidentLookups.SourceListConstants.IssueManagerReported); }
		}

		public bool NotifyInternalSubscribersForInternalLog { get; set; }

		#region Triage

		public IncidentTriage IncidentTriage => Factory.Load<IncidentTriage>(IM_IMT_Triage);

		public string TriageDescription => IncidentTriage != null ? IncidentTriage.IMT_TriageNumber + " - " + IncidentTriage.IMT_SupportDescription : "Not Applied";

		[RelatedBusinessObject("IncidentTriage")]
		[List("Lookups.TriageList")]
		public override ZGuid IM_IMT_Triage { get => base.IM_IMT_Triage; set => base.IM_IMT_Triage = value; }

		public ZString TriageSupportDescription => IncidentTriage?.IMT_SupportDescription ?? string.Empty;

		public ZString TriageNumber => IncidentTriage?.IMT_TriageNumber ?? string.Empty;

		#endregion

		#region Metric

		IncidentMetricsCollection metricsCollection;

		public IncidentMetricsCollection MetricsCollection
		{
			get
			{
				if (metricsCollection == null)
				{
					metricsCollection = new IncidentMetricsCollection(Factory, IM_IncidentNumber);
				}
				return metricsCollection;
			}
		}

		public bool IsFirstResponsePending
		{
			get
			{
				var firstResponseMetric = MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(metric =>
					metric.IME_MetricCode == IncidentMetricConstants.FirstResponseTime);

				return firstResponseMetric != null &&
					   firstResponseMetric.IME_EndTimeUtc == ZDateTime.Empty &&
					   firstResponseMetric.IME_CalculatedMetric == 0;
			}
		}

		#endregion

		#region Chargeable Fields

		protected bool ReadOnlyStateForChargeableFields
		{
			get { return !IM_ChargableWork || SupportFieldsReadonly; }
		}

		#endregion

		#region Stage

		public ZString Stage
		{
			get { return Lookups.StageList.GetDescriptionFromCode(IM_Category); }
		}

		public ZPropertyInfo StageInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Stage), x => IM_CategoryInfo); }
		}

		#endregion

		#region Outage Duration

		ZString outageDuration;

		public ZString OutageDuration
		{
			get
			{
				return outageDuration;
			}
			set
			{
				if (outageDuration != value)
				{
					outageDuration = value;
					OutageDurationInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo OutageDurationInfo => GetZPropertyInfo(nameof(OutageDuration));

		#endregion

		#region Overall Assigned To

		public ZString OverallAssignedToCode
		{
			get
			{
				ZString result = ZString.Empty;
				var task = CurrentTask;
				if (task != null)
				{
					if (!task.P9_GS_NKAssignedStaffMember.IsEmpty)
					{
						result = task.P9_GS_NKAssignedStaffMember;
					}
					else if (!task.P9_G4_RequiredCapability.IsEmpty && task.RequiredCapability != null)
					{
						result = task.RequiredCapability.G4_Code;
					}
				}
				else if (IM_Status == SupportIncidentLookups.Status.Closed && WorkflowItems.Count > 0)
				{
					var staff = LastTaskClosedByStaff;
					result = staff != null ? staff.GS_Code : ZString.Empty;
				}
				else if (OverallAssignedToStaff != null)
				{
					result = OverallAssignedToStaffNK;
				}
				return result;
			}
		}

		public ZPropertyInfo OverallAssignedToCodeInfo
		{
			get { return GetZPropertyInfo(nameof(OverallAssignedToCode)); }
		}

		public ZString OverallAssignedToDescription
		{
			get
			{
				ZString result = ZString.Empty;
				var task = CurrentTask;
				if (task != null)
				{
					if (task.AssignedStaffMember != null)
					{
						result = task.AssignedStaffMember.GS_FullName;
					}
					else if (!task.P9_G4_RequiredCapability.IsEmpty)
					{
						result = task.RequiredCapability?.G4_Description ?? ZString.Empty;
					}
				}
				else if (IM_Status == SupportIncidentLookups.Status.Closed && WorkflowItems.Count > 0)
				{
					var staff = LastTaskClosedByStaff;
					result = staff != null ? staff.GS_FullName : ZString.Empty;
				}
				else if (OverallAssignedToStaff != null)
				{
					result = OverallAssignedToStaff.GS_FullName;
				}
				return result;
			}
		}

		public ZPropertyInfo OverallAssignedToDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(OverallAssignedToDescription)); }
		}

		public ZString OverallAssignedToLabelText
		{
			get
			{
				ZString result = "Assigned To:";
				var task = CurrentTask;
				if (task != null)
				{
					if (!task.P9_GS_NKAssignedStaffMember.IsEmpty)
					{
						result = "Task Assigned:";
					}
					else if (!task.P9_G4_RequiredCapability.IsEmpty)
					{
						result = "Capability:";
					}
				}
				else if (IM_Status == SupportIncidentLookups.Status.Closed)
				{
					if (WorkflowItems.Count > 0)
					{
						result = "Last Task Closed By:";
					}
					else
					{
						result = "Incident Closed By:";
					}
				}
				return result;
			}
		}

		public ZPropertyInfo OverallAssignedToLabelTextInfo
		{
			get { return GetZPropertyInfo(nameof(OverallAssignedToLabelText)); }
		}

		[RelatedBusinessObject("OverallAssignedToStaff")]
		public ZString OverallAssignedToStaffNK
		{
			get
			{
				switch (IM_Category)
				{
					case SupportIncidentCategoriesList.Codes.Support:
						return IM_GS_NKCustServiceContact;

					default:
						return IM_GS_NKAssignedToCurrent;
				}
			}
		}

		public GlbStaff OverallAssignedToStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, OverallAssignedToStaffNK); }
		}

		public GlbStaff LastTaskClosedByStaff
		{
			get
			{
				GlbStaff result = null;
				var lastClosedTask = WorkflowItems.Tasks.Cast<ProcessTask>()
						.Where(task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed && !task.P9_GS_NKAssignedStaffMember.IsEmpty)
						.OrderByDescending(task => task.P9_CompletedTimeUtc)
						.FirstOrDefault();
				if (lastClosedTask != null)
				{
					result = lastClosedTask.AssignedStaffMember;
				}
				return result;
			}
		}

		#endregion

		#region IM_Product

		public override ZString IM_Product
		{
			get { return base.IM_Product; }
			set
			{
				if (base.IM_Product != value)
				{
					base.IM_Product = value;
					RecalculateProductArea();
					HandleWorflowPropertyChange();
				}
			}
		}

		#endregion

		#region IM_Module

		[List("Lookups.ModuleListEnabledModulesOnly")]
		[MaxLength(3)]
		public override ZString IM_Module
		{
			get { return base.IM_Module; }
			set
			{
				if (base.IM_Module != value)
				{
					var preStatus = IM_Status;
					var hadNotClosedTasks = WorkflowItems.Tasks.Cast<ProcessTask>().Any(t => t.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && t.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled);

					base.IM_Module = value;
					ContactAccreditationStatusInfo.RefreshBinding();
					IM_ServiceTypeInfo.RefreshBinding();

					if (!IM_ModuleInfo.HasErrors())
					{
						RecalculateProductArea();
					}

					HandleWorflowPropertyChange();
					TriggerOnCloseIncident(preStatus, hadNotClosedTasks);
				}
			}
		}

		void TriggerOnCloseIncident(ZString preStatus, bool hadNotClosedTasks)
		{
			if (hadNotClosedTasks &&
				preStatus != IM_Status &&
				IM_Status == SupportIncidentLookups.Status.Closed &&
				WorkflowItems.Tasks.Cast<ProcessTask>().All(t => t.P9_Status == ProcessTaskStatusCodeList.Codes.Closed || t.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled))
			{
				InvokeOnCloseIncidentEvent();
			}
		}

		internal virtual void InvokeOnCloseIncidentEvent()
		{
			OnCloseIncident?.Invoke(null, new TaskStatusChangeEventArgs() { IgnoreStatusRollback = true });
		}

		public ModuleListType ModuleType
		{
			get { return IncidentApprovalLookups.GetModuleListType(IM_Priority); }
		}

		public ZString IM_ModuleDescription
		{
			get { return Lookups.ProductAreaIndependentModuleList.GetDescriptionFromCode(IM_Module); }
		}

		public void RecalculateProductArea(bool shouldOverrideTriage = false)
		{
			if (IsGroupControlled || this.HasContext(SupportIncident.Context.OnOverrideProductClassification) || this.HasContext(SupportIncident.Context.InIncidentResolvedServiceTask))
			{
				return;
			}

			if (!shouldOverrideTriage && IncidentTriage != null && ProductArea == IncidentTriage.IMT_ProductArea)
			{
				return;
			}

			var newProductArea = GetRecalculatedProductArea();
			if (!newProductArea.IsEmpty)
			{
				ProductArea = newProductArea;
			}
		}

		#endregion

		#region SourceModule

		[LoggingValueChanges(ChangedFieldDescription.IM_SourceModuleId)]
		public override ZString IM_SourceModuleId
		{
			get { return base.IM_SourceModuleId; }
			set
			{
				if (base.IM_SourceModuleId != value)
				{
					base.IM_SourceModuleId = value;

					if (IsInDatabase)
					{
						isSourceModuleOverridenCache = true;
					}

					if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(IncidentTriage?.IMT_ProductArea))
					{
						ProductArea = IncidentTriage.IMT_ProductArea;
					}
					else
					{
						RecalculateProductArea(shouldOverrideTriage: true);
					}

					HandleWorflowPropertyChange();
				}
			}
		}

		ZBool? isSourceModuleOverridenCache;
		public ZBool IsSourceModuleOverriden
		{
			get
			{
				if (!isSourceModuleOverridenCache.HasValue)
				{
					var mostRecentLogSourceModuleChange = FieldChangedEventLogger.MostRecentLogStatusChange(ChangedFieldDescription.IM_SourceModuleId);
					var expectedReferenceFreeText = FieldChangedEventLogger.BuildReferenceFreeText(ChangedFieldDescription.IM_SourceModuleId, "", IM_SourceModuleId);
					isSourceModuleOverridenCache = (mostRecentLogSourceModuleChange != null) && (mostRecentLogSourceModuleChange.ReferenceFreeText.Equals(expectedReferenceFreeText));
				}
				return isSourceModuleOverridenCache.Value;
			}
		}

		public static ZString GetSourceModuleWithPath(ZString moduleID, ZString product)
		{
			if (string.IsNullOrEmpty(moduleID) || moduleID.EqualsIgnoringCase(IncidentApproval.NotAvailableActiveModuleID))
			{
				return "<Not Available>";
			}

			if (moduleID == ModuleListBuilder.Codes.All)
			{
				return ModuleListBuilder.Descriptions.All;
			}

			var sourceModule = EDIDataRegistry.Instance.SourceModules.Value.GetSourceModule(moduleID, product);
			if (sourceModule != null)
			{
				return sourceModule.Path + " " + sourceModule.Description;
			}

			return moduleID;
		}

		public ZString SourceModuleWithPath => GetSourceModuleWithPath(IM_SourceModuleId, IM_Product);

		public ZWrappedPropertyInfo SourceModuleWithPathInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SourceModuleWithPath), x => IM_SourceModuleIdInfo); }
		}

		#endregion

		#region IM_Source

		public override ZString IM_Source
		{
			get { return base.IM_Source; }
			set
			{
				if (base.IM_Source != value)
				{
					base.IM_Source = value;
					base.IM_SourceInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region IM_Category

		[LoggingValueChanges(ChangedFieldDescription.IM_Category)]
		public override ZString IM_Category
		{
			get { return base.IM_Category; }
			set
			{
				if (base.IM_Category != value)
				{
					if (!IsFromPerformAction && IsInDatabase && RelatedWorkItems.Any() && base.IM_Category == SupportIncidentCategoriesList.Codes.Defect && value == SupportIncidentCategoriesList.Codes.Support)
					{
						var key = "SupportIncident.Unexpected_Category_Change";
						var message = new StackTrace().ToString();
						ErrorReporter.ReportOnce(key, message);
						Logs.AddNew(Events.ErrorReport, key);
					}

					base.IM_Category = value;

					IM_ChargableWorkInfo.RefreshBinding();
					PropagateClientToFeatureRequestIfRequired(true);
					Validation.ValidateIM_Priority();
				}
			}
		}

		public void SetIncidentStageWithoutReason(ZString newStage)
		{
			if (!this.IM_Category.Equals(newStage) && Lookups.StageList.ContainsCode(newStage))
			{
				this.IM_Category = newStage;
				if (newStage != SupportIncidentCategoriesList.Codes.Support)
				{
					var escalationText = string.Format(CultureInfo.InvariantCulture, IncidentConstants.StageChangedMessageTemplate, Lookups.StageList.GetDescriptionFromCode(newStage));
					AddPublicSystemLogMessage(escalationText);
				}
			}
		}

		#endregion

		#region IM_ResolutionCode

		[LoggingValueChanges(ChangedFieldDescription.IM_ResolutionCode)]
		public override ZString IM_ResolutionCode
		{
			get => base.IM_ResolutionCode;
			set
			{
				if (base.IM_ResolutionCode != value)
				{
					base.IM_ResolutionCode = value;

					if (IsResolutionCodeClosedOrResolved(value))
					{
						return;
					}

					IM_ClosureResolution = string.Empty;
				}
			}
		}

		public ZString IM_ResolutionCodeDescription
		{
			get { return Lookups.StatusDispositionList.GetDescriptionFromCode(IM_ResolutionCode); }
		}

		public ZPropertyInfo IM_ResolutionCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(IM_ResolutionCodeDescription)); }
		}

		#endregion

		#region ERequestStatus

		public ZString ERequestStatus
		{
			get => IM_ResolutionCode;
			set { IM_ResolutionCode = value; }
		}

		public ZString ERequestStatusDescription
		{
			get { return IM_ResolutionCodeDescription; }
		}

		public ZPropertyInfo ERequestStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ERequestStatusDescription)); }
		}

		#endregion

		#region IM_ClosureResolution

		public ZString IM_ClosureResolutionDescription
		{
			get { return Lookups.StatusDispositionList.GetDescriptionFromCode(IM_ClosureResolution); }
		}

		public ZPropertyInfo IM_ClosureResolutionDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(IM_ClosureResolutionDescription)); }
		}

		#endregion

		#region IM_RN_NKCountry

		[List("Lookups.CountryList")]
		public override ZString IM_RN_NKCountry
		{
			get { return base.IM_RN_NKCountry; }
			set
			{
				if (base.IM_RN_NKCountry != value)
				{
					base.IM_RN_NKCountry = value;
					HandleWorflowPropertyChange();
				}
			}
		}

		#endregion

		#region IM_Priority

		public override ZString IM_Priority
		{
			get { return base.IM_Priority; }
			set
			{
				if (value != IM_Priority)
				{
					var criticalityChangeFrom = IM_Priority;
					if (PreviouslySavedCriticality == string.Empty)
					{
						PreviouslySavedCriticality = criticalityChangeFrom;
					}

					var previousModuleListType = IncidentApprovalLookups.GetModuleListType(IM_Priority);
					if (previousModuleListType != ModuleListType.Unspecified)
					{
						var newModuleListType = IncidentApprovalLookups.GetModuleListType(value);
						if (newModuleListType != ModuleListType.Unspecified && previousModuleListType != newModuleListType)
						{
							ProductArea = "";
						}
					}

					base.IM_Priority = value;

					if (criticalityChangeFrom == Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest)
					{
						IM_ResolutionCode = ZString.Empty;
						CalculateWorkflowDependentProperties();
					}

					if (!IsInDatabase)
					{
						var defaultStageMapping = Lookups.CriticalityDefaultStageMapping;
						var stageCriticalityMapping = Lookups.ActiveStageCriticalityMapping;
						if (defaultStageMapping.ContainsKey(IM_Priority)
							&& stageCriticalityMapping.ContainsKey(IM_Category)
							&& !stageCriticalityMapping[IM_Category].Contains(IM_Priority))
						{
							IM_Category = defaultStageMapping[IM_Priority];
						}
					}

					if (value == Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest)
					{
						IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.DevelopmentEstimateRequested, this);
						IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate;
					}

					PropagateClientToFeatureRequestIfRequired(true);
				}
			}
		}

		public ZString PriorityDescription
		{
			get { return (IM_Priority.IsEmpty) ? ZString.Empty : new ZString(IM_Priority + " - " + Lookups.CriticalityList.GetDescriptionFromCode(IM_Priority)); }
		}

		public ZPropertyInfo PriorityDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(PriorityDescription)); }
		}

		public ZString PriorityDisplayedOnClientSide
		{
			get
			{
				var priorityIsCr8OrCr9 = (IM_Priority == Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement || IM_Priority == Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest);
				if (priorityIsCr8OrCr9 && !ClientSupportsCr8Cr9)
				{
					return Core.Constants.CustomerService.CriticalityCodes.CR5_Training;
				}
				else
				{
					return IM_Priority;
				}
			}
			set
			{
				var priorityIsCr8OrCr9 = (IM_Priority == Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement || IM_Priority == Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest);
				if (priorityIsCr8OrCr9 && !ClientSupportsCr8Cr9 && value == Core.Constants.CustomerService.CriticalityCodes.CR5_Training)
				{
					return; // do not update IM_Priority
				}
				else
				{
					IM_Priority = value;
				}
			}
		}

		public bool IsDefect => IncidentConstants.IsDefect(IM_Priority);

		#endregion

		#region IM_Status

		public ZString IM_StatusDescription
		{
			get { return Lookups.StatusList.GetDescriptionFromCode(IM_Status); }
		}

		public ZPropertyInfo IM_StatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(IM_StatusDescription)); }
		}

		#endregion

		#region IM_GS_NKSpecifiedBy

		public override ZPropertyInfo IM_GS_NKSpecifiedByInfo
		{
			get { return GetZPropertyInfo(IncidentMainSchema.Constants.IM_GS_NKSpecifiedBy, "Business Consultant"); }
		}

		#endregion

		#region ResolutionComment

		public ZString ResolutionComment
		{
			get { return ShouldShowResolutionComment ? ResolutionNoteText : ZString.Empty; }
		}

		public ZPropertyInfo ResolutionCommentInfo
		{
			get { return GetZPropertyInfo(nameof(ResolutionComment)); }
		}

		public ZBool ShouldShowResolutionComment
		{
			get { return (IM_Category != SupportIncidentCategoriesList.Codes.Support || IM_Status == SupportIncidentLookups.Status.Closed); }
		}

		#endregion

		#region Close In Support Date

		public ZDateTime CloseInSupportDate
		{
			get { return IM_PlannedInstall; }
			set { IM_PlannedInstall = value; }
		}

		public ZPropertyInfo CloseInSupportDateInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CloseInSupportDate), x => IM_PlannedInstallInfo); }
		}

		#endregion

		#region Initial Staff Assignment Date

		public ZDateTime InitialStaffAssignmentDate
		{
			get { return IM_OrderReceived; }
			set { IM_OrderReceived = value; }
		}

		#endregion

		#region System Create Date

		public ZDateTime SystemCreateTimeInUTC
		{
			get { return IM_InstallDate; }
			set { IM_InstallDate = value; }
		}

		public ZDateTime SystemCreateTimeInLocalTimeZone
		{
			get
			{
				if (systemCreateTimeInLocalTimeZone == null)
				{
					systemCreateTimeInLocalTimeZone = IM_SystemCreateTimeUtc;
					if (!SystemCreateTimeInUTC.IsEmpty && GlbBranch.CurrentBranch.HomePort != null && GlbBranch.CurrentBranch.HomePort.TimeZoneSet != null)
					{
						ITimeZone timeZone = GlbBranch.CurrentBranch.HomePort.TimeZoneSet.GetCalculationTimeZone();
						if (timeZone != null)
						{
							systemCreateTimeInLocalTimeZone = timeZone.ToLocalTime(SystemCreateTimeInUTC.ToDateTime());
						}
					}
				}
				return systemCreateTimeInLocalTimeZone.Value;
			}
		}

		public ZPropertyInfo SystemCreateTimeInLocalTimeZoneInfo
		{
			get { return GetZPropertyInfo(nameof(SystemCreateTimeInLocalTimeZone)); }
		}

		ZDateTime? systemCreateTimeInLocalTimeZone;

		#endregion

		#region Resolution Note

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString ResolutionNoteText
		{
			get { return ResolutionNote.Text; }
			set
			{
				ResolutionNote.Text = value;
				ResolutionNoteTextInfo.RefreshBinding();
				this.HasChanges = true;
			}
		}

		public bool ResolutionNoteText_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo ResolutionNoteTextInfo
		{
			get { return GetZPropertyInfo(nameof(ResolutionNoteText)); }
		}

		internal UniqueNote ResolutionNote
		{
			get { return resolutionNote ?? (resolutionNote = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentResolutionDetail)); }
		}
		UniqueNote resolutionNote;

		#endregion

		#region Incident Comment

		public ZString IncidentComment
		{
			get { return IncidentCommentNote.Text; }
			set
			{
				IncidentCommentNote.Text = value;
				this.HasChanges = true;
			}
		}

		UniqueNote IncidentCommentNote
		{
			get { return incidentCommentNote ?? (incidentCommentNote = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentComment)); }
		}
		UniqueNote incidentCommentNote;

		#endregion

		#region IM_ClientContractStatus

		/// <summary>
		/// The following methods must be synchronized logically.
		/// Enterprise.Client.EDI.IncidentManager.Business.SupportIncident.IM_ClientContractStatus
		/// Enterprise.Client.EDI.IncidentManager.Module.SupportIncidentFilterBusinessObject.GoLiveDateQuery
		/// </summary>
		public ZString IM_ClientContractStatus
		{
			get
			{
				ZString result = ZString.Empty;

				var client = (EDIOrgHeader)Client;
				if (client != null)
				{
					result = "Current";

					LicenceHeader licenceHeader = null;
					if (client.LicCompany != null)
					{
						var licenceHeaders = client.LicCompany.LicHeadersForAllDatabases.Cast<LicenceHeader>();
						if (!IM_LD.IsEmpty)
						{
							licenceHeader = licenceHeaders.FirstOrDefault(x => x.LA_LD == IM_LD);
						}
						if (licenceHeader == null)
						{
							licenceHeader = licenceHeaders.FirstOrDefault(x => x.Database.LD_Product == IM_Product) ?? licenceHeaders.OrderBy(x => x.LA_SiteLiveDate).FirstOrDefault(x => !x.LA_SiteLiveDate.IsEmpty);
						}
					}
					else if (Database != null)
					{
						var dbLicenceHeaders = Database.ActiveLicHeadersForAllCompanies.Cast<LicenceHeader>().ToArray();
						licenceHeader = dbLicenceHeaders.OrderBy(x => x.LA_SiteLiveDate).FirstOrDefault(x => !x.LA_SiteLiveDate.IsEmpty) ?? dbLicenceHeaders.FirstOrDefault();
					}

					if (licenceHeader != null && licenceHeader.LA_ContractExpiryDate.IsValid && ZDateTime.Now.Date > licenceHeader.LA_ContractExpiryDate.Date)
					{
						result = "Contract has expired (" + licenceHeader.LA_ContractExpiryDate.Date.ToShortDateString() + ")";
					}
					else if (client.ManagementGrouping.CompanyData.OB_AROnCreditHold)
					{
						result = "Credit on hold";
					}
					else if (licenceHeader != null)
					{
						ZString desc = licenceHeader.Lookups.SupportModeList.GetDescriptionFromCode(licenceHeader.LA_SupportMode);
						if (!desc.IsEmpty)
						{
							result += " - " + desc;
						}
					}

					if (licenceHeader != null && !licenceHeader.LA_SiteLiveDate.IsEmpty)
					{
						result += " - Go-Live: " + licenceHeader.LA_SiteLiveDate.ToShortDateString();
					}
				}

				return result;
			}
		}

		public ZPropertyInfo IM_ClientContractStatusInfo
		{
			get { return base.GetZPropertyInfo(nameof(IM_ClientContractStatus)); }
		}

		#endregion

		#region Work Item Status

		public ZString WorkItemStatus
		{
			get
			{
				ZString result = "";
				if (RelatedWorkItemsReadOnly.Count > 0)
				{
					NewWorkItem workItem = GetFirstNonClosedRelatedWorkItem() ?? RelatedWorkItemsReadOnly[0];

					var taskStatus = workItem.OverallTaskStatusDescription;
					result = workItem.Lookups.StatusList.GetDescriptionFromCode(workItem.WKI_Status);
					if (!taskStatus.IsEmpty)
					{
						result += " - " + taskStatus;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo WorkItemStatusInfo
		{
			get { return GetZPropertyInfo(nameof(WorkItemStatus)); }
		}

		NewWorkItem GetFirstNonClosedRelatedWorkItem()
		{
			NewWorkItem result = null;

			foreach (NewWorkItem workItem in RelatedWorkItemsReadOnly)
			{
				if (workItem.WKI_Status != ProcessTaskStatusCodeList.Codes.Closed)
				{
					result = workItem;
					break;
				}
			}

			return result;
		}

		#endregion

		#region Work Item Assigned To

		public ZGuid WorkItemAssignedToPK
		{
			get { return WorkItemAssignedToStaff != null ? WorkItemAssignedToStaff.PK : ZGuid.Empty; }
		}

		public ZPropertyInfo WorkItemAssignedToPKInfo
		{
			get { return GetZPropertyInfo(nameof(WorkItemAssignedToPK)); }
		}

		public ZString WorkItemAssignedTo
		{
			get { return ZString.Join(", ", RelatedWorkItemsReadOnly.Cast<NewWorkItem>().Where(wi => !wi.AssignedStaffCode.IsEmpty).Select(wi => wi.AssignedStaffCode).ToArray()); }
		}

		GlbStaff WorkItemAssignedToStaff
		{
			get { return RelatedWorkItemsReadOnly.Count > 0 ? RelatedWorkItemsReadOnly[0].AssignedToStaff : null; }
		}

		public ZPropertyInfo WorkItemAssignedToInfo
		{
			get { return GetZPropertyInfo(nameof(WorkItemAssignedTo)); }
		}

		#endregion

		#region Work Item Number

		public ZString WorkItemNumber
		{
			get { return ZString.Join(", ", RelatedWorkItemsReadOnly.Cast<NewWorkItem>().Where(wi => !wi.WKI_WorkItemNumber.IsEmpty).Select(wi => wi.WKI_WorkItemNumber).ToArray()); }
		}

		public ZPropertyInfo WorkItemNumberInfo
		{
			get { return GetZPropertyInfo(nameof(WorkItemNumber)); }
		}

		#endregion

		#region HasWorkItemsWithShelfCheckInTask

		public bool HasWorkItemsWithShelfCheckInTask
		{
			get
			{
				foreach (NewWorkItem workItem in RelatedWorkItems)
				{
					if (workItem.HasShelfCheckInTasks)
					{
						return true;
					}
				}

				return false;
			}
		}

		#endregion

		#region Can Create Work Item

		public bool CanCreateWorkItem
		{
			get
			{
				return IM_Category != SupportIncidentCategoriesList.Codes.Support;
			}
		}

		public override void PopulateWorkItem(NewWorkItem workItem)
		{
			base.PopulateWorkItem(workItem);

			workItem.WKI_WorkItemType = IM_Product;
			workItem.WKI_WorkItemArea = ProductArea;
			workItem.WKI_ActivityType = IM_Module;
			workItem.WKI_Summary = IM_Description;
			if (IM_Category == SupportIncidentCategoriesList.Codes.Defect)
			{
				workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			}
			else if (IsFeatureRequest)
			{
				workItem.WKI_ActivitySubtype = ZString.Empty;
			}
		}

		#endregion

		#region ClientLocalTime

		public ZDateTime ClientLocalTime
		{
			get
			{
				ITimeZone zone = null;

				if (BranchAddress != null && BranchAddress.RelatedPortCode != null && BranchAddress.RelatedPortCode.TimeZoneSet != null)
				{
					zone = BranchAddress.RelatedPortCode.TimeZoneSet.GetCalculationTimeZone();
				}
				else if (Client != null && Client.ClosestPort != null && Client.ClosestPort.TimeZoneSet != null)
				{
					zone = Client.ClosestPort.TimeZoneSet.GetCalculationTimeZone();
				}

				ITimeZone localZone = GlbBranch.CurrentBranch.HomePort.TimeZoneSet.GetCalculationTimeZone();
				DateTime universalTime = localZone.ToUniversalTime(ZDateTime.Now.ToDateTime());

				return zone != null ? zone.ToLocalTime(universalTime) : ZDateTime.Now;
			}
		}

		public ZPropertyInfo ClientLocalTimeInfo
		{
			get { return GetZPropertyInfo(nameof(ClientLocalTime)); }
		}

		public ZString ClientLocalTimeString
		{
			get { return ClientLocalTime.ToLongTimeString(); }
		}

		public ZPropertyInfo ClientLocalTimeStringInfo
		{
			get { return GetZPropertyInfo(nameof(ClientLocalTimeString)); }
		}
		#endregion

		#region Defect Compliance

		public bool IM_IsADefectPerDefinition_ReadOnly
		{
			get { return true; }
		}

		public bool IM_DefectNonCompliantReason_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Realted Items

		internal IEnumerable<GenPivot> RelatedWorkItemsPivot
		{
			get
			{
				if (relatedItemsWRKPivot == null)
				{
					relatedItemsWRKPivot = new GenPivotCollection(this, Core.Constants.GenPivotTypes.ProcessManagement);
				}
				return relatedItemsWRKPivot;
			}
		}
		GenPivotCollection relatedItemsWRKPivot;

		// Include GenPivot type WRK only
		// For readonly binding
		public WorkTaskRelatedItemCollection RelatedWorkTaskItemsReadOnly
		{
			get
			{
				if (relatedWorkTaskItemsReadOnly == null)
				{
					var localRelatedWorkTaskItemsReadOnly = new WorkTaskRelatedItemGenPivotCollection(this);
					localRelatedWorkTaskItemsReadOnly.Load();
					relatedWorkTaskItemsReadOnly = localRelatedWorkTaskItemsReadOnly;
				}
				return relatedWorkTaskItemsReadOnly;
			}
		}
		WorkTaskRelatedItemCollection relatedWorkTaskItemsReadOnly;

		// Include GenPivot type WRK and OPP
		// Include IncidentGroupLink
		public WorkTaskRelatedItemCollection RelatedItems
		{
			get
			{
				if (relatedItems == null)
				{
					var localRelatedItems = new SupportIncidentRelatedItemGenPivotCollection(this);
					localRelatedItems.RelatedItemAdded += RelatedItems_RelatedItemAdded;
					localRelatedItems.RelatedItemRemoved += RelatedItems_RelatedItemRemoved;
					localRelatedItems.Load();
					relatedItems = localRelatedItems;
					relatedItems.CountChanged += delegate
					{ ReloadRelatedWorkItems(); };
				}
				return relatedItems;
			}
		}
		WorkTaskRelatedItemCollection relatedItems;

		void RelatedItems_RelatedItemAdded(object sender, RelatedItemEventArgs e)
		{
			if (e.BusinessObject is EDIProject && IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest && IM_Source == SupportIncidentLookups.SourceListConstants.ERequestPortal)
			{
				IM_Source = SupportIncidentLookups.SourceListConstants.CreatedFromProject;
				isRelatedProjectLoaded = false;
				RelatedProjectPKInfo.RefreshBinding();
			}
			else if (e.BusinessObject is NewWorkItem newWorkItem)
			{
				((IWorkItemRelatedItem)this).OnWorkItemAdded(newWorkItem);
				ReCalculateTDTMetricCount();
			}
			else if (e.BusinessObject is EDIOrgOpportunity opportunity)
			{
				AddOpportunityRelatedActivity(opportunity);
			}

			relatedWorkTaskItemsReadOnly?.Load();
		}

		void RelatedItems_RelatedItemRemoved(object sender, RelatedItemEventArgs e)
		{
			if (e.BusinessObject is EDIProject && IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest && IM_Source == SupportIncidentLookups.SourceListConstants.CreatedFromProject)
			{
				IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
				isRelatedProjectLoaded = false;
				RelatedProjectPKInfo.RefreshBinding();
			}
			else if (e.BusinessObject is NewWorkItem newWorkItem)
			{
				((IWorkItemRelatedItem)this).OnWorkItemRemoved(newWorkItem);
				ReCalculateTDTMetricCount();
			}
			else if (e.BusinessObject is EDIOrgOpportunity opportunity)
			{
				DeleteOpportunityRelatedActivity(opportunity, e.IsParent);
			}

			relatedWorkTaskItemsReadOnly?.Load();
		}

		public ZBool ShowOnlyNonClosedItems
		{
			get { return !FilteredRelatedItems.IncludeAllItems; }
			set { FilteredRelatedItems.IncludeAllItems = !value; }
		}

		public FilteredWorkTaskRelatedItemCollection FilteredRelatedItems
		{
			get
			{
				if (filteredRelatedItems == null)
				{
					filteredRelatedItems = new FilteredWorkTaskRelatedItemCollection(RelatedItems);
				}
				return filteredRelatedItems;
			}
		}
		FilteredWorkTaskRelatedItemCollection filteredRelatedItems;

		public ZQuery GetLinkedToWorkItemAsDefectCausingWorkItemQuery()
		{
			ZQuery query = new ZQuery();

			ZDBOnlyQuery defectCausedSubQuery = new ZDBOnlyQuery(typeof(NewWorkItem));
			ZDBOnlySubQuery defectCausedByWorkItem = new ZDBOnlySubQuery(typeof(GenPivot), WorkItemSchema.PK, true);
			defectCausedByWorkItem.AddToFilter(GenPivotSchema.XX_RelationType, EDIGenPivotTypes.DefectCausedByWorkItem);
			defectCausedByWorkItem.AddToFilter(GenPivotSchema.XX_Relation2TableCode, IncidentMainSchema.Constants.Prefix);
			defectCausedByWorkItem.AddToFilter(GenPivotSchema.XX_Relation2ID, PK);
			defectCausedByWorkItem.AddToFilter(GenPivotSchema.XX_Relation1TableCode, WorkItemSchema.Constants.Prefix);

			defectCausedSubQuery.AddSubQuery(WorkItemSchema.PK, GenPivotSchema.XX_Relation1ID, defectCausedByWorkItem, JoinCondition.And);
			query.AddToFilter(defectCausedSubQuery);

			return query;
		}

		public void ReloadRelatedWorkItems()
		{
			if (relatedWorkItems != null)
			{
				relatedWorkItems = new NewWorkItemRelatedCollectionView(RelatedItems);
			}
		}

		#endregion

		#region Accreditation Status

		public ZString ContactAccreditationStatus
		{
			get
			{
				string result = "";

				var ediOrgContact = (EDIOrgContact)Contact;
				if (ediOrgContact != null)
				{
					var applicant = ediOrgContact.RelatedCertificateApplicant;
					bool isContactAccredited = false;
					if (applicant != null)
					{
						isContactAccredited = applicant.HasCompletedJobSkill(RecruiterTestCategory.Codes.Accreditation, IM_Module);
					}
					result = (isContactAccredited) ? "Accredited for " + IM_Module : "Not accredited for " + IM_Module;
				}

				return result;
			}
		}

		public ZPropertyInfo ContactAccreditationStatusInfo
		{
			get { return GetZPropertyInfo(nameof(ContactAccreditationStatus)); }
		}

		#endregion

		#region Current Task Estimated Date

		public ZString CurrentTaskEstimatedDateAsText
		{
			get
			{
				ZString result = ZString.Empty;
				var task = CurrentTask;
				if (task != null)
				{
					result = task.P9_ScheduledDate.ToZDateTime().ToLongTimeString();
				}
				return result;
			}
		}

		public ZPropertyInfo CurrentTaskEstimatedDateAsTextInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentTaskEstimatedDateAsText)); }
		}

		#endregion

		#region Client Relationship Manager/Coordinator

		public GlbStaff ClientPrimaryRelationshipManager
		{
			get { return GetStaffAssignmentCodeBasedOnCurrentUserBranch(EDIOrgStaffAssignmentsLookups.PrimaryKAM); }
		}

		public GlbStaff ClientSecondaryRelationshipManager
		{
			get { return GetStaffAssignmentCodeBasedOnCurrentUserBranch(EDIOrgStaffAssignmentsLookups.SecondaryKAM); }
		}

		public GlbStaff ClientRelationshipCoordinator
		{
			get { return GetStaffAssignmentCodeBasedOnCurrentUserBranch("RC"); }
		}

		GlbStaff GetStaffAssignmentCodeBasedOnCurrentUserBranch(ZString role)
		{
			GlbStaff result = null;
			if (Client != null && GlbStaff.CurrentUser.HomeBranch != null)
			{
				var homeCompanyForCurrentUser = GlbStaff.CurrentUser.HomeBranch.Company;
				var regionCompanyCode = EDIDataRegistry.Instance.RelationshipManagerCompanyLookup.Value.GetDescriptionFromCode(homeCompanyForCurrentUser.GC_Code);
				var regionCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, regionCompanyCode);
				var lookupCompany = regionCompany ?? homeCompanyForCurrentUser;

				var staffCode = GetStaffCode(lookupCompany, role);
				if (!staffCode.IsEmpty)
				{
					result = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode);
				}
			}
			return result;
		}

		ZString GetStaffCode(GlbCompany lookupCompany, ZString role)
		{
			var staffAssignments = Client.GetStaffAssignmentsForGlbCompany(lookupCompany).Cast<OrgStaffAssignments>();

			var productSpecificStaffWithCompany = staffAssignments.FirstOrDefault(s => s.O8_Role == role && s.O8_GC == lookupCompany.PK && !IM_Product.IsEmpty && s.O8_Product == IM_Product);
			if (productSpecificStaffWithCompany != null)
			{
				return productSpecificStaffWithCompany.O8_GS_NKPersonResponsible;
			}

			var staffAssignmentWithCompany = staffAssignments.FirstOrDefault(s => s.O8_Role == role && s.O8_GC == lookupCompany.PK && s.O8_Product.IsEmpty);
			if (staffAssignmentWithCompany != null)
			{
				return staffAssignmentWithCompany.O8_GS_NKPersonResponsible;
			}

			var productSpecificStaff = staffAssignments.FirstOrDefault(s => s.O8_Role == role && !IM_Product.IsEmpty && s.O8_Product == IM_Product);
			if (productSpecificStaff != null)
			{
				return productSpecificStaff.O8_GS_NKPersonResponsible;
			}

			var staffAssignment = staffAssignments.FirstOrDefault(s => s.O8_Role == role && s.O8_Product.IsEmpty);
			if (staffAssignment != null)
			{
				return staffAssignment.O8_GS_NKPersonResponsible;
			}

			return ZString.Empty;
		}

		public ZString RelationshipStaffCodeAndName
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				AddStaff(builder, ClientPrimaryRelationshipManager);
				AddStaff(builder, ClientSecondaryRelationshipManager);
				if (builder.Length == 0)
				{
					AddStaff(builder, ClientRelationshipCoordinator);
				}
				return builder.ToString();
			}
		}

		void AddStaff(ZStringBuilder builder, GlbStaff staff)
		{
			if (staff != null)
			{
				if (builder.Length != 0)
				{
					builder.Append(" / ");
				}
				builder.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1}", staff.GS_Code, staff.GS_FullName));
			}
		}

		public ZString RelationshipManagerOrCoordinatorLabel
		{
			get
			{
				bool showingCoordinator = ClientRelationshipCoordinator != null && ClientPrimaryRelationshipManager == null && ClientSecondaryRelationshipManager == null;
				return showingCoordinator ? "Key Account Co:" : "Key Account Manager:";
			}
		}

		#endregion

		#region Last Disposition Change DateTime

		public ZDateTime LastDispositionChangeDateTimeUtc
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				ZQuery query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, ChangedFieldDescription.IM_ResolutionCode);
				StmALog lastDispositionChangeLog = this.GetLogs().Find(query).OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault();

				if (lastDispositionChangeLog != null)
				{
					result = lastDispositionChangeLog.SL_PostedTimeUtc;
				}

				return result;
			}
		}

		#endregion

		#region Client Size

		public ZString ClientSizeDescription
		{
			get { return Client != null ? Client.MiscServ.OM_CMClientSize_List.GetDescriptionFromCode(Client.MiscServ.OM_CMClientSize) : string.Empty; }
		}

		#endregion

		#region Product Area

		public ZString ProductArea
		{
			get { return IM_ProgramArea; }
			set
			{
				if (base.IM_ProgramArea != value)
				{
					base.IM_ProgramArea = value;
					HandleWorflowPropertyChange();
				}
			}
		}

		public ZWrappedPropertyInfo ProductAreaInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ProductArea), x => IM_ProgramAreaInfo); }
		}

		public ZString ProductAreaDescription
		{
			get { return !ProductArea.IsEmpty ? Lookups.ProductAreaList.GetDescriptionFromCode(ProductArea) : string.Empty; }
		}

		#endregion

		#region Product Area Assigned Staff

		public GlbStaff ProductAreaAssignedStaff
		{
			get
			{
				GlbStaff result = null;
				ZString staffCode = EDIDataRegistry.Instance.ProductAreaAssignments.Value.GetAssignedStaffCodeByProductArea(ProductArea);
				if (!staffCode.IsEmpty)
				{
					result = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode);
				}
				return result;
			}
		}

		#endregion

		#region Licence Database Hosted Location

		public ZString LicenceDatabaseHostedLocation
		{
			get { return Database != null ? Database.HostedLocationDesciption : ZString.Empty; }
		}

		#endregion

		#region Licence Database Ring Release

		public ZString ClientReleaseRing
		{
			get
			{
				return Database != null ? Database.LD_ReleaseRing : ZString.Empty;
			}
		}

		public ZString RingRelease
		{
			get { return Lookups.ReleaseRingsList.GetDescriptionFromCode(ClientReleaseRing); }
		}

		#endregion

		#region IsResolutionWizardEnabled

		public bool IsResolutionWizardEnabled => Factory.GetCachedValue($"IsResolutionWizardEnabled-{IM_Product}", () =>
		{
			var data = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.CR5ResolutionWizard);
			return data != null && data.TryDeserializeParameterAsJson<IEnumerable<string>>(out var result) && result.Contains(IM_Product.ToString());
		});

		#endregion

		#region ResolutionWizardOptionTextNote

		public ZString ResolutionWizardOptionText
		{
			get { return ResolutionWizardOptionTextNote.Text; }
			set
			{
				ResolutionWizardOptionTextNote.Text = value;
			}
		}

		UniqueNote ResolutionWizardOptionTextNote
		{
			get { return resolutionWizardOptionTextNote ?? (resolutionWizardOptionTextNote = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.ResolutionWizardOptionCode)); }
		}
		UniqueNote resolutionWizardOptionTextNote;

		#endregion

		#region Is Internal

		public bool IsInternal
		{
			get { return IM_Source != SupportIncidentLookups.SourceListConstants.ERequestPortal && IsInternalLicence; }
		}

		bool IsInternalLicence
		{
			get { return ClientCompany != null && ClientCompany.Database != null && EDIDataRegistry.Instance.InternalIncidentLicenceSettings.Value.LicenceEnterpriseKeys.ContainsLicenceEnterprise(ClientCompany.Database.LD_LE); }
		}

		public const string InternalIncidentComment = "Internal Incident";

		#endregion

		#region Handle Worflow Property Change

		void HandleWorflowPropertyChange()
		{
			if (IsInDatabase
				&& (IM_Status != SupportIncidentLookups.Status.Closed || IM_CloseTimeUtc.IsEmpty)
				&& Validation.ValidateWorkflowTemplateMatchingProperties()
				&& !SupportIncidentEvent.TriggerLastEvent(this))
			{
				IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.ChangeProperty, this);
			}
		}

		#endregion

		#region ShouldStayIndependent

		public bool ShouldStayIndependent
		{
			get
			{
				return IM_Category == SupportIncidentCategoriesList.Codes.ContentDevelopment;
			}
		}

		#endregion

		#endregion

		#region Notes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = new NoteTypeCollection();
				types.Add(EDIPredefinedNoteTypes.Instance.IncidentLog);
				types.Add(EDIPredefinedNoteTypes.Instance.IncidentDetail);
				types.Add(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog);
				types.Add(EDIPredefinedNoteTypes.Instance.IncidentResolutionDetail);
				types.Add(EDIPredefinedNoteTypes.Instance.IncidentComment);
				types.Add(EDIPredefinedNoteTypes.Instance.FeatureRequestPrerequisites);
				types.Add(EDIPredefinedNoteTypes.Instance.FeatureRequestInternalNote);
				types.Add(EDIPredefinedNoteTypes.Instance.BusinessRequirements);
				types.Add(EDIPredefinedNoteTypes.Instance.TechnicalSpecification);
				types.Add(EDIPredefinedNoteTypes.Instance.FeatureRequestSoftwareChangeNote);
				types.Add(EDIPredefinedNoteTypes.Instance.IncidentClosingStaffCode);
				types.Add(EDIPredefinedNoteTypes.Instance.IncidentClosureDate);
				types.Add(EDIPredefinedNoteTypes.Instance.IncidentResolutionComment);
				types.Add(EDIPredefinedNoteTypes.Instance.IncidentDispositionText);
				return types;
			}
		}

		public bool SynchroniseNotes()
		{
			bool modified = DetailNote.Synchronise() || DetailNote.HasChanges;
			modified |= ResolutionNote.Synchronise() || ResolutionNote.HasChanges;
			modified |= IncidentCommentNote.Synchronise() || IncidentCommentNote.HasChanges;
			modified |= FeatureRequestInternalNote.Synchronise() || FeatureRequestInternalNote.HasChanges;
			modified |= FeatureRequestPrerequisites.Synchronise() || FeatureRequestPrerequisites.HasChanges;
			modified |= BusinessRequirements.Synchronise() || BusinessRequirements.HasChanges;
			modified |= TechnicalSpecification.Synchronise() || TechnicalSpecification.HasChanges;
			modified |= SoftwareChange.Synchronise() || SoftwareChange.HasChanges;

			if (modified)
			{
				// Force this object to be treated as modified to update the audit columns.
				// IM_SystemLastEditTimeUtc will be updated again when this is saved.
				IM_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}

			return modified;
		}

		#region Project Related Incident Note

		ZBlob GetNoteValue(UniqueNote note)
		{
			return note != null ? note.Blob : ZBlob.Empty;
		}

		void SetNoteValue(ZBlob value, UniqueNote note, ZPropertyInfo info)
		{
			if (note == null || note.Blob == value)
			{
				return;
			}

			note.Blob = value;
			info.RefreshBinding();
		}

		#region FeatureRequestPrerequisites

		UniqueNote FeatureRequestPrerequisites
		{
			get
			{
				if (featureRequestPrerequisites == null)
				{
					featureRequestPrerequisites = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.FeatureRequestPrerequisites);
				}
				return featureRequestPrerequisites;
			}
		}
		UniqueNote featureRequestPrerequisites;

		public ZBlob FeatureRequestPrerequisitesAsBlob
		{
			get { return GetNoteValue(FeatureRequestPrerequisites); }
			set
			{
				SetNoteValue(value, FeatureRequestPrerequisites, FeatureRequestPrerequisitesAsBlobInfo);
			}
		}

		public ZPropertyInfo FeatureRequestPrerequisitesAsBlobInfo
		{
			get { return GetZPropertyInfo(nameof(FeatureRequestPrerequisitesAsBlob)); }
		}

		public ZString FeatureRequestPrerequisitesText
		{
			get { return FeatureRequestPrerequisitesAsBlob.ToUTF8(); }
		}

		public ZPropertyInfo FeatureRequestPrerequisitesTextInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(FeatureRequestPrerequisitesText), x => FeatureRequestPrerequisitesAsBlobInfo); }
		}

		public ZBlob FeatureRequestPrerequisitesAsBlob_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(FeatureRequestPrerequisitesAsBlob);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				FeatureRequestPrerequisitesAsBlob = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		#endregion

		#region FeatureRequestInternalNote

		UniqueNote FeatureRequestInternalNote
		{
			get
			{
				if (featureRequestInternalNote == null)
				{
					featureRequestInternalNote = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.FeatureRequestInternalNote);
				}
				return featureRequestInternalNote;
			}
		}
		UniqueNote featureRequestInternalNote;

		public ZBlob FeatureRequestInternalNoteAsBlob
		{
			get { return GetNoteValue(FeatureRequestInternalNote); }
			set
			{
				SetNoteValue(value, FeatureRequestInternalNote, FeatureRequestInternalNoteAsBlobInfo);
			}
		}

		public ZBlob FeatureRequestInternalNoteAsBlob_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(FeatureRequestInternalNoteAsBlob);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				FeatureRequestInternalNoteAsBlob = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public ZPropertyInfo FeatureRequestInternalNoteAsBlobInfo
		{
			get { return GetZPropertyInfo(nameof(FeatureRequestInternalNoteAsBlob)); }
		}

		public ZString FeatureRequestInternalNoteText
		{
			get { return FeatureRequestInternalNoteAsBlob.ToUTF8(); }
		}

		public ZPropertyInfo FeatureRequestInternalNoteTextInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(FeatureRequestInternalNoteText), x => FeatureRequestInternalNoteAsBlobInfo); }
		}

		#endregion

		#region BusinessRequirement

		protected UniqueNote BusinessRequirements
		{
			get
			{
				if (businessRequirements == null)
				{
					businessRequirements = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.BusinessRequirements);
				}
				return businessRequirements;
			}
		}
		UniqueNote businessRequirements;

		public ZBlob BusinessRequirementsAsBlob
		{
			get { return GetNoteValue(BusinessRequirements); }
			set
			{
				SetNoteValue(value, BusinessRequirements, BusinessRequirementsAsBlobInfo);
			}
		}

		public ZPropertyInfo BusinessRequirementsAsBlobInfo
		{
			get { return GetZPropertyInfo(nameof(BusinessRequirementsAsBlob)); }
		}

		public ZBlob BusinessRequirementsAsBlob_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(BusinessRequirementsAsBlob);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				BusinessRequirementsAsBlob = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		#endregion

		#region TechnicalSpecification

		UniqueNote TechnicalSpecification
		{
			get
			{
				if (technicalSpecification == null)
				{
					technicalSpecification = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.TechnicalSpecification);
				}
				return technicalSpecification;
			}
		}
		UniqueNote technicalSpecification;

		public ZBlob TechnicalSpecificationAsBlob
		{
			get { return GetNoteValue(TechnicalSpecification); }
			set
			{
				SetNoteValue(value, TechnicalSpecification, TechnicalSpecificationAsBlobInfo);
			}
		}

		public ZBlob TechnicalSpecificationAsBlob_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(TechnicalSpecificationAsBlob);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				TechnicalSpecificationAsBlob = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public ZPropertyInfo TechnicalSpecificationAsBlobInfo
		{
			get { return GetZPropertyInfo(nameof(TechnicalSpecificationAsBlob)); }
		}

		public ZString TechnicalSpecificationText
		{
			get { return TechnicalSpecificationAsBlob.ToUTF8(); }
		}

		public ZPropertyInfo TechnicalSpecificationTextInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(TechnicalSpecificationText), x => TechnicalSpecificationAsBlobInfo); }
		}

		#endregion

		#region SoftwareChange

		public UniqueNote SoftwareChange
		{
			get
			{
				if (softwareChange == null)
				{
					softwareChange = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.FeatureRequestSoftwareChangeNote);
				}
				return softwareChange;
			}
		}
		UniqueNote softwareChange;

		public ZBlob SoftwareChangeAsBlob
		{
			get { return GetNoteValue(SoftwareChange); }
			set
			{
				SetNoteValue(value, SoftwareChange, SoftwareChangeAsBlobInfo);
			}
		}

		public ZBlob SoftwareChangeAsBlob_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(SoftwareChangeAsBlob);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				SoftwareChangeAsBlob = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public ZPropertyInfo SoftwareChangeAsBlobInfo
		{
			get { return GetZPropertyInfo(nameof(SoftwareChangeAsBlob)); }
		}

		public ZString SoftwareChangeText
		{
			get { return SoftwareChangeAsBlob.ToUTF8(); }
		}

		public ZPropertyInfo SoftwareChangeTextInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SoftwareChangeText), x => SoftwareChangeAsBlobInfo); }
		}

		#endregion

		#endregion

		#endregion

		#region Actions

		#region Support

		#region Reopen

		void ReopenSupport()
		{
			CloseInSupportDate = ZDateTime.Empty;

			if (IM_GS_NKCustServiceContact.IsEmpty)
			{
				if (AssignedToCurrent != null)
				{
					IM_GS_NKCustServiceContact = AssignedToCurrent.GS_Code;
				}
				else if (GlbStaff.CurrentUser.GS_Code != User.ServiceUserCode && GlbStaff.CurrentUser.GS_Code != User.WebUserCode)
				{
					IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
				}
			}

			if (!IM_GS_NKCustServiceContact.IsEmpty)
			{
				IM_Status = SupportIncidentLookups.Status.Working;
				IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;
			}
			else
			{
				IM_Status = SupportIncidentLookups.Status.Open;
				IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			}

			Request.INC_IsCustomerResolved = false;
		}

		#endregion

		#region Assign To

		void AssignToSupportPerson(GlbStaff staff, string comment)
		{
			if (IM_Status == SupportIncidentLookups.Status.Closed)
			{
				CloseInSupportDate = ZDateTime.Empty;
				IM_Status = SupportIncidentLookups.Status.Open;
			}
			IM_GS_NKCustServiceContact = staff.GS_Code;
			IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.Assigned;
			AssignmentComment = comment;
		}

		public string AssignmentComment { get; private set; }

		#endregion

		#region Investigate

		public void InvestigateInSupport()
		{
			AssignAndInvestigateInSupport(GlbStaff.CurrentUser);
		}

		#endregion

		#region Assign and Investigate

		public void AssignAndInvestigateInSupport(GlbStaff staff)
		{
			CloseInSupportDate = ZDateTime.Empty;

			if (IM_GS_NKCustServiceContact.IsEmpty)
			{
				IM_GS_NKCustServiceContact = staff != null ? staff.GS_Code : GlbStaff.CurrentUser.GS_Code;
			}

			IM_Status = SupportIncidentLookups.Status.Working;
			IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;
		}

		#endregion

		#endregion

		#region Non Support

		void ReopenNonSupport()
		{
			if (!ShouldKeepCurrentDispositionWhileAwaitingResponse)
			{
				if (!IM_GS_NKAssignedToCurrent.IsEmpty)
				{
					IM_Status = SupportIncidentLookups.Status.Working;
					IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction;
				}
				else
				{
					IM_Status = SupportIncidentLookups.Status.Open;
					IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
				}
			}
		}

		#endregion

		#region Defect

		#region Assign To Development

		public void AssignDefectToDevelopment(GlbStaff staff)
		{
			IM_GS_NKAssignedToCurrent = staff.GS_Code;
			if (IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment)
			{
				IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction;
			}
		}

		#endregion

		#region Populate from Issue Occurrence

		public void PopulateFromIssueOccurrence(HelpErrorLogOccurrence occurrence, ClientCompany clientCompany)
		{
			HelpErrorLog issue = occurrence.Issue;

			var database = occurrence.Database;
			if (database != null && !database.LD_OH_WebAccessOrg.IsEmpty)
			{
				IM_OH_Client = occurrence.Database.LD_OH_WebAccessOrg;
			}
			else if (clientCompany?.Org != null)
			{
				IM_OH_Client = clientCompany.LCC_OH;
			}

			if (clientCompany != null)
			{
				EnterprisePK = clientCompany.Database.LD_LE;
				DatabaseServerCode = clientCompany.Database.LD_ServerCode;
				IM_LCC = clientCompany.PK;
			}

			DetailNoteText = issue.HE_ExceptionMessage;
			IM_Description = GetDescriptionFromIssueOccurrence(occurrence);
			IM_HL_ClientReportedOnVersion = occurrence.HO_HL;
			var legacyMapping = EDIDataRegistry.Instance.LegacyMenuSectionMappings.Value.GetMapping("ALL");
			IM_Module = legacyMapping != null ? legacyMapping.ModuleMapping.ToString() : "ALL";
			IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			IM_Product = ProductTypes.Codes.Enterprise;
			IM_Source = SupportIncidentLookups.SourceListConstants.IssueManagerReported;

			IM_OC_Contact = database.LD_OC_ContractInstallerOrInternalTechContact;
			if (IM_OC_Contact.IsEmpty)
			{
				IM_OC_Contact = database.LD_OC_LicenseeAdminContact;
			}

			Escalate(SupportIncidentCategoriesList.Codes.Defect, "Incident automatically created from issue.");
			AssignToStaff(GlbStaff.CurrentUser, "");
			Logs.AddNew(Events.AutoMatchDone, "Created from issue " + issue.HE_IssueNumber);
		}

		string GetDescriptionFromIssueOccurrence(HelpErrorLogOccurrence occurrence)
		{
			string header = "Error " + occurrence.HO_ExceptionID + " reported by ";
			string tail = " on " + occurrence.HO_ExceptionDateTime.ToLongTimeString();
			string userLoginName = occurrence.UserLoginName;
			int currentLength = header.Length + tail.Length;
			int difference = (currentLength + userLoginName.Length) - IncidentMainSchema.IM_Description.MaxLength;
			if (difference > 0)
			{
				userLoginName = userLoginName.Substring(0, userLoginName.Length - (difference + 3));
				userLoginName += "...";
			}
			return header + userLoginName + tail;
		}

		#endregion

		#endregion

		#region Feature Request

		#region Assign to Development

		void AssignFeatureRequestToDevelopment(GlbStaff staff)
		{
			IM_GS_NKAssignedToCurrent = staff.GS_Code;
			IM_Status = SupportIncidentLookups.Status.Working;
			IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;
		}

		#endregion

		#endregion

		#region Revert Status to Close/Resolved, AwaitingResponse

		public bool CanRevertToAwaitingResponse
		{
			get
			{
				if (IsCurrentResolutionCodeClosedOrResolved)
				{
					return false;
				}

				if (FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(IM_ResolutionCodeInfo, out var result) == LoadingPreviousFieldValueResult.Success)
				{
					return CheckPreviousIsValidAndCurrentIsActive(result, new List<string> { SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse });
				}

				return false;
			}
		}

		bool CheckPreviousIsValidAndCurrentIsActive(string previousResolutionCode, IEnumerable<string> validCodes)
		{
			if (previousResolutionCode == ZString.Empty || !validCodes.Contains(previousResolutionCode))
			{
				return false;
			}

			var registryValue = Lookups.GetClosureDispositionList(activeOnly: false, IM_Category, IM_Priority, IM_Product);
			var isCurrentActive = !registryValue.ContainsCode(IM_ResolutionCode);

			return isCurrentActive;
		}

		public bool IsResolutionCodeClosedOrResolved(string resolutionCode)
		{
			return resolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Resolved
				|| resolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
		}

		public bool IsCurrentResolutionCodeClosedOrResolved => IsResolutionCodeClosedOrResolved(IM_ResolutionCode);
		public bool ShouldShowClosedDispositions => IsCurrentResolutionCodeClosedOrResolved || IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
		public bool IsCurrentResolutionCodeClosedResolvedOrAwaitingCustomer => IsCurrentResolutionCodeClosedOrResolved
			|| IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse
			|| IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade
			|| IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed
			|| IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided
			|| IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided;

		public ZString GetPreviousClosureResolution()
		{
			if (IM_ClosureResolutionInfo.HasChanges)
			{
				return (ZString)IM_ClosureResolutionInfo.OriginalValue;
			}
			else
			{
				var mostRecentIRSLog = GetMostRecentLogByEventCode(Events.IncidentResolvedCode);

				if (mostRecentIRSLog == null)
				{
					return ZString.Empty;
				}

				try
				{
					return mostRecentIRSLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New];
				}
				catch (KeyNotFoundException)
				{
					return string.Empty;
				}
			}
		}

		protected StmALog MostRecentLogStatusChange(string description)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.IncidentResolvedCode);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, description);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			return Factory.LoadTop1<StmALog>(query);
		}

		public RevertResult RevertIncidentToAwaitingResponse()
		{
			if (EDIDataRegistry.Instance.IncidentClosureDispositions.Value.ContainsCode(IM_ResolutionCode))
			{
				return RevertResult.CurrentStatusIsClosed;
			}

			if (FieldChangedEventLogger.TryGetPreviousFieldValue<ZString>(IM_ResolutionCodeInfo, out var result) == LoadingPreviousFieldValueResult.Success)
			{
				if (result == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse)
				{
					OnRevertingToAwaitingResponse();
					return RevertResult.Success;
				}
			}

			return RevertResult.OldStatusNotMatch;
		}

		public void OnRevertingToClosedOrResolved(ZString resolutionCode, bool isClosedOrResolved, ZString previousResolutionCode)
		{
			IM_Status = SupportIncidentLookups.Status.Closed;
			Logs.AddNew(AutoEvents.StatusChange, string.Format(IncidentConstants.LogFreeText.RevertIncidentToClosed, IM_ResolutionCode, resolutionCode));
			var isResolved = previousResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			var closeText = string.Empty;
			if (isClosedOrResolved)
			{
				IM_ResolutionCode = previousResolutionCode;
				IM_ClosureResolution = resolutionCode;
				closeText = (isResolved ? "Resolved As " : "Closed As ") + Lookups.StatusDispositionList.GetDescriptionFromCode(resolutionCode);
			}
			else
			{
				IM_ResolutionCode = resolutionCode;
				closeText = "Closed As " + Lookups.StatusDispositionList.GetDescriptionFromCode(resolutionCode);
			}

			CloseTasks(resolutionCode, false);
			EConversation.AddMessageFromCurrentUser(ResString.GetMultilingualString("3E25318E-48F4-4A64-A175-34EBA6140D9C", "Client response is not genuine. Revert status to {0}.", isResolved ? "Resolved" : "Closed"), true, true);

			if (!string.IsNullOrEmpty(closeText))
			{
				AddPublicSystemLogMessage(closeText);
			}

			RefreshPropertyBindingForClosing();
			IsRevertingToClosedOrResolved = true;
		}

		void OnRevertingToAwaitingResponse()
		{
			IM_Status = SupportIncidentLookups.Status.Closed;
			this.Logs.AddNew(AutoEvents.StatusUpdated, string.Format(IncidentConstants.LogFreeText.RevertIncidentToClosed, IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse));
			IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			CloseTasks(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, true);
			EConversation.AddMessageFromCurrentUser(ResString.GetMultilingualString("7CD8938D-378E-4A8F-A17F-6FF012C9C4D2", "Client response is not genuine. Revert status to Awaiting Response."), true, true);

			AddPublicSystemLogMessage("Awaiting Client Response");

			RefreshPropertyBindingForClosing();
		}

		public bool IsRevertingToClosedOrResolved { get; private set; }

		public enum RevertResult
		{
			Success,
			OldStatusIsOpen,
			OldStatusNotMatch,
			CurrentStatusIsClosed,
			MissingOldStatus,
			OldValueMismatchCurrentDetails,
			DataIsNotInTheDatabase,
			DataOutDated,
			NoValidLog
		}

		#endregion

		#region Close on behalf of Client (Confirmed Resolved)

		public bool CanCloseOnBehalfOfClient
		{
			get
			{
				if (!SupportIncidentLookups.CanCloseOnBehalfCodeList.ContainsCode(IM_ResolutionCode))
				{
					return false;
				}

				var messages = EConversation.GetTimeOrderedMessages();
				var firstReOpenMsg = messages.FirstOrDefault(msg => msg.MessageType == MessageType.LocalPublished && msg.MessageSubType == MessageSubType.SystemLog && msg.Body == "Incident Re-opened");
				if (firstReOpenMsg == null)
				{
					return false;
				}

				var existSupportMsgAfterReopen = messages.TakeWhile(msg => msg != firstReOpenMsg)
													.Any(msg => msg.MessageSubType == MessageSubType.UserMessage && msg.MessageType == MessageType.LocalPublished);
				return !existSupportMsgAfterReopen;
			}
		}

		public bool CloseViaCloseOnBehalfOfClient { set; get; }

		#endregion

		#region General

		#region Assign to Staff

		public void AssignToStaff(GlbStaff staff, string comment)
		{
			if (IM_Category == SupportIncidentCategoriesList.Codes.Support)
			{
				AssignToSupportPerson(staff, comment);
			}
			else if (IM_Category == SupportIncidentCategoriesList.Codes.Defect)
			{
				AssignDefectToDevelopment(staff);
			}
			else if (IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest)
			{
				AssignFeatureRequestToDevelopment(staff);
			}
			else
			{
				IM_GS_NKAssignedToCurrent = staff.GS_Code;
				if (IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment)
				{
					IM_Status = SupportIncidentLookups.Status.Working;
					IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.Assigned;
				}
			}
			AssignmentComment = comment;
		}

		#endregion

		#region Closing Staff Code Text

		public ZString ClosingStaffCodeText
		{
			get { return ClosingStaffCodeTextNote.Text; }
			set
			{
				ClosingStaffCodeTextNote.Text = value;
				this.HasChanges = true;
			}
		}

		UniqueNote ClosingStaffCodeTextNote
		{
			get { return closingStaffCodeTextNote ?? (closingStaffCodeTextNote = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentClosingStaffCode)); }
		}
		UniqueNote closingStaffCodeTextNote;

		#endregion

		#region Closure Date Text

		public ZString ClosureDateText
		{
			get { return ClosureDateTextNote.Text; }
			set
			{
				ClosureDateTextNote.Text = value;
				this.HasChanges = true;
			}
		}

		UniqueNote ClosureDateTextNote
		{
			get { return closureDateTextNote ?? (closureDateTextNote = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentClosureDate)); }
		}
		UniqueNote closureDateTextNote;

		#endregion

		#region Set eRequest Status Only

		public ZBool SetERequestStatusOnly
		{
			get { return setERequestStatusOnly; }
			set
			{
				setERequestStatusOnly = value;
				SetERequestStatusOnlyInfo.RefreshBinding();
			}
		}
		ZBool setERequestStatusOnly;

		public ZPropertyInfo SetERequestStatusOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(SetERequestStatusOnly)); }
		}

		#endregion

		#region Resolution Comment Text

		public ZString ResolutionCommentText
		{
			get { return ResolutionCommentTextNote.Text; }
			set
			{
				ResolutionCommentTextNote.Text = value;
				this.HasChanges = true;
			}
		}

		UniqueNote ResolutionCommentTextNote
		{
			get { return resolutionCommentTextNote ?? (resolutionCommentTextNote = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentResolutionComment)); }
		}
		UniqueNote resolutionCommentTextNote;

		#endregion

		#region Disposition Text

		public ZString DispositionText
		{
			get { return DispositionTextNote.Text; }
			set
			{
				DispositionTextNote.Text = value;
				this.HasChanges = true;
			}
		}

		UniqueNote DispositionTextNote
		{
			get { return dispositionTextNote ?? (dispositionTextNote = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentDispositionText)); }
		}
		UniqueNote dispositionTextNote;

		#endregion

		void SetResolutionNoteText(ZDateTime closureDate, ZString closingStaffCode, ZString resolutionNoteText, ZString resolutionComment, ZString closeText)
		{
			ResolutionNoteText = closureDate.ToString() + " " + closingStaffCode + " - " + resolutionNoteText;
			ClosingStaffCodeText = closingStaffCode;
			ClosureDateText = closureDate.ToString();
			ResolutionCommentText = resolutionComment;
			DispositionText = closeText;
		}

		void ResetResolutionNoteText()
		{
			ResolutionNoteText = "";
			ClosingStaffCodeText = "";
			ClosureDateText = "";
			ResolutionCommentText = "";
		}

		#region Escalate

		public void Escalate(ZString escalationStage, ZString resolutionComment)
		{
			try
			{
				IsFromPerformAction = true;
				if (!escalationStage.IsEmpty && Lookups.StageList.ContainsCode(escalationStage) && IM_Category != escalationStage)
				{
					if (!resolutionComment.IsEmpty)
					{
						AddInternalMessage(resolutionComment);
					}

					if (escalationStage == SupportIncidentCategoriesList.Codes.Support)
					{
						IM_Status = SupportIncidentLookups.Status.Open;
						if (IM_GS_NKCustServiceContact == User.ServiceUserCode)
						{
							IM_GS_NKCustServiceContact = ZString.Empty;
						}
					}
					else
					{
						if (IM_Category == SupportIncidentCategoriesList.Codes.Support)
						{
							if (IM_GS_NKCustServiceContact.IsEmpty)
							{
								IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
							}
							CloseInSupportDate = ZDateTime.Now;
						}

						var escalationText = string.Format(CultureInfo.InvariantCulture, IncidentConstants.StageChangedMessageTemplate, Lookups.StageList.GetDescriptionFromCode(escalationStage));
						AddPublicSystemLogMessage(escalationText);
					}

					IM_Category = escalationStage;
					if (!IsGroupControlled)
					{
						IsUpdatingStage = true;
						if (escalationStage == SupportIncidentCategoriesList.Codes.ContentDevelopment && IM_ClosureResolution == SupportIncidentLookups.DispositionList.Constants.Closed.TrainingFlaggedForContentDevelopment)
						{
							IsStageBeingToContentDevelopment = true;
						}
						IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.Escalate, this);
					}
				}
			}
			finally
			{
				IsFromPerformAction = false;
				IsUpdatingStage = false;
				IsStageBeingToContentDevelopment = false;
			}
		}

		bool IsUpdatingStage { get; set; }

		bool IsStageBeingToContentDevelopment { get; set; }

		public void ChangeIncidentStageFromSupportToFeatureRequestWithoutLogging(string featureStatus = SupportIncidentLookups.Status.Open, string featureDisposition = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment)
		{
			if (IM_GS_NKCustServiceContact.IsEmpty)
			{
				IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
			}

			IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			IM_Status = featureStatus;
			IM_ResolutionCode = featureDisposition;

			if (FeatureRequestClientPK.IsEmpty)
			{
				FeatureRequestClientPK = IM_OH_Client;
			}

			if (FeatureRequestClientAddressPK.IsEmpty)
			{
				FeatureRequestClientAddressPK = IM_OA_BranchAddress;
			}

			CloseInSupportDate = ZDateTime.Now;
		}

		#endregion

		#region Close

		[List("Lookups.ActiveCloseStatusDispositionList")]
		public ZString IM_ClosureResolutionForBinding
		{
			get => IM_ClosureResolution;
			set
			{
				UpdateClosureResolutionForResolvedIncident(value);
			}
		}

		public ZPropertyInfo IM_ClosureResolutionForBindingInfo => IM_ClosureResolutionInfo;

		public bool IsCurrentResolutionMethodAResolution => (Lookups.StatusDispositionList[IM_ClosureResolution] as IncidentClosureDisposition)?.IsResolution ?? false;

		public bool ClosingAsIsAwaitingClient { get; private set; }

		public void UpdateClosureResolutionForResolvedIncident(string newResolution)
		{
			if (string.IsNullOrEmpty(newResolution) || !IsCurrentResolutionCodeClosedOrResolved || newResolution == IM_ClosureResolution)
			{
				return;
			}

			if (Lookups.StatusDispositionList[newResolution] is IncidentClosureDisposition resolutionObject)
			{
				IM_ClosureResolution = resolutionObject.Code;
				if (!resolutionObject.IsResolution && IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Resolved)
				{
					IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
					IM_CloseTimeUtc = IM_ResolveTimeUtc;
				}

				AddPublicSystemLogMessage($"Closed As {IM_ClosureResolutionDescription}");
			}
		}

		public void CloseIncident(ZString resolutionMethod, ZString resolutionComment, bool isAwaitingClient = false)
		{
			CloseIncident(resolutionMethod, resolutionComment, ZDateTime.Empty, isAwaitingClient);
		}

		public void CloseIncident(ZString resolutionMethod, ZString resolutionComment, ZDateTime resolvedTimeUtcOverride, bool isAwaitingClient = false, bool closeTasks = true)
		{
			ResetResolutionNoteText();
			ClosingAsIsAwaitingClient = isAwaitingClient;
			IM_Status = SupportIncidentLookups.Status.Closed;
			var registryDisposition = Lookups.StatusDispositionList[resolutionMethod] as IncidentClosureDisposition;

			if (resolutionMethod.EqualsIgnoringCase(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted))
			{
				IM_ResolutionCode = resolutionMethod;
			}
			else if (registryDisposition != null)
			{
				UpdateToClosedResolutionAndAddEvents(registryDisposition, CloseViaCloseOnBehalfOfClient || !registryDisposition.IsResolution, resolvedTimeUtcOverride);
			}
			//if closed by workflow task, the IM_ResolutionCode and IM_ClosureResolution have already been set. Just need to add events and messages
			else if (resolutionMethod == SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed || resolutionMethod == SupportIncidentLookups.DispositionList.Constants.Closed.Resolved)
			{
				if (Lookups.StatusDispositionList[IM_ClosureResolution] is IncidentClosureDisposition registryClosureResolution)
				{
					registryDisposition = registryClosureResolution;
					AddClosureEvents(IM_ClosureResolution, IsInDatabase ? (ZString)IM_ClosureResolutionInfo.OriginalValue : ZString.Empty, registryClosureResolution, resolutionMethod == SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, resolvedTimeUtcOverride);
					resolutionMethod = IM_ClosureResolution;
				}
				else
				{
					ErrorReporter.ReportOnce("Incident has been closed with an invalid resolution method", FormattableString.Invariant($"Incident {IM_IncidentNumber} ResolutionCode: {IM_ResolutionCode} ClosureResolution: {IM_ClosureResolution}"));
				}
			}
			else
			{
				IM_ResolutionCode = resolutionMethod;
			}

			if (!resolutionComment.IsEmpty)
			{
				AddResolutionMessageToCustomer(resolutionComment);
			}

			var closeIncidentViaLastTask = !CloseIncidentViaPopupForm && IM_StatusInfo.Value.ToString() == SupportIncidentLookups.Status.Closed && IM_StatusInfo.OriginalValue.ToString() != SupportIncidentLookups.Status.Closed
									&& WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>()
										.Any(task => task.P9_StatusInfo.OriginalValue.ToString() != ProcessTaskStatusCodeList.Codes.Cancelled && task.P9_StatusInfo.OriginalValue.ToString() != ProcessTaskStatusCodeList.Codes.Closed && (task.P9_StatusInfo.Value.ToString() == ProcessTaskStatusCodeList.Codes.Closed || task.P9_StatusInfo.Value.ToString() == ProcessTaskStatusCodeList.Codes.Cancelled));

			var closeText = ZString.Empty;
			if (isAwaitingClient || resolutionMethod == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse)
			{
				// Don't say "Closed As" if we're only waiting for a response, since it is not actually closed
				if (resolutionMethod == SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided || resolutionMethod == SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided)
				{
					closeText = Lookups.StatusDispositionList.GetDescriptionFromCode(resolutionMethod);
				}
				else
				{
					closeText = Lookups.StatusDispositionList.GetDescriptionFromCode(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse);
				}
			}
			else if (!setERequestStatusOnly && !closeIncidentViaLastTask)
			{
				var isResolution = registryDisposition?.IsResolution ?? false;
				closeText = ((isResolution && !CloseViaCloseOnBehalfOfClient) ? "Resolved As " : "Closed As ") + Lookups.StatusDispositionList.GetDescriptionFromCode(resolutionMethod);
			}

			if (isAwaitingClient && resolutionMethod == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse)
			{
				this.Logs.AddNew(AutoEvents.IncidentAwaitingResponse, AwaitingClientResponseLogMessage);
			}

			if (CloseViaCloseOnBehalfOfClient)
			{
				AddPublicSystemLogMessage("This incident was closed on behalf of the client by WiseTech Global support");
			}

			AddPublicSystemLogMessage(closeText);

			if (!isAwaitingClient)
			{
				ZString logText = closeText;
				if (!resolutionComment.IsEmpty)
				{
					if (!logText.IsEmpty)
					{
						logText += " - ";
					}
					logText += resolutionComment;
				}

				if (!logText.IsEmpty)
				{
					SetResolutionNoteText(ZDateTime.Now, GlbStaff.CurrentUser.GS_Code, logText, resolutionComment, closeText);
				}
			}

			if (IM_Category == SupportIncidentCategoriesList.Codes.Support)
			{
				CloseInSupportDate = ZDateTime.Now;
			}

			if (closeTasks)
			{
				CloseTasks(resolutionMethod, isAwaitingClient);
			}

			RefreshPropertyBindingForClosing();
			CloseIncidentViaPopupForm = false;
		}

		public void PopulateCloseAction(SupportIncidentCloseAction incidentCloseAction)
		{
			var reOpenMsg = EConversation.GetTimeOrderedMessages().FirstOrDefault(msg => msg.MessageType == MessageType.LocalPublished && msg.MessageSubType == MessageSubType.SystemLog);
			if (reOpenMsg == null || reOpenMsg.Body != "Incident Re-opened" || reOpenMsg is not EdiJobConversationMessage reOpenConversationMsg)
			{
				return;
			}

			var reOpenPostTimeUtc = reOpenConversationMsg.JCM_PostedTimeUtc;
			var dispositionQuery = new ZQuery(StmALogSchema.SL_Parent, this.PK);
			dispositionQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
			dispositionQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, ChangedFieldDescription.IM_ResolutionCode);
			dispositionQuery.AddToFilter(StmALogSchema.SL_EventTimeUtc, SQLComparisonOperator.LessThan, reOpenPostTimeUtc);
			dispositionQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			var dispositionChangeLogBeforeReOpen = Factory.LoadTop1<StmALog>(dispositionQuery);

			if (!FieldChangedEventLogger.TryParseFieldChangedEvent(dispositionChangeLogBeforeReOpen, out var dispositionResult))
			{
				return;
			}

			if (dispositionResult.To == DispositionList.Constants.Closed.ClosedAwaitingClientResponse)
			{
				incidentCloseAction.PrePopulateResolutionMethod = DispositionList.Constants.Closed.SelfResolved;
				var markedAsCWRQuery = new ZQuery(StmALogSchema.SL_Parent, this.PK);
				markedAsCWRQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.IncidentAwaitingResponseCode);
				markedAsCWRQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, "Incident has been set to Awaiting Client Response.");
				markedAsCWRQuery.AddToFilter(StmALogSchema.SL_EventTimeUtc, SQLComparisonOperator.LessThan, reOpenPostTimeUtc);
				markedAsCWRQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
				var targetCWREvent = Factory.LoadTop1<StmALog>(markedAsCWRQuery);

				if (targetCWREvent != null)
				{
					incidentCloseAction.PostIRSEvent = true;
					incidentCloseAction.IRSEventTimeUtcOverride = targetCWREvent.SL_EventTimeUtc;
				}
			}
			else
			{
				var closeEventQuery = new ZQuery(StmALogSchema.SL_Parent, this.PK);
				closeEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.IncidentClosedCode);
				closeEventQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
				var latestCloseEvent = Factory.LoadTop1<StmALog>(closeEventQuery);

				var preexistingIRSQuery = new ZQuery(StmALogSchema.SL_Parent, this.PK);
				preexistingIRSQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.IncidentResolvedCode);
				var previousIRSExists = Factory.Exists(typeof(StmALog), preexistingIRSQuery);
				if (latestCloseEvent != null && !previousIRSExists)
				{
					var resolutionMethodQuery = new ZQuery(StmALogSchema.SL_Parent, this.PK);
					resolutionMethodQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
					resolutionMethodQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, ChangedFieldDescription.IM_ClosureResolution);
					resolutionMethodQuery.AddToFilter(StmALogSchema.SL_EventTimeUtc, SQLComparisonOperator.LessThan, reOpenPostTimeUtc);
					resolutionMethodQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
					var resolutionMethodChangeLogBeforeReOpen = Factory.LoadTop1<StmALog>(resolutionMethodQuery);

					if (FieldChangedEventLogger.TryParseFieldChangedEvent(resolutionMethodChangeLogBeforeReOpen, out var resolutionMethod))
					{
						incidentCloseAction.PrePopulateResolutionMethod = resolutionMethod.To;
						incidentCloseAction.PostIRSEvent = true;
						incidentCloseAction.IRSEventTimeUtcOverride = latestCloseEvent.SL_EventTimeUtc;
					}
				}
			}
		}

		const string AwaitingClientResponseLogMessage = "Incident has been set to Awaiting Client Response.";

		void RefreshPropertyBindingForClosing()
		{
			OverallAssignedToCodeInfo.RefreshBinding();
			OverallAssignedToDescriptionInfo.RefreshBinding();
			OverallAssignedToLabelTextInfo.RefreshBinding();
		}

		void UpdateToClosedResolutionAndAddEvents(IncidentClosureDisposition registryDisposition, bool shouldSkipResolved, ZDateTime resolvedTimeUtcOverride)
		{
			var closureResolution = registryDisposition.Code;
			var originalClosureResolution = IM_ClosureResolution;
			IM_ResolutionCode = shouldSkipResolved ? SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed : SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;

			IM_ClosureResolution = closureResolution;
			IM_ResolveTimeUtc = resolvedTimeUtcOverride;
			AddClosureEvents(closureResolution, originalClosureResolution, registryDisposition, shouldSkipResolved, resolvedTimeUtcOverride);
		}

		void AddClosureEvents(ZString closureResolution, ZString originalClosureResolution, IncidentClosureDisposition registryDisposition, bool shouldSkipResolved, ZDateTime resolvedTimeUtcOverride)
		{
			var resolvedParamList = new List<KeyValuePair<string, string>>();
			var incidentResolvedDescription = FormattableString.Invariant($"{IncidentConstants.LogFreeText.CloseAsResolvedClosureResolutionLog} {closureResolution} - {registryDisposition.Description}");
			resolvedParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, incidentResolvedDescription));

			if (!string.IsNullOrEmpty(originalClosureResolution) && originalClosureResolution != IM_ClosureResolution)
			{
				resolvedParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, originalClosureResolution));
			}

			resolvedParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, closureResolution));

			if (!CloseViaCloseOnBehalfOfClient || (CloseViaCloseOnBehalfOfClient && !resolvedTimeUtcOverride.IsEmpty))
			{
				if (!resolvedTimeUtcOverride.IsEmpty)
				{
					var offset = new ZDateTimeOffset(resolvedTimeUtcOverride, DateTimeKind.Utc);
					Logs.AddNew(
						AutoEvents.IncidentResolved,
						offset,
						resolvedParamList.ToArray()
					);
				}
				else
				{
					Logs.AddNew(
						AutoEvents.IncidentResolved,
						resolvedParamList.ToArray()
					);
				}
			}

			if (CloseViaCloseOnBehalfOfClient)
			{
				Logs.AddNew(AutoEvents.MiscellaneousEvent, "This eRequest has been deemed resolved");
			}

			var logText = shouldSkipResolved ? IncidentConstants.LogFreeText.CloseSkipResolvedChangedUserLog : IncidentConstants.LogFreeText.CloseAsResolvedChangedUserLog;
			var statusUpdatedDescription = FormattableString.Invariant($"{logText} {GlbStaff.CurrentUser.GS_Code}");
			AddStatusUpdatedEvent(statusUpdatedDescription);
		}

		public void AddStatusUpdatedEvent(string statusUpdatedDescription)
		{
			var statusUpdatedParamList = new List<KeyValuePair<string, string>>();
			statusUpdatedParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, statusUpdatedDescription));
			statusUpdatedParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, (ZString)IM_ResolutionCodeInfo.OriginalValue));
			statusUpdatedParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, IM_ResolutionCode));

			Logs.AddNew(
				AutoEvents.StatusUpdated,
				statusUpdatedParamList.ToArray()
			);
		}

		public event EventHandler<TaskStatusChangeEventArgs> OnCloseIncident;
		public class TaskStatusChangeEventArgs : EventArgs
		{
			public ZString OriginalTaskStatus { get; set; }
			public ZBool IgnoreStatusRollback { get; set; } = false;
		}

		public void CloseIncidentByWorkflowTask(ZString originalStatusBeforeStatusCalculation, SupportIncidentProcessTask task, TaskStatusChangeEventArgs eventArgs)
		{
			if (!SuspendTriggerCloseIncident && (IM_Status == SupportIncidentLookups.Status.Closed
				&& (originalStatusBeforeStatusCalculation != SupportIncidentLookups.Status.Closed || !Lookups.StatusDispositionList.ContainsCode(IM_ResolutionCode))
				&& IM_ResolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse))
			{
				if (OnCloseIncident != null)
				{
					OnCloseIncident(task, eventArgs);
				}
				else
				{
					if (CanShowCloseIncidentForm)
					{
						var action = new SupportIncidentCloseAction(this);
						if (Factory.IsInSaveTransaction || ObjectFactory.Get<ICloseIncidentPopupForm>(nameof(ICloseIncidentPopupForm), action).ShowDialogAndDispose() != ZDialogResult.OK)
						{
							task.P9_Status = eventArgs.OriginalTaskStatus;
							CalculateWorkflowDependentProperties(shouldOverrideDisposition: true);
						}
					}
					else
					{
						CloseIncident(IM_ResolutionCode, ZString.Empty);
					}
				}
			}
		}

		public virtual bool SuspendTriggerCloseIncident { get; set; }

		public virtual bool CloseIncidentViaPopupForm { get; set; }

		protected virtual bool CanShowCloseIncidentForm
		{
			get { return Globals.IsUserInteractive && !Globals.IsTest && !IsGroupControlled && !IsPendingUpgrade; }
		}

		public bool IsPendingUpgrade => IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade || IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed;

		public StmALog GetLatestAwaitingResponseEvent()
		{
			var markedAsAwaitingResponseQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.IncidentAwaitingResponseCode);
			markedAsAwaitingResponseQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, AwaitingClientResponseLogMessage);
			markedAsAwaitingResponseQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			var markedAsAwaitingResponseLogs = Logs.Find(markedAsAwaitingResponseQuery);

			if (markedAsAwaitingResponseLogs.Any())
			{
				return markedAsAwaitingResponseLogs[0];
			}

			return null;
		}

		#endregion

		#region Reopen

		public bool CanReOpen => SupportIncidentLookups.CanReOpenRequestStatusList.ContainsCode(IM_RequestStatus);

		/// <summary>
		/// Set addToEConversation to false if the customer requested reopen, since no need to send them a notification that it reopened.
		/// We avoid the notification by not adding the reopen log message.
		/// </summary>
		public void Reopen(string eventCode, bool addToEConversation = true)
		{
			var previousResolutionCode = IM_ResolutionCode;
			IM_CloseTimeUtc = ZDateTime.Empty;
			IncidentEventFactory.TriggerEvent(eventCode, this);
			Logs.AddNew(Events.IncidentReopened, Events.IncidentReopened.Description);
			ClearSingleUseMetrics();

			if (IM_Category == SupportIncidentCategoriesList.Codes.Support)
			{
				ReopenSupport();
			}
			else
			{
				ReopenNonSupport();
			}

			if (addToEConversation)
			{
				AddPublicSystemLogMessage("Incident Re-opened");
			}

			if (awaitingResolutionCodes.Contains(previousResolutionCode))
			{
				InitMetric(IncidentMetricConstants.NextResponseTime, ZDateTime.Empty);
			}
		}

		/// <summary>
		/// Reopen if closed, otherwise trigger ERequestReopen event to add the normal reopen tasks
		/// </summary>
		/// <returns>true if changed</returns>
		public bool EnsureOpenTask(bool isLegacyReopenRequest = false, bool isByNewMessage = false, bool isByNewEmail = false)
		{
			bool isModified = false;
			bool isClosed = IM_Status == SupportIncidentLookups.Status.Closed;

			if (ShouldReopenTicket(isByNewMessage || isByNewEmail, isClosed))
			{
				Reopen(IncidentEventFactory.Codes.ERequestReopen, !isLegacyReopenRequest);
				isModified = true;
			}
			else if (!awaitingResolutionCodes.Contains(IM_ResolutionCode))
			{
				var messages = EConversation.GetTimeOrderedMessages();
				var lastClientMsg = (JobConversationMessage)messages.FirstOrDefault(msg => msg.MessageSubType == MessageSubType.UserMessage && msg.MessageType == MessageType.Remote);
				if (lastClientMsg != null)
				{
					UpdateSingleUseMetric(IncidentMetricConstants.AdditionalResponseTime, lastClientMsg.JCM_PostedTimeUtc);
					InitMetric(IncidentMetricConstants.NextResponseTime, lastClientMsg.JCM_PostedTimeUtc);
				}
			}

			if (!isModified && !isClosed && WorkflowItems.AllTasksClosedOrCancelled)
			{
				IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.ERequestReopen, this);
				isModified = true;
			}

			return isModified;
		}

		bool ShouldReopenTicket(bool isByNewMessageOrEmail, bool isClosed)
		{
			if (!isByNewMessageOrEmail)
			{
				return isClosed;
			}

			if (IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed
				&& GetClosedReopenRule() == ResolutionAndClosureBehaviour.Constants.Code.NeverAllow)
			{
				return false;
			}

			if (awaitingResolutionCodes.Contains(IM_ResolutionCode))
			{
				return true;
			}

			return isClosed;
		}

		public string GetClosedReopenRule()
		{
			return GetResolutionAndClosureBehaviour().ClosedReopenRule;
		}

		public ResolutionAndClosureBehaviour GetResolutionAndClosureBehaviour()
		{
			var criticalityResolutionAndClosureBehaviour = SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, IM_Priority, ZGuid.Empty);
			var productResolutionAndClosureBehaviour = SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, IM_Product, (criticalityResolutionAndClosureBehaviour)?.ID ?? ZGuid.Empty);

			if (productResolutionAndClosureBehaviour == null)
			{
				ErrorReporter.ReportOnce("Missing Resolution And Closure Behaviour Registry entry", FormattableString.Invariant($"Criticality={IM_Priority} Product={IM_Product}"));
				return new ResolutionAndClosureBehaviour();
			}

			return productResolutionAndClosureBehaviour;
		}

		public ZInt ResolvedToClosedDay
		{
			get
			{
				return GetResolutionAndClosureBehaviour().DaysResolvedToClosed;
			}
		}

		#endregion

		#region Close As Accepted Feature Request

		public void CloseAsAcceptedFeatureRequest(ZString resolutionComment)
		{
			if (IM_Category != SupportIncidentCategoriesList.Codes.FeatureRequest)
			{
				IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
				Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			}

			CloseIncident(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, resolutionComment);
		}

		#endregion

		#region Wait for Upgrade

		public void WaitForUpgrade()
		{
			if (IM_Category != SupportIncidentCategoriesList.Codes.Support)
			{
				if (!IsInternal)
				{
					IM_Status = SupportIncidentLookups.Status.Working;
				}

				if (IM_Category == SupportIncidentCategoriesList.Codes.Defect)
				{
					IM_ResolutionCode = IsInternal
					? SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal
					: SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
				}
				else
				{
					IM_ResolutionCode = IsInternal
					? SupportIncidentLookups.DispositionList.Constants.Closed.Completed
					: SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
				}
			}
		}

		public void DelayUpgrade()
		{
			IM_Status = SupportIncidentLookups.Status.Working;
			IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed;
		}

		#endregion

		#region Set Work Item Created

		public void SetWorkItemCreated()
		{
			if (IM_Category != SupportIncidentCategoriesList.Codes.Support)
			{
				IM_Status = SupportIncidentLookups.Status.Working;
				IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			}
		}

		#endregion

		#region Set Upgrade Delivered

		public void SetUpgradeDelivered()
		{
			//If there is an open/working/suspended task, the incident's IM_Status will be reopened on save
			CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, "", ZDateTime.UtcNow, closeTasks: false);
		}

		#endregion

		#endregion

		#region Set Criticality

		public void SetCriticalityWithoutLoggingReason(ZString newCriticality)
		{
			IsSynchronisingCriticalityFromAction = true;
			IM_Priority = newCriticality;
			IsSynchronisingCriticalityFromAction = false;
		}

		public bool IsSynchronisingCriticalityFromAction { get; private set; }

		public void TriggerCriticalityChangeEvent()
		{
			IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.ChangeCriticality, this);
		}

		#endregion

		#endregion

		#region Saving & Deleting

		public override void OnSaving()
		{
			LogModuleChange();
			LogProductChange();
			LogProductAreaChange();
			LogAssignedStaffChange();
			LogMenuItemChange();
			LogReOpenRuleWhenIncidentClosed();

			AddContactToRelatedParty();
			SetInitialStaffAssignmentDate();
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();

			if (!IsInDatabase)
			{
				SetIncidentNumberIfRequired();
			}

			if (IM_RN_NKCountry.IsEmpty)
			{
				IM_RN_NKCountry = new[] { request?.INC_RN_NKCountry, ClientCompany?.LCC_RN_NKCountryCode, Client?.CountryCode,
					Contact?.Header?.CountryCode }.Where(x => x.HasValue && !x.Value.IsEmpty).Select(x => x.Value).FirstOrDefault();
			}

			base.OnSaving();

			if (IM_ResolveTimeUtc.IsEmpty)
			{
				var resolvedOrClosed = IM_ResolutionCode == DispositionList.Constants.Closed.ResolvedAndClosed || IM_ResolutionCode == DispositionList.Constants.Closed.Resolved;
				if (resolvedOrClosed)
				{
					IM_ResolveTimeUtc = ZDateTime.UtcNow;
				}
			}
			if (IM_Priority != (ZString)IM_PriorityInfo.OriginalValue)
			{
				Validation.ValidateIM_Priority();

				if (!IM_PriorityInfo.HasErrors() && !((ZString)IM_PriorityInfo.OriginalValue).IsEmpty)
				{
					string changedLog = string.Format(CultureInfo.CurrentCulture, IncidentConstants.CriticalityChangedMessageTemplate, (ZString)IM_PriorityInfo.OriginalValue, this.IM_Priority);
					this.AddSystemMessageToCustomer(changedLog);
				}
			}

			SynchroniseNotes();

			if (!this.IsInDatabase)
			{
				this.SystemCreateTimeInUTC = ZDateTime.UtcNow;
				InitMetrics();
			}
			else if (IsFirstResponsePending)
			{
				var firstSupportMsg = (JobConversationMessage)EConversation.GetTimeOrderedMessages().LastOrDefault(msg => msg.MessageSubType == MessageSubType.UserMessage && msg.MessageType == MessageType.LocalPublished);
				if (firstSupportMsg != null)
				{
					var firstSupportMsgPostedTime = firstSupportMsg.JCM_PostedTimeUtc;
					UpdateSingleUseMetric(IncidentMetricConstants.FirstResponseTime, firstSupportMsgPostedTime, metricCount: 1);
				}
			}

			// OnSaveSucceeded can do another Factory.Save.
			// Assume anything changed during OnSaveSucceeded does not need publishing.
			if (!inOnSaveSucceeded)
			{
				UpdateRequest();

				if (IsInDatabase)
				{
					IsUpdatingDisposition = IM_ResolutionCodeInfo.HasChanges;
				}
				else
				{
					IsUpdatingDisposition = false;
				}
			}
		}

		void ReCalculateTDTMetricCount()
		{
			var resolvedEvent = Logs.Find(f => f.SL_SE_NKEvent == Events.IncidentResolvedCode).OrderByDescending(log => log.SL_PostedTimeUtc.IsEmpty ? log.SL_EventTimeUtc : log.SL_PostedTimeUtc).FirstOrDefault();
			var attachedEvent = Logs.Find(f => f.SL_SE_NKEvent == Events.AttachedCode && (resolvedEvent == null || f.SL_PostedTimeUtc > resolvedEvent.SL_PostedTimeUtc) && RelatedWorkItems.Any(wki => f.SL_Reference.Contains(((NewWorkItem)wki).WKI_WorkItemNumber))).OrderBy(log => log.SL_PostedTimeUtc.IsEmpty ? log.SL_EventTimeUtc : log.SL_PostedTimeUtc);
			var totalDevelopmentTimeMetric = MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(m => m.IME_MetricCode == IncidentMetricConstants.TotalDevelopmentTime && m.IME_EndTimeUtc.IsEmpty);
			var totalOtherDevelopmentTimeCount = MetricsCollection.Where(m => m.IME_MetricCode == IncidentMetricConstants.TotalDevelopmentTime && m != totalDevelopmentTimeMetric).Sum(m => m.IME_MetricCount);
			if (totalDevelopmentTimeMetric != null && attachedEvent != null)
			{
				totalDevelopmentTimeMetric.IME_MetricCount = RelatedWorkItems.Count - totalOtherDevelopmentTimeCount;
			}
		}

		#region Metrics

		void InitMetrics()
		{
			InitMetric(IncidentMetricConstants.FirstResponseTime, IM_SystemCreateTimeUtc);
			InitMetric(IncidentMetricConstants.TotalERequestAge, IM_SystemCreateTimeUtc);
			InitMetric(IncidentMetricConstants.TotalResolutionAge, IM_SystemCreateTimeUtc);
		}

		public void InitMetric(string metricCode, ZDateTime? startTimeUtc)
		{
			if (!MetricsCollection.Any(m => m.IME_MetricCode == metricCode && m.IME_EndTimeUtc.IsEmpty))
			{
				var metric = MetricsCollection.AddNew();
				metric.IME_IncidentNumber = IM_IncidentNumber;
				metric.IME_MetricCode = metricCode;
				metric.IME_StartTimeUtc = startTimeUtc.Value;
			}
		}

		public void UpdateSingleUseMetric(string metricCode, ZDateTime? endTimeUtc, int metricCount = 0)
		{
			var metric = MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(m => m.IME_MetricCode == metricCode);
			if (metric != null)
			{
				metric.IME_EndTimeUtc = endTimeUtc.Value;
				metric.IME_CalculatedMetric = (int)(metric.IME_EndTimeUtc - metric.IME_StartTimeUtc).TotalSeconds;
				if (metricCount != 0)
				{
					metric.IME_MetricCount = metricCount;
				}
			}
		}

		public void CompleteMetric(SupportIncident incident, string metricCode, ZDateTime? startTimeUtc, ZDateTime? endTimeUtc, int metricCount = 0)
		{
			var metric = incident.MetricsCollection.AddNew();
			metric.IME_IncidentNumber = incident.IM_IncidentNumber;
			metric.IME_MetricCode = metricCode;

			metric.IME_StartTimeUtc = startTimeUtc.Value;
			metric.IME_EndTimeUtc = endTimeUtc.Value;
			metric.IME_CalculatedMetric = (int)(metric.IME_EndTimeUtc - metric.IME_StartTimeUtc).TotalSeconds;
			metric.IME_MetricCount = metricCount;
		}

		void ClearSingleUseMetrics()
		{
			ClearSingleUseMetric(IncidentMetricConstants.TotalERequestAge);
			ClearSingleUseMetric(IncidentMetricConstants.TotalResolutionAge);
			ClearSingleUseMetric(IncidentMetricConstants.NetResolutionAge);
		}

		void ClearSingleUseMetric(string metricCode)
		{
			var metric = MetricsCollection.OrderByDescending(m => m.IME_SystemLastEditTimeUtc).FirstOrDefault(m => m.IME_MetricCode == metricCode);
			if (metric != null)
			{
				metric.IME_EndTimeUtc = ZDateTime.Empty;
				metric.IME_CalculatedMetric = 0;
			}
		}

		public readonly List<string> awaitingResolutionCodes = new List<string>
			{
				SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse,
				SupportIncidentLookups.DispositionList.Constants.Closed.Resolved,
				SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed,
				SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided,
				SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided
			};

		public readonly List<string> awaitingEventCodes = new List<string>
			{
				Events.IncidentAwaitingResponseCode,
				Events.IncidentResolvedCode,
				Events.IncidentClosedCode,
				Events.IncidentDevelopmentEstimateProvidedCode,
				Events.IncidentFormalQuoteProvidedCode
			};

		#endregion

		protected virtual void PreHookValidateTimestampsForTest()
		{
		}

		void ValidateTimestamps()
		{
			PreHookValidateTimestampsForTest();
			if (inOnSaveSucceeded)
			{
				return;
			}
			var isResolvedOrClosed = IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed || IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			if (!isResolvedOrClosed && IM_Status != SupportIncidentLookups.Status.Closed)
			{
				return;
			}
			Reload();
			var messages = new List<string>();
			if (!IM_CloseTimeUtc.IsEmpty && IM_CloseTimeUtc < IM_SystemCreateTimeUtc)
			{
				messages.Add("CloseTimeUtc shouldn't be less than Incident create time");
			}
			if (!IM_ResolveTimeUtc.IsEmpty && IM_ResolveTimeUtc < IM_SystemCreateTimeUtc)
			{
				messages.Add("ResolveTimeUtc shouldn't be less than Incident create time");
			}
			if (isResolvedOrClosed && IM_ResolveTimeUtc.IsEmpty)
			{
				messages.Add("ResolveTimeUtc shouldn't be empty for a resolved incident");
			}
			if (IM_Status == SupportIncidentLookups.Status.Closed && IM_CloseTimeUtc.IsEmpty)
			{
				messages.Add("CloseTimeUtc shouldn't be empty for a closed incident");
			}
			foreach (var message in messages)
			{
				ErrorReporter.ReportDeveloperExceptionOnce(message, new IncidentInvalidTimestampException(message, this));
			}
		}

		public ZString PreviouslySavedCriticality { get; private set; }

		void AddLogIfChanged(ZString originalValue, ZString currentValue, ZString fieldName)
		{
			if (IsInDatabase && originalValue != currentValue)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, ChangedPropertyTemplate, fieldName, originalValue, currentValue));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		const string ChangedPropertyTemplate = "Changed {0} from {1} to {2}.";

		public bool IsUpdatingDisposition { get; private set; }

		public IEnumerable<StorageDocsBase> ChangedPublishedEDocs => eDocsToPublish ?? Enumerable.Empty<StorageDocsBase>();

		List<StorageDocsBase> eDocsToPublish;

		protected void SetIncidentNumberIfRequired()
		{
			if (!IsInDatabase)
			{
				var req = Request;
				req.PopulateIncidentNumber();
				base.IM_IncidentNumber = req.INC_IncidentNumber;
			}
		}

		public override ZString IM_IncidentNumber
		{
			get
			{
				return base.IM_IncidentNumber;
			}
			set
			{
				base.IM_IncidentNumber = value;
				Request.INC_IncidentNumber = value;
			}
		}

		bool inOnSaveSucceeded;

		protected virtual void OnSaveSucceeded()
		{
			// Note: framework base class has recursion prevention and won't call OnSaved recursively
			// if we call call Factory.Save again from here
			inOnSaveSucceeded = true;
			DbFeatureRequestContactPK = featureRequestContactPK;

			try
			{
				SendUnsubscribedParticipantsNotifications();
				SendInternalNotifications();
				SendCustomerNotifications();

				OriginalSnapShot.TakeSnapShot(this, DetailNoteTextLoaded);
				IsNewIncident = false;

				if (eConversation != null)
				{
					eConversation.IncidentSaveSucceeded();
				}

				using (Factory.SetTempContext(SupportIncident.Context.OnSecondFactorySave))
				{
					Factory.Save();
				}
			}
			finally
			{
				inOnSaveSucceeded = false;
				PreviouslySavedCriticality = Criticality;
				IsRevertingToClosedOrResolved = false;
				IsUpdatingDisposition = false;
				SuspendTriggerCloseIncident = false;
				CloseIncidentViaPopupForm = false;
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && CurrentActivityObjectDuringSaving != null)
			{
				AddATCEventForCommunication(CurrentActivityObjectDuringSaving);
				CurrentActivityObjectDuringSaving = null;
				using (SetIgnoreHasChangesForWorkflowTemplateApplication())
				{
					Factory.Save();
				}
			}
		}

		public bool PreventSaving
		{
			get { return fPreventSaving; }
			set { fPreventSaving = value; }
		}

		bool fPreventSaving;

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && !PreventSaving; }
		}

		public override void Delete()
		{
			RemoveEstimate();
			RemoveQuote();
			WorkflowItems.RemoveAndDeleteAll();
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();
			RemoveDefectCausedByWorkItem();
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			var isInitialSave = !IsInDatabase && WorkflowItems.Count == 0;
			var shouldRecalculateReleaseGroups = !isInitialSave && HasChanges;

			if (isInitialSave)
			{
				ApplyWorkflowTemplateResult applyTemplateResult;

				using (Factory.SetTempContext(Context.ApplyingWorkflowTemplates))
				{
					applyTemplateResult = this.ApplyWorkflowTemplates();
				}

				if (applyTemplateResult.HasTemplateApplied)
				{
					CalculateWorkflowDependentProperties();
				}

				if (isInitialSave && WorkflowItems.Tasks.Count > 0)
				{
					var firstTask = WorkflowItems.Tasks.Cast<ProcessTask>().OrderBy(task => task.P9_Sequence).First();
					if (firstTask.P9_GS_NKAssignedStaffMember == User.ServiceUserCode || firstTask.P9_GS_NKAssignedStaffMember == User.WebUserCode)
					{
						firstTask.P9_GS_NKAssignedStaffMember = ZString.Empty;
						if (firstTask.P9_G4_RequiredCapability.IsEmpty)
						{
							firstTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
						}
					}
				}
			}
			else
			{
				if (!IsInDatabase && this.HasContext(Context.NewWebRequest) && !WorkflowItems.OfType<ProcessTask>().Any(x => x.IsMilestone || x.IsTrigger()))
				{
					this.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificEntityTypes(TemplateEntityType.Milestones | TemplateEntityType.Triggers));
				}

				if (shouldRecalculateReleaseGroups)
				{
					this.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificEntityTypes(TemplateEntityType.ReleaseGroupRules));
				}

				var parameters = TemplateApplicationParameters.ApplySpecificEntityTypes(TemplateEntityType.Milestones | TemplateEntityType.Triggers);
				if (IgnoreHasChangesForWorkflowTemplateApplication)
				{
					parameters = TemplateApplicationParameters.ApplyIgnoreHasChangesToExistingParameters(parameters);
				}
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this, parameters);
			}

			if (!IsInDatabase || HasChanges)
			{
				// OnSaveSucceeded calls SendEmails which can do another Factory.Save.
				// We don't want to populate any more tasks in that case to avoid duplication.
				if (!inOnSaveSucceeded)
				{
					RecalculateProductArea();

					foreach (ProcessTask task in WorkflowItems.Tasks)
					{
						task.P9_ParentTemplateID = ZGuid.Empty; //If product area is changed tasks need to be copied again. Clear task template id so new task is added rather than merged.
					}
				}
			}
		}

		void LogModuleChange()
		{
			if (IsInDatabase && IM_ModuleInfo.HasChanges && IM_Module != OriginalSnapShot.IM_Module)
			{
				string logText = string.Format(CultureInfo.InvariantCulture, "Module changed from {0} ({1}) to {2} ({3})",
												OriginalSnapShot.IM_Module,
												Lookups.GetModuleList(GetModuleListType(OriginalSnapShot.IM_Priority), OriginalSnapShot.IM_Product, ZString.Empty).GetDescriptionFromCode(OriginalSnapShot.IM_Module),
												IM_Module,
												Lookups.GetModuleList(GetModuleListType(IM_Priority), IM_Product, ZString.Empty).GetDescriptionFromCode(IM_Module));
				AddInternalSystemLogMessage(logText);
			}
		}

		ModuleListType GetModuleListType(ZString criticalityCode)
		{
			if (criticalityCode == Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement)
			{
				return ModuleListType.Cr8;
			}
			else if (criticalityCode == Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
			{
				return ModuleListType.Cr9;
			}
			else if (!string.IsNullOrEmpty(criticalityCode))
			{
				return ModuleListType.MenuSection;
			}
			return ModuleListType.Unspecified;
		}

		protected virtual void LogProductChange()
		{
			if (IsInDatabase && IM_ProductInfo.HasChanges && IM_Product != OriginalSnapShot.IM_Product)
			{
				var logText = string.Format(CultureInfo.InvariantCulture, "Product changed from {0} ({1}) to {2} ({3})",
												OriginalSnapShot.IM_Product,
												Lookups.ProductList.GetDescriptionFromCode(OriginalSnapShot.IM_Product),
												IM_Product,
												Lookups.ProductList.GetDescriptionFromCode(IM_Product));
				AddInternalSystemLogMessage(logText);
			}
		}

		void LogProductAreaChange()
		{
			if (IsInDatabase && ProductAreaInfo.HasChanges && ProductArea != OriginalSnapShot.ProductArea)
			{
				string logText = string.Format(CultureInfo.InvariantCulture, "Product Area changed from {0} ({1}) to {2} ({3})",
												OriginalSnapShot.ProductArea,
												Lookups.ProductAreaList.GetDescriptionFromCode(OriginalSnapShot.ProductArea),
												ProductArea,
												Lookups.ProductAreaList.GetDescriptionFromCode(ProductArea));
				AddInternalSystemLogMessage(logText);
			}
		}

		void SetInitialStaffAssignmentDate()
		{
			if (InitialStaffAssignmentDate.IsEmpty && (!IsInDatabase || (ZString)IM_ResolutionCodeInfo.OriginalValue == SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment))
			{
				if (!IM_Status.IsEmpty && (IM_Category == SupportIncidentCategoriesList.Codes.Support) && (IM_ResolutionCode != SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment))
				{
					InitialStaffAssignmentDate = ZDateTime.Now;
				}
			}
		}

		void LogAssignedStaffChange()
		{
			if (ShouldSendEmailToCustomerServiceContact && CheckShouldSendEmailToChangedStaff(CustServiceContact, OriginalIM_GS_NKCustServiceContact))
			{
				string logText = string.Format(CultureInfo.InvariantCulture, "Email sent to new assigned staff - {0}.", CustServiceContact.GS_FullName);
				AddInternalSystemLogMessage(logText);
			}

			if (ShouldSendEmailToAssignedStaff && CheckShouldSendEmailToChangedStaff(AssignedToCurrent, OriginalIM_GS_NKAssignedToCurrent))
			{
				string logText = string.Format(CultureInfo.InvariantCulture, "Email sent to new assigned staff - {0}.", AssignedToCurrent.GS_FullName);
				AddInternalSystemLogMessage(logText);
			}
		}

		void LogMenuItemChange()
		{
			if (IsInDatabase && SourceModuleWithPathInfo.HasChanges && SourceModuleWithPath != OriginalSnapShot.SourceModuleWithPath)
			{
				var logText = string.Format(CultureInfo.InvariantCulture, "Menu Item changed from [{0}] to [{1}]", OriginalSnapShot.SourceModuleWithPath, SourceModuleWithPath);
				AddInternalSystemLogMessage(logText);
			}
		}

		void LogReOpenRuleWhenIncidentClosed()
		{
			var closedReopenRule = GetClosedReopenRule();
			if (IM_ResolutionCodeInfo.HasChanges && IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed)
			{
				var logText = string.Format(CultureInfo.InvariantCulture, "The re-open rule for this incident at time of closure: {0} - {1}",
								closedReopenRule,
								GetResolutionAndClosureBehaviour().ClosedReopenRuleList.GetDescriptionFromCode(closedReopenRule));
				AddInternalSystemLogMessage(logText);
			}
		}

		protected bool ShouldSendEmailToCustomerServiceContact
		{
			get { return IM_Category == SupportIncidentCategoriesList.Codes.Support; }
		}

		protected bool ShouldSendEmailToAssignedStaff
		{
			get { return IM_Category != SupportIncidentCategoriesList.Codes.Support; }
		}

		#endregion

		#region Email

		public bool MuteEmailNotification()
		{
			return SupportIncidentEmailTriggeringRules.SuppressAll(this);
		}

		public void UnmuteEmailNotification()
		{
			SupportIncidentEmailTriggeringRules.UnsuppressAll(this);
		}

		void SendMessageIfEmailNotificationSwitchStatusChanged()
		{
			var ruleTagStatus = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(this, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);

			if (ruleTagStatus == IncidentEmailTagRuleStatus.New)
			{
				AddPublicSystemLogMessage("We have disabled outbound email notifications for this eRequest.");
			}
			else if(ruleTagStatus == IncidentEmailTagRuleStatus.Deleted)
			{
				AddPublicSystemLogMessage("We have re-enabled outbound email notifications for this eRequest.");
			}
		}

		protected string AdditionalNote
		{
			get
			{
				if (!string.IsNullOrEmpty(AssignmentComment))
				{
					return "<BR /><BR />Comment:<BR />" + AssignmentComment;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public void SendUnsubscribedParticipantsNotifications()
		{
			var unsubscribedParticipants = EConversation.ExistingConversation.UnsubscribedParticipants;
			if (unsubscribedParticipants.IsNullOrEmpty())
			{
				return;
			}

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			foreach (var item in unsubscribedParticipants)
			{
				var emailAddress = item.Email;
				if (string.IsNullOrEmpty(emailAddress))
				{
					continue;
				}

				var template = new SendUnsubscribedNotificationEmailContentBuilder(this);
				var email = EDIEmailBuilder.GetInstance(this).BuildHtmlEmailDefByTemplate(template);
				if (email != null)
				{
					email.FromAddress = SupportIncident.SupportEmailAddress;
					email.FromDisplayName = SupportIncident.SupportDisplayName;
					email.AddRecipientForUserCommunication(emailAddress, RecipientDef.RecipientTypes.TO);
				}

				Env.OutgoingMailManager.Create(factory, email);
				factory.Save();
			}

			EConversation.ExistingConversation.UnsubscribedParticipants.Clear();
		}

		protected void SendInternalNotifications()
		{
			SendEmailToCustomerServiceContact();
			SendEmailToAssignedStaff();
			SendStaffMessagesToOtherSubscribedStaff();
			AssignmentComment = null;
		}

		public void SendStaffMessagesToOtherSubscribedStaff()
		{
			if (!Env.CurrentUser.IsSystemAccount)
			{
				var newMessages = EConversation.GetNewMessagesAndClear();
				if (newMessages.Any(x => !x.JCM_IsInternal) || NotifyInternalSubscribersForInternalLog)
				{
					if (newMessages.Length > 0)
					{
						var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ClientControllerRegistration.SupportIncident, PK.ToGuid());
						var factory = new BusinessObjectFactory() { RefreshEnabled = false };
						if (EConversationEmailBuilder.GenerateStaffNotification(factory, EConversation.ExistingConversation, newMessages, null, url))
						{
							factory.Save();
						}
					}
				}
			}
		}

		protected void SendCustomerNotifications()
		{
			CustomerNotifier.SendChanges();
		}

		public IIncidentCustomerNotifier CustomerNotifier
		{
			get
			{
				return customerNotifier ?? (customerNotifier = IncidentCustomerNotifierFactory.CreateNotifier(this));
			}
			set
			{
				if (value != null && value.RelatedIncident != this)
				{
					throw new ArgumentException("RelatedIncident does not match");
				}

				customerNotifier = value;
			}
		}
		IIncidentCustomerNotifier customerNotifier;

		internal IIncidentCustomerNotifier CurrentCustomerNotiferForTest => customerNotifier;

		public bool HasClientAndContact
		{
			get { return Client != null && Contact != null; }
		}

		public const string SupportDisplayName = "Customer Service";

		public static string SupportEmailAddress
		{
			get { return EDIDataRegistry.Instance.IncidentFromEmailAddress.Value; }
		}

		protected ControllerID ControllerIDForHtmlUrl
		{
			get { return ClientControllerRegistration.SupportIncident; }
		}

		#region Creation Notification Email to Contact

		/// <summary>
		/// Only used by form -> Actions > "Re-send Email &Notification".
		/// Incident must be saved beforehand, and is saved after.
		/// </summary>
		public SupportIncidentEmail GetEmailObjectForNotification()
		{
			SupportIncidentEmail result = null;
			bool allowedGenerting = true;
			if (CheckIsInValidStateToSendEmail())
			{
				var hasConversation = IncidentCustomerNotifier.CustomerCanAccessEConversation(this);

				var emailContents = new CustomerServiceNotificationEmailContentBuilder(this, hasConversation, IM_Product);
				result = SupportIncidentEmail.New(this, emailContents, out allowedGenerting);
				if (result != null)
				{
					result.PublicLogComment = "Notification email re-sent to contact";
					result.XAutoResponseSuppressHeaderValue = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
				}
			}
			return result;
		}

		#endregion

		#region Correspondence / Update to Contact

		/// <summary>
		/// Only used by Form > Action > Email Update,
		/// which is only visible if eConversation is not available.
		/// Remove when we enable web portal.
		/// </summary>
		public SupportIncidentEmail GetEmailObjectForCorrespondence()
		{
			SupportIncidentEmail result = null;
			if (CheckIsInValidStateToSendEmail())
			{
				var emailContents = new SupportIncidentCorrespondenceEmailContentBuilder(this);
				result = SupportIncidentEmail.New(this, emailContents, out var allowedGenerating);
				if (result != null)
				{
					result.XAutoResponseSuppressHeaderValue = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
					result.InternalLogComment = "Email Update sent and attached to eDocs";
				}
			}
			return result;
		}

		#endregion

		#region Closed Message to Client

		public string ChargeableWorkNoticeText
		{
			get { return "Because of the service provided this incident is chargeable under your maintenance contract and will be billed at the end of the current billing period."; }
		}

		public string IncidentCloseTypeText
		{
			get
			{
				string result = string.Empty;
				if (IM_Status == SupportIncidentLookups.Status.Closed)
				{
					var isResolved = IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
					var closedDescriptionText = (isResolved || IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed)
						? IM_ClosureResolutionDescription
						: IM_ResolutionCodeDescription;
					result = (isResolved ? "Resolved with reason: " : "Closed with reason: ") + closedDescriptionText.ToString();
				}
				return result;
			}
		}

		#endregion

		public void EstimateOrQuoteAdded(ZString resolutionNoteText, IeDoc sentFile)
		{
			if (IsWebRequest || !ERequestEHubCustomerNotificationSender.IsBiDirectionResponseSupported(this))
			{
				if (sentFile != null && HasClientAndContact && !Contact.OC_Email.IsEmpty)
				{
					var oldResolutionNoteText = ResolutionNoteText;
					bool hasEConversation = IsWebRequest;

					// Temporarily set so given text can be substitued in macros
					ResolutionNoteText = resolutionNoteText;
					(var email, var isAllowed) = SupportIncidentEmailQuickBuilder.CreateIncidentClosedEmail(this, hasEConversation, IM_ResolutionCode, true);
					ResolutionNoteText = oldResolutionNoteText;

					if (email != null)
					{
						email.eDoc = sentFile;
						email.MarkAsNoNeedSaveToEDocs();
						email.ShouldAddNoteAndEvent = false;
						email.PublicLogComment = string.Empty;
						CustomerNotifier.SendEmailOnIncidentSave(email);
					}
					else if (isAllowed)
					{
						if (IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided)
						{
							AddInternalSystemLogMessage("No development estimate provided notification email sent because no template has been setup");
						}
						else if (IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided)
						{
							AddInternalSystemLogMessage("No formal quotation provided notification email sent because no template has been setup");
						}
					}
				}
			}
		}

		public ZString JobSpecificToken
		{
			get { return CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(this); }
		}

		#endregion

		#region Related Business Objects

		#region Feature Requests

		public ZGuid ParentFeatureRequestPK
		{
			get { return (ParentFeatureRequest != null) ? ParentFeatureRequest.PK : ZGuid.Empty; }
		}

		public ZPropertyInfo ParentFeatureRequestPKInfo
		{
			get { return GetZPropertyInfo(nameof(ParentFeatureRequestPK)); }
		}

		public SupportIncident ParentFeatureRequest
		{
			get
			{
				return RelatedItems.GetElements<SupportIncident>(s => s.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest && !RelatedFeatureRequests.Contains(s)).FirstOrDefault();
			}
		}

		public FeatureRequestRelatedFeatureRequestsCollection RelatedFeatureRequests
		{
			get
			{
				if (fRelatedFeatureRequests == null)
				{
					fRelatedFeatureRequests = new FeatureRequestRelatedFeatureRequestsCollection(this);
					fRelatedFeatureRequests.Load();
				}
				return fRelatedFeatureRequests;
			}
		}

		public bool IsFeatureRequest
		{
			get
			{
				return IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest;
			}
		}

		public bool IsInternalFeatureRequest
		{
			get
			{
				return IsFeatureRequest && IsInternal;
			}
		}

		public const string InternalFeatureRequestComment = "Internal Feature Request";

		FeatureRequestRelatedFeatureRequestsCollection fRelatedFeatureRequests;

		#endregion

		#region Client On Feature Request Tab

		public ZString FeatureRequestClientName
		{
			get { return FeatureRequestClient != null ? FeatureRequestClient.OH_FullName : ZString.Empty; }
		}

		public EDIOrgHeader FeatureRequestClient
		{
			get { return Factory.Load<EDIOrgHeader>(FeatureRequestClientPK); }
		}

		public ZString FeatureRequestClientRegType
		{
			get { return FeatureRequestClient != null ? ZString.Format("{0}:", FeatureRequestClient.PrimaryRegistrationNumber.NumberTypeForDisplay) : ZString.Empty; }
		}

		public ZString FeatureRequestClientRegNo
		{
			get { return FeatureRequestClient != null ? FeatureRequestClient.PrimaryRegistrationNumber.Number : ZString.Empty; }
		}

		public ZGuid FeatureRequestClientPK
		{
			get
			{
				if (featureRequestClientPK.IsEmpty)
				{
					featureRequestClientPK = FeatureRequestClientAddress != null ? FeatureRequestClientAddress.OA_OH : ZGuid.Empty;
				}
				return featureRequestClientPK;
			}
			set
			{
				if (FeatureRequestClientPK != value)
				{
					featureRequestClientPK = value;
					if (!FeatureRequestContactPK.IsEmpty && FeatureRequestContact != null && FeatureRequestContact.OC_OH != value)
					{
						FeatureRequestContactPK = ZGuid.Empty;
					}
					if (!value.IsEmpty)
					{
						if (Estimate.CIE_RX_NKCurrency.IsEmpty && FeatureRequestClient != null && FeatureRequestClient.LicCompany != null)
						{
							char[] separator = new char[] { ',' };
							ZString[] currency = FeatureRequestClient.LicCompany.InvoiceCurrency.Split(separator);

							Estimate.CIE_RX_NKCurrency = currency[0].Trim();
							Quote.CIQ_RX_NKCurrency = currency[0].Trim();
						}

						if (FeatureRequestClientAddressPK.IsEmpty && !IM_OA_BranchAddress.IsEmpty)
						{
							FeatureRequestClientAddressPK = IM_OA_BranchAddress;
						}
					}
					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
					FeatureRequestClientPKInfo.RefreshBinding();
				}
			}
		}

		ZGuid featureRequestClientPK;

		public ZPropertyInfo FeatureRequestClientPKInfo
		{
			get { return GetZPropertyInfo(nameof(FeatureRequestClientPK), "Feature Request Client"); }
		}

		public ZString FeatureRequestClientAddressAsASingleLineWithoutCompanyName
		{
			get { return FeatureRequestClientAddress != null ? FeatureRequestClientAddress.AddressAsASingleLineWithoutCompanyName : ZString.Empty; }
		}

		public OrgAddress FeatureRequestClientAddress
		{
			get { return Factory.Load<OrgAddress>(FeatureRequestClientAddressPK); }
		}

		[RelatedBusinessObject("FeatureRequestClientAddress")]
		[List("Lookups.BranchAddresses")]
		public ZGuid FeatureRequestClientAddressPK
		{
			get
			{
				if (!isFeatureRequestClientAddressPKLoaded)
				{
					featureRequestClientAddressPK = LoadFeatureRequestClientAddressPK();
					isFeatureRequestClientAddressPKLoaded = true;
				}
				return featureRequestClientAddressPK;
			}
			set
			{
				featureRequestClientAddressPK = value;
				FeatureRequestClientPK = FeatureRequestClientAddress != null ? FeatureRequestClientAddress.OA_OH : ZGuid.Empty;
				AttachOrDetachFeatureRequestClientAddress();
				if (!IsSettingHasChangesSuspended)
				{
					HasChanges = true;
				}
				FeatureRequestClientAddressPKInfo.RefreshBinding();
			}
		}

		ZGuid featureRequestClientAddressPK;
		ZBool isFeatureRequestClientAddressPKLoaded = false;

		public ZPropertyInfo FeatureRequestClientAddressPKInfo
		{
			get { return GetZPropertyInfo(nameof(FeatureRequestClientAddressPK), "Feature Request Client Address"); }
		}

		public ZAddress FeatureRequestClientAddressPK_ZAddress
		{
			get
			{
				if (featureRequestClientAddressPK_ZAddress == null)
				{
					featureRequestClientAddressPK_ZAddress = new ZAddress(FeatureRequestClientAddressPKInfo);
				}
				return featureRequestClientAddressPK_ZAddress;
			}
		}
		ZAddress featureRequestClientAddressPK_ZAddress;

		ZGuid LoadFeatureRequestClientAddressPK()
		{
			GenPivot link = LoadClientAddressPivot();
			return link != null ? link.XX_Relation2ID : ZGuid.Empty;
		}

		void AttachOrDetachFeatureRequestClientAddress()
		{
			GenPivot link = LoadClientAddressPivot();

			if (link == null && !featureRequestClientAddressPK.IsEmpty)
			{
				link = Factory.New<GenPivot>();
				link.XX_Relation2ID = featureRequestClientAddressPK;
				link.XX_Relation2TableCode = OrgAddressSchema.Constants.Prefix;
				link.XX_Relation1ID = PK;
				link.XX_Relation1TableCode = IncidentMainSchema.Constants.Prefix;
				link.XX_RelationType = EDIGenPivotTypes.FeatureRequestClientAddress;
			}
			else if (link != null && !featureRequestClientAddressPK.IsEmpty && link.XX_Relation2ID != featureRequestClientAddressPK)
			{
				link.XX_Relation2ID = featureRequestClientAddressPK;
			}
			else if (link != null && !link.IsDeleted && featureRequestClientAddressPK.IsEmpty && isFeatureRequestClientAddressPKLoaded)
			{
				link.Delete();
			}
		}

		GenPivot LoadClientAddressPivot()
		{
			return GenPivot.LoadRelation1Pivot(Factory, PK, IncidentMainSchema.Constants.Prefix, EDIGenPivotTypes.FeatureRequestClientAddress);
		}

		#endregion

		#region Contact On Feature Request Tab

		public OrgContact FeatureRequestContact
		{
			get { return Factory.Load<OrgContact>(FeatureRequestContactPK); }
		}

		public ZString FeatureRequestContactPrimaryWorkplace
		{
			get
			{
				return GetPrimaryWorkplace(FeatureRequestContact, FeatureRequestContact?.ParentOrg);
			}
		}

		public ZPropertyInfo FeatureRequestContactPrimaryWorkplaceInfo
		{
			get { return GetZPropertyInfo(nameof(FeatureRequestContactPrimaryWorkplace), "Feature Request Contact Primary Workplace"); }
		}

		[List("Lookups.FeatureRequestContactList")]
		public ZGuid FeatureRequestContactPK
		{
			get
			{
				if (!isFeatureRequestContactPKLoaded)
				{
					featureRequestContactPK = LoadFeatureRequestContactPK();
					DbFeatureRequestContactPK = featureRequestContactPK;
					isFeatureRequestContactPKLoaded = true;
				}
				return featureRequestContactPK;
			}
			set
			{
				if (FeatureRequestContactPK != value)
				{
					featureRequestContactPK = value;
					AttachOrDetachFeatureRequestContact();
					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
					FeatureRequestContactPKInfo.RefreshBinding();
				}
			}
		}

		ZGuid featureRequestContactPK;
		public ZGuid DbFeatureRequestContactPK { get; private set; }

		ZBool isFeatureRequestContactPKLoaded = false;

		public ZPropertyInfo FeatureRequestContactPKInfo
		{
			get { return GetZPropertyInfo(nameof(FeatureRequestContactPK), "Feature Request Contact"); }
		}

		ZGuid LoadFeatureRequestContactPK()
		{
			GenPivot link = LoadContactPivot();
			return link != null ? link.XX_Relation2ID : ZGuid.Empty;
		}

		void AttachOrDetachFeatureRequestContact()
		{
			GenPivot link = LoadContactPivot();

			if (link == null && !featureRequestContactPK.IsEmpty)
			{
				link = Factory.New<GenPivot>();
				link.XX_Relation2ID = featureRequestContactPK;
				link.XX_Relation2TableCode = OrgContactSchema.Constants.Prefix;
				link.XX_Relation1ID = PK;
				link.XX_Relation1TableCode = IncidentMainSchema.Constants.Prefix;
				link.XX_RelationType = EDIGenPivotTypes.FeatureRequestContact;
			}
			else if (link != null && !featureRequestContactPK.IsEmpty && link.XX_Relation2ID != featureRequestContactPK)
			{
				link.XX_Relation2ID = featureRequestContactPK;
			}
			else if (link != null && !link.IsDeleted && featureRequestContactPK.IsEmpty && isFeatureRequestContactPKLoaded)
			{
				link.Delete();
			}
		}

		GenPivot LoadContactPivot()
		{
			return GenPivot.LoadRelation1Pivot(Factory, PK, IncidentMainSchema.Constants.Prefix, EDIGenPivotTypes.FeatureRequestContact);
		}

		#endregion

		#region Estimate

		public ClientIncidentEstimate Estimate
		{
			get
			{
				if (estimate == null)
				{
					estimate = LoadOrCreateEstimate();
					RegisterEditableChildObject(estimate);
				}
				return estimate;
			}
		}

		public ClientIncidentEstimate EstimateForBinding => LoadEstimate();

		ClientIncidentEstimate estimate;

		ClientIncidentEstimate LoadOrCreateEstimate()
		{
			var result = LoadEstimate();
			if (result == null)
			{
				result = Factory.New<ClientIncidentEstimate>();
				using (result.SuspendSettingHasChanges())
				{
					result.CIE_IM = PK;
				}
			}
			return result;
		}

		ClientIncidentEstimate LoadEstimate()
		{
			ZQuery query = new ZQuery(ClientIncidentEstimateSchema.CIE_IM, PK);
			return Factory.LoadTop1<ClientIncidentEstimate>(query);
		}

		void RemoveEstimate()
		{
			var result = LoadEstimate();
			if (result != null)
			{
				result.Delete();
			}
		}

		#endregion

		#region Quote

		public ClientIncidentQuote Quote
		{
			get
			{
				if (quote == null)
				{
					quote = LoadOrCreateQuote();
					RegisterEditableChildObject(quote);
				}
				return quote;
			}
		}

		public ClientIncidentQuote QuoteForBinding => LoadQuote();

		public void SetQuoteDeliveredTimeIfEmpty()
		{
			if (quote == null)
			{
				quote = LoadQuote();
				if (quote != null)
				{
					RegisterEditableChildObject(quote);
				}
			}

			if (quote != null && quote.CIQ_DeliveredDateUTC.IsEmpty)
			{
				quote.CIQ_DeliveredDateUTC = ZDateTime.UtcNow;
			}
		}

		ClientIncidentQuote quote;

		ClientIncidentQuote LoadOrCreateQuote()
		{
			var result = LoadQuote();
			if (result == null)
			{
				result = Factory.New<ClientIncidentQuote>();
				using (result.SuspendSettingHasChanges())
				{
					result.CIQ_IM = PK;
				}
			}
			return result;
		}

		ClientIncidentQuote LoadQuote()
		{
			ZQuery query = new ZQuery(ClientIncidentQuoteSchema.CIQ_IM, PK);
			return Factory.LoadTop1<ClientIncidentQuote>(query);
		}

		void RemoveQuote()
		{
			var result = LoadQuote();
			if (result != null)
			{
				result.Delete();
			}
		}

		#endregion

		#region Work Items

		public NewWorkItemRelatedCollectionView RelatedWorkItems
		{
			get
			{
				if (relatedWorkItems == null)
				{
					relatedWorkItems = new NewWorkItemRelatedCollectionView(RelatedItems);
				}
				return relatedWorkItems;
			}
		}
		NewWorkItemRelatedCollectionView relatedWorkItems;

		public NewWorkItemRelatedCollectionView RelatedWorkItemsReadOnly
		{
			get
			{
				if (relatedWorkItemsReadOnly == null)
				{
					relatedWorkItemsReadOnly = new NewWorkItemRelatedCollectionView(RelatedWorkTaskItemsReadOnly);
				}
				return relatedWorkItemsReadOnly;
			}
		}
		NewWorkItemRelatedCollectionView relatedWorkItemsReadOnly;

		#endregion

		#region Defect Caused By Work Item

		public NewWorkItem DefectCausedByWorkItem
		{
			get { return Factory.Load<NewWorkItem>(DefectCausedByWorkItemPK); }
		}

		[List("Lookups.WorkItems")]
		public ZGuid DefectCausedByWorkItemPK
		{
			get
			{
				if (!isDefectCausedByWorkItemPKLoaded)
				{
					defectCausedByWorkItemPK = LoadDefectCausedByWorkItemPK();
					isDefectCausedByWorkItemPKLoaded = true;
				}
				return defectCausedByWorkItemPK;
			}
			set { SetNonPersistentPropertyValue(DefectCausedByWorkItemPKInfo, ref defectCausedByWorkItemPK, value); }
		}

		ZGuid defectCausedByWorkItemPK;
		ZBool isDefectCausedByWorkItemPKLoaded = false;

		public ZPropertyInfo DefectCausedByWorkItemPKInfo
		{
			get { return GetZPropertyInfo(nameof(DefectCausedByWorkItemPK)); }
		}

		public GenPivot LoadDefectCausedByIncidentPivot()
		{
			return GenPivot.LoadRelation2Pivot(Factory, PK, IncidentMainSchema.Constants.Prefix, EDIGenPivotTypes.DefectCausedByWorkItem);
		}

		ZGuid LoadDefectCausedByWorkItemPK()
		{
			ZGuid result = ZGuid.Empty;
			var link = LoadDefectCausedByIncidentPivot();
			if (link != null)
			{
				result = link.XX_Relation1ID;
			}
			return result;
		}

		void AttachOrDetachDefectCausedByWorkItem()
		{
			var link = LoadDefectCausedByIncidentPivot();

			if (link == null && !defectCausedByWorkItemPK.IsEmpty)
			{
				link = Factory.New<GenPivot>();
				link.XX_Relation2ID = this.PK;
				link.XX_Relation2TableCode = IncidentMainSchema.Constants.Prefix;
				link.XX_Relation1ID = defectCausedByWorkItemPK;
				link.XX_Relation1TableCode = WorkItemSchema.Constants.Prefix;
				link.XX_RelationType = EDIGenPivotTypes.DefectCausedByWorkItem;
			}
			else if (link != null && !defectCausedByWorkItemPK.IsEmpty && link.XX_Relation1ID != defectCausedByWorkItemPK)
			{
				link.XX_Relation1ID = defectCausedByWorkItemPK;
			}
			else if (link != null && !link.IsDeleted && defectCausedByWorkItemPK.IsEmpty && isDefectCausedByWorkItemPKLoaded)
			{
				link.Delete();
			}
		}

		public void RemoveDefectCausedByWorkItem()
		{
			var link = LoadDefectCausedByIncidentPivot();
			if (link != null)
			{
				link.Delete();
			}
			defectCausedByWorkItemPK = ZGuid.Empty;
		}

		#endregion

		#region Last Ten Created Incidents For Same Enterprise

		public SupportIncidentCollection LastTenIncidentsForSameEnterprise
		{
			get
			{
				SupportIncidentCollection collection = new SupportIncidentCollection(new BusinessObjectFactory() { RefreshEnabled = false });
				if (!IM_OH_Client.IsEmpty && IM_OH_Client.IsValid && !((EDIOrgHeader)Client).LicenceEnterpriseCode.IsEmpty)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SupportIncident));
					ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), IncidentMainSchema.IM_OH_Client);
					ZDBOnlySubQuery licenceCompanySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
					licenceCompanySubQuery.AddToFilter(LicenceCompanySchema.LC_LE, ((EDIOrgHeader)Client).LicCompany.LC_LE);
					orgSubQuery.AddSubQuery(licenceCompanySubQuery, JoinCondition.And);
					query.AddSubQuery(orgSubQuery, JoinCondition.And);

					query.AddToFilter(IncidentMainSchema.PK, SQLComparisonOperator.NotEqual, PK);
					query.OrderBy = IncidentMainSchema.Constants.IM_SystemCreateTimeUtc + OrderByClause.Descending;
					query.MaximumRows = 10;
					collection.Load(query);
				}
				return collection;
			}
		}

		public ZString LastTenIncidentsForSameEnterpriseAsText
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				foreach (SupportIncident incident in LastTenIncidentsForSameEnterprise)
				{
					builder.AppendLine(ZString.Format("{0}\t{1} - {2}", incident.IM_SystemCreateTimeUtc.ToLongTimeString(), incident.IM_IncidentNumber, incident.IM_Description));
					builder.AppendLine("---------------------------------------------------------------------------------------------------------------------------------------------");
				}
				return builder.ToString();
			}
		}

		#endregion

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return IM_IncidentNumber; }
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		SupportIncidentInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new SupportIncidentInvoicingSupporter(this)); }
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					fDocumentSupporter = new SupportIncidentDocumentSupporter(this);
				}
				return fDocumentSupporter;
			}
		}

		SupportIncidentDocumentSupporter fDocumentSupporter;

		#region Document Supporter

		public class SupportIncidentDocumentSupporter : DocumentSupporter
		{
			public SupportIncidentDocumentSupporter(SupportIncident professionalServicesQuote)
				: base(professionalServicesQuote)
			{
			}

			protected SupportIncident SupportIncident
			{
				get { return (SupportIncident)BusinessObject; }
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.SupportIncident; }
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return new Core.Constants.DataContext[] { Core.Constants.DataContext.GenericFreightJob };
			}

			protected override List<DataContextValue> GetSupportedBODataSources()
			{
				return GetSupportedBODataSourcesFor(typeof(SupportIncident));
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return EDISecurityCheckpoints.CustomerServiceIncident; }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				if (dataContext == Enterprise.Core.Constants.DataContext.GenericFreightJob)
				{
					return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, SupportIncident);
				}
				else
				{
					return new DocumentWrapper[] { null };
				}
			}

			public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
			{
				IDocumentDeliveryContact result = null;

				if (contactType == ContactType.LocalClient)
				{
					if (SupportIncident.Client != null)
					{
						result = new OrgHeaderContact(SupportIncident.Client, null);
					}
				}
				else
				{
					result = base.GetContactOrganisation(menuName, contactType, direction);
				}

				return result;
			}

			protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
			{
				return new IBODocDataProvider[] { BODocDataProvider.Get(BusinessObject) };
			}
		}

		#endregion

		#endregion

		#region IWorkItemRelatedItem Members

		public ZString Type
		{
			get
			{
				switch (IM_Category)
				{
					case SupportIncidentCategoriesList.Codes.FeatureRequest:
						return EDIWorkTaskRelatedItemTypes.FeatureRequest;
					case SupportIncidentCategoriesList.Codes.Defect:
						return EDIWorkTaskRelatedItemTypes.Defect;
					case SupportIncidentCategoriesList.Codes.ComplianceRequirement:
						return EDIWorkTaskRelatedItemTypes.ComplianceRequirement;
					case SupportIncidentCategoriesList.Codes.CustomerServiceRequest:
						return EDIWorkTaskRelatedItemTypes.CustomerServiceRequest;
					case SupportIncidentCategoriesList.Codes.ContentDevelopment:
						return EDIWorkTaskRelatedItemTypes.ContentDevelopment;
					default:
						return EDIWorkTaskRelatedItemTypes.SupportIncident;
				}
			}
		}

		ZPropertyInfo IWorkTaskRelatedItem.TypeInfo
		{
			get { return GetZPropertyInfo(nameof(Type)); }
		}

		#region Status and Disposition Description

		public ZString StatusDescription
		{
			get
			{
				ZString result = IM_StatusDescription;
				if (!result.IsEmpty && !IM_ResolutionCodeDescription.IsEmpty)
				{
					result = ZString.Format("Overall Status: {0} - eRequest Status: {1}", IM_StatusDescription, IM_ResolutionCodeDescription);
				}
				return result;
			}
		}

		public ZPropertyInfo StatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(StatusDescription)); }
		}

		#endregion

		public ZString ItemDescription
		{
			get { return IM_Description; }
		}

		ZPropertyInfo IWorkTaskRelatedItem.ItemDescriptionInfo
		{
			get { return IM_DescriptionInfo; }
		}

		public ZString Number
		{
			get { return IM_IncidentNumber; }
		}

		ZPropertyInfo IWorkTaskRelatedItem.NumberInfo
		{
			get { return IM_IncidentNumberInfo; }
		}

		public ZString AssignedStaffCode
		{
			get { return OverallAssignedToStaff != null ? OverallAssignedToStaff.GS_Code : ZString.Empty; }
		}

		ZPropertyInfo IWorkTaskRelatedItem.AssignedStaffCodeInfo
		{
			get { return GetZPropertyInfo(nameof(AssignedStaffCode)); }
		}

		bool IWorkItemRelatedItem.OnRelatedWorkItemClosed(WorkItem closedWorkItem)
		{
			if (ShouldStayIndependent)
			{
				return true;
			}

			var relatedWorkItems = RelatedWorkItems.OfType<NewWorkItem>();
			var allClosedOrCancelled = !relatedWorkItems.Any(x => !x.IsClosedOrCancelled);
			var allCancelled = !relatedWorkItems.Any(x => !x.IsCancelled);

			if (allClosedOrCancelled || allCancelled)
			{
				var tasks = WorkflowItems;
				var hasNoOpenTasks = tasks.Count == 0 || tasks.AllTasksClosedOrCancelled;

				if (allCancelled && hasNoOpenTasks)
				{
					DelaySendingMessage(
						IncidentConstants.DelayedMessageKey.AllWorkItemsCancelled,
						AddInternalSystemLogMessage,
						"All related work items are cancelled.");
					CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, "");
				}
				else if (IM_ClosureResolution != SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered && IM_ResolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed)
				{
					if (!NeedUpgrade)
					{
						ClosedWithNoUpgrade();
					}
					else
					{
						WaitForUpgrade();
					}
				}

				return true;
			}

			return false;
		}

		public void ClosedWithNoUpgrade()
		{
			DelaySendingMessage(
				IncidentConstants.DelayedMessageKey.WorkCompletedButNoUpgrades,
				AddInternalSystemLogMessage,
				"Work completed, no upgrade will be sent");

			if (WorkflowItems.Count == 0 || WorkflowItems.AllTasksClosedOrCancelled)
			{
				var disposition = IsInternal ? SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal : SupportIncidentLookups.DispositionList.Constants.Closed.Completed;
				CloseIncident(disposition, "");
			}
			else
			{
				IM_ResolutionCode = ZString.Empty; //Clear "Work Item Created / Linked" disposition so it can be set to calculated value
				CalculateWorkflowDependentProperties();
			}
		}

		public virtual bool NeedUpgrade => RelatedWorkItems.Cast<NewWorkItem>().Any(wi => ReleaseBuildContent.New(Factory).IsCargoWiseOneChange(wi)) && Database != null;

		void IWorkItemRelatedItem.OnRelatedWorkItemReOpened(WorkItem workItem)
		{
			if (ShouldStayIndependent)
			{
				return;
			}

			RemoveDelayedMessage(IncidentConstants.DelayedMessageKey.AllWorkItemsCancelled);
			RemoveDelayedMessage(IncidentConstants.DelayedMessageKey.WorkCompletedButNoUpgrades);
			SetWorkItemCreated();
		}

		void IWorkItemRelatedItem.OnWorkItemAdded(WorkItem workItem)
		{
			if (ShouldStayIndependent)
			{
				return;
			}

			if (IsWorkItemChangeReadyForUpgrade(workItem))
			{
				WaitForUpgrade();
			}
			else if (workItem.WKI_Status == ProcessTaskStatusCodeList.Codes.Closed)
			{
				((IWorkItemRelatedItem)this).OnRelatedWorkItemClosed(workItem);
			}
			else
			{
				((IWorkItemRelatedItem)this).OnRelatedWorkItemReOpened(workItem);
			}

			var jobHeader = ProcessJobHeaderProvider.GetForParent(workItem, Factory);
			if (jobHeader != null)
			{
				jobHeader.NudgeUp();
			}
		}

		bool IsWorkItemChangeReadyForUpgrade(WorkItem workItem)
		{
			if (workItem == null || workItem.IsClosedOrCancelled)
			{
				return false;
			}

			if (workItem is NewWorkItem newWorkItem)
			{
				var checkInTaskCheckResult =  newWorkItem.HasClosedShelfCheckInTasksForReleaseRing(ClientReleaseRing) && !newWorkItem.HasOpenedShelfCheckInTasksForReleaseRing(ClientReleaseRing);

				if (checkInTaskCheckResult)
				{
					var otherWorkItems = RelatedWorkItems.Where(wi => wi.Number != newWorkItem.Number);
					return otherWorkItems.All(wi => !wi.HasOpenedShelfCheckInTasksForReleaseRing(ClientReleaseRing));
				}
			}

			return false;
		}

		void IWorkItemRelatedItem.OnWorkItemRemoved(WorkItem workItem)
		{
			if (!IsDeleted && !ShouldStayIndependent)
			{
				if (RelatedWorkItems.Count == 0)
				{
					if (IM_Category != SupportIncidentCategoriesList.Codes.Support)
					{
						IM_Status = (ZString)SupportIncidentLookups.Status.Working;
						IM_ResolutionCode = (ZString)SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;

						if (WorkflowItems.Count > 0)
						{
							CalculateWorkflowDependentProperties();
						}
					}
				}
				else if (workItem.WKI_Status != ProcessTaskStatusCodeList.Codes.Closed && workItem.WKI_Status != ProcessTaskStatusCodeList.Codes.Cancelled)
				{
					((IWorkItemRelatedItem)this).OnRelatedWorkItemClosed(workItem);
				}
			}
		}

		ControllerID IWorkTaskRelatedItem.ControllerID
		{
			get { return ClientControllerRegistration.SupportIncident; }
		}

		public ZString Criticality
		{
			get { return IM_Priority; }
		}

		ZPropertyInfo IWorkTaskRelatedItem.CriticalityInfo
		{
			get { return IM_PriorityInfo; }
		}

		public ZString Source
		{
			get { return IM_Source; }
		}

		ZPropertyInfo IWorkTaskRelatedItem.SourceInfo
		{
			get { return IM_SourceInfo; }
		}

		public ZBool IsClosedOrCancelled
		{
			get { return IM_Status == SupportIncidentLookups.Status.Closed; }
		}

		protected override string GetSelectionCriterion3Core() => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.ModuleListAllModules, IM_Module);

		#endregion

		#region Send Automated Message To Client System

		#region Conditions

		public bool ClientSupportsCr8Cr9
		{
			get
			{
				return IM_Product != ProductTypes.Codes.Enterprise
					|| Database == null
					|| Database.VersionSupportsCr8Cr9;
			}
		}

		public bool ClientSystemSupportsSendingIncidentEmailFromClient
		{
			get { return (Database != null && Database.VersionCanSupportSendIncidentEmailFromClient == TriState.True) || ForceSendingIncidentEmailFromClient; }
		}

		public bool ForceSendingIncidentEmailFromClient;

		public bool IsWebRequest
		{
			get
			{
				return Request?.INC_SystemCreateUser.ToString() == User.WebUserCode
					|| (Database?.VersionHasGlowERequests ?? TriState.False) == TriState.True;
			}
		}

		public bool ClientSystemSupportsBiDirectionUpdate
		{
			get { return (Database != null && Database.VersionCanSupportBiDirectionIncidentMessage == TriState.True); }
		}

		public bool IsCreatingFromEdiProd
		{
			get { return IsNewIncident && IM_ClientIncidentReference.IsEmpty; }
		}

		public bool EConversationHasChanges
		{
			get { return eConversation != null && eConversation.HasChanges; }
		}

		#endregion

		#region Customer System Status

		void UpdateRequestStatus()
		{
			var result = new SupportIncidentStatusSnapShot();
			result.TakeSnapShot(this, includeIncidentDetails: false);
			IM_RequestStatus = GetCustomerSystemStatusCodeV2(result);
		}

		public ZString GetCurrentCustomerSystemStatusCode()
		{
			SupportIncidentStatusSnapShot result = new SupportIncidentStatusSnapShot();
			result.TakeSnapShot(this, false);
			return GetCustomerSystemStatusCode(result);
		}

		public ZString GetOriginalCustomerSystemStatusCode()
		{
			return OriginalSnapShot != null ? GetCustomerSystemStatusCode(OriginalSnapShot) : ZString.Empty;
		}

		public static ZString GetCustomerSystemStatusCodeV2(SupportIncidentStatusSnapShot snapShot)
		{
			ZString result;
			if (snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.ClosedAwaitingResponse;
			}
			else if (snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.Closed;
			}
			else if (snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Resolved)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.Resolved;
			}
			else if (snapShot.IM_Status == SupportIncidentLookups.Status.Closed && snapShot.IM_Category != SupportIncidentCategoriesList.Codes.FeatureRequest)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.Closed;
			}
			else
			{
				switch (snapShot.IM_Category)
				{
					case SupportIncidentCategoriesList.Codes.Defect:
					case SupportIncidentCategoriesList.Codes.ContentDevelopment:
						result = SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam;
						break;

					case SupportIncidentCategoriesList.Codes.FeatureRequest:
						result = GetCustomerSystemFeatureRequestStatusCodeV2(snapShot);
						break;

					default:
						result = SupportIncidentLookups.LegacyStatusCodes.SupportTeam;
						break;
				}
			}

			return result;
		}

		public static ZString GetCustomerSystemStatusCodeV1(SupportIncidentStatusSnapShot snapShot)
		{
			ZString result = ZString.Empty;

			if (snapShot.IM_Status == SupportIncidentLookups.Status.Closed && snapShot.IM_Category != SupportIncidentCategoriesList.Codes.FeatureRequest)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.Closed;
			}
			else
			{
				switch (snapShot.IM_Category)
				{
					case SupportIncidentCategoriesList.Codes.Defect:
					case SupportIncidentCategoriesList.Codes.ContentDevelopment:
						result = SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam;
						break;

					case SupportIncidentCategoriesList.Codes.FeatureRequest:
						result = GetCustomerSystemFeatureRequestStatusCodeV1(snapShot);
						break;

					default:
						result = SupportIncidentLookups.LegacyStatusCodes.SupportTeam;
						break;
				}
			}

			return result;
		}

		public ZString GetCustomerSystemStatusCode(SupportIncidentStatusSnapShot snapShot)
		{
			ZString result = ZString.Empty;

			if (ClientSystemSupportsBiDirectionUpdate)
			{
				result = GetCustomerSystemStatusCodeV2(snapShot);
			}
			else
			{
				result = GetCustomerSystemStatusCodeV1(snapShot);
			}

			return result;
		}

		static ZString GetCustomerSystemFeatureRequestStatusCodeV2(SupportIncidentStatusSnapShot snapShot)
		{
			ZString result = ZString.Empty;

			if (snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.FeatureAccepted)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.FeatureRequestAccepted;
			}
			else if (snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.DevelopmentEstimateProvided;
			}
			else if (snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided
				|| snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.FormalQuotationExpired)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.FormalQuotationProvided;
			}
			else if (snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.DevelopmentEstimateRequested;
			}
			else if (snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.FormalQuotationRequested;
			}
			else if (snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.FormalQuotationAccepted)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.FormalQuotationAccepted;
			}
			else if (snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.FormalQuotationDeclined)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.FormalQuotationDeclined;
			}
			else if (snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated
				|| snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade
				|| snapShot.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam;
			}
			else if (snapShot.IM_Status == SupportIncidentLookups.Status.Closed)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.Closed;
			}
			else
			{
				result = SupportIncidentLookups.LegacyStatusCodes.PendingFeatureResult;
			}
			return result;
		}

		static ZString GetCustomerSystemFeatureRequestStatusCodeV1(SupportIncidentStatusSnapShot snapShot)
		{
			ZString result = ZString.Empty;

			if (snapShot.IM_Status == SupportIncidentLookups.Status.Closed)
			{
				result = SupportIncidentLookups.LegacyStatusCodes.Closed;
			}
			else
			{
				result = SupportIncidentLookups.LegacyStatusCodes.PendingFeatureResult;
			}
			return result;
		}

		#endregion

		#endregion

		#region ISendEmailSource Members

		string ISendEmailSource.EmailSubject
		{
			get { return "Customer Service Incident: " + ((IWorkItemRelatedItem)this).Number; }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return EDIMailTemplateCategoryList.Codes.CustomerService; }
		}

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();
			result.AddRecipient(Client);
			result.AddRecipient(Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, IM_SystemCreateUser));
			result.AddRecipient(CustServiceContact);
			result.AddRecipient(AssignedToCurrent);
			return result;
		}

		string ISendEmailSource.DefaultFromDisplayName
		{
			get { return SupportDisplayName; }
		}

		string ISendEmailSource.OverridingDefaultFromEmailAddress
		{
			get { return SupportEmailAddress; }
		}

		Type ISendEmailSource.DocWrapperType
		{
			get { return null; }
		}

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
				customBusinessObject = new CustomBusinessObject(Factory, this, properties);
			}

			return customBusinessObject;
		}

		CustomBusinessObject customBusinessObject;

		#endregion

		#region IWorkflowProvider Members

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			// Must match SupportIncidentFormCustomisationSettingsProvider.GetPropertiesThatAffectWorkflow
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, OrgPkForTemplateSelection, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, IM_Product, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, IM_Category, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, IM_Source, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType4, ProductArea, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType5, IM_Language, ZString.Empty);
			return result;
		}

		ZGuid OrgPkForTemplateSelection
		{
			get
			{
				ZGuid result = IM_OH_Client;
				if (((IIncidentEventConsumer)this).IsMatchingEventTemplates)
				{
					var templateOrg = Factory.Load<OrgHeader>(EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.Value);
					if (templateOrg != null)
					{
						result = templateOrg.PK;
					}
				}
				return result;
			}
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public SupportIncidentProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new SupportIncidentProcessTaskCollection(this));
					workflowItems.Tasks.CountChanged += WorkflowItems_CountChanged;
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		void WorkflowItems_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (workflowItems != null && !workflowItems.IsLoading && !this.HasContext(SupportIncident.Context.ApplyingWorkflowTemplates))
			{
				CalculateWorkflowDependentProperties();
			}
		}

		public ZString CurrentTaskStatus
		{
			get { return CurrentTask != null ? CurrentTask.P9_Status : ZString.Empty; }
		}

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		public ZString WorkflowType
		{
			get { return EDIJobInvoicingConsumerTypes.Incident.Code; }
		}

		SupportIncidentProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		public ZString CurrentTaskDescriptionLabelText
		{
			get
			{
				return IM_Status != SupportIncidentLookups.Status.Closed ? "Current Task:" : "";
			}
		}

		public event EventHandler OnCurrentOrNextTaskStatusChange;
		public void TriggerTaskStatusChangeEvent()
		{
			if (OnCurrentOrNextTaskStatusChange != null)
			{
				OnCurrentOrNextTaskStatusChange(this, EventArgs.Empty);
			}
		}

		IDisposable SetIgnoreHasChangesForWorkflowTemplateApplication()
		{
			if (IgnoreHasChangesForWorkflowTemplateApplication)
			{
				return null;
			}

			IgnoreHasChangesForWorkflowTemplateApplication = true;
			return new DisposableAction(() => IgnoreHasChangesForWorkflowTemplateApplication = false);
		}
		bool IgnoreHasChangesForWorkflowTemplateApplication { get; set; }

		#endregion

		#region IIncidentEventConsumer Members

		BusinessObjectFactory IIncidentEventConsumer.Factory
		{
			get { return Factory; }
		}

		bool IIncidentEventConsumer.IsMatchingEventTemplates { get; set; }

		#endregion

		#region IARInvoiceSavingNotificationSubscriber Members

		ZString IARInvoiceSavingNotificationSubscriber.Identifier
		{
			get { return IM_IncidentNumber; }
		}

		ZString IARInvoiceSavingNotificationSubscriber.HTMLLinkForDirectOpen
		{
			get { return "<a href=\"" + ObjectFactory.Get<IShowEditFormUrlCreator>().CreateWithoutApplicationContext(ClientControllerRegistration.SupportIncident, PK) + "\">" + IM_IncidentNumber + "</a>"; }
		}

		ZString IARInvoiceSavingNotificationSubscriber.Description
		{
			get { return IM_Description; }
		}

		ZString IARInvoiceSavingNotificationSubscriber.ClientCode
		{
			get { return Client != null ? Client.OH_Code : ZString.Empty; }
		}

		ZString IARInvoiceSavingNotificationSubscriber.ClientName
		{
			get { return Client != null ? Client.OH_FullName : ZString.Empty; }
		}

		string[] IARInvoiceSavingNotificationSubscriber.GetRecipientsForNotification()
		{
			List<string> recipients = new List<string>();

			GlbStaff groupManager = GetFeatureRequestNotificationGroupManager();
			if (groupManager != null && !groupManager.GS_EmailAddress.IsEmpty)
			{
				recipients.Add(groupManager.GS_EmailAddress);
			}

			return recipients.ToArray();
		}

		GlbStaff GetFeatureRequestNotificationGroupManager()
		{
			GlbStaff result = null;
			GlbGroup featureRequestNotificationGroup = Factory.Load<GlbGroup>(EDIDataRegistry.Instance.IncidentFeatureRequestGroupENT.Value);
			if (featureRequestNotificationGroup != null)
			{
				result = featureRequestNotificationGroup.FindManager();
			}
			return result;
		}

		#endregion

		#region IWorkTaskRelatedItemSource Members

		public IEnumerable<WorkTaskRelatedItemModuleInfo> SupportedRelatedItemModules
		{
			get
			{
				yield return EDIWorkTaskRelatedItemModuleInfo.Issue(Factory);
				yield return EDIWorkTaskRelatedItemModuleInfo.NewWorkItem(Factory, allowNew: true, GetLinkedToWorkItemAsDefectCausingWorkItemQuery());
				yield return EDIWorkTaskRelatedItemModuleInfo.EDIProject(Factory);
				yield return EDIWorkTaskRelatedItemModuleInfo.GenericIncident(Factory, allowAttach: true, allowNew: true);
				yield return EDIWorkTaskRelatedItemModuleInfo.Opportunity(Factory, allowNew: true, this);
			}
		}

		public void PopulateNewRelatedItem(string relatedItemType, IWorkTaskRelatedItem relatedItem)
		{
			if (relatedItemType == ProcessManagement.Business.WorkTaskRelatedItemTypes.Project)
			{
				EDIProject project = (EDIProject)relatedItem;
				if (Client != null && Client.MainAddress != null)
				{
					project.WKP_OA_ClientAddress = Client.MainAddress.PK;
				}
				project.WKP_OC_Contact = IM_OC_Contact;
				project.WKP_SubType = IM_Product;
				if (ClientCompany != null && ClientCompany.UsageOwnerLicence != null)
				{
					project.LicenceHeaderPK = ClientCompany.UsageOwnerLicence.PK;
				}
			}
			if (relatedItemType == ProcessManagement.Business.WorkTaskRelatedItemTypes.Opportunity)
			{
				EDIOrgOpportunity opportunity = (EDIOrgOpportunity)relatedItem;
				opportunity.P8_OH = IM_OH_Client;
			}
		}

		public ZBool ShouldAddRelatedItemAsParent { get; set; }

		#endregion

		#region IDocManagerSupport/IEDocsPluginHostDecider Members

		public override DocManagerInfo DocManagerInfo
		{
			get { return Request.DocManagerInfo; }
		}

		IBusiness IEDocsPluginHostDecider.HostBusinessEntity => Request;

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return EDIRelatableActivityTypeList.Codes.Incident; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return Client; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return IM_OH_ClientInfo.HasChanges; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return Contact; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return IM_OC_ContactInfo.HasChanges; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { IM_Priority, IM_Description, Client?.OH_Code ?? string.Empty, IM_Status }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
			/* During the saving, the data source "StmALog" will be changed to LogCopy which cannot be seen as a normal data source
			 * So ATC events must be added after saving
			 */
			CurrentActivityObjectDuringSaving = null;
			if (relatedActivity is OrgSalesCall call)
			{
				if (!call.IsDeleted && !this.IsDeleted)
				{
					CurrentActivityObjectDuringSaving = relatedActivity;
				}
			}
		}

		IRelatableActivity CurrentActivityObjectDuringSaving { get; set; }

		void AddATCEventForCommunication(IRelatableActivity relatedActivity)
		{
			if (relatedActivity is OrgSalesCall call)
			{
				var communicationCode = (relatedActivity as ICodeDescription)?.Code ?? string.Empty;
				var parameters = new Dictionary<string, string>
				{
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber] = IM_IncidentNumber,
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = relatedActivity.ActivityType,
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber] = communicationCode,
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description] = "Communication " + communicationCode + " attached to Incident " + IM_IncidentNumber,
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Mode] = call.OQ_TypeOfCall,
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason] = call.OQ_Category
				};
				var eventValue = new EventValue(AutoEvents.Attached, eventTime: ZDateTimeOffset.Now, reference: "", parameters: parameters);
				Logs.AddNew(eventValue);
			}
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		ZDateTime IAuditDetails.SystemCreateTimeUtc
		{
			get { return IM_SystemCreateTimeUtc; }
		}

		ZString IAuditDetails.SystemCreateUser
		{
			get { return IM_SystemCreateUser; }
		}

		ZDateTime IAuditDetails.SystemLastEditTimeUtc
		{
			get { return IM_SystemLastEditTimeUtc; }
		}

		ZString IAuditDetails.SystemLastEditUser
		{
			get { return IM_SystemLastEditUser; }
		}

		public OrgSalesCallCollection SupportIncidentRelatedCommunicationsCollection
		{
			get
			{
				var query = new ZQuery(OrgSalesCallSchema.PK, RelatedChildActivityPivotCollection.Select(x => x.RAP_ChildActivityID));
				return new OrgSalesCallCollection(Factory, query);
			}
		}

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		#region Related Project

		public EDIProject RelatedProject
		{
			get
			{
				return (EDIProject)RelatedItems.Cast<IWorkTaskRelatedItem>().FirstOrDefault(x => x.Type == ProcessManagement.Business.WorkTaskRelatedItemTypes.Project);
			}
		}

		public EDIProject RelatedProjectReadOnly
		{
			get
			{
				return (EDIProject)RelatedWorkTaskItemsReadOnly.Cast<IWorkTaskRelatedItem>().FirstOrDefault(x => x.Type == ProcessManagement.Business.WorkTaskRelatedItemTypes.Project);
			}
		}

		[List("Lookups.Projects")]
		public ZGuid RelatedProjectPK
		{
			get
			{
				if (!isRelatedProjectLoaded)
				{
					var relatedProject = RelatedProject;
					relatedProjectPK = relatedProject != null ? relatedProject.PK : ZGuid.Empty;
					isRelatedProjectLoaded = true;
				}
				return relatedProjectPK;
			}
			set
			{
				if (!relatedProjectPK.IsEmpty && value != relatedProjectPK)
				{
					DetachProject(relatedProjectPK);
				}

				if (!value.IsEmpty)
				{
					AttachProject(value);
				}

				SetNonPersistentPropertyValue(RelatedProjectPKInfo, ref relatedProjectPK, value);
			}
		}
		ZGuid relatedProjectPK;
		bool isRelatedProjectLoaded;

		public ZPropertyInfo RelatedProjectPKInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedProjectPK), "Related Project"); }
		}

		void AttachProject(ZGuid projectPK)
		{
			EDIProject relatedProject = Factory.Load<EDIProject>(projectPK);
			if (relatedProject != null)
			{
				RelatedItems.Add(relatedProject);
				RelatedProject.PopulateRelatedIncidentMain(this, false);
			}
		}

		void DetachProject(ZGuid projectPK)
		{
			RelatedItems.Remove(projectPK);
			IM_GS_NKSpecifiedBy = ZString.Empty;
		}

		public bool IsProjectRelatedIncident
		{
			get { return IM_Source == SupportIncidentLookups.SourceListConstants.CreatedFromProject || !RelatedProjectPK.IsEmpty; }
		}

		public GlbStaff BusinessConsultant
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, IM_GS_NKSpecifiedBy); }
		}

		#endregion

		#region Calculate Work Item Dependent Status and Dispostion

		void CalculateWorkItemDependentStatusAndDisposition()
		{
			if (IsGroupControlled || ShouldStayIndependent)
			{
				return;
			}

			if (RelatedWorkItems.Count > 0 && !WorkitemExcludeDispositionSet.Contains(IM_ResolutionCode))
			{
				var workItems = RelatedWorkItems.Cast<NewWorkItem>();
				var anyNonClosedWorkItems = workItems.Any(wi => wi.WKI_Status != ProcessTaskStatusCodeList.Codes.Closed && wi.WKI_Status != ProcessTaskStatusCodeList.Codes.Cancelled);

				if (anyNonClosedWorkItems)
				{
					//all work items have checked in the ring fix, it's ok to wait/deliver upgrade.
					if (IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade || IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered
						|| IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed
						|| ((IM_ResolutionCode == DispositionList.Constants.Closed.ResolvedAndClosed || IM_ResolutionCode == DispositionList.Constants.Closed.Resolved) && (IM_ClosureResolution == DispositionList.Constants.Closed.UpgradeDelivered)))
					{
						if (!workItems.Any(x => x.HasOpenedShelfCheckInTasksForReleaseRing(ClientReleaseRing)) &&
							workItems.Any(x => x.HasClosedShelfCheckInTasksForReleaseRing(ClientReleaseRing)))
						{
							return;
						}
					}

					SetWorkItemCreated();
				}
				else if (IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated)
				{
					IM_ResolutionCode = string.Empty;
				}
			}
		}

		HashSet<ZString> WorkitemExcludeDispositionSet
		{
			get
			{
				if (workitemExcludeDispositionSet == null)
				{
					workitemExcludeDispositionSet = new HashSet<ZString>();
					workitemExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate);
					workitemExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation);
					workitemExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided);
					workitemExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided);
				}
				return workitemExcludeDispositionSet;
			}
		}
		HashSet<ZString> workitemExcludeDispositionSet;

		#endregion

		#region Calculate Workflow Properties

		public void CalculateWorkflowDependentProperties(bool shouldOverrideDisposition = false)
		{
			if (IsGroupControlled)
			{
				return;
			}

			if (WorkflowItems.Count > 0)
			{
				CalculateStatusAndDisposition(shouldOverrideDisposition: shouldOverrideDisposition);
				CalculateAssignedStaff();
			}
		}

		public ZBool IncidentEventFactoryDisabled
		{
			get
			{
				return incidentEventFactoryDisabled;
			}
		}

		public IDisposable DisableIncidentEventFactory()
		{
			incidentEventFactoryDisabled = true;
			return new DisposableAction(delegate
				{
					incidentEventFactoryDisabled = false;
				}
			);
		}
		bool incidentEventFactoryDisabled;

		public IDisposable SuspendCalculateStatusAndDisposition()
		{
			suspendCalculateStatusAndDisposition = true;
			return new DisposableAction(delegate
			{
				suspendCalculateStatusAndDisposition = false;
				CalculateWorkflowDependentProperties();
			}
			);
		}
		bool suspendCalculateStatusAndDisposition;

		void CalculateStatusAndDisposition(bool shouldOverrideDisposition)
		{
			if (!suspendCalculateStatusAndDisposition)
			{
				var currentTask = CurrentTask;
				var isClosedOrAwaitingClientResponseDisposition = IsClosedDisposition(IM_ResolutionCode) || IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
				var shouldCalculateDisposition = (shouldOverrideDisposition || IsUpdatingStage || !isClosedOrAwaitingClientResponseDisposition) && (!IsStageBeingToContentDevelopment && !IsGroupControlled);

				if (WorkflowItems.AllTasksClosedOrCancelled)
				{
					if (IM_Status != SupportIncidentLookups.Status.Closed)
					{
						var shouldUpdateToCancelledDisposition = WorkflowItems.AllTasksCancelled || IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled;

						var calculatedDisposition = shouldUpdateToCancelledDisposition ? SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled : SupportIncidentLookups.DispositionList.Constants.Closed.Completed;
						SetCalculatedStatusAndDisposition(SupportIncidentLookups.Status.Closed, shouldCalculateDisposition, calculatedDisposition);
					}

					return;
				}

				if (currentTask != null && (currentTask.AssignedStaffMember != null || currentTask.RequiredCapability != null))
				{
					if (currentTask.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned)
					{
						if (WorkflowItems.AnyTaskIsWorkingOrCompleted)
						{
							SetCalculatedStatusAndDisposition(SupportIncidentLookups.Status.Working, shouldCalculateDisposition, SupportIncidentLookups.DispositionList.Constants.Working.Assigned);
						}
						else
						{
							SetCalculatedStatusAndDisposition(SupportIncidentLookups.Status.Open, shouldCalculateDisposition, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction);
						}
					}
					else if (currentTask.P9_Status == ProcessTaskStatusCodeList.Codes.Working)
					{
						SetCalculatedStatusAndDisposition(SupportIncidentLookups.Status.Working, shouldCalculateDisposition, SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress);
					}
					else if (currentTask.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended)
					{
						SetCalculatedStatusAndDisposition(SupportIncidentLookups.Status.Suspended, shouldCalculateDisposition, SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred);
					}
				}
				else
				{
					SetCalculatedStatusAndDisposition(SupportIncidentLookups.Status.Open, shouldCalculateDisposition, SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment);
				}
			}
		}

		void SetCalculatedStatusAndDisposition(ZString calculatedStatus, ZBool shouldSetCalculatedDisposition, ZString calculatedDisposition)
		{
			if ((IM_ResolutionCode != SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated
				&& IM_ResolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade
				&& IM_ResolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed) || IM_Category == SupportIncidentCategoriesList.Codes.ContentDevelopment) //If disposition is 'Waiting For Upgrade' but is changed then auto upgrade service task will not pick up the incident
			{
				IM_Status = calculatedStatus;

				if (!WorkflowExcludeDispositionSet.Contains(IM_ResolutionCode)
					&& shouldSetCalculatedDisposition)
				{
					if (IM_Status == SupportIncidentLookups.Status.Closed && SuspendTriggerCloseIncident)
					{
						return;
					}

					if (Lookups.StatusDispositionList[calculatedDisposition] is IncidentClosureDisposition)
					{
						CloseIncident(calculatedDisposition, string.Empty);
					}
					else
					{
						IM_ResolutionCode = calculatedDisposition;
					}
				}
			}
		}

		HashSet<ZString> WorkflowExcludeDispositionSet
		{
			get
			{
				if (workflowExcludeDispositionSet == null)
				{
					workflowExcludeDispositionSet = new HashSet<ZString>();
					workflowExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate);
					workflowExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation);
					workflowExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided);
					workflowExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided);
					workflowExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.FormalQuotationAccepted);
					workflowExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.FormalQuotationDeclined);
					workflowExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.FormalQuotationExpired);
					WorkflowExcludeDispositionSet.Add(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed);
				}
				return workflowExcludeDispositionSet;
			}
		}
		HashSet<ZString> workflowExcludeDispositionSet;

		public HashSet<ZString> NonTriageOverridableCriticalities
		{
			get
			{
				if (nonTriageOverridableCriticalities == null)
				{
					nonTriageOverridableCriticalities = new HashSet<ZString>();
					nonTriageOverridableCriticalities.Add(Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown);
					nonTriageOverridableCriticalities.Add(Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown);
					nonTriageOverridableCriticalities.Add(Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround);
					nonTriageOverridableCriticalities.Add(Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround);
					nonTriageOverridableCriticalities.Add(Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR5_Training);
				}
				return nonTriageOverridableCriticalities;
			}
		}
		HashSet<ZString> nonTriageOverridableCriticalities;

		public bool IsClosedDisposition(ZString disposition)
		{
			if (disposition.IsEmpty)
			{
				return false;
			}

			if (disposition.EqualsIgnoringCase(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved) || disposition.EqualsIgnoringCase(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed))
			{
				return true;
			}

			return Lookups.StatusDispositionList[disposition] is IncidentClosureDisposition;
		}

		void CalculateAssignedStaff()
		{
			ProcessTask currentTask = CurrentTask;
			ZString assignedStaff = ZString.Empty;
			if (currentTask != null)
			{
				assignedStaff = currentTask.AssignedStaffMember != null ? currentTask.AssignedStaffMember.GS_Code : ZString.Empty;
			}

			if (IM_Category == SupportIncidentCategoriesList.Codes.Support)
			{
				IM_GS_NKCustServiceContact = assignedStaff;
			}
			else
			{
				IM_GS_NKAssignedToCurrent = assignedStaff;
			}
		}

		void CloseTasks(string closeDisposition, bool isAwaitingClient)
		{
			using (SuspendCalculateStatusAndDisposition())
			{
				var tasksToCloseOrCancel = WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>()
					.Where(task => task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled && task.P9_Status != ProcessTaskStatusCodeList.Codes.Closed)
					.ToArray();

				var incorrectStageDispositions = new HashSet<string>
				{
					SupportIncidentLookups.DispositionList.Constants.Closed.NotFeatureRequest,
					SupportIncidentLookups.DispositionList.Constants.Closed.NotComplianceRequirement,
					SupportIncidentLookups.DispositionList.Constants.Closed.NotCustomerServiceRequest
				};

				foreach (SupportIncidentProcessTask task in tasksToCloseOrCancel)
				{
					if (closeDisposition == SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled || incorrectStageDispositions.Contains(closeDisposition))
					{
						task.CancelAndSuspendValidationOnTaskCancellation();
					}
					else if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Working || task.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended)
					{
						if (task.P9_GS_NKAssignedStaffMember.IsEmpty && task.P9_GG_AssignedGroup.IsEmpty)
						{
							task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
						}
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
					else
					{
						task.CancelAndSuspendValidationOnTaskCancellation();
					}

					if (isAwaitingClient)
					{
						task.Logs.AddNew(Events.StatusChange, SupportIncidentProcessTask.AwaitingClientResponseLogReference);
					}
				}
			}
		}

		public void SuspendPopulatingWorkflowTemplate()
		{
			shouldPopulateWorkflowTemplate = false;
			suspendCalculateStatusAndDisposition = true;
		}

		public void ResumePopulatingWorkflowTemplate()
		{
			shouldPopulateWorkflowTemplate = true;
			suspendCalculateStatusAndDisposition = false;
		}

		bool shouldPopulateWorkflowTemplate = true;

		void RemoveNonPersistentTasks()
		{
			if (!shouldPopulateWorkflowTemplate)
			{
				using (SuspendCalculateStatusAndDisposition())
				{
					foreach (ProcessTask task in WorkflowItems.ToArray())
					{
						if (!task.IsInDatabase)
						{
							task.Delete();
						}
					}
				}
			}
		}

		#endregion

		#region Request

		public EdiIncidentRequest Request
		{
			get
			{
				if (request == null)
				{
					EnsureRequestCreated();
					if (request == null)
					{
						request = Factory.Load<EdiIncidentRequest>(IM_INC_Request);
					}

					RegisterEditableChildObject(request);
				}

				return request;
			}
		}
		EdiIncidentRequest request;

		void EnsureRequestCreated()
		{
			if (request == null && IM_INC_Request.IsEmpty)
			{
				request = Factory.New<EdiIncidentRequest>();
				using (SuspendSettingHasChanges())
				{
					IM_INC_Request = request.PK;
				}

				_ = EConversation.Conversation;
			}
		}

		void UpdateRequest()
		{
			UpdateRequestStatus();
			var req = Request;
			req.INC_Status = IM_RequestStatus;
			req.INC_Criticality = IM_Priority;
			req.INC_OC_ReportedBy = IM_OC_Contact;
			req.INC_Type = IM_Product;
			req.INC_SubType = IM_Module;
			req.INC_ServiceType = IM_ServiceType;

			if (req.INC_Summary.IsEmpty)
			{
				req.INC_Summary = IM_Description;
				req.INC_Details = DetailNoteText;
			}
			if (!IsWebRequest)
			{
				req.INC_IsCustomerResolved = req.INC_Status == SupportIncidentLookups.LegacyStatusCodes.Closed;
			}
			if (req.INC_RN_NKCountry.IsEmpty)
			{
				req.INC_RN_NKCountry = IM_RN_NKCountry;
			}

			if (IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse)
			{
				var mostRecentIWRLog = GetMostRecentLogByEventCode(Events.IncidentAwaitingResponseCode);
				if (mostRecentIWRLog != null)
				{
					req.INC_ExpiryCountdownStartTimeUtc = mostRecentIWRLog.SL_EventTimeUtc;
				}
			}
			else if (IsClosedDisposition(IM_ResolutionCode))
			{
				req.INC_ExpiryCountdownStartTimeUtc = IM_ResolveTimeUtc;
			}
		}

		public StmALog GetMostRecentLogByEventCode(string eventCode)
		{
			var eventCodes = new List<string> { eventCode };
			return GetMostRecentLogByEventCodes(eventCodes);
		}

		public StmALog GetMostRecentLogByEventCodes(IEnumerable<string> eventCodes)
		{
			var mostRecentLogQuery = new ZQuery(StmALogSchema.SL_Parent, PK);
			mostRecentLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCodes);
			mostRecentLogQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			var mostRecentLog = Factory.LoadTop1<StmALog>(mostRecentLogQuery);
			return mostRecentLog;
		}

		public StmALog[] GetLogsByEventCode(string eventCode)
		{
			var logQuery = new ZQuery(StmALogSchema.SL_Parent, PK);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
			logQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			var logs = Factory.Load<StmALog>(logQuery);
			return logs;
		}

		#endregion

		#region eConversation

		#region Add Message

		public static IDisposable SwitchToNewUserTemporarily(string loginName)
		{
			return SwitchToNewUserTemporarily(loginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
		}

		public static IDisposable SwitchToNewUserTemporarily(string loginName, Guid branchPK, Guid departmentPK)
		{
			return EnvProxy.Instance.SetTemporaryUserContext(loginName, branchPK, departmentPK);
		}

		public void AddMessageFromSupport(ZString text, bool shouldAddMessageSentEvent = true)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				EConversation.AddMessageFromSupport(text, isInternal: false, isSystem: false, kind: SupportIncidentEConversation.LocalMessageKind.ForCustomer, shouldAddMessageSentEvent: shouldAddMessageSentEvent);
			}
		}

		/// <summary>
		/// Add an eConversation message from the logged-in staff that needs to be sent to the customer.
		/// They will receive a notification via email.
		/// </summary>
		public void AddStaffMessageToCustomer(ZString text, bool shouldAddMessageSentEvent = true)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				EConversation.AddMessageFromCurrentUser(text, isInternal: false, isSystem: false, kind: SupportIncidentEConversation.LocalMessageKind.ForCustomer, shouldAddMessageSentEvent: shouldAddMessageSentEvent);
			}
		}

		/// <summary>
		/// Add the message from staff when incident is resolved.
		/// </summary>
		[SupportIncidentSendingConversationMethod]
		public void AddResolutionMessageToCustomer(ZString text)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				EConversation.AddMessageFromCurrentUser(text, isInternal: false, isSystem: false, kind: SupportIncidentEConversation.LocalMessageKind.Resolution);
			}
		}

		/// <summary>
		/// Add an eConversation message from the system that needs to be sent to the customer.
		/// They will receive a notification via email.
		/// </summary>
		[SupportIncidentSendingConversationMethod]
		public void AddSystemMessageToCustomer(ZString text)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				EConversation.AddMessageFromCurrentUser(text, isInternal: false, isSystem: true, kind: SupportIncidentEConversation.LocalMessageKind.ForCustomer);
			}
		}

		/// <summary>
		/// Add public eConversation message for logging purposes.
		/// It doesn't require an email notification to the customer.
		/// They can read it in their version of the incident, if they have one, if they are interested.
		/// </summary>
		[SupportIncidentSendingConversationMethod]
		public void AddPublicSystemLogMessage(ZString comment)
		{
			if (!string.IsNullOrWhiteSpace(comment))
			{
				EConversation.AddMessageFromCurrentUser(comment, isInternal: false, isSystem: true, shouldAddMessageSentEvent: !IsGroupControlled);
			}
		}

		[SupportIncidentSendingConversationMethod]
		public void AddInternalSystemLogMessage(ZString comment)
		{
			if (!string.IsNullOrWhiteSpace(comment))
			{
				EConversation.AddMessageFromCurrentUser(comment, true, true);
			}
		}

		[SupportIncidentSendingConversationMethod]
		public void AddInternalMessage(ZString comment)
		{
			if (!string.IsNullOrWhiteSpace(comment))
			{
				EConversation.AddMessageFromCurrentUser(comment, true, false);
			}
		}

		#endregion

		public SupportIncidentEConversation EConversation
		{
			get
			{
				if (eConversation == null)
				{
					eConversation = new SupportIncidentEConversation(this);
					EnsureRequestCreated();
				}
				return eConversation;
			}
		}

		SupportIncidentEConversation eConversation;

		public void CreateClientCommunicationTaskIfNeeded()
		{
			var currentTask = CurrentTask;
			if (currentTask == null || currentTask.AssignedStaffMember == null || currentTask.AssignedStaffMember.GS_Code != GlbStaff.CurrentUser.GS_Code)
			{
				this.SuspendTriggerCloseIncident = true;
				var lastClosedOrCancelledTask = GetLastClosedOrCancelledTask();

				var newTask = WorkflowItems.Tasks.AddNew();
				newTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
				newTask.P9_Description = "Client Communication";
				newTask.P9_Type = "INV";
				newTask.TaskProperties.ActualDate = ZDateTimeOffset.Now.AddMinutes(-1);
				newTask.P9_CompletedTime = ZDateTimeOffset.Now;
				newTask.P9_ActualDuration = TimeSpan.FromMinutes(1);
				newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				newTask.P9_EstDuration = TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(0, 10, 0));

				SetNewTaskSequence(newTask, lastClosedOrCancelledTask);
				SetNewTaskProcessHeader(newTask, currentTask, lastClosedOrCancelledTask);

				void SetNewTaskSequence(ProcessTask newTask, ProcessTask lastClosedOrCancelledTask)
				{
					if (lastClosedOrCancelledTask == null)
					{
						newTask.P9_Sequence = WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().OrderBy(task => task.P9_Sequence).FirstOrDefault(task => task.PK != newTask.PK)?.P9_Sequence ?? 1;
					}
					else
					{
						newTask.P9_Sequence = lastClosedOrCancelledTask.P9_Sequence + 1;
					}
				}

				void SetNewTaskProcessHeader(ProcessTask newTask, ProcessTask currentTask, ProcessTask lastClosedOrCancelledTask)
				{
					if (currentTask?.ProcessHeader != null)
					{
						newTask.P9_FH_ProcessHeader = currentTask.ProcessHeader.PK;
					}
					else if (lastClosedOrCancelledTask?.ProcessHeader != null)
					{
						newTask.P9_FH_ProcessHeader = lastClosedOrCancelledTask.ProcessHeader.PK;
					}

					if (newTask.ProcessHeader == null)
					{
						var lastTask = WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().OrderBy(task => task.P9_Sequence).LastOrDefault(task => task.ProcessHeader != null);
						if (lastTask != null)
						{
							newTask.P9_FH_ProcessHeader = lastTask.ProcessHeader.PK;
						}
					}
				}

				this.SuspendTriggerCloseIncident = false;
			}
		}

		public ProcessTask GetLastClosedOrCancelledTask()
		{
			ProcessTask lastClosedOrCancelledTask = null;
			var orderedTasks = WorkflowItems.Tasks.Cast<ProcessTask>().OrderByDescending(task => task.P9_Sequence);
			foreach (var task in orderedTasks)
			{
				if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
				{
					lastClosedOrCancelledTask = task;
					break;
				}
				else if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled && lastClosedOrCancelledTask == null)
				{
					lastClosedOrCancelledTask = task;
				}
			}
			return lastClosedOrCancelledTask;
		}

		#endregion

		#region IConversationProvider

		JobConversation IConversationProvider.eConversation => EConversation.Conversation;
		ModuleIdentifier IConversationProvider.ParentModule => ClientModuleRegistration.SupportIncident;
		ControllerID IConversationProvider.ParentController => ClientControllerRegistration.SupportIncident;

		IEnumerable<EConversation.Business.RelatedParty> IConversationProvider.AdditionalParticipants
		{
			get { return Array.Empty<EConversation.Business.RelatedParty>(); }
		}

		bool IConversationProvider.SendEmailNotificationsOnSave => false;
		void IConversationProvider.RunConversationUpdateActionBeforeSaving()
		{
			return;
		}

		string IConversationProvider.EmailSubjectContentOverride => default;
		string IConversationProvider.FromAddressOverride => default;
		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => default;

		#endregion

		#region IConversationBroadcastRecipient

		void IConversationBroadcastRecipient.GenerateAndSendBroadcastEmailNotifications()
		{
			var messages = EConversationMessageProvider.GetNewMessages(EConversation.Conversation, false);
			(var email, var isAllowed) = SupportIncidentEmailQuickBuilder.CreateAwaitingResponseEmail(this, messages, true);
			if (email != null)
			{
				if (email.AllRecipients.Any())
				{
					email.SendEmail();
				}

				if (!email.AllRecipients.Any() && isAllowed)
				{
					var alreadyReceivedRecipients = email?.AllRecipients;
					SupportIncidentEConversationNotificationSender.GenerateAndQueueStaffOnlyBroadcastEmailNotifications(this, EConversation.Conversation, alreadyReceivedRecipients, messages);
				}
			}
		}

		JobConversation IConversationBroadcastRecipient.EConversation => EConversation.Conversation;

		#endregion

		#region IWorkTaskTreeNode Members

		public BusinessObjectCollection ChildrenOnlyRelatedItems
		{
			get
			{
				if (childrenOnlyRelatedItems == null)
				{
					childrenOnlyRelatedItems = new SupportIncidentRelatedItemGenPivotCollection(this, RelatedLinkType.MasterAlwaysParent);
					childrenOnlyRelatedItems.Load();
				}
				return childrenOnlyRelatedItems;
			}
		}
		WorkTaskRelatedItemGenPivotCollection childrenOnlyRelatedItems;

		public BusinessObjectCollection ParentsOnlyRelatedItems
		{
			get
			{
				if (parentsOnlyRelatedItems == null)
				{
					parentsOnlyRelatedItems = new SupportIncidentRelatedItemGenPivotCollection(this, RelatedLinkType.MasterAlwaysChild);
					parentsOnlyRelatedItems.Load();
				}
				return parentsOnlyRelatedItems;
			}
		}
		WorkTaskRelatedItemGenPivotCollection parentsOnlyRelatedItems;

		public ZDateTime AgreedDeliveryDate
		{
			get
			{
				if (jobHeaderDeliveryDate == null)
				{
					jobHeaderDeliveryDate = ProcessJobHeaderProvider.GetForParent(this, Factory, false);
				}
				return jobHeaderDeliveryDate?.AgreedDeliveryDateLocal ?? ZDateTime.Empty;
			}
		}
		IProcessJobHeader jobHeaderDeliveryDate;

		public ZString CurrentTaskDescription => CurrentTask?.P9_Description ?? ZString.Empty;

		public ZString CurrentTaskCapabilityCodeDescription
		{
			get
			{
				var result = ZString.Empty;
				var task = CurrentTask;
				if (task != null && task.RequiredCapability != null)
				{
					if (!string.IsNullOrEmpty(task.RequiredCapability.G4_Code))
					{
						result = task.RequiredCapability.G4_Code + " - ";
					}

					result += task.RequiredCapability.G4_Description;
				}

				return result;
			}
		}

		public ZString CurrentTaskAssigned => OverallAssignedToCode;

		public ZString SelectionCriterion1Code => IM_Product;

		public ZString SelectionCriterion2Code => Criticality;

		public ZString SelectionCriterion3Code => IM_Module;

		public ZString SelectionCriterion4Code => IM_ProgramArea;

		public ZString SelectionCriterion5Code => string.Empty;

		public void AddFetchHintsForOrgAddressIfRequired()
		{
		}

		public void AddFetchHintsForOrgHeaderIfRequired()
		{
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		bool IImportParentRelatedActivityInfoOnNew.ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (parentActivity is EDIOrgOpportunity parentOpportunity)
			{
				CreateOpportunityPivot(parentOpportunity);
			}

			return true;
		}

		public void CreateOpportunityPivot(EDIOrgOpportunity parentOpportunity)
		{
			var opportunityPivot = Factory.New<GenPivot>();
			opportunityPivot.XX_RelationType = Core.Constants.GenPivotTypes.Opportunity;
			opportunityPivot.XX_Relation1ID = parentOpportunity.PK;
			opportunityPivot.XX_Relation1TableCode = OrgOpportunitySchema.Constants.Prefix;
			opportunityPivot.XX_Relation2ID = PK;
			opportunityPivot.XX_Relation2TableCode = IncidentMainSchema.Constants.Prefix;
		}

		#endregion

		#region ImportChildInfo

		bool IImportChildRelatedActivityInfoOnAttach.ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (childActivity is OrgOpportunity childOpportunity)
			{
				DeleteGenPivotIfExists(childOpportunity.PK, OrgOpportunitySchema.Constants.Prefix, PK, IncidentMainSchema.Constants.Prefix);
				CreateGenPivot(PK, IncidentMainSchema.Constants.Prefix, childOpportunity.PK, OrgOpportunitySchema.Constants.Prefix);
			}
			return true;
		}

		bool IImportChildRelatedActivityInfoOnDetach.ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (childActivity is OrgOpportunity childOpportunity)
			{
				DeleteGenPivotIfExists(PK, IncidentMainSchema.Constants.Prefix, childOpportunity.PK, OrgOpportunitySchema.Constants.Prefix);
			}
			return true;
		}

		void CreateGenPivot(ZGuid relation1ID, string relation1TableCode, ZGuid relation2ID, string relation2TableCode)
		{
			var incidentPivot = Factory.New<GenPivot>();
			incidentPivot.XX_RelationType = Core.Constants.GenPivotTypes.Opportunity;
			incidentPivot.XX_Relation1ID = relation1ID;
			incidentPivot.XX_Relation1TableCode = relation1TableCode;
			incidentPivot.XX_Relation2ID = relation2ID;
			incidentPivot.XX_Relation2TableCode = relation2TableCode;
		}

		void DeleteGenPivotIfExists(ZGuid relation1ID, string relation1TableCode, ZGuid relation2ID, string relation2TableCode)
		{
			var query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.Opportunity);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, relation1ID);
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, relation1TableCode);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, relation2ID);
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, relation2TableCode);

			var genPivot = Factory.LoadTop1<GenPivot>(query);
			genPivot?.Delete();
		}

		#endregion

		#region AddOpportunityRelatedActivity

		void AddOpportunityRelatedActivity(EDIOrgOpportunity childOpportunity)
		{
			var relatedActivityPivot = Factory.New<ViewRelatedActivityPivot>();
			relatedActivityPivot.RAP_ParentActivityID = PK;
			relatedActivityPivot.RAP_ParentActivityTableCode = IncidentMainSchema.Constants.Prefix;
			relatedActivityPivot.RAP_ChildActivityID = childOpportunity.PK;
			relatedActivityPivot.RAP_ChildActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			relatedActivityPivot.RAP_IsEditable = true;
		}

		#endregion

		#region DeleteOpportunityRelatedActivity

		void DeleteOpportunityRelatedActivity(EDIOrgOpportunity opportunity, bool isOpportunityParent)
		{
			var relatedActivityQuery = new ZQuery();
			if (isOpportunityParent)
			{
				relatedActivityQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityID, opportunity.PK);
				relatedActivityQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityTableCode, OrgOpportunitySchema.Constants.Prefix);
				relatedActivityQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityID, PK);
				relatedActivityQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityTableCode, IncidentMainSchema.Constants.Prefix);
			}
			else
			{
				relatedActivityQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityID, PK);
				relatedActivityQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityTableCode, IncidentMainSchema.Constants.Prefix);
				relatedActivityQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityID, opportunity.PK);
				relatedActivityQuery.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityTableCode, OrgOpportunitySchema.Constants.Prefix);
			}
			var relatedActivityPivot = Factory.LoadTop1<ViewRelatedActivityPivot>(relatedActivityQuery);
			relatedActivityPivot?.Delete();
		}

		#endregion

		#region sync Deatils to eRequest
		public void SyncDetailsToeRequest()
		{
			if (Request != null)
			{
				Request.INC_Summary = IM_Description;
				Request.INC_Details = DetailNoteText;
			}
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			base.Logs.AddNew(Events.EditedARecord, "Incident details synced to eRequest");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			EConversation.AddMessageFromCurrentUser("eRequest edited by support", isInternal: false, isSystem: true);
		}
		#endregion

		#region Service Type

		[List("Lookups.ServiceTypeList")]
		[MaxLength(3)]
		public override ZString IM_ServiceType
		{
			get { return base.IM_ServiceType; }
			set
			{
				if (base.IM_ServiceType != value)
				{
					base.IM_ServiceType = value;
					HandleServiceTypeWorflow();
				}
			}
		}

		public ZString IM_ServiceTypeDescription
		{
			get { return Lookups.ServiceTypeList.GetDescriptionFromCode(IM_ServiceType); }
		}

		void HandleServiceTypeWorflow()
		{
			if (IsInDatabase
				&& (IM_Status != SupportIncidentLookups.Status.Closed || IM_CloseTimeUtc.IsEmpty)
				&& Validation.ValidateWorkflowServiceTypeProperties()
				&& !SupportIncidentEvent.TriggerLastEvent(this))
			{
				IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.ChangeProperty, this);
			}
		}

		#endregion

		#region Service Status

		[List("Lookups.ServiceStatusList")]
		[MaxLength(3)]
		public ZString ServiceStatus
		{
			get
			{
				return Lookups.ServiceStatusList.GetDescriptionFromCode(base.IM_ServiceStatus);
			}
			set
			{
				if (base.IM_ServiceStatus != value)
				{
					base.IM_ServiceStatus = value;
				}
			}
		}

		public ZPropertyInfo ServiceStatusInfo => GetZPropertyInfo(nameof(IM_ServiceStatus));

		#endregion

		#region Incident Management Group

		public IncidentManagementLink IncidentManagementLink
		{
			get
			{
				if (incidentManagementLink == null || incidentManagementLink.IsDeleted)
				{
					incidentManagementLink = Factory.LoadTop1<IncidentManagementLink>(new ZQuery(IncidentManagementLinkSchema.INL_IM_Incident, PK));
				}

				return incidentManagementLink;
			}
		}
		IncidentManagementLink incidentManagementLink;

		public bool IsGroupControlled
		{
			get
			{
				return IncidentManagementLink != null && IncidentManagementLink.IsControlled;
			}
		}

		public IncidentManagementGroup ManagementGroup => IncidentManagementLink?.IncidentManagementGroup;

		#endregion

		public enum Context
		{
			NewWebRequest = 0,
			OnSecondFactorySave = 1,
			OnOverrideProductClassification = 2,
			ApplyingWorkflowTemplates = 3,
			InIncidentResolvedServiceTask = 4,
		}

		#region IIncidentDetailsSource

		ZString IIncidentDetailsSource.Product => IM_Product;

		ZString IIncidentDetailsSource.Module => IM_Module;

		ZString IIncidentDetailsSource.SourceModuleId => IM_SourceModuleId;

		#endregion

		#region ITirageAssistParent

		ZString ITriageAssistParent.Priority { get => IM_Priority; set => IM_Priority = value; }

		ZString ITriageAssistParent.Product { get => IM_Product; set => IM_Product = value; }

		ZString ITriageAssistParent.Module { get => IM_Module; set => IM_Module = value; }

		ZString ITriageAssistParent.ProductArea { get => ProductArea; set => ProductArea = value; }

		ZString ITriageAssistParent.Category { get => IM_Category; set => IM_Category = value; }

		ZString ITriageAssistParent.SourceModuleId { get => IM_SourceModuleId; set => IM_SourceModuleId = value; }

		ZString ITriageAssistParent.SourceModuleWithPath { get => SourceModuleWithPath; }

		ZBool ITriageAssistParent.IsSourceModuleOverriden { get => IsSourceModuleOverriden; }

		HashSet<ZString> ITriageAssistParent.nonTriageOverridableCriticalities => NonTriageOverridableCriticalities;

		ZPropertyInfo ITriageAssistParent.TriagePKInfo => IM_IMT_TriageInfo;

		[RelatedBusinessObject("IncidentTriage")]
		[List("Lookups.TriageList")]
		public ZGuid TriagePK { get => IM_IMT_Triage; set => IM_IMT_Triage = value; }

		#endregion

		#region Method Delayer

		readonly Dictionary<string, (Action<ZString>, string)> DelayedMessageList = new Dictionary<string, (Action<ZString>, string)>();

		void DelaySendingMessage(string key, Action<ZString> sendingMethod, string message)
		{
			if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(message) || sendingMethod == null)
			{
				return;
			}

			if (Attribute.GetCustomAttribute(sendingMethod.GetMethodInfo(), typeof(SupportIncidentSendingConversationMethodAttribute)) == null)
			{
				return;
			}

			if (DelayedMessageList.ContainsKey(key))
			{
				return;
			}

			this.HasChanges = true;
			DelayedMessageList[key] = (sendingMethod, message);
		}

		void RemoveDelayedMessage(string key)
		{
			DelayedMessageList.Remove(key);
		}

		bool isRunningSendingDelayedMessage;

		void SendDelayedMessage()
		{
			if (isRunningSendingDelayedMessage)
			{
				return;
			}

			isRunningSendingDelayedMessage = true;

			var currentMessage = "";
			foreach (var key in DelayedMessageList.Keys.ToArray())
			{
				try
				{
					if (DelayedMessageList.TryGetValue(key, out var currentValue))
					{
						var method = currentValue.Item1;
						currentMessage = currentValue.Item2;
						method?.Invoke(currentMessage);
					}
				}
				catch (Exception ex)
				{
					ErrorReporter.ReportOnce($"Cannot send this delayed message {key} \r\n Message: {currentMessage}", ex);
				}
			}

			isRunningSendingDelayedMessage = false;
			DelayedMessageList.Clear();
		}

		#endregion

		#region Content Finder Jwt Token

		public string CreateContentFinderJwtToken()
		{
			var claims = new[]
			{
				new Claim("userStaffCode", Env.CurrentUser.Initials),
				new Claim("incidentNumber", IM_IncidentNumber),
				new Claim("summary", IM_Description),
				new Claim("details", DetailNoteText),
				new Claim("product", IM_Product),
				new Claim("productArea", ProductArea),
				new Claim("criticality", Criticality),
				new Claim("module", IM_Module),
				new Claim("incidentType", IM_IncidentType),
				new Claim("language",IM_Language)
			};

			var token = new JwtSecurityToken(claims: claims);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		#endregion
	}

	#region Invoicing Supporter

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = true)]
	public class SupportIncidentSendingConversationMethodAttribute : Attribute
	{
	}

	public class SupportIncidentInvoicingSupporter : JobInvoicingSupporter
	{
		public SupportIncidentInvoicingSupporter(SupportIncident parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly SupportIncident Parent;

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return EDISecurityCheckpoints.CustomerServiceIncidentJobInvoicing;
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return EDIJobInvoicingConsumerTypes.Incident; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return EDISecurityCheckpoints.CustomerServiceIncidentAuditBilling;
		}

		public override ZGuid OverriddenDepartmentPK
		{
			get { return GlbDepartment.CurrentDepartment.PK; }
		}

		public override OrgHeader Consignee
		{
			get { return Parent.Client; }
		}

		public override OrgHeader Consignor
		{
			get { return Parent.Client; }
		}
	}

	#endregion
}
