using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADIntegrationActivator : IADIntegrationActivator
	{
		public ADIntegrationActivator(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		#region EnableIntegration

		public IEnumerable<EntitySynchronisedEventArgs> EnableIntegration(EntitiesToSync entitiesToSync)
		{
			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.DefaultValue); // Ensure to sync all
			var syncResult = Synchronise();
			EnableADServiceTask();
			return syncResult;

			List<EntitySynchronisedEventArgs> Synchronise()
			{
				var syncHistories = new List<EntitySynchronisedEventArgs>();
				// Get a new syncDirector every time we enable and sync
				syncDirector = ObjectFactory.Get<ISynchronisationDirectorProvider>().GetSyncDirector(new BusinessObjectFactory(), null);
				syncDirector.EntitySynchronised += (s, e) => syncHistories.Add(e);
				syncDirector.Synchronise(entitiesToSync);
				RemoveHistoryOfEntitiesWithErrors(syncHistories);
				return syncHistories;
			}

			void RemoveHistoryOfEntitiesWithErrors(List<EntitySynchronisedEventArgs> syncHistories)
			{
				foreach (var entity in syncDirector.EntitiesWithErrors)
				{
					syncHistories.RemoveAll((e) => e.Entity == entity);
				}
			}

			void EnableADServiceTask()
			{
				ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskIsActive(Constants.ActiveDirectorySynchronisationTask.Code, isActive: true);
			}
		}

		ISynchronisationDirector syncDirector;

		#endregion

		#region DisableIntegration

		public bool DisableIntegration(bool disableGroupOnly = false)
		{
			//Disable User
			if (!disableGroupOnly)
			{
				foreach (var staff in GetTrackedStaff())
				{
					staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
					staff.GS_ChangePasswordAtNextLogin = ZBool.True;
					staff.LocalPasswordMustBeReset = true;
					staff.SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
				}
			}

			//Disable Group
			foreach (var group in GetTrackedGroups())
			{
				group.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
				group.SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			}

			ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskIsActive(Constants.ActiveDirectorySynchronisationTask.Code, isActive: disableGroupOnly);

			if (!disableGroupOnly)
			{
				//Clear OneOffSyncMode when disable AD Integration
				ActiveDirectoryRegistry.Instance.OneOffSyncMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			}
			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value);
			return true;
		}

		GlbStaff[] GetTrackedStaff()
		{
			return factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null));
		}

		GlbGroup[] GetTrackedGroups()
		{
			return factory.Load<GlbGroup>(new ZQuery(GlbGroupSchema.GG_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null));
		}

		#endregion

		#region SaveChanges

		public void SaveChanges()
		{
			if (syncDirector != null)
			{
				syncDirector.Save(); // This is to save bizos that have been synced
			}
			factory.Save(); // This is to save enable service task
		}

		#endregion
	}
}
