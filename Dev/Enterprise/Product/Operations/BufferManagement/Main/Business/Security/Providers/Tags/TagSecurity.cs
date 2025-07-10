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
	public sealed class TagSecurity : TagSecurityBase, ISecurityCheckpointProvider
	{
		#region Checkpoint Codes

		static string GetTagAddSecurityCheckPointCode(ZGuid pk)
		{
			return "TagAdd" + pk;
		}

		static string GetTagRemoveSecurityCheckPointCode(ZGuid pk)
		{
			return "TagRemove" + pk;
		}

		#endregion

		#region ISecurityCheckpointProvider Members

		void ISecurityCheckpointProvider.LoadCheckpoints(BusinessObjectFactory factory, IZSecurity securityInstance)
		{
			var tagsAddCheckpoint = securityInstance.FindCheckPoint(new CheckpointLookupKey(Env.Security.TagAdd.Code));
			var tagsRemoveCheckpoint = securityInstance.FindCheckPoint(new CheckpointLookupKey(Env.Security.TagRemove.Code));

			var ownerGroups = GetOwnerGroups(factory);
			var groupedTags = GetActiveTags(factory, isWorkQueue: false);

			foreach (var grouping in groupedTags)
			{
				var ownerGroup = ownerGroups[grouping.Key];
				var tagAddCheckpoint = new Lazy<SecurityCheckpoint>(() => new SecurityCheckpoint(GetTagAddSecurityCheckPointCode(grouping.Key), (NoResString)ownerGroup.DisplayName, tagsAddCheckpoint, securityInstance), isThreadSafe: false);
				var tagRemoveCheckpoint = new Lazy<SecurityCheckpoint>(() => new SecurityCheckpoint(GetTagRemoveSecurityCheckPointCode(grouping.Key), (NoResString)ownerGroup.DisplayName, tagsRemoveCheckpoint, securityInstance), isThreadSafe: false);

				foreach (var tag in grouping)
				{
					tagAddCheckpoint.Value.AddChild(new SecurityCheckpoint(GetTagAddSecurityCheckPointCode(tag.PK), (NoResString)tag.DisplayText, null, securityInstance));
					tagRemoveCheckpoint.Value.AddChild(new SecurityCheckpoint(GetTagRemoveSecurityCheckPointCode(tag.PK), (NoResString)tag.DisplayText, null, securityInstance));
				}
			}
		}

		#endregion

		#region SecurityChecks

		public static bool CheckTagAddSecurity(ITagMagnitude tag, bool showSecurityDialog = true)
		{
			Argument.NotNull(tag, "tag");

			var parentCheckpoint = Env.Security.TagAdd.FindChild(GetTagAddSecurityCheckPointCode(tag.TGM_GG_OwnerGroup));
			var tagAddSecurityCheckPointCode = GetTagAddSecurityCheckPointCode(tag.PK);
			return CheckSecurity(parentCheckpoint, tagAddSecurityCheckPointCode, showSecurityDialog);
		}

		public static bool CheckTagRemoveSecurity(ITagMagnitude tag, bool showSecurityDialog = true)
		{
			Argument.NotNull(tag, "tag");

			var parentCheckpoint = Env.Security.TagRemove.FindChild(GetTagRemoveSecurityCheckPointCode(tag.TGM_GG_OwnerGroup));
			var tagRemoveSecurityCheckPointCode = GetTagRemoveSecurityCheckPointCode(tag.PK);
			return CheckSecurity(parentCheckpoint, tagRemoveSecurityCheckPointCode, showSecurityDialog);
		}

		#endregion
	}
}
