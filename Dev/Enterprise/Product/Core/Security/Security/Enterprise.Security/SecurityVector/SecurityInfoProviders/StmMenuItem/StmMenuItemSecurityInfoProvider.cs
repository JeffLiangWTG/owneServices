using CargoWise.Common;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Provider
{
	abstract class StmMenuItemSecurityInfoProvider : SecurityInfoProvider
	{
		internal StmMenuItemSecurityInfoProvider(SecurityInfoProvider parent, INamedModule module, StmMenuItemsForSecurity menuItems, int index, StmMenuItemCheckpointHelper helper)
			: base(parent, GetDocumentCheckPoint(parent, module, menuItems, index, helper))
		{
		}

		public override string Name { get { return Checkpoint.DisplayText; } }

		internal static SecurityCheckpoint GetDocumentCheckPoint(SecurityInfoProvider parent, INamedModule module, StmMenuItemsForSecurity menuItems, int index, StmMenuItemCheckpointHelper helper)
		{
			Argument.NotNull(helper, "helper");

			var item = menuItems.GetValue(index); //O(1)
			bool isDuplicate = menuItems.IsDuplicate(item); //O(1)
			string code = helper.ModuleIDPrefix + module.ModuleID.ToString();
			if (parent.Security.FindCheckPoint(new CheckpointLookupKey(code, item.PK.ToGuid())) == null)
			{
				var checkpointDisplayText = string.IsNullOrEmpty(item.SU_MenuPath) ? (string)item.SU_MenuName : (item.SU_MenuPath.Replace("/", " -- ") + " -- " + item.SU_MenuName);
				if (isDuplicate)
				{
					var description = item.SU_Hint.IsEmpty ? helper.FallbackHint : item.SU_Hint.ToString();
					checkpointDisplayText += " (" + description + ")";
				}

				return new SecurityCheckpoint(code, (NoResString)checkpointDisplayText, (SecurityCheckpoint)parent.Checkpoint, parent.Security, item.PK.ToGuid());
			}

			return null;
		}
	}
}
