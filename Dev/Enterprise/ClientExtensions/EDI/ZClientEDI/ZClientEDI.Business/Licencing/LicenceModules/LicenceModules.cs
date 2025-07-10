using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	[DependentBusinessObject(typeof(LicenceHeader), "Modules")]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class LicenceModules : AutoLicenceModules
	{
		public LicenceModules(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region LM_Calc_GroupModuleDescription

		[MaxLength(25)]
		public ZString LM_Calc_GroupModuleDescription
		{
			get { return LicenceModuleList.Instance.GetDescription(LM_GroupModuleCode); }
		}

		public ZPropertyInfo LM_Calc_GroupModuleDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(LM_Calc_GroupModuleDescription)); }
		}

		#endregion

		#region LM_Calc_IndentedGroupModuleDescription

		[MaxLength(25)]
		public ZString LM_Calc_IndentedGroupModuleDescription
		{
			get
			{
				if (LicHeader != null)
				{
					return LicHeader.CalcIndentedGroupModuleDescription(LM_GroupModuleCode);
				}
				else
				{
					return LicenceModuleList.Instance.GetIndentedDescriptionFromCode(LM_GroupModuleCode);
				}
			}
		}

		public ZPropertyInfo LM_Calc_IndentedGroupModuleDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(LM_Calc_IndentedGroupModuleDescription)); }
		}

		#endregion

		#region LM_Calc_IsEnabled

		public ZBool LM_Calc_IsEnabled
		{
			get { return lM_Calc_IsEnabled ?? (lM_Calc_IsEnabled = LM_LicenceType != LicenceTypes.Codes.NON).Value; }
			set
			{
				lM_Calc_IsEnabled = value;
				if (value && LicHeader != null)
				{
					if (!IsCoreModule)
					{
						var checkpoint = LegacyLicence.Instance.GetCheckpointFromCode(LM_GroupModuleCode);
						var defaultValue = checkpoint == null ? LicenceTypes.Codes.NON : checkpoint.GetDefaultEnabledLicenceValue();
						if (defaultValue == LicenceTypes.Codes.NON)
						{
							LicenceModules coreModule = LicHeader.GetCoreModule();
							LM_LicenceType = coreModule.LM_LicenceType;
							if (LM_LicenceType != LicenceTypes.Codes.ODM)
							{
								LM_UserCount = coreModule.LM_UserCount;
							}
							LM_ExpiryDate = coreModule.LM_ExpiryDate;
						}
						else
						{
							LM_LicenceType = defaultValue;
						}
					}
					else
					{
						LM_LicenceType = LicHeader.IsOnDemandModuleTypeAllowed ? LicenceTypes.Codes.ODM : LicenceTypes.Codes.PUR;
					}
				}
				else
				{
					using (GetValidationSuspender())
					{
						LM_LicenceType = LicenceTypes.Codes.NON;
						LM_UserCount = 0;
						LM_ExpiryDate = ZDateTime.Empty;
					}
				}
				new DependencyBetweenModulesRule("HVS", new string[] { "SCD", "SCR" }).ExecuteRule(this);
				LM_Calc_IsEnabledInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LM_Calc_IsEnabledInfo
		{
			get { return GetZPropertyInfo(nameof(LM_Calc_IsEnabled)); }
		}

		bool? lM_Calc_IsEnabled;

		#endregion

		#region LM_ExpiryDate

		[ReadOnlyMember(nameof(ExpiryDateReadOnly))]
		public override ZDateTime LM_ExpiryDate
		{
			get { return base.LM_ExpiryDate; }
			set
			{
				if (base.LM_ExpiryDate != value)
				{
					base.LM_ExpiryDate = value;
				}
			}
		}

		protected bool ExpiryDateReadOnly
		{
			get
			{
				return ModuleLicenceDetailsReadOnly
					|| (LM_LicenceType != LicenceTypes.Codes.REN && LM_LicenceType != LicenceTypes.Codes.TRI);
			}
		}

		#endregion

		#region LM_LicenceType

		[List("Lookups.LicenceTypesList")]
		[ReadOnlyMember(nameof(ModuleLicenceDetailsReadOnly))]
		public override ZString LM_LicenceType
		{
			get { return base.LM_LicenceType; }
			set
			{
				if (base.LM_LicenceType != value)
				{
					base.LM_LicenceType = value;

					switch (LM_LicenceType)
					{
						case LicenceTypes.Codes.ODM:
							if (UserCountReadOnly)
							{
								LM_UserCount = 0;
							}
							break;

						case LicenceTypes.Codes.SRU:
							LM_UserCount = 9999;
							break;

						case LicenceTypes.Codes.REN:
							if (LicHeader != null)
							{
								LM_ExpiryDate = LicHeader.LA_ContractExpiryDate;
							}
							break;
					}
				}
			}
		}

		public bool HasLineLevelOnDemandLicenceType
		{
			get
			{
				switch (LM_LicenceType)
				{
					case LicenceTypes.Codes.ODM:
					case LicenceTypes.Codes.OTM:
					case LicenceTypes.Codes.SRU:
						return true;

					default:
						return false;
				}
			}
		}

		#endregion

		#region LM_UserCount

		[ReadOnlyMember(nameof(UserCountReadOnly))]
		public override ZShort LM_UserCount
		{
			get { return base.LM_UserCount; }
			set
			{
				if (base.LM_UserCount != value)
				{
					base.LM_UserCount = value;
				}
			}
		}

		public bool UserCountReadOnly
		{
			get
			{
				return ModuleLicenceDetailsReadOnly || (LicHeader != null && LicHeader.IsPureOnDemand);
			}
		}

		#endregion

		#endregion

		#region Related Business objects

		public LicenceModules ParentModule
		{
			get
			{
				LicenceModules parent = null;
				string parentCode = LicenceModuleList.Instance.GetCodeOfModulesParent(LM_GroupModuleCode);
				if (!string.IsNullOrEmpty(parentCode) && LicHeader != null)
				{
					parent = LicHeader.Modules.FindByCode(parentCode);
				}
				return parent;
			}
		}

		public LicenceModules GetNonOnDemandParentModule()
		{
			LicenceModules result = this;
			do
			{
				result = result.ParentModule;
			}
			while (result != null && result.LM_LicenceType == LicenceTypes.Codes.ODM);
			return result;
		}

		public LicenceHeader LicHeader
		{
			get { return Factory.Load<LicenceHeader>(LM_LA); }
		}

		#endregion

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		ZString CommentForChangedModule
		{
			get
			{
				ZString moduleChangedComment = ZString.Empty;
				moduleChangedComment = LogHelper.AddChangeLog(moduleChangedComment, "User Limit", LM_UserCountInfo);
				moduleChangedComment = LogHelper.AddChangeLog(moduleChangedComment, "Licence Type", LM_LicenceTypeInfo);
				moduleChangedComment = LogHelper.AddShortDateChangeLog(moduleChangedComment, "Expiry Date", LM_ExpiryDateInfo);
				return moduleChangedComment;
			}
		}

		public override void OnSaving()
		{
			ZString moduleChangedComment = CommentForChangedModule;

			if (!moduleChangedComment.IsEmpty)
			{
				moduleChangedComment =
					"Server " + ((LicHeader != null && LicHeader.Database != null) ? LicHeader.Database.LD_ServerCode.ToString() : "N/A") +
					" Module " + LM_Calc_GroupModuleDescription + ": " + moduleChangedComment;

				EnterpriseBusinessObject logParent = (EnterpriseBusinessObject)LicHeader ?? this;
				var builder = EventLogReferenceBuilder.New()
					.AddShortenable(moduleChangedComment);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				SaveLogEvent = logParent.Logs.AddNew(Events.EditedARecord, builder.Build());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			CheckForDuplicateLicenceModules();

			base.OnSaving();
		}

		void CheckForDuplicateLicenceModules()
		{
			ZQuery query = new ZQuery(Enterprise.ZArchitecture.Schema.LicenceModulesSchema.LM_GroupModuleCode, this.LM_GroupModuleCode);
			query.AddToFilter(Enterprise.ZArchitecture.Schema.LicenceModulesSchema.LM_LA, this.LM_LA);
			Factory.ClearQueryCache();
			LicenceModules[] duplicateModules = Factory.Load<LicenceModules>(query);
			foreach (LicenceModules module in duplicateModules)
			{
				if (module.PK != this.PK)
				{
					module.Delete();
				}
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && SaveLogEvent != null)
			{
				SaveLogEvent.Delete();
			}
			base.OnSaved(saveSucceeded);
		}

		StmALog SaveLogEvent;

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		protected bool ModuleLicenceDetailsReadOnly
		{
			get { return !LM_Calc_IsEnabled || (!IsCoreModule && LicHeader == null); }
		}

		bool IsCoreModule
		{
			get { return LM_GroupModuleCode == LegacyLicence.Codes.Core; }
		}

		public void CopyTransientPropertiesFrom(LicenceModules source)
		{
			if (source != null)
			{
				this.lM_Calc_IsEnabled = source.lM_Calc_IsEnabled;
			}
		}
	}
}

