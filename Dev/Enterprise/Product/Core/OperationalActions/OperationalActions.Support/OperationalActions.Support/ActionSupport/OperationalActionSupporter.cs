using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Security;

namespace Enterprise.Services.OperationalActions.Support
{
	public abstract class OperationalActionSupporter
	{
		public virtual ITargetRecordSelection GetModuleSelection(IZFilterGridModule module)
		{
			return new ModuleSelection(module);
		}

		public virtual string SingularElementNoun
		{
			get { return Res.GetString("OperationalActionSupporter|SingularElementNoun", "record"); }
		}

		public virtual string PluralElementNoun
		{
			get { return Res.GetString("OperationalActionSupporter|PluralElementNoun", "records"); }
		}

		public bool SupportsBulkUpdates
		{
			get { return SupportsBulkUpdatesCore; }
		}

		public OperationalActionMethodList Methods
		{
			get
			{
				if (methods == null)
				{
					methods = new OperationalActionMethodList(this);
					PopulateMethods(methods);
				}
				return methods;
			}
		}

		protected virtual bool SupportsBulkUpdatesCore
		{
			get { return true; }
		}

		protected virtual void PopulateMethods(OperationalActionMethodList list)
		{
			if (RootType != null)
			{
				list.Add(ActionMethodProviderIDs.General);
			}
		}

		/// <summary>
		/// Run prior to and subsequent to PseudoApplicator method ApplyCore() being run.
		/// Example use: Indicating to the business objects in targets that they are about to be edited,
		/// and that the editing has finished.
		/// </summary>
		/// <param name="targets">The business objects that PseudoApplicator is about to run on</param>
		/// <param name="pseudoApplicaion">Action for calling the PseudoApplicator method ApplyCore()</param>
		public virtual void AroundPseudoApplication(BusinessObject[] targets, Action pseudoApplication)
		{
			pseudoApplication();
		}

		public abstract Type RootType { get; }
		public abstract BusinessContext BusinessContext { get; }
		public virtual SecurityCheckpoint BaseCheckpoint => Env.Security.None;
		public virtual SecurityCheckpoint CustomizationSecurityCheckpoint
		{
			get
			{
				return Env.Security.FindOrCreateOperationalActionsCustomiseCheckpoint(BaseCheckpoint);
			}
		}
		public virtual SecurityCheckpoint RunSecurityCheckpoint
		{
			get
			{
				return Env.Security.FindOrCreateOperationalActionsRunCheckpoint(BaseCheckpoint);
			}
		}
		public virtual SecurityCheckpoint AllowRunOnAllMatchingRecordsCheckpoint
		{
			get
			{
				return Env.Security.FindOrCreateOperationalActionsAllowRunOnAllMatchingRecordsCheckpoint(BaseCheckpoint);
			}
		}

		OperationalActionMethodList methods;

		public virtual ResourceStringData AllowAllResourceStringData
		{
			get { return null; }
		}

		public virtual BusinessContext DocumentBusinessContext
		{
			get { return BusinessContext; }
		}
	}
}
