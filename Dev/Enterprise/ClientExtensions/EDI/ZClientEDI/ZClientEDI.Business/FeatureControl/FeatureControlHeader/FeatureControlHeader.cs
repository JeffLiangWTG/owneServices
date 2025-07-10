using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.Definitions.LicenceFeatureCodeList;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	[CodeProperty(Schema.FCM_FeatureControlCode)]
	public class FeatureControlHeader : AutoFeatureControlHeader, IDocManagerSupport, IAuditParent
	{
		public FeatureControlHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => "Feature Control";

		protected override ZString HumanReadableShortcutNameCore => $"{FCM_FeatureControlCode} - {FCM_Description}";

		[ResourceStringData("FeatureControlHeader|FCM_FeatureControlCode", Caption = "Code")]
		public override ZString FCM_FeatureControlCode { get => base.FCM_FeatureControlCode; set => base.FCM_FeatureControlCode = value; }

		[ResourceStringData("FeatureControlHeader|FCM_Description", Caption = "Description")]
		public override ZString FCM_Description { get => base.FCM_Description; set => base.FCM_Description = value; }

		[ResourceStringData("FeatureControlHeader|FCM_GG_ReleaseGroup", Caption = "Release Group")]
		public override ZGuid FCM_GG_ReleaseGroup { get => base.FCM_GG_ReleaseGroup; set => base.FCM_GG_ReleaseGroup = value; }

		[ResourceStringData("FeatureControlHeader|ActiveWorkItem", Caption = "Feature Control Active WI")]
		[RelatedBusinessObject("ActiveWorkItem")]
		[List("Lookups.WorkItems")]
		public override ZGuid FCM_WKI_ActiveWorkItem { get => base.FCM_WKI_ActiveWorkItem; set => base.FCM_WKI_ActiveWorkItem = value; }

		[ResourceStringData("FeatureControlHeader|DeactivateWorkItem", Caption = "Feature Control Deactivate WI")]
		[RelatedBusinessObject("DeactivateWorkItem")]
		[List("Lookups.WorkItems")]
		public override ZGuid FCM_WKI_DeactivateWorkItem { get => base.FCM_WKI_DeactivateWorkItem; set => base.FCM_WKI_DeactivateWorkItem = value; }

		public WorkItem ActiveWorkItem => Factory.Load<WorkItem>(FCM_WKI_ActiveWorkItem);

		public WorkItem DeactivateWorkItem => Factory.Load<WorkItem>(FCM_WKI_DeactivateWorkItem);

		public bool HasGlobalEditRight => EDISecurityCheckpoints.AllowFeatureEditForAnyReleaseGroupReferenceName.IsAllowed;

		public bool IsInReleaseGroup => ReleaseGroup?.Staff?.Contains(Env.CurrentUser.PK) ?? false;

		public bool BaseReadOnly => !HasGlobalEditRight && !IsInReleaseGroup;

		public bool FCM_FeatureControlCode_ReadOnly => true;

		public bool FCM_Description_ReadOnly => BaseReadOnly;

		public bool FCM_WKI_ActiveWorkItem_ReadOnly => BaseReadOnly;

		public bool FCM_WKI_DeactivateWorkItem_ReadOnly => BaseReadOnly;

		public bool FCM_GG_ReleaseGroup_ReadOnly => !HasGlobalEditRight && IsInDatabase;

		[ResourceStringData("FeatureControlHeader|Status", Caption = "Feature Control Stage")]
		public ZString Status => Factory.GetCachedValue($"FeatureControlHeader.Status.{FCM_FeatureControlCode}", () =>
			GetLicenceFeatureCodePairs().FirstOrDefault(x => FCM_FeatureControlCode.EqualsIgnoringCase(x.Code))?.Stage switch
			{
				FeatureStage.Development => "In Development",
				FeatureStage.Active => "Active",
				_ => "Sunsetting",
			});

		#region IDocManagerSupport

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, FeatureControlHeaderSchema.Constants.Prefix));

		DocManagerInfo docManagerInfo;

		#endregion

		[ChildEditable]
		public FeatureControlRuleCollection FeatureControlRules
		{
			get
			{
				if (featureControlRules == null)
				{
					featureControlRules = new FeatureControlRuleCollection(this);
					RegisterEditableChildObject(featureControlRules);
					featureControlRules.Load();
				}

				return featureControlRules;
			}
		}

		FeatureControlRuleCollection featureControlRules;

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(FeatureControlRuleSchema.FCR_FCM_FeatureControl, FeatureControlRuleSchema.FCR_Description);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				RefreshBinding();
			}
		}
	}
}
