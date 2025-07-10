using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory.Synchronisation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public class SynchronisationDirector : ISynchronisationDirector
	{
		public SynchronisationDirector(BusinessObjectFactory factory, IEnumerable<BusinessObject> objectsToSync = null)
		{
			synchroniser = objectsToSync != null
				? new SelectiveEntitySynchroniser(factory, objectsToSync)
				: new EntitySynchroniser(factory);
			synchroniser.ProgressUpdated += (s, e) => ProgressUpdated(e);
		}

		readonly IEntitySynchroniser synchroniser;

		ISecurityCheckpoint[] GetCheckSecurityCheckPoints(EntitiesToSync? entitiesToSync)
		{
			if (entitiesToSync == null)
			{
				entitiesToSync = ActiveDirectoryRegistry.Instance.EntitiesToSync;
			}

			ISecurityCheckpoint[] checkpoints;

			if (entitiesToSync == EntitiesToSync.UsersAndGroups)
			{
				checkpoints = new[] {
						EnvProxy.Instance.Security.FindCheckPoint("StaffViewHomeAddressDetails"),
						EnvProxy.Instance.Security.FindCheckPoint("GroupsModify")
					};
			}
			else
			{
				checkpoints = new[] {
						EnvProxy.Instance.Security.FindCheckPoint("StaffViewHomeAddressDetails"),
					};
			}

			return checkpoints;
		}

		public void Synchronise(EntitiesToSync? entitiesToSync = null, SyncMode? preferredSyncMode = null)
		{
			try
			{
				var checkpoints = GetCheckSecurityCheckPoints(entitiesToSync);
				if (checkpoints.All(c => c.IsAllowed))
				{
					synchroniser.Synchronise(entitiesToSync, preferredSyncMode);
				}
				else
				{
					throw new SecurityAccessDeniedException(EnvProxy.Instance.Security.GetErrorMessageForNotAllowed(checkpoints));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ProgressUpdated(new SyncProgressEventArgs { OverallPercentComplete = 100 });
				throw;
			}
		}

		public IEnumerable<IADEntity> EntitiesWithErrors => synchroniser.EntitiesWithErrors;

		public void Save() => synchroniser.Save();

		public void SyncUsersToRoboticGroupIfRequired(IEnumerable<IADEntity> entities) => synchroniser.SyncUsersToRoboticGroupIfRequired(entities);

		#region Progress

		public event EntitySynchronisedEventHandler EntitySynchronised
		{
			add { synchroniser.EntitySynchronised += value; }
			remove { synchroniser.EntitySynchronised -= value; }
		}

		void ProgressUpdated(SyncProgressEventArgs e)
		{
			var status = string.Format("{0}{1}", e.TaskName, !string.IsNullOrWhiteSpace(e.TaskName) && !string.IsNullOrWhiteSpace(e.TaskDetails) ? ": " + e.TaskDetails : !string.IsNullOrEmpty(e.TaskDetails) ? e.TaskDetails : "");
			if (!string.IsNullOrEmpty(status))
			{
				Progress.Status = status;
			}
			if (e.OverallPercentComplete.HasValue)
			{
				Progress.PercentComplete = e.OverallPercentComplete.Value;
				if (e.OverallPercentComplete >= 100)
				{
					HideProgressForm();
				}
			}
		}

		public IProgressForm Progress
		{
			get
			{
				if (progressForm == null)
				{
					var form = new ProgressForm
					{
						ShowCancelButton = false,
						ShowProgressBar = true,
					};
					var owner = Form.ActiveForm;
					if (owner != null)
					{
						form.ShowModalTo(owner);
					}
					else
					{
						form.Show();
					}

					progressForm = form;
				}

				return progressForm;
			}
			set { progressForm = value; }
		}
		IProgressForm progressForm;

		void HideProgressForm()
		{
			if (progressForm != null)
			{
				progressForm.Dispose();
				progressForm = null;
			}
		}

		#endregion
	}
}
