using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.Core;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Security.Core.ResString;

namespace Enterprise.Security
{
	public class CRMSecurity
	{
		public SecurityCheckpoint IgnoreOSMG { get; private set; }
		public SecurityCheckpoint IgnoreTaskAssignment { get; private set; }
		public SecurityCheckpoint ViewByStaffNotAssigned { get; private set; }
		public SecurityCheckpoint EditByStaffNotAssigned { get; private set; }
		public ReadOnlyDictionary<string, SecurityCheckpoint> EditByStaffRoleAssignedLookup { get; private set; }
		public bool IsViewOnly
		{
			get
			{
				return EditByStaffNotAssigned == null;
			}
		}

		readonly IZSecurity SecurityInstance;

		internal CRMSecurity(IZSecurity securityInstance)
		{
			SecurityInstance = securityInstance;
		}

		internal void CreateViewCheckpoint(SecurityCheckpoint viewParent)
		{
			ViewByStaffNotAssigned = new SecurityCheckpoint(string.Concat(viewParent.Code, ViewByStaffNotAssignedCheckpointSuffix), Constants.CRMSecurityCaptions.ViewByStaffNotAssigned, viewParent, SecurityInstance);
		}

		internal void CreateOSMGCheckpoint(SecurityCheckpoint osmgParent)
		{
			IgnoreOSMG = new SecurityCheckpoint(string.Concat(osmgParent.Code, IgnoreOSMGCheckpointSuffix), Constants.CRMSecurityCaptions.IgnoreOSMG, osmgParent, SecurityInstance);
		}

		internal void CreateTaskAssignmentCheckpoint(SecurityCheckpoint taskAssignmentParent)
		{
			IgnoreTaskAssignment = new SecurityCheckpoint(string.Concat(taskAssignmentParent.Code, IgnoreTaskAssignmentCheckpointSuffix), Constants.CRMSecurityCaptions.IgnoreTaskAssignment, taskAssignmentParent, SecurityInstance);
		}

		internal void CreateEditCheckpoints(SecurityCheckpoint editParent)
		{
			EditByStaffNotAssigned = new SecurityCheckpoint(string.Concat(editParent.Code, EditByStaffNotAssignedCheckpointSuffix), Constants.CRMSecurityCaptions.EditByStaffNotAssigned, editParent, SecurityInstance);

			var lookup = new Dictionary<string, SecurityCheckpoint>();

			foreach (var pair in DataRegistry.Instance.OrgStaffMemberAssignmentRoles.OfType<CodeDescriptionPair>().Where(x => !string.IsNullOrWhiteSpace(x.Code)))
			{
				if (!lookup.ContainsKey(pair.Code))
				{
					lookup.Add(pair.Code, new SecurityCheckpoint(string.Concat(editParent.Code, EditByStaffRoleAssignedCheckpointSuffix, pair.Code),
						ResString.GetMultilingualString("0286fbea-9dac-4f2f-9098-e8c84f577ee3", "Edit by {0} Assigned", pair.MultilingualDescription),
						editParent, SecurityInstance));
				}
			}

			EditByStaffRoleAssignedLookup = new ReadOnlyDictionary<string, SecurityCheckpoint>(lookup);
		}

		const string IgnoreOSMGCheckpointSuffix = ".IgnoreOSMG";
		const string IgnoreTaskAssignmentCheckpointSuffix = ".IgnoreTaskASN";
		const string ViewByStaffNotAssignedCheckpointSuffix = ".ViewBySNA";
		const string EditByStaffNotAssignedCheckpointSuffix = ".EditBySNA";
		const string EditByStaffRoleAssignedCheckpointSuffix = ".EditBySRA.";

#if DEBUG
		public void DisableCRMSecurityForTesting(bool parentEditAllowed)
		{
			ViewByStaffNotAssigned.IsAllowed = true;

			if (EditByStaffNotAssigned != null)
			{
				EditByStaffNotAssigned.IsAllowed = parentEditAllowed;
			}

			if (EditByStaffRoleAssignedLookup != null)
			{
				foreach (var checkpoint in EditByStaffRoleAssignedLookup.Values)
				{
					checkpoint.IsAllowed = parentEditAllowed;
				}
			}

			if (IgnoreOSMG != null)
			{
				IgnoreOSMG.IsAllowed = true;
			}
			if (IgnoreTaskAssignment != null)
			{
				IgnoreTaskAssignment.IsAllowed = true;
			}
		}
#endif
	}
}
