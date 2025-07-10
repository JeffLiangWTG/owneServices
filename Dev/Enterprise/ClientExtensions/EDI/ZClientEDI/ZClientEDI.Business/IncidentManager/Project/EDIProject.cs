using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface IClientOrgLicenceProvider : IBusiness
	{
		EDIOrgHeader LicenceOrganisation { get; }
		ZString ReferenceNumber { get; }
	}

	public class EDIProject :
		Project,
		IAllowAttachEmailsToEDocs,
		IClientOrgLicenceProvider,
		IARInvoiceSavingNotificationSubscriber,
		IRelatableActivity,
		IWorkTaskTreeNode
	{
		public EDIProject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WKP_Status = ProcessTaskStatusCodeList.Codes.Open;
		}

		public override void Delete()
		{
			using (SuspendCalculateStatus())
			{
				RemoveClientWorkProject();
				RelatedChildActivityPivotCollection.DeleteAll();
				RelatedParentActivityPivotCollection.DeleteAll();

				base.Delete();
			}
		}

		public override void OnSaving()
		{
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			if (!IsInDatabase && IsInstallationProject && !WKP_GS_NKProjectManager.IsEmpty)
			{
				BeginPreInstall();
			}
			SetTechnicalContactNotificationGroup();
			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				RunRollbackActions();
			}
			else
			{
				RollbackActions.Clear();
			}
		}

		#endregion

		#region Properties

		[List("Lookups.Clients")]
		public override ZGuid ClientOrganisationPK
		{
			get { return base.ClientOrganisationPK; }
			set
			{
				if (base.ClientOrganisationPK != value)
				{
					base.ClientOrganisationPK = value;
					LicenceHeaderPK = ZGuid.Empty;
				}
			}
		}

		EDIOrgHeader ClientOrg
		{
			get { return Factory.Load<EDIOrgHeader>(ClientOrganisationPK); }
		}

		public ZString EnterpriseCode
		{
			get
			{
				var org = ClientOrg;
				return org != null ? org.LicenceEnterpriseCode : ZString.Empty;
			}
		}

		public ZString EnterpriseID => ClientOrg?.LicenceEnterpriseID ?? ZString.Empty;

		#region Status

		public bool IsInstallationProject
		{
			get { return WKP_Type == InstallationProjectCode; }
		}

		internal string InstallationProjectCode
		{
			get { return "INS"; }
		}

		public void CalculateStatus()
		{
			if (!suspendCalculateStatus && WKP_Status != ProcessTaskStatusCodeList.Codes.Cancelled)
			{
				if (WorkflowItems.AllTasksClosedOrCancelled)
				{
					WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;
				}
				else if (CurrentTask != null || WorkflowItems.AnyTaskIsAssigned)
				{
					WKP_Status = ProcessTaskStatusCodeList.Codes.Working;
				}
				else
				{
					WKP_Status = ProcessTaskStatusCodeList.Codes.Open;
				}
			}
		}

		IDisposable SuspendCalculateStatus()
		{
			suspendCalculateStatus = true;
			return new DisposableAction(delegate { suspendCalculateStatus = false; });
		}

		bool suspendCalculateStatus;

		#endregion

		public override ZGuid WKP_OC_Contact
		{
			get { return base.WKP_OC_Contact; }
			set
			{
				if (value != base.WKP_OC_Contact)
				{
					ZGuid originalValue = base.WKP_OC_Contact;
					base.WKP_OC_Contact = value;
					foreach (ProjectProcessTask task in WorkflowItems)
					{
						if (task.P9_OC == originalValue)
						{
							task.P9_OC = value;
						}
					}
				}
			}
		}

		#region ClientWorkProject Properties

		#region ClientWorkProject

		internal ClientWorkProject ClientWorkProject
		{
			get
			{
				if (clientWorkProject == null)
				{
					clientWorkProject = LoadOrCreateClientWorkProject();
					RegisterEditableChildObject(clientWorkProject);
				}
				return clientWorkProject;
			}
		}

		ClientWorkProject clientWorkProject;

		ClientWorkProject LoadOrCreateClientWorkProject()
		{
			var result = LoadClientWorkProject();
			if (result == null)
			{
				result = Factory.New<ClientWorkProject>();
				using (result.SuspendSettingHasChanges())
				{
					result.CWP_WKP = PK;
				}
			}
			return result;
		}

		ClientWorkProject LoadClientWorkProject()
		{
			ZQuery query = new ZQuery(ClientWorkProjectSchema.CWP_WKP, PK);
			return Factory.LoadTop1<ClientWorkProject>(query);
		}

		void RemoveClientWorkProject()
		{
			var result = LoadClientWorkProject();
			if (result != null)
			{
				result.Delete();
			}
		}

		#endregion

		#region LicenceHeaderPK

		[List("Lookups.LicenceHeaderList")]
		public ZGuid LicenceHeaderPK
		{
			get { return ClientWorkProject.CWP_LA; }
			set
			{
				ClientWorkProject.CWP_LA = value;
				LicenceHeaderPKInfo.RefreshBinding();
				SiteLiveDateInfo.RefreshBinding();
				EstimatedSiteLiveDateInfo.RefreshBinding();
				AgreedLiveDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LicenceHeaderPKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LicenceHeaderPK), x => ClientWorkProject.CWP_LAInfo); }
		}

		public LicenceHeader Licence
		{
			get { return Factory.Load<LicenceHeader>(LicenceHeaderPK); }
		}

		#endregion

		#region PlannedInstall

		public ZDateTime PlannedInstall
		{
			get { return ClientWorkProject.CWP_PlannedInstall; }
			set
			{
				ClientWorkProject.CWP_PlannedInstall = value;
				if (!value.IsEmpty)
				{
					CompletePreInstall();
				}
				PlannedInstallInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PlannedInstallInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PlannedInstall), x => ClientWorkProject.CWP_PlannedInstallInfo); }
		}

		#endregion

		#region InstallDate

		public ZDateTime InstallDate
		{
			get { return ClientWorkProject.CWP_InstallDate; }
			set
			{
				ClientWorkProject.CWP_InstallDate = value;
				if (!value.IsEmpty)
				{
					if (AnyTaskOpen("PRE"))
					{
						ChangeStatusToWorkingAndAddToLog("Install Completed - Pending Pre-Install Task(s)");
					}
					else
					{
						ChangeStatusToWorkingAndAddToLog("Install Completed");
					}
				}
				InstallDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo InstallDateInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(InstallDate), x => ClientWorkProject.CWP_InstallDateInfo); }
		}

		bool AnyTaskOpen(string taskType)
		{
			bool result = false;
			foreach (ProcessTask task in WorkflowItems.Tasks)
			{
				if (task.P9_Type == taskType && !task.IsClosed)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		#endregion

		#region CallbackBy

		public ZDateTime CallbackBy
		{
			get { return ClientWorkProject.CWP_CallbackBy; }
			set { ClientWorkProject.CWP_CallbackBy = value; }
		}

		public ZPropertyInfo CallbackByInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CallbackBy), x => ClientWorkProject.CWP_CallbackByInfo); }
		}

		#endregion

		#region Agreed Go-Live Date

		[BusinessObjectTestExclude] //Cannot set value if licence is blank
		public ZDateTime AgreedLiveDate
		{
			get { return Licence != null ? Licence.LA_AgreedLiveDate : ZDateTime.Empty; }
			set
			{
				if (Licence != null)
				{
					EnsureLicenceValidation();
					Licence.LA_AgreedLiveDate = value;
					this.HasChanges = true;
				}
				AgreedLiveDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AgreedLiveDateInfo
		{
			get
			{
				return Licence != null
						  ? GetWrappedZPropertyInfo(nameof(AgreedLiveDate), x => Licence.LA_AgreedLiveDateInfo)
						  : GetZPropertyInfo(nameof(AgreedLiveDate));
			}
		}

		public bool AgreedLiveDate_ReadOnly
		{
			get { return Licence == null || !EDISecurityCheckpoints.OrgLicenceModifyAgreedGoLive.IsAllowed; }
		}

		#endregion

		#region Estimated Go-Live Date

		[BusinessObjectTestExclude]	//Cannot set value if licence is blank
		public ZDateTime EstimatedSiteLiveDate
		{
			get { return Licence != null ? Licence.LA_EstimatedLiveDate : ZDateTime.Empty; }
			set
			{
				if (Licence != null)
				{
					EnsureLicenceValidation();
					Licence.LA_EstimatedLiveDate = value;
					this.HasChanges = true;
				}
				EstimatedSiteLiveDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EstimatedSiteLiveDateInfo
		{
			get
			{
				return Licence != null
						  ? GetWrappedZPropertyInfo(nameof(EstimatedSiteLiveDate), x => Licence.LA_EstimatedLiveDateInfo)
						  : GetZPropertyInfo(nameof(EstimatedSiteLiveDate));
			}
		}

		public bool EstimatedSiteLiveDate_ReadOnly
		{
			get { return Licence == null || !EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed; }
		}

		#endregion

		#region Go-Live Date

		[BusinessObjectTestExclude] //Cannot set value if licence is blank
		public ZDateTime SiteLiveDate
		{
			get { return Licence != null ? Licence.LA_SiteLiveDate : ZDateTime.Empty; }
			set
			{
				if (Licence != null)
				{
					EnsureLicenceValidation();
					Licence.LA_SiteLiveDate = value;
					HasChanges = true;
				}
				SiteLiveDateInfo.RefreshBinding();
			}
		}

		public ZWrappedPropertyInfo SiteLiveDateInfo => GetWrappedZPropertyInfo(nameof(SiteLiveDate), x => Licence?.LA_SiteLiveDateInfo ?? GetZPropertyInfo(nameof(SiteLiveDate)));

		public bool SiteLiveDate_ReadOnly
		{
			get { return Licence == null || !EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed; }
		}

		#endregion

		void EnsureLicenceValidation()
		{
			var licence = Licence;
			if (licence != null && licence.IsValidationSuspended)
			{
				licence.ResumeValidation();
			}
		}

		#endregion

		#region Related Items / IWorkTaskRelatedItemSource

		protected override WorkTaskRelatedItemCollection CreateAndLoadRelatedItems()
		{
			var result = base.CreateAndLoadRelatedItems();
			result.RelatedItemAdded += RelatedItems_RelatedFeatureRequestAdded;
			result.RelatedItemRemoved += RelatedItems_RelatedFeatureRequestRemoved;

			return result;
		}

		static void RelatedItems_RelatedFeatureRequestAdded(object sender, RelatedItemEventArgs e)
		{
			SupportIncident incident = e.BusinessObject as SupportIncident;
			if (incident != null && incident.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest && incident.IM_Source == SupportIncidentLookups.SourceListConstants.ERequestPortal)
			{
				incident.IM_Source = SupportIncidentLookups.SourceListConstants.CreatedFromProject;
			}
		}

		static void RelatedItems_RelatedFeatureRequestRemoved(object sender, RelatedItemEventArgs e)
		{
			SupportIncident incident = e.BusinessObject as SupportIncident;
			if (incident != null && incident.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest && incident.IM_Source == SupportIncidentLookups.SourceListConstants.CreatedFromProject)
			{
				incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			}
		}

		public override IEnumerable<WorkTaskRelatedItemModuleInfo> SupportedRelatedItemModules
		{
			get
			{
				yield return EDIWorkTaskRelatedItemModuleInfo.FeatureRequest(Factory);
				yield return EDIWorkTaskRelatedItemModuleInfo.GenericIncident(Factory);
				yield return EDIWorkTaskRelatedItemModuleInfo.Issue(Factory);
				yield return EDIWorkTaskRelatedItemModuleInfo.Quote(Factory);
				yield return EDIWorkTaskRelatedItemModuleInfo.EDIProject(Factory, false);
				yield return EDIWorkTaskRelatedItemModuleInfo.NewWorkItem(Factory);
			}
		}

		public override void PopulateNewRelatedItem(string relatedItemType, IWorkTaskRelatedItem relatedItem)
		{
			IncidentMainBase item = relatedItem as IncidentMainBase;
			if (item != null)
			{
				PopulateRelatedIncidentMain(item, true);
				if (relatedItemType == EDIWorkTaskRelatedItemTypes.FeatureRequest)
				{
					SupportIncident featureRequest = (SupportIncident)relatedItem;
					featureRequest.SetupForProjectFeatureRequest();
					featureRequest.RelatedProjectPK = PK;
					featureRequest.IM_GS_NKAssignedToCurrent = WKP_GS_NKProjectManager;
					if (featureRequest.RelatedProject == null)
					{
						featureRequest.Factory.ImportFromAnotherFactory(this);
					}
				}
			}
		}

		public void PopulateRelatedIncidentMain(IncidentMainBase relatedItem, bool overwriteExistingValues)
		{
			if (overwriteExistingValues || relatedItem.IM_OH_Client.IsEmpty)
			{
				relatedItem.IM_OH_Client = ClientOrganisationPK;
			}

			if (overwriteExistingValues || relatedItem.IM_OA_BranchAddress.IsEmpty)
			{
				relatedItem.IM_OA_BranchAddress = WKP_OA_ClientAddress;
			}

			if (overwriteExistingValues || relatedItem.IM_OC_Contact.IsEmpty)
			{
				relatedItem.IM_OC_Contact = WKP_OC_Contact;
			}

			if ((overwriteExistingValues || relatedItem.IM_LCC.IsEmpty) && Licence != null && Licence.ClientCompany != null)
			{
				relatedItem.IM_LCC = Licence.ClientCompany.PK;
			}
		}

		#endregion

		#endregion

		#region Lookups

		protected override WorkProjectLookups GetNewLookups()
		{
			return new ProjectLookups(this);
		}

		public new ProjectLookups Lookups
		{
			get { return (ProjectLookups)base.Lookups; }
		}

		#endregion

		#region Actions

		public override void Close(ZString closeType, ZString comment)
		{
			using (SuspendCalculateStatus())
			{
				base.Close(closeType, comment);
				CallbackBy = ZDateTime.Empty;
			}
		}

		public override void ReOpen(ZString comment)
		{
			using (SuspendCalculateStatus())
			{
				base.ReOpen(comment);
			}
		}

		public void BeginPreInstall()
		{
			ChangeStatusToWorkingAndAddToLog("Pre-Install Started");
		}

		public void CompletePreInstall()
		{
			ChangeStatusToWorkingAndAddToLog("Pre-Install Completed");
		}

		public void CompleteInstall()
		{
			InstallDate = ZDateTime.Now;
		}

		public void BeginTraining()
		{
			ChangeStatusToWorkingAndAddToLog("Training Started");
		}

		public void CompleteTraining()
		{
			ChangeStatusToWorkingAndAddToLog("Training Completed");
		}

		void ChangeStatusToWorkingAndAddToLog(string logComment)
		{
			WKP_Status = ProcessTaskStatusCodeList.Codes.Working;
			AddToLog(logComment);
		}

		#endregion

		#region IJobInvoicingPlugIn

		protected override IJobInvoicingSupporter CreateProjectInvoicingSupporter()
		{
			return new EDIProjectInvoicingSupporter(this);
		}

		#endregion

		#region Workflow

		[ChildEditable(true)]
		public new ProjectProcessTaskCollection WorkflowItems
		{
			get { return (ProjectProcessTaskCollection)base.WorkflowItems; }
		}

		protected override ProcessManagement.Business.ProjectProcessTaskCollection CreateProjectProcessTaskCollection()
		{
			var result = new ProjectProcessTaskCollection(this);
			result.Tasks.CountChanged += delegate
			{
				if (!WorkflowItems.IsLoading)
				{
					CalculateWorkflowDependentProperties();
				}
			};
			return result;
		}

		void CalculateWorkflowDependentProperties()
		{
			CalculateStatus();
		}

		#endregion

		#region IARInvoiceSavingNotificationSubscriber Members

		string[] IARInvoiceSavingNotificationSubscriber.GetRecipientsForNotification()
		{
			List<string> recipients = new List<string>();
			GlbGroup projectNotificationGroup = Factory.Load<GlbGroup>(EDIDataRegistry.Instance.ProjectInvoiceEmailNotificationGroup.Value);
			if (projectNotificationGroup != null)
			{
				foreach (GlbStaff staff in projectNotificationGroup.Staff)
				{
					recipients.Add(staff.GS_EmailAddress);
				}
			}
			return recipients.ToArray();
		}

		ZString IARInvoiceSavingNotificationSubscriber.Identifier
		{
			get { return WKP_ProjectNumber; }
		}

		ZString IARInvoiceSavingNotificationSubscriber.HTMLLinkForDirectOpen
		{
			get { return "<a href=\"" + ObjectFactory.Get<IShowEditFormUrlCreator>().CreateWithoutApplicationContext(ControllerIDs.Project, PK) + "\">" + WKP_ProjectNumber + "</a>"; }
		}

		ZString IARInvoiceSavingNotificationSubscriber.Description
		{
			get { return WKP_Summary; }
		}

		ZString IARInvoiceSavingNotificationSubscriber.ClientCode
		{
			get { return ClientOrganisation != null ? ClientOrganisation.OH_Code : ZString.Empty; }
		}

		ZString IARInvoiceSavingNotificationSubscriber.ClientName
		{
			get { return ClientOrganisation != null ? ClientOrganisation.OH_FullName : ZString.Empty; }
		}

		#endregion

		public GlbStaff GetGroupManager(GlbGroup group)
		{
			if (group != null)
			{
				foreach (GlbStaff staff in group.Staff)
				{
					GlbGroupLink link = (GlbGroupLink)group.Staff.GetRelationshipBusinessObject(staff);
					if (link.GK_MembershipType == MembershipTypeList.Codes.MGR)
					{
						return staff;
					}
				}
			}

			return null;
		}

		#region IAllowAttachEmailsToEDocs Members

		string IAllowAttachEmailsToEDocs.ReferenceNumber
		{
			get { return WKP_ProjectNumber; }
		}

		#endregion

		#region Send Email

		protected override string GetEmailSubject()
		{
			return WKP_ProjectNumber + ": Installation Update";
		}

		protected override string GetDefaultFromDisplayName()
		{
			return IncidentConstants.ImplementationDefaultReplyToName;
		}

		protected override string GetDefaultFromEmailAddress()
		{
			return EDIDataRegistry.Instance.ImplementationDefaultFromEmailAddress.Value;
		}

		protected override AddressBookSelection GetAddressBookSelection()
		{
			AddressBookSelection result = base.GetAddressBookSelection();
			result.AddRecipient(Contact, true);
			return result;
		}

		protected override Type GetDocWrapperType()
		{
			return typeof(ProjectEmailWrapper);
		}

		#endregion

		#region IClientOrgLicenceProvider Members

		EDIOrgHeader IClientOrgLicenceProvider.LicenceOrganisation
		{
			get { return (EDIOrgHeader)ClientOrganisation; }
		}

		ZString IClientOrgLicenceProvider.ReferenceNumber
		{
			get { return WKP_ProjectNumber; }
		}

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return EDIRelatableActivityTypeList.Codes.Project; }
		}
		IOrgHeader IRelatableActivity.Client
		{
			get { return ClientOrganisation; }
		}
		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return ClientOrganisationPKInfo.HasChanges; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return Contact; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return WKP_OC_ContactInfo.HasChanges; }
		}
		ZString IRelatableActivity.Summary
		{
			get { return WKP_Summary; }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

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

		public BusinessObjectCollection ParentsOnlyRelatedItems => null; // WorkItem don't implement related items tree view, so don't need to display parent

		public ZDateTime AgreedDeliveryDate => HasJobWorkflow ? JobWorkflow.AgreedDeliveryDateLocal : ZDateTime.Empty;

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

		public ZString CurrentTaskAssigned => AssignedToCode;

		public ZString SelectionCriterion1Code => WKP_Type;

		public ZString SelectionCriterion2Code => WKP_SubType;

		public ZString SelectionCriterion3Code => WKP_Module;

		public ZString SelectionCriterion4Code => WKP_Priority;

		public ZString SelectionCriterion5Code => string.Empty;

		public void AddFetchHintsForOrgAddressIfRequired()
		{
			Factory.AddFetchHint(typeof(OrgAddress), WKP_OA_ClientAddress);
		}

		public void AddFetchHintsForOrgHeaderIfRequired()
		{
			if (ClientAddress != null)
			{
				Factory.AddFetchHint(typeof(OrgHeader), ClientAddress.OA_OH);
			}
		}

		#endregion

		#region TechnicalContactNotificationGroup

		void SetTechnicalContactNotificationGroup()
		{
			if (TechnicalContact is EDIOrgContact contact && !contact.IsInformationServicesTechnicalAdministrator)
			{
				contact.IsInformationServicesTechnicalAdministrator = true;
				RollbackActions.Push(() => { contact.IsInformationServicesTechnicalAdministrator = false; });
			}
		}

		void RunRollbackActions()
		{
			RollbackActions.ForEach(x => x());
			RollbackActions.Clear();
		}

		readonly Stack<Action> RollbackActions = new Stack<Action>();

		#endregion

		#region Test Data
		internal void ChangeClientOrganisation(OrgHeader client)
		{
			WKP_OA_ClientAddress = client.MainAddress.PK;
			if (client.Contacts.Count > 0)
			{
				WKP_OC_Contact = client.Contacts[0].PK;
			}
			else
			{
				var contact = client.Contacts.AddNew();
				contact.OC_ContactName = "Jenny";
				WKP_OC_Contact = contact.PK;
			}
		}
		#endregion
	}
}

