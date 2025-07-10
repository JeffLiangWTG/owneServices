using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	public sealed class WorkQueueSecurity : TagSecurityBase, ISecurityCheckpointProvider
	{
		#region Checkpoint Codes

		static string GetAddToQueueSecurityCheckPointCode(ZGuid pk)
		{
			return "AddTo-Q" + pk;
		}

		static string GetRemoveFromQueueSecurityCheckPointCode(ZGuid pk)
		{
			return "RemoveFrom-Q" + pk;
		}

		static string GetResequenceQueueSecurityCheckPointCode(ZGuid pk)
		{
			return "Resequence-Q" + pk;
		}

		#endregion

		#region Implementation

		void ISecurityCheckpointProvider.LoadCheckpoints(BusinessObjectFactory factory, IZSecurity securityInstance)
		{
			var addToQueueCheckpoint = securityInstance.FindCheckPoint(new CheckpointLookupKey(Env.Security.WorkQueuesAddToQueue.Code));
			var removeFromQueueCheckpoint = securityInstance.FindCheckPoint(new CheckpointLookupKey(Env.Security.WorkQueuesRemoveFromQueue.Code));
			var resequenceQueueCheckpoint = securityInstance.FindCheckPoint(new CheckpointLookupKey(Env.Security.WorkQueuesResequenceQueue.Code));

			var ownerGroups = GetOwnerGroups(factory);
			var groupedWorkQueues = GetActiveTags(factory, isWorkQueue: true);

			foreach (var grouping in groupedWorkQueues)
			{
				if (ownerGroups.TryGetValue(grouping.Key, out var ownerGroup))
				{
					var queueAddToCheckpoint = new Lazy<SecurityCheckpoint>(() => new SecurityCheckpoint(GetAddToQueueSecurityCheckPointCode(ownerGroup.PK), (NoResString)ownerGroup.DisplayName, addToQueueCheckpoint, securityInstance), isThreadSafe: false);
					var queueRemoveFromCheckpoint = new Lazy<SecurityCheckpoint>(() => new SecurityCheckpoint(GetRemoveFromQueueSecurityCheckPointCode(ownerGroup.PK), (NoResString)ownerGroup.DisplayName, removeFromQueueCheckpoint, securityInstance), isThreadSafe: false);
					var queueResequenceCheckpoint = new Lazy<SecurityCheckpoint>(() => new SecurityCheckpoint(GetResequenceQueueSecurityCheckPointCode(ownerGroup.PK), (NoResString)ownerGroup.DisplayName, resequenceQueueCheckpoint, securityInstance), isThreadSafe: false);

					foreach (var workQueue in grouping)
					{
						queueAddToCheckpoint.Value.AddChild(new SecurityCheckpoint(GetAddToQueueSecurityCheckPointCode(workQueue.PK), (NoResString)workQueue.DisplayText, null, securityInstance));
						queueRemoveFromCheckpoint.Value.AddChild(new SecurityCheckpoint(GetRemoveFromQueueSecurityCheckPointCode(workQueue.PK), (NoResString)workQueue.DisplayText, null, securityInstance));
						queueResequenceCheckpoint.Value.AddChild(new SecurityCheckpoint(GetResequenceQueueSecurityCheckPointCode(workQueue.PK), (NoResString)workQueue.DisplayText, null, securityInstance));
					}
				}
			}
		}

		#endregion

		#region SecurityChecks

		public static bool CheckAddToQueueSecurity(ITagMagnitude tag, bool showSecurityDialog = true)
		{
			Argument.NotNull(tag, "tag");

			var parentCheckpoint = Env.Security.WorkQueuesAddToQueue.FindChild(GetAddToQueueSecurityCheckPointCode(tag.TGM_GG_OwnerGroup));
			var addToQueueSecurityCheckPointCode = GetAddToQueueSecurityCheckPointCode(tag.PK);
			return CheckSecurity(parentCheckpoint, addToQueueSecurityCheckPointCode, showSecurityDialog);
		}

		public static bool CheckRemoveFromQueueSecurity(ITagMagnitude tag, bool showSecurityDialog = true)
		{
			Argument.NotNull(tag, "tag");

			var parentCheckpoint = Env.Security.WorkQueuesRemoveFromQueue.FindChild(GetRemoveFromQueueSecurityCheckPointCode(tag.TGM_GG_OwnerGroup));
			var removeFromQueueSecurityCheckPointCode = GetRemoveFromQueueSecurityCheckPointCode(tag.PK);
			return CheckSecurity(parentCheckpoint, removeFromQueueSecurityCheckPointCode, showSecurityDialog);
		}

		public static bool CheckResequenceQueueSecurity(ITagMagnitude tag, bool showSecurityDialog = false)
		{
			Argument.NotNull(tag, "tag");

			var parentCheckpoint = Env.Security.WorkQueuesResequenceQueue.FindChild(GetResequenceQueueSecurityCheckPointCode(tag.TGM_GG_OwnerGroup));
			var resequenceQueueSecurityCheckPointCode = GetResequenceQueueSecurityCheckPointCode(tag.PK);
			return CheckSecurity(parentCheckpoint, resequenceQueueSecurityCheckPointCode, showSecurityDialog);
		}

		#endregion
	}
}
