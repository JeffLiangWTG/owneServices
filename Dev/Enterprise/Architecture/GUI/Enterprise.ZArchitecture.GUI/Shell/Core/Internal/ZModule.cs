using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	public abstract class ZModule : Disposable, IZModule, IZModuleHelper, IZModuleInternals
	{
		#region ID / Name

		public abstract ModuleIdentifier ID { get; }

		public MultilingualString Description
		{
			get { return (IDInfo == null) ? ID.Description : IDInfo.Description; }
		}

		protected ModuleInfo IDInfo
		{
			get
			{
				if (idInfo == null || idInfo.ID != ID)
				{
					idInfo = ZModuleFactory.Instance.GetRegisteredModuleInfo(ID, Enterprise.ZArchitecture.Business.StaticCurrentFetcher.Instance.CurrentCompany.Country.RN_Code, true);
				}
				return idInfo;
			}
		}
		ModuleInfo idInfo;

		#endregion

		public IZLimitedColumnsProvider LimitedColumns { get; set; }

		#region Getting an Instance of ZModule

		public static ZModule GetZModule(ModuleIdentifier moduleID, string countryOverride = null)
		{
			if (!string.IsNullOrEmpty(countryOverride))
			{
				return ZModuleFactory.Instance.Create(moduleID, countryOverride);
			}
			else
			{
				return ZModuleFactory.Instance.Create(moduleID);
			}
		}

		#endregion

		#region Allowed Actions

		public virtual bool AllowNew
		{
			get { return allowNew; }
		}
		bool allowNew = true;

		public virtual bool AllowEdit
		{
			get { return allowEdit; }
		}
		bool allowEdit = true;

		public virtual bool AllowView
		{
			get { return true; }
		}

		public virtual bool AllowDelete
		{
			get { return allowDelete; }
		}
		bool allowDelete = true;

		public virtual bool AllowDefaultActivateDeactivate
		{
			get { return false; }
		}

		public virtual bool SupportsWorkflow
		{
			get { return false; }
		}

		public virtual bool AllowUniversalCopy
		{
			get { return true; }
		}

		public virtual bool BypassParentSecurityCheckpointVerification
		{
			get { return false; }
		}

		public virtual bool CouldAllowUniversalCopy
		{
			get { return AllowUniversalCopy && ObjectFactory.New<IModuleUniversalCopyManager>(this).AllowsUniversalCopy; }
		}

		public virtual bool SupportsConversations
		{
			get { return false; }
		}

		public virtual bool AllowToggleFilterVisibilityMenuItem
		{
			get
			{
				return allowToggleFilterVisibilityMenuItem;
			}
			set
			{
				allowToggleFilterVisibilityMenuItem = value;
			}
		}
		bool allowToggleFilterVisibilityMenuItem = true;

		public bool AllowToggleIndexSearchFilterMenuItemVisibility { get; set; } = true;

		#endregion

		#region Business Contexts

		public virtual BusinessContext[] BusinessContexts
		{
			get { return null; }
		}

		#endregion

		#region Show
		public virtual IZForm GetForm()
		{
			return null;
		}

		public virtual IZForm ShowPopup()
		{
			return null;
		}

		#endregion

		#region WorkflowSecurity

		public virtual ISecurityCheckpoint GetWorkflowTasksCheckpoint(string workflowTasksCode)
		{
			return Env.Security.FindOrCreateWorkflowItemCheckpoint(this.SecurityCheckpoint, workflowTasksCode);
		}

		public virtual ISecurityCheckpoint GetWorkflowTasksCheckpoint(IBusiness parentJob, string workflowTasksCode)
		{
			return GetWorkflowTasksCheckpoint(workflowTasksCode);
		}

		public virtual SecurityCheckpoint GetWorkflowTasksCheckpointFromParentModuleID()
		{
			SecurityCheckpoint securityCheckpoint = Env.Security.None;

			if (SecurityCheckpointDefault != null && ParentModuleID == ModuleIDs.NotAssigned)
			{
				return SecurityCheckpointDefault;
			}
			else if (ParentModuleID != null && ParentModuleID != ModuleIDs.NotAssigned && WorkflowItemCheckpointCode != null)
			{
				using (ZModule module = ZModuleFactory.Instance.Create(ParentModuleID))
				{
					if (module.SecurityCheckpoint != Env.Security.None && module.SupportsWorkflow)
					{
						securityCheckpoint = Env.Security.FindOrCreateWorkflowItemCheckpoint(module.SecurityCheckpoint, WorkflowItemCheckpointCode);
					}
				}
			}

			return securityCheckpoint;
		}

		public ModuleIdentifier ParentModuleID { get; set; } = ModuleIDs.NotAssigned;

		public virtual string WorkflowItemCheckpointCode { get; }

		public virtual SecurityCheckpoint SecurityCheckpointDefault { get; }

		#endregion

		#region Dispose

		protected override void Dispose(bool isDisposing)
		{
		}

		#endregion

		#region INamedModule Members

		ModuleIdentifier INamedModule.ModuleID
		{
			get { return ID; }
		}

		#endregion

		#region ISecured Members

		public abstract SecurityCheckpoint SecurityCheckpoint { get; }

		ISecurityCheckpoint ISecured.SecurityCheckpoint
		{
			get { return SecurityCheckpoint; }
		}

		public virtual SecurityCheckpoint[] GetSecurityCheckpointForPopups()
		{
			return SecurityCheckpoint != null ? new[] { Env.Security.FindOrCreateAccessModuleCheckPoint(SecurityCheckpoint) } : null;
		}
		ISecurityCheckpoint[] ISecured.GetSecurityCheckpointForPopups()
		{
			return GetSecurityCheckpointForPopups();
		}

		#endregion

		#region ISecuredModule Members

		public LicenceCheckpoint LicenceCheckPoint
		{
			get
			{
				return LicenceCheckPointOverride as LicenceCheckpoint ?? LicenceCheckPointCore;
			}
		}
		protected abstract LicenceCheckpoint LicenceCheckPointCore { get; }

		ILicenceCheckpoint ISecuredModule.LicenceCheckpoint
		{
			get { return LicenceCheckPoint; }
		}

		public ILicenceCheckpoint LicenceCheckPointOverride { get; set; }

		#endregion

		#region IIdentifiable Members

		string IIdentifiable.ID => ID.ToString();

		#endregion

		#region ReadOnly Setter

		void IZModuleInternals.SetReadOnly(bool isReadOnly)
		{
			allowNew = !isReadOnly;
			allowEdit = !isReadOnly;
			allowDelete = !isReadOnly;
		}

		#endregion

#if DEBUG
		public virtual bool IsExcludedFromUtcTestsEgNoGuiNeededOrNoFilterBusinessStripNeeded
		{
			get { return false; }
		}
#endif
	}
}
