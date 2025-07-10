using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Services.Calendar;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CRM.Common
{
	[UserDefinedValues]
	[CodeProperty(CrmOpportunity.Schema.COP_OpportunityID), DescriptionProperty(CrmOpportunity.Schema.COP_OpportunityName)]
	public class CrmOpportunity : AutoCrmOpportunity,
		ICrmOpportunity,
		IWorkflowProvider,
		ICustomFieldProvider,
		IRelatableActivity,
		IDocManagerSupport
	{
		public CrmOpportunity(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = Res.GetString("d683272c-ab19-4437-9c47-3428db149794", "Opportunity");
				if (!IsDeleted && !COP_OpportunityID.IsEmpty)
				{
					result += " (" + COP_OpportunityID + ")";
				}

				return result;
			}
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = COP_OpportunityID;

				if (Organization != null)
				{
					result += " - " + Organization.OH_Code;
				}

				if (!COP_OpportunityName.IsEmpty)
				{
					result += " - " + COP_OpportunityName;
				}

				return result;
			}
		}

		#region Delete

		public override void Delete()
		{
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();
			base.Delete();
			WorkflowItems.RemoveAndDeleteAll();
		}

		#endregion

		#region IWorkflowProvider

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
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
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (tasks == null)
				{
					tasks = this.GetOrCreateProcessTaskCollection(() => new CrmOpportunityProcessTasksCollection(this));
					RegisterEditableChildObject(tasks);
				}
				return tasks;
			}
		}
		ProcessTaskCollection tasks;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.CrmOpportunityWorkflowDescriptorCode;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			// Must match CrmOpportunityWorkflowDescriptor.SubTypeInformation
			// and CrmOpportunityFormCustomisationSettingProvider.GetPropertiesThatAffectWorkflow
			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_SubType1, COP_SalesType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, COP_SourceType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, COP_ProductType, ZString.Empty);
			return result;
		}

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region Saving

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		public ZString GlowLinkHtml
		{
			get
			{
				var linkText = Res.GetString("f4d5d844-b153-4496-a838-00ab4a035a12", "{0} - {1}", COP_OpportunityID, Organization?.OH_FullName);
				return $"<a href='{GlowLink}'>{linkText}</a>";
			}
		}

		public ZString GlowLink
		{
			get
			{
				var baseUrl = GlowRegistry.Instance.GlowPortalsUri.Value;
				var editOpportunityUrl = (NoResString)"/goto/OpportunityEdit?entityPK=";
				return baseUrl.TrimEnd('/') + "/" + editOpportunityUrl.ToString().TrimStart('/') + PK.ToString();
			}
		}

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new CrmOpportunityDocManagerInfo(this, Core.Constants.DocManagerCodes.CrmOpportunity);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IRelatableActivity Members

		public ZString ActivityType => RelatableActivityTypeList.Codes.CrmOpportunityManager;

		public IOrgHeader Client => Organization;

		public ZBool ClientHasChanges => COP_OH_OrganizationInfo.HasChanges;

		public IOrgContact Contact => Contact;

		public ZBool ContactHasChanges => COP_OC_ReferringContactInfo.HasChanges;

		public ZString Summary => string.Join("; ", new[] { COP_OpportunityName, COP_ProductType, COP_SalesType, StatusDescription }.Where(x => !x.IsEmpty)) ;

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

		public ZBool ShouldIgnoreSuperAndSubActivityRelationships => ZBool.False;

		public ZBool SupportViewRelatedCommunications => ZBool.True;

		public void OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		#endregion

		#region Status Description

		public ZString StatusDescription
		{
			get { return Lookups.Statuses.GetDescriptionFromCode(COP_Status); }
		}

		public ZPropertyInfo StatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(StatusDescription)); }
		}

		#endregion

		#region EntityStaffRestriction

		public EntityStaffRestrictionCollection EntityStaffRestrictions
		{
			get
			{
				if (entityStaffRestrictions == null)
				{
					entityStaffRestrictions = new EntityStaffRestrictionCollection(this);
				}
				return entityStaffRestrictions;
			}
		}
		EntityStaffRestrictionCollection entityStaffRestrictions;

		public bool IsRestrictedForCurrentUser()
		{
			var currentUser = GlbStaff.CurrentUser;
			return !currentUser.GS_IsController && COP_IsRestricted && !EntityStaffRestrictions.Any(r => r.ESR_GS_NKStaff == currentUser.GS_Code);
		}

		#endregion

		public Reminder GetNewReminder(ZDateTime initialFollowUpDate, ZDateTime newFollowUpDate)
		{
			Reminder reminder;

			if (!newFollowUpDate.IsEmpty)
			{
				var htmlBody = WrapWithHtmlDeclaration(Res.GetString("06a162c5-ab6d-4450-b981-ceff7be38a42", "Next follow up for Opportunity {0}", GlowLinkHtml));
				reminder = new Reminder(PK.ToString(), PK, new ZString(TableName), DateTimeKind.Utc, newFollowUpDate, newFollowUpDate, Res.GetString("0020b01f-666c-47a2-a973-1f034c25a012", "Next follow up for Opportunity {0} - {1}", COP_OpportunityID, Organization?.OH_FullNameTruncated), Res.GetString("c864d225-9955-4503-938b-a131dc6d83f2", "Next follow up for Opportunity {0} - {1}", COP_OpportunityID, Organization?.OH_FullName), htmlBody);
			}
			else
			{
				var htmlBody = WrapWithHtmlDeclaration(Res.GetString("32178d14-716d-48ea-aef7-657f0ec07595", "Next follow up has been canceled for Opportunity {0}", GlowLinkHtml));
				reminder = new Reminder(PK.ToString(), PK, new ZString(TableName), DateTimeKind.Utc, initialFollowUpDate, initialFollowUpDate, Res.GetString("3aa52d02-283b-4fcd-8829-9cc2c4e59785", "Next follow up has been canceled for Opportunity {0} - {1}", COP_OpportunityID, Organization?.OH_FullNameTruncated), Res.GetString("f6c36afd-4696-4e8b-8e23-0a046241579a", "Next follow up has been canceled for Opportunity {0} - {1}", COP_OpportunityID, Organization?.OH_FullName), htmlBody);
				reminder.ReminderType = ReminderType.Cancellation;
			}

			if (SalesPerson != null && !SalesPerson.GS_EmailAddress.IsEmpty)
			{
				reminder.Recipients.Add(SalesPerson.GS_FullName, SalesPerson.GS_EmailAddress);
			}

			reminder.Location = Organization.MainAddress != null ? Organization.MainAddress.AddressAsASingleLineWithoutCompanyName : ZString.Empty;

			return reminder;
		}

		string WrapWithHtmlDeclaration(string innerHtmlBody) => FormattableString.Invariant($"<HTML><HEAD><TITLE></TITLE></HEAD><BODY>{innerHtmlBody}</BODY></HTML>");
	}
}
