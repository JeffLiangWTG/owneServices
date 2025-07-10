using System;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.AuditDataServices.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.ZArchitecture.GUI.ZAudit.PlugIn
{
	/// <summary>
	/// Module Controller for Audit Plugin.
	/// </summary>
	public class AuditController : ZSingletonController
	{
		public AuditController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Audit; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Audit; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Audit); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("Audit|DatabaseAudit", "Audit Data", "The audit data tab."); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var audit = (Audit)base.GetNewBusinessEntityInLocalFactory();
			return audit;
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		#region PlugIn

		[ThreadStatic]
		static bool? isAuditServerSetInRegistry;

		public static bool IsAuditServerSetInRegistry
		{
			get
			{
				if (isAuditServerSetInRegistry == null)
				{
					string registryValue = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
					isAuditServerSetInRegistry = !string.IsNullOrEmpty(registryValue);
				}
				return isAuditServerSetInRegistry.Value;
			}
		}

#if DEBUG
		public static void ResetIsAuditServerSetInRegistry()
		{
			isAuditServerSetInRegistry = null;
		}
#endif

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			if (CheckPointForNew.IsAllowed)
			{
#if DEBUG
				if (Environment.Globals.IsTest)
				{
					return null;
				}
#endif

				if (!IsAuditServerSetInRegistry)
				{
					return null;
				}

				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
				if (!AuditServerValidator.AuditHasValidServerHost(auditServer))
				{
					return null;
				}

				var auditParentBizObj = businessEntity as BusinessObject;
				return new AuditPlugIn(new Audit(auditParentBizObj, auditServer));
			}
			else
			{
				return new AuditPlugIn(new ResourceStringData("913028A6-263F-4291-A8B4-404DF342B236", CheckPointForNew.ErrorMessageForNotAllowed));
			}
		}

		#endregion

		#region FormOverrides

		public override IZForm ShowNewForm()
		{
			throw new ModuleGuiNotSupportedException("Audit plugin does not support ShowNewForm");
		}

		#endregion
	}
}
