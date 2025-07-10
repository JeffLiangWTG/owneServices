using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.OpportunityManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Interop.OutlookIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public partial class ProfessionalServicesQuote : IncidentMainBase,
		IWorkflowProvider,
		IJobInvoicingPlugIn,
		IEDocsProvider,
		IWorkTaskRelatedItem,
		IWorkItemRelatedItem,
		IWorkTaskRelatedItemSource,
		IARInvoiceSavingNotificationSubscriber,
		IRelatableActivity,
		IWorkTaskTreeNode
	{
		public ProfessionalServicesQuote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region IM_OH_Client

		public override ZGuid IM_OH_Client
		{
			get { return base.IM_OH_Client; }
			set
			{
				bool isDifferentClient = (base.IM_OH_Client != value);
				base.IM_OH_Client = value;

				if (isDifferentClient)
				{
					IM_RX_NKQuoteCurrency = "";

					if (Client != null && Client.MiscServ != null)
					{
						if (Client.CompanyData.ARDDefltCurrency != null)
						{
							IM_RX_NKQuoteCurrency = Client.CompanyData.ARDDefltCurrency.RX_Code;
						}
					}
				}
			}
		}

		#endregion

		#region IM_OA_BranchAddress

		public override ZGuid IM_OA_BranchAddress
		{
			get { return base.IM_OA_BranchAddress; }
			set
			{
				base.IM_OA_BranchAddress = value;

				if (BranchAddress != null)
				{
					var retrievedClient = Factory.Load<OrgHeader>(BranchAddress.OA_OH);
					if (retrievedClient != null)
					{
						IM_OH_Client = retrievedClient.PK;
					}
				}
				else
				{
					IM_OH_Client = ZGuid.Empty;
				}

				IM_OC_Contact = ZGuid.Empty;
				IM_ClientContractStatusInfo.RefreshBinding();

				if (BranchAddress != null && BranchAddress.Header != null && !BranchAddress.Header.ARSettlementGroupPK.IsEmpty)
				{
					Job.Loader loader = new Job.Loader(this);
					Job job = loader.Load();
					if (job != null)
					{
						job.LocalChargesPK = BranchAddress.Header.ARSettlementGroupPK;
					}
				}
			}
		}

		#endregion

		#region IM_QuoteAmount

		[DecimalPlaces(2)]
		public override ZDecimal IM_QuoteAmount
		{
			get { return base.IM_QuoteAmount; }
			set
			{
				if (base.IM_QuoteAmount != value)
				{
					IM_DepositAmountRequired = value;
				}

				base.IM_QuoteAmount = value;
			}
		}

		#endregion

		#region IM_DepositAmountRequired

		[DecimalPlaces(2)]
		public override ZDecimal IM_DepositAmountRequired
		{
			get { return base.IM_DepositAmountRequired; }
			set { base.IM_DepositAmountRequired = value; }
		}

		#endregion

		#region Invoice Details

		[MaxLength(500)]
		public ZString InvoiceDetails
		{
			get { return (InvoiceDetailsNote != null) ? InvoiceDetailsNote.ST_NoteText : ZString.Empty; }
			set
			{
				CheckMaximumLength(InvoiceDetailsInfo, value);

				StmNote note = InvoiceDetailsNote;

				if (!value.IsEmpty)
				{
					if (note == null)
					{
						note = Notes.AddNew();
						note.ST_Description = PredefinedNoteTypes.Instance.InvoiceDetails.Description;
					}

					note.ST_NoteText = value;
				}
				else if (note != null)
				{
					note.Delete();
				}

				HasChanges = true;
				InvoiceDetailsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo InvoiceDetailsInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceDetails)); }
		}

		StmNote InvoiceDetailsNote
		{
			get
			{
				StmNote[] invoiceDetailsNotes = Notes.FindByDescription(PredefinedNoteTypes.Instance.InvoiceDetails.Description);
				return (invoiceDetailsNotes.Length > 0) ? invoiceDetailsNotes[0] : null;
			}
		}

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get { return IncidentConstants.GetIncidentTypeDescription(IM_IncidentType) + " " + IM_IncidentNumber; }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			IM_Details = ORtfTextUtil.EmptyRtfByteArray;
			IM_Priority = IncidentConstants.Priority.Medium;
			IM_OA_BranchAddress_ZAddress.DefaultAddressType = AddressType.OFC;

			IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
			IM_Status = ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.Quote;
			IM_ChargableWork = true;
			IM_Product = EnterpriseModuleList.Codes.ClientProjects;
			IM_ProgramArea = IncidentConstants.ProgramArea.General;

			if (Env.CurrentUser.IsDeveloper)
			{
				IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			}

			IM_IncidentType = IncidentConstants.IncidentType.ProfessionalServicesQuote;
		}

		public void SetTeamFromLastQuoteAddedByCurrentUser()
		{
			ProfessionalServicesQuote lastAdded = GetLastQuoteAddedByCurrentUser();
			if (lastAdded != null)
			{
				IM_GG_Team = lastAdded.IM_GG_Team;
			}
		}

		ProfessionalServicesQuote GetLastQuoteAddedByCurrentUser()
		{
			ZQuery filter = new ZQuery(IncidentMainSchema.IM_SystemCreateUser, Env.CurrentUser.Initials);
			filter.AddToFilter(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.ProfessionalServicesQuote);
			filter.OrderBy = IncidentMainSchema.Constants.IM_SystemCreateTimeUtc + " desc";

			return Factory.LoadTop1<ProfessionalServicesQuote>(filter);
		}

		protected virtual string GetNewIncidentNumber()
		{
			return Modules.ClientNumberFountainRegistration.GetInstance().ProfessionalServicesQuoteNo.GetNextFormatted(Factory);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			OriginalIM_Status = IM_Status;
			OriginalIM_GS_NKAssignedToCurrent = IM_GS_NKAssignedToCurrent;

			ReadOnly = false;		// never make PS Quote readonly - so that eDocs remains active even for closed quotes
		}

		#endregion

		#region Validation

		protected override IncidentMainValidation GetNewValidation()
		{
			ProfessionalServicesQuoteValidation result = new ProfessionalServicesQuoteValidation(this);
			result.Add(new BranchAddressAndContactValidation(this));
			return result;
		}

		#endregion

		#region Lookups

		public new ProfessionalServicesQuoteLookups Lookups
		{
			get { return (ProfessionalServicesQuoteLookups)base.Lookups; }
		}

		protected override IncidentMainLookups GetNewLookups()
		{
			return new ProfessionalServicesQuoteLookups(this);
		}

		#endregion

		#region Email

		public ZString HTMLLink
		{
			get { return "<a href=\"" + ObjectFactory.Get<IShowEditFormUrlCreator>().CreateWithoutApplicationContext(ClientControllerRegistration.ProfessionalServicesQuote, PK) + "\">" + IM_IncidentNumber + "</a>"; }
		}

		internal static class OutlookHeaderFields
		{
			public const string To = "To: ";
			public const string Subject = "Subject: ";
			public const string NewMessage = "X-Unsent: 1";
			public const string MIMEVersion = "MIME-Version: 1.0";
			public const string ContentType = "Content-type: text/html; Charset=utf-8";
		}

		public ZString GetEmailInEmlFormat()
		{
			OrgContact[] emailRecipients = new OrgContact[] { Contact };
			string emailText = IncidentConstants.GetTextFromResource(IncidentCorrespondenceResource);
			string recipientGreetingHeader = GetOutlookEmailRecipientGreetingHeader(emailRecipients);
			ZString body = InsertValuesIntoEmailTags(emailText, recipientGreetingHeader);

			ZStringBuilder emailBuilder = new ZStringBuilder();
			emailBuilder.AppendLine(OutlookHeaderFields.MIMEVersion);
			emailBuilder.AppendLine(OutlookHeaderFields.ContentType);
			if (Contact != null && !Contact.OC_Email.IsEmpty)
			{
				emailBuilder.AppendLine(OutlookHeaderFields.To + "<" + Contact.OC_Email + ">"); //only ever 1 recipient as you can see below
			}
			emailBuilder.AppendLine(OutlookHeaderFields.NewMessage);
			emailBuilder.AppendLine(OutlookHeaderFields.Subject + OutlookMailItemSubjectIncidentNumberPrefix + " #" + IM_IncidentNumber + " (" + IM_Description + ")");
			emailBuilder.AppendLine();

			emailBuilder.Append(string.Format(CultureInfo.CurrentCulture, "<Body style='font-family:Calibri, Arial;'>{0}</Body>", body.CleanUpTextForHTML()));

			return emailBuilder.ToString();
		}

		IOutlookMailItem SetupAndReturnMailItem(IOutlookMailItem mailItem)
		{
			OrgContact[] emailRecipients = new OrgContact[] { Contact };

			string emailText = IncidentConstants.GetTextFromResource(IncidentCorrespondenceResource);
			string recipientGreetingHeader = GetOutlookEmailRecipientGreetingHeader(emailRecipients);
			string body = InsertValuesIntoEmailTags(emailText, recipientGreetingHeader);

			AddRecipientsToOutlookMailItem(mailItem, emailRecipients);
			mailItem.Subject = OutlookMailItemSubjectIncidentNumberPrefix + " #" + IM_IncidentNumber + " (" + IM_Description + ")";
			mailItem.Body = body;
			mailItem.AddReplyRecipient(GlbStaff.CurrentUser.GS_EmailAddress);

			return mailItem;
		}

		void AddRecipientsToOutlookMailItem(IOutlookMailItem mailItem, OrgContact[] recipients)
		{
			foreach (OrgContact recipient in recipients)
			{
				if (recipient != null && !recipient.OC_Email.IsEmpty)
				{
					mailItem.AddRecipient(recipient.OC_Email);
				}
			}
		}

		string GetOutlookEmailRecipientGreetingHeader(OrgContact[] recipients)
		{
			string result = "";

			foreach (OrgContact recipient in recipients)
			{
				if (recipient != null)
				{
					if (result.Length != 0)
					{
						result += "\n";
					}

					ZString contactName = (recipient.OC_Salutation.IsEmpty) ? recipient.OC_ContactName.CapitaliseFirstLettersOfWords() : recipient.OC_Salutation.CapitaliseFirstLettersOfWords();
					result += contactName + ",";
				}
			}

			return result;
		}

		ZString InsertValuesIntoEmailTags(string body, string recipientGreetingHeader)
		{
			body = body.Replace("<Recipients>", recipientGreetingHeader);
			body = body.Replace("<LoginFullName>", GlbStaff.CurrentUser.GS_FullName.CapitaliseFirstLettersOfWords());
			body = body.Replace("<UserTitle>", GlbStaff.CurrentUser.GS_Title);
			body = body.Replace("<LoginEmail>", GlbStaff.CurrentUser.GS_EmailAddress);
			body = body.Replace("<CargoWisePhone>", GlbCompany.CurrentCompany.GC_Phone);
			body = body.Replace("<CargoWiseFax>", GlbCompany.CurrentCompany.GC_Fax);

			return body;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		void CheckAdditionalFieldsAreValidForSendingEmail()
		{
			if (GlbStaff.CurrentUser.GS_EmailAddress.IsEmpty)
			{
				throw new ApplicationException("Your user account currently has no Email Address specified. Please edit your account and specify an Email Address before sending an Email.");
			}
		}

		string IncidentCorrespondenceResource
		{
			get { return "Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote.ProfServicesQuoteLetter.txt"; }
		}

		public override ZString IM_GS_NKAssignedToCurrent
		{
			get { return base.IM_GS_NKAssignedToCurrent; }
			set
			{
				if (!value.IsEmpty && IM_Status == IncidentConstants.IncidentStatus.Unassigned)
				{
					IM_Status = IncidentConstants.IncidentStatus.Assigned;
				}
				if (value.IsEmpty && IM_Status == IncidentConstants.IncidentStatus.Assigned)
				{
					IM_Status = IncidentConstants.IncidentStatus.Unassigned;
				}

				base.IM_GS_NKAssignedToCurrent = value;

				Validation.ValidateIM_GG_Team();
				IsAssignedStaffChangedWithoutSendingEmail = false;
			}
		}

		string OutlookMailItemSubjectIncidentNumberPrefix
		{
			get { return "Quotation"; }
		}

		#region Assigned Staff Changed Email Notification

		public void ChangeAssignedStaffWithoutSendingEmailNotification(ZString assignedStaffNK)
		{
			IM_GS_NKAssignedToCurrent = assignedStaffNK;
			IsAssignedStaffChangedWithoutSendingEmail = true;
		}

		void SendEmailToAssignedStaff()
		{
			bool isAssignedStaffSameAsCurrentUser = Factory.Load<GlbStaff>(Env.CurrentUser.PK).GS_Code == IM_GS_NKAssignedToCurrent;
			bool assignedStaffHasEmailAddress = AssignedToCurrent != null && !AssignedToCurrent.GS_EmailAddress.IsEmpty;

			if (!isAssignedStaffSameAsCurrentUser &&
				!IsAssignedStaffChangedWithoutSendingEmail &&
				IsAssignedStaffChangedNotificationEnabled &&
				IsCurrentlyAssignedChanged &&
				assignedStaffHasEmailAddress)
			{
				EmailDef email = GetNewIncidentManagerEmailTemplate();

				email.AddRecipientForUserCommunication(AssignedToCurrent.GS_EmailAddress);
				email.Subject = IncidentTypeName + " " + IM_IncidentNumber + " has been assigned to you";
				email.Body = EmailBodyForAssignedStaff;
				email.ContentType = EmailContentTypes.HTML;

				if (IM_Priority == IncidentConstants.Priority.Critical || IM_Priority == IncidentConstants.Priority.High)
				{
					email.Priority = EmailDef.PriorityFlag.High;
				}
				else if (IM_Priority == IncidentConstants.Priority.Low)
				{
					email.Priority = EmailDef.PriorityFlag.Low;
				}

				Env.OutgoingMailManager.CreateAndSave(email);
			}
		}

		string EmailBodyForAssignedStaff
		{
			get
			{
				IncidentTemplatedTextGenerator bodyTextGenarator = new IncidentTemplatedTextGenerator(this);
				string bodyTemplate = IncidentConstants.GetTextFromResource(AssignedStaffEmailNotificationLetter);
				return
					bodyTextGenarator.GenerateTemplatedText(
						bodyTemplate,
						HTMLLink,
						IncidentTypeName,
						"has been assigned to you by " + GlbStaff.CurrentUser.GS_FullName + ".");
			}
		}

		string AssignedStaffEmailNotificationLetter
		{
			get { return "Enterprise.Client.EDI.IncidentManager.Business.Common.AssignedStaffEmailNotificationLetter.txt"; }
		}

		bool IsAssignedStaffChangedNotificationEnabled
		{
			get { return EDIDataRegistry.Instance.EnableProfessionalServiceQuoteAssignedStaffChangedEmailNotificationRegistryItem.Value; }
		}

		bool IsAssignedStaffChangedWithoutSendingEmail;

		#endregion

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				ReadOnly = IsClosedOrCancelled;
				OriginalIM_Status = IM_Status;
				OriginalIM_GS_NKAssignedToCurrent = IM_GS_NKAssignedToCurrent;
				SendEmailToAssignedStaff();
				IsAssignedStaffChangedWithoutSendingEmail = false;
			}
			else
			{
				Logs.RemoveAddedLogs();
			}
		}

		#endregion

		#region Related Business Objects

		#region Work Items	

		public NewWorkItemRelatedCollection RelatedWorkItems
		{
			get
			{
				if (relatedWorkItems == null)
				{
					relatedWorkItems = new NewWorkItemRelatedCollection(this);
					relatedWorkItems.Load();
				}
				return relatedWorkItems;
			}
		}

		NewWorkItemRelatedCollection relatedWorkItems;

		public override void PopulateWorkItem(NewWorkItem workItem)
		{
			base.PopulateWorkItem(workItem);

			workItem.WKI_WorkItemType = ProductTypes.Codes.Enterprise;
			workItem.WKI_ActivityType = IM_Product;
			workItem.WKI_Summary = IM_Description;
			workItem.WKI_ActivitySubtype = IM_WorkItemType == "ENH" ? NewWorkItemLookups.WorkItemTypeConstants.EnhanceCoPayment : NewWorkItemLookups.WorkItemTypeConstants.EnhanceClientSpecific;

			if (IM_WorkItemType == "ENH")
			{
				workItem.WKI_Priority = ReleaseRings.Codes.ALP;
			}
			else if (IM_WorkItemType == "CTM" || IM_WorkItemType == "CTW")
			{
				workItem.WKI_Priority = ReleaseRings.Codes.DPR;
			}
			else
			{
				workItem.WKI_Priority = NewWorkItemLookups.PatchToConstants.None;
			}

			if (IM_WorkItemType == "GRA" || IM_WorkItemType == "SRV")
			{
				workItem.WKI_Details = IM_Details;
			}
			else
			{
				workItem.WKI_Details = ZBlob.FromUTF8("Refer to related Professional Service Quotations for description information.");
			}

			ProcessTask firstTask = workItem.WorkflowItems.Count == 0 ? workItem.WorkflowItems.AddNew() : workItem.WorkflowItems[0];

			firstTask.P9_EstDuration = TimeSpan.FromHours(IM_EstimatedHours);
			firstTask.P9_GS_NKAssignedStaffMember = AssignedStaffCode;
		}

		#endregion

		public ZString RelatedIncidentNumbersCommaDelimited
		{
			get
			{
				ZString result = "";
				if (RelatedWorkItems.Count > 0)
				{
					foreach (NewWorkItem item in RelatedWorkItems)
					{
						result += item.WKI_WorkItemNumber + ", ";
					}
					result = result.Remove(result.LastIndexOf(", ", StringComparison.Ordinal), 2);
				}
				return result;
			}
		}

		public ZPropertyInfo RelatedIncidentNumbersCommaDelimitedInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedIncidentNumbersCommaDelimited)); }
		}

		#endregion

		#region Notes

		public override Notes Notes
		{
			get
			{
				if (fNotes == null)
				{
					fNotes = new IncidentNotes(this);
				}

				return fNotes;
			}
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				if (fNoteTypes == null)
				{
					fNoteTypes = base.NoteTypesCore;
					fNoteTypes.Add(PredefinedNoteTypes.Instance.InvoiceDetails);
				}

				return fNoteTypes;
			}
		}

		NoteTypeCollection fNoteTypes;

		#endregion

		#region Business Object Overrides

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();

			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();
			RemoveAllPSQOpportunityPivots();
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		protected IncidentMainInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new ProfessionalServicesQuoteInvoicingSupporter(this);
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
					fDocumentSupporter = new ProfessionalServicesQuoteDocumentSupporter(this);
				}
				return fDocumentSupporter;
			}
		}

		ProfessionalServicesQuoteDocumentSupporter fDocumentSupporter;

		#region Document Supporter

		public class ProfessionalServicesQuoteDocumentSupporter : DocumentSupporter
		{
			public ProfessionalServicesQuoteDocumentSupporter(ProfessionalServicesQuote professionalServicesQuote)
				: base(professionalServicesQuote)
			{
			}

			protected ProfessionalServicesQuote ProfessionalServicesQuote
			{
				get { return (ProfessionalServicesQuote)BusinessObject; }
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.ProfessionalSrvQuote; }
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return new Core.Constants.DataContext[] { Core.Constants.DataContext.GenericFreightJob };
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return EDISecurityCheckpoints.ProfessionalServicesQuote; }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				if (dataContext == Enterprise.Core.Constants.DataContext.GenericFreightJob)
				{
					return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, ProfessionalServicesQuote);
				}
				else
				{
					return new DocumentWrapper[] { null };
				}
			}
		}

		#endregion

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return EDIRelatableActivityTypeList.Codes.ProfessionalServicesQuote; }
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
			get { return IM_Description; }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
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

		#region IWorkTaskRelatedItem Members

		public ZString Type
		{
			get { return EDIWorkTaskRelatedItemTypes.ProfessionalServiceQuote; }
		}

		public ZPropertyInfo TypeInfo
		{
			get { return GetZPropertyInfo(nameof(Type)); }
		}

		public ZString Number
		{
			get { return IM_IncidentNumber; }
		}

		public ZPropertyInfo NumberInfo
		{
			get { return IM_IncidentNumberInfo; }
		}

		public ZString StatusDescription
		{
			get { return IM_StatusWithDescription; }
		}

		public ZPropertyInfo StatusDescriptionInfo
		{
			get { return IM_StatusWithDescriptionInfo; }
		}

		public ZString AssignedStaffCode
		{
			get { return AssignedToCurrent != null ? AssignedToCurrent.GS_Code : ZString.Empty; }
		}

		public ZPropertyInfo AssignedStaffCodeInfo
		{
			get { return GetZPropertyInfo(nameof(AssignedStaffCode)); }
		}

		public ZString ItemDescription
		{
			get { return IM_Description; }
		}

		public ZPropertyInfo ItemDescriptionInfo
		{
			get { return IM_DescriptionInfo; }
		}

		public ControllerID ControllerID
		{
			get { return ClientControllerRegistration.ProfessionalServicesQuote; }
		}

		public ZString Criticality
		{
			get { return IM_Priority; }
		}

		public ZPropertyInfo CriticalityInfo
		{
			get { return IM_PriorityInfo; }
		}

		public ZString Source
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo SourceInfo
		{
			get { return GetZPropertyInfo(nameof(Source)); }
		}

		ZBool IWorkTaskRelatedItem.IsClosedOrCancelled
		{
			get { return IsClosed; }
		}

		protected override string GetSelectionCriterion3Core() => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.WorkItemTypeList, IM_Module);

		#endregion

		#region IWorkItemRelatedItem

		public void OnRelatedWorkItemReOpened(WorkItem workItem)
		{
			ReopenIncident();
		}

		public bool OnRelatedWorkItemClosed(WorkItem workItem)
		{
			return true;
		}

		public void OnWorkItemAdded(WorkItem workItem)
		{
		}

		public void OnWorkItemRemoved(WorkItem workItem)
		{
		}

		#endregion

		#region IWorkTaskRelatedItemSource

		public WorkTaskRelatedItemCollection RelatedItems
		{
			get
			{
				if (relatedItems == null)
				{
					var localRelatedItems = new WorkTaskRelatedItemGenPivotCollection(this);
					localRelatedItems.Load();
					relatedItems = localRelatedItems;
					relatedItems.RelatedItemAdded += RelatedItemAdded;
					relatedItems.RelatedItemRemoved += RelatedItemRemoved;
				}
				return relatedItems;
			}
		}

		WorkTaskRelatedItemCollection relatedItems;

		void RelatedItemAdded(object sender, RelatedItemEventArgs e)
		{
			NewWorkItem workItem = e.BusinessObject as NewWorkItem;
			if (workItem != null)
			{
				OnWorkItemAdded(workItem);
			}
		}

		void RelatedItemRemoved(object sender, RelatedItemEventArgs e)
		{
			NewWorkItem workItem = e.BusinessObject as NewWorkItem;
			if (workItem != null)
			{
				OnWorkItemRemoved(workItem);
			}
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

		public IEnumerable<WorkTaskRelatedItemModuleInfo> SupportedRelatedItemModules
		{
			get
			{
				yield return EDIWorkTaskRelatedItemModuleInfo.GenericIncident(Factory);
				yield return EDIWorkTaskRelatedItemModuleInfo.Issue(Factory);
				yield return EDIWorkTaskRelatedItemModuleInfo.NewWorkItem(Factory);
				yield return EDIWorkTaskRelatedItemModuleInfo.EDIProject(Factory, false);
			}
		}

		public ZBool ShouldAddRelatedItemAsParent { get; set; }

		#endregion

		#region IWorkflowProvider Members

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, IM_WorkItemType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, IM_ProgramArea, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, IM_Product, ZString.Empty);
			return result;
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
		public PSQuoteProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new PSQuoteProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		public ZString WorkflowType
		{
			get { return EDIJobInvoicingConsumerTypes.PSQuote.Code; }
		}

		public void PopulateNewRelatedItem(string relatedItemType, IWorkTaskRelatedItem relatedItem)
		{
			if (relatedItemType == ProcessManagement.Business.WorkTaskRelatedItemTypes.WorkItem)
			{
				PopulateWorkItem((NewWorkItem)relatedItem);
			}
		}

		PSQuoteProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region IARInvoiceSavingNotificationSubscriber Members

		ZString IARInvoiceSavingNotificationSubscriber.Identifier
		{
			get { return IM_IncidentNumber; }
		}

		ZString IARInvoiceSavingNotificationSubscriber.HTMLLinkForDirectOpen
		{
			get { return HTMLLink; }
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

			GlbStaff customerContact = this.CustServiceContact;
			if (customerContact != null && !customerContact.GS_EmailAddress.IsEmpty)
			{
				recipients.Add(customerContact.GS_EmailAddress);
			}

			GlbStaff groupManager = GetFeatureRequestNotificationGroupManager();
			if (groupManager != null && !groupManager.GS_EmailAddress.IsEmpty && !recipients.Contains(groupManager.GS_EmailAddress))
			{
				recipients.Add(groupManager.GS_EmailAddress);
			}

			GlbStaff assignedToGroupManager = GetPSQAssignedToGroupManager();
			if (assignedToGroupManager != null && !assignedToGroupManager.GS_EmailAddress.IsEmpty && !recipients.Contains(assignedToGroupManager.GS_EmailAddress))
			{
				recipients.Add(assignedToGroupManager.GS_EmailAddress);
			}

			return recipients.ToArray();
		}

		GlbStaff GetFeatureRequestNotificationGroupManager()
		{
			GlbStaff result = null;
			GlbGroup featureRequestNotificationGroup = Factory.Load<GlbGroup>(EDIDataRegistry.Instance.IncidentCustomisationGroupENT.Value);
			if (featureRequestNotificationGroup != null)
			{
				result = featureRequestNotificationGroup.FindManager();
			}
			return result;
		}

		GlbStaff GetPSQAssignedToGroupManager()
		{
			GlbStaff result = null;
			if (this.Team != null)
			{
				result = this.Team.FindManager();
			}
			return result;
		}

		#endregion

		#region Related Opportunity

		public EDIOrgOpportunityCollection RelatedOpportunities
		{
			get
			{
				if (relatedOpportunities == null)
				{
					relatedOpportunities = new EDIOrgOpportunityCollection(Factory);
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(EDIOrgOpportunity));
					ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID);
					pivotSubQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, IncidentMainSchema.Constants.Prefix);
					pivotSubQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, this.PK);
					query.AddSubQuery(pivotSubQuery, JoinCondition.And);
					relatedOpportunities.Load(query);
					RelatedOpportunities.ItemAdded += new EDIOrgOpportunityCollection.ItemCountChangedEventHandler(RelatedOpportunities_ItemAdded);
					RelatedOpportunities.ItemRemoved += new EDIOrgOpportunityCollection.ItemCountChangedEventHandler(RelatedOpportunities_ItemRemoved);
				}
				return relatedOpportunities;
			}
		}

		void RelatedOpportunities_ItemAdded(BusinessObject bizO)
		{
			if (!RelatedOpportunities.IsLoading)
			{
				EDIOrgOpportunity opportunity = bizO as EDIOrgOpportunity;
				if (opportunity != null)
				{
					this.CreateNewPSQOpportunityPivot(opportunity);
					this.HasChanges = true;
				}
			}
		}

		void RelatedOpportunities_ItemRemoved(BusinessObject bizO)
		{
			EDIOrgOpportunity opportunity = bizO as EDIOrgOpportunity;
			if (opportunity != null)
			{
				this.RemovePSQOpportunityPivot(opportunity);
				this.HasChanges = true;
			}
		}

		EDIOrgOpportunityCollection relatedOpportunities;

		public void InitializeFromOpportunity(EDIOrgOpportunity oppurtunity)
		{
			IM_Description = oppurtunity.P8_OpportunityDescription;
			IM_QuoteAmount = oppurtunity.P8_EstimatedValue;
			IM_OA_BranchAddress = oppurtunity.P8_OA;
			IM_OH_Client = oppurtunity.P8_OH;

			//Should set IM_OC_Contact after setting the value of IM_OA_BranchAddress, otherwise IM_OC_Contact will be set to ZGuid.Empty in 
			//the setter of IM_OA_BranchAddress.
			IM_OC_Contact = oppurtunity.P8_OC;
		}

		#region PSQ Opportunity Pivot

		public GenPivot CreateNewPSQOpportunityPivot(OrgOpportunity opportunity)
		{
			GenPivot psqOpportunityPivot = Factory.New<GenPivot>();
			psqOpportunityPivot.XX_Relation1ID = this.PK;
			psqOpportunityPivot.XX_Relation1TableCode = IncidentMainSchema.Constants.Prefix;
			psqOpportunityPivot.XX_Relation2ID = opportunity.PK;
			psqOpportunityPivot.XX_Relation2TableCode = OrgOpportunitySchema.Constants.Prefix;
			return psqOpportunityPivot;
		}

		public GenPivot LoadPSQOpportunityPivot(OrgOpportunity opportunity)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, IncidentMainSchema.Constants.Prefix);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, this.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, opportunity.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, OrgOpportunitySchema.Constants.Prefix);
			GenPivot psqOpportunityPivot = Factory.LoadTop1<GenPivot>(query);
			return psqOpportunityPivot;
		}

		public void RemovePSQOpportunityPivot(OrgOpportunity opportunity)
		{
			GenPivot psqOpportunityPivot = LoadPSQOpportunityPivot(opportunity);
			if (psqOpportunityPivot != null)
			{
				psqOpportunityPivot.Delete();
			}
		}

		void RemoveAllPSQOpportunityPivots()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GenPivot));
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, IncidentMainSchema.Constants.Prefix);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, this.PK);

			GenPivot[] pivots = Factory.Load<GenPivot>(query);
			foreach (GenPivot pivot in pivots)
			{
				pivot.Delete();
			}
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, IncidentConstants.ProfessionalServicesQuoteDocManagerCode)); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IWorkTaskTreeNode Members

		public BusinessObjectCollection ChildrenOnlyRelatedItems
		{
			get
			{
				if (childrenOnlyRelatedItems == null)
				{
					childrenOnlyRelatedItems = new WorkTaskRelatedItemGenPivotCollection(this, RelatedLinkType.MasterAlwaysParent);
					childrenOnlyRelatedItems.Load();
				}
				return childrenOnlyRelatedItems;
			}
		}
		WorkTaskRelatedItemGenPivotCollection childrenOnlyRelatedItems;

		public BusinessObjectCollection ParentsOnlyRelatedItems => null; // Doesn't implement TreeView on Related Items

		public ZDateTime AgreedDeliveryDate => ZDateTime.Empty;

		public ZString CurrentTaskDescription => ZString.Empty;

		public ZString CurrentTaskCapabilityCodeDescription => ZString.Empty;

		public ZString CurrentTaskAssigned => string.Empty;

		public ZString SelectionCriterion1Code => string.Empty;

		public ZString SelectionCriterion2Code => string.Empty;

		public ZString SelectionCriterion3Code => string.Empty;

		public ZString SelectionCriterion4Code => string.Empty;

		public ZString SelectionCriterion5Code => string.Empty;

		public ZString CurrentTaskStatus => string.Empty;

		public void AddFetchHintsForOrgAddressIfRequired()
		{
		}

		public void AddFetchHintsForOrgHeaderIfRequired()
		{
		}

		#endregion
	}

	public class ProfessionalServicesQuoteInvoicingSupporter : IncidentMainInvoicingSupporter
	{
		public ProfessionalServicesQuoteInvoicingSupporter(ProfessionalServicesQuote parent)
			: base(parent)
		{
		}

		protected override JobInvoicingConsumerType ConsumerTypeCore
		{
			get { return EDIJobInvoicingConsumerTypes.PSQuote; }
		}
	}
}
