using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlRule : AutoFeatureControlRule, IAuditParent
	{
		public FeatureControlRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("ControlHeader")]
		public override ZGuid FCR_FCM_FeatureControl { get => base.FCR_FCM_FeatureControl; set => base.FCR_FCM_FeatureControl = value; }

		public FeatureControlHeader ControlHeader => Factory.Load<FeatureControlHeader>(FCR_FCM_FeatureControl);

		[ResourceStringData("FeatureControlRule|FeatureHeaderDescription", Caption = "Feature")]
		public ZString FeatureHeaderDescription => ControlHeader?.HumanReadableShortcutName ?? ZString.Empty;

		[RelatedBusinessObject("FeatureSet")]
		[List("Lookups.FeatureSets")]
		[ResourceStringData("FeatureControlRule|FCR_FCS_FeatureSet", Caption = "Feature Set")]
		public override ZGuid FCR_FCS_FeatureSet { get => base.FCR_FCS_FeatureSet; set => base.FCR_FCS_FeatureSet = value; }

		public FeatureControlSet FeatureSet => Factory.Load<FeatureControlSet>(FCR_FCS_FeatureSet);

		[ResourceStringData("FeatureControlRule|FCR_Description", Caption = "Description")]
		public override ZString FCR_Description { get => base.FCR_Description; set => base.FCR_Description = value; }

		[ResourceStringData("FeatureControlRule|FCR_StartDateUtc", Caption = "Enable From (UTC)")]
		public override ZDateTime FCR_StartDateUtc { get => base.FCR_StartDateUtc; set => base.FCR_StartDateUtc = value; }

		[ResourceStringData("FeatureControlRule|FCR_EndDateUtc", Caption = "Enable Until (UTC)")]
		public override ZDateTime FCR_EndDateUtc { get => base.FCR_EndDateUtc; set => base.FCR_EndDateUtc = value; }

		[ResourceStringData("FeatureControlRule|FCR_IsActive", Caption = "Is Active")]
		public override ZBool FCR_IsActive { get => base.FCR_IsActive; set => base.FCR_IsActive = value; }

		[ResourceStringData("FeatureControlRule|RuleTypeDescription", Caption = "Type")]
		public ZString RuleTypeDescription => new FeatureControlRuleTypeList().GetDescriptionFromCode(FCR_RuleType);

		public ZWrappedPropertyInfo RuleTypeDescriptionInfo => GetWrappedZPropertyInfo(nameof(RuleTypeDescription), x => FCR_RuleTypeInfo);

		[ResourceStringData("FeatureControlRule|DatabaseCount", Caption = "# of License Databases")]
		public ZInt DatabaseCount => IsFeatureSetRule ? FeatureSet.Databases.Count : LicenceDatabasePivots.Count;

		public ZPropertyInfo DatabaseCountInfo => GetZPropertyInfo(nameof(DatabaseCount));

		#region RuleType

		public override ZString FCR_RuleType
		{
			get => base.FCR_RuleType;
			set
			{
				base.FCR_RuleType = value;
				if (IsGlobalRule)
				{
					FCR_UseGlobalParameters = false;
				}
				if (!IsFeatureSetRule)
				{
					FCR_FCS_FeatureSet = ZGuid.Empty;
				}
			}
		}

		public ZBool IsGlobalRule
		{
			get => FCR_RuleType.EqualsIgnoringCase(FeatureControlRuleTypeList.Codes.Global);
			set
			{
				if (value)
				{
					FCR_RuleType = FeatureControlRuleTypeList.Codes.Global;
				}
				else
				{
					if (IsFeatureSetRule)
					{
						FCR_RuleType = FeatureControlRuleTypeList.Codes.FeatureSet;
					}
					else
					{
						FCR_RuleType = FeatureControlRuleTypeList.Codes.Client;
					}
				}
			}
		}
		public ZWrappedPropertyInfo IsGlobalRuleInfo => GetWrappedZPropertyInfo(nameof(IsGlobalRule), x => FCR_RuleTypeInfo);

		public ZBool IsFeatureSetRule
		{
			get => FCR_RuleType.EqualsIgnoringCase(FeatureControlRuleTypeList.Codes.FeatureSet);
			set
			{
				if (value)
				{
					FCR_RuleType = FeatureControlRuleTypeList.Codes.FeatureSet;
				}
				else
				{
					if (IsGlobalRule)
					{
						FCR_RuleType = FeatureControlRuleTypeList.Codes.Global;
					}
					else
					{
						FCR_RuleType = FeatureControlRuleTypeList.Codes.Client;
					}
				}
			}
		}
		public ZWrappedPropertyInfo IsFeatureSetRuleInfo => GetWrappedZPropertyInfo(nameof(IsFeatureSetRule), x => FCR_RuleTypeInfo);

		#endregion

		#region Parameters

		public override ZBool FCR_UseGlobalParameters
		{
			get => base.FCR_UseGlobalParameters;
			set
			{
				if (!FCR_UseGlobalParameters && value && !IsGlobalRule && !FCR_Parameters.IsEmpty)
				{
					var eventArgs = new CancelEventArgs();
					BeforeParametersOverwrite?.Invoke(this, eventArgs);
					if (eventArgs.Cancel)
					{
						FCR_UseGlobalParametersInfo.RefreshBinding();
						return;
					}
				}

				base.FCR_UseGlobalParameters = value;
				if (FCR_UseGlobalParameters)
				{
					FCR_Parameters = ZString.Empty;
					FCR_UseGlobalParametersInfo.RefreshBinding();
				}
			}
		}

		public event CancelEventHandler BeforeParametersOverwrite;

		public override ZString FCR_Parameters
		{
			get
			{
				return !FCR_UseGlobalParameters ? base.FCR_Parameters :
					ControlHeader.FeatureControlRules.OfType<FeatureControlRule>()
						.FirstOrDefault(x => x.PK != PK && x.IsGlobalRule)?.FCR_Parameters ?? "";
			}
			set => base.FCR_Parameters = value;
		}

		public bool FCR_UseGlobalParameters_ReadOnly => IsGlobalRule;

		public bool FCR_Parameters_ReadOnly => FCR_UseGlobalParameters;

		#endregion

		#region LicenceDatabase

		[ChildEditable]
		public FeatureControlRuleLicenceDatabasePivotCollection LicenceDatabasePivots
		{
			get
			{
				if (licenceDatabasePivots == null)
				{
					licenceDatabasePivots = new FeatureControlRuleLicenceDatabasePivotCollection(this);
					licenceDatabasePivots.CountChanged += (_, __) => DatabaseCountInfo.RefreshBinding();
					RegisterEditableChildObject(licenceDatabasePivots);
					licenceDatabasePivots.Load();
				}

				return licenceDatabasePivots;
			}
		}

		FeatureControlRuleLicenceDatabasePivotCollection licenceDatabasePivots;

		public bool CanAttachLicenceDatabase(LicenceDatabase licenceDatabase, out string errorMessage)
		{
			errorMessage = null;
			var result = !(FCR_IsActive && ControlHeader.FeatureControlRules.OfType<FeatureControlRule>()
				.Any(rule => rule.PK != PK && rule.FCR_IsActive
						&& rule.LicenceDatabasePivots.OfType<FeatureControlRuleLicenceDatabasePivot>()
			.Any(pvt => pvt.FCD_LD_LicenceDatabase == licenceDatabase.PK)));
			if (!result)
			{
				errorMessage = $"License Database {licenceDatabase.LD_DatabaseNumber} for client {licenceDatabase.WebAccessOrg?.OH_Code} is already attached to an active rule";
			}
			return result;
		}

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			if ((IsGlobalRule || IsFeatureSetRule) && LicenceDatabasePivots.Count > 0)
			{
				LicenceDatabasePivots.DeleteAll();
			}
		}

		public override void Delete()
		{
			if (ControlHeader != null)
			{
				ControlHeader.FCM_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}
			base.Delete();
		}

		public override bool CanDelete => !(IsGlobalRule && FCR_IsActive && ControlHeader.FeatureControlRules.OfType<FeatureControlRule>()
		.Any(x => x.PK != PK && x.FCR_UseGlobalParameters));

		public override MultilingualString ReasonForNotAbleToDelete => (NoResString)"This global rule cannot be deleted because its parameters are currently in use by other rules.";

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(FeatureControlRuleLicenceDatabasePivotSchema.FCD_FCR_FeatureControlRule, null);
			}
		}
	}
}
