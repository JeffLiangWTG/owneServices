using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	class AddAssistanceTaskForMenuItemForTaskCard : AddAssistanceTaskForMenuItem
	{
		public AddAssistanceTaskForMenuItemForTaskCard(BusinessObjectFactory factory, ProcessTask task, ProcessHeader workflow, IReadOnlyCollection<ZGuid> primaryUserPKs, IReadOnlyCollection<ZGuid> primaryCapabilityPKs)
			: this(factory, task, workflow, GetPrimaryUsers(factory, primaryUserPKs), GetPrimaryCapabilities(factory, primaryCapabilityPKs))
		{
		}

		AddAssistanceTaskForMenuItemForTaskCard(BusinessObjectFactory factory, ProcessTask task, ProcessHeader workflow, IReadOnlyCollection<GlbStaff> primaryUsers, IReadOnlyCollection<GlbCapability> primaryCapabilities)
			: this(factory, task, workflow, primaryUsers, primaryCapabilities, GetSecondaryUsers(factory, task, workflow, primaryCapabilities))
		{
		}

		AddAssistanceTaskForMenuItemForTaskCard(BusinessObjectFactory factory, ProcessTask task, ProcessHeader workflow, IReadOnlyCollection<GlbStaff> primaryUsers, IReadOnlyCollection<GlbCapability> primaryCapabilities, IReadOnlyCollection<GlbStaff> secondaryUsers)
			: base(task,
				() => GetStaffCodesAndDescriptionsForAssistanceTask(task, primaryUsers, secondaryUsers),
				() => GetCapabilityCodesAndDescriptionsForAssistanceTask(factory, workflow, primaryUsers, primaryCapabilities, secondaryUsers),
				saveAfterActions: true,
				informUserOnTaskCreation: true)
		{
		}

		static IReadOnlyCollection<GlbStaff> GetPrimaryUsers(BusinessObjectFactory factory, IReadOnlyCollection<ZGuid> primaryUserPKs)
		{
			var primaryUsersUnsorted = factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, primaryUserPKs)).ToDictionary(u => u.PK);
			var primaryUsersSorted = primaryUserPKs.Select(pk => primaryUsersUnsorted[pk]).Where(u => u.GS_IsActive).ToArray();
			return primaryUsersSorted;
		}

		static IReadOnlyCollection<GlbCapability> GetPrimaryCapabilities(BusinessObjectFactory factory, IReadOnlyCollection<ZGuid> primaryCapabilityPKs)
		{
			var primaryCapabilitiesUnsorted = factory.Load<GlbCapability>(new ZQuery(GlbCapabilitySchema.PK, primaryCapabilityPKs)).ToDictionary(u => u.PK);
			var primaryCapabilitiesSorted = primaryCapabilityPKs.Select(pk => primaryCapabilitiesUnsorted[pk]).Where(c => c.G4_IsActive).ToArray();
			return primaryCapabilitiesSorted;
		}

		static IReadOnlyCollection<GlbStaff> GetSecondaryUsers(BusinessObjectFactory factory, ProcessTask task, ProcessHeader workflow, IReadOnlyCollection<GlbCapability> primaryCapabilities)
		{
			var secondaryUsers = new List<GlbStaff>();

			foreach (var capability in primaryCapabilities)
			{
				secondaryUsers.AddRange(task.GetIntersectionOfCapabilityAndGroup(workflow, factory, capability));
			}
			return secondaryUsers.Distinct().Where(u => u.GS_IsActive).OrderBy(c => c.GS_Code).ToArray();
		}

		static IReadOnlyCollection<AddAssistanceTaskForStaffMenuItemInfo> GetStaffCodesAndDescriptionsForAssistanceTask(ProcessTask task, IReadOnlyCollection<GlbStaff> primaryUsers, IReadOnlyCollection<GlbStaff> secondaryUsers)
		{
			var users = new List<GlbStaff>();

			users.AddRange(primaryUsers);
			users.AddRange(secondaryUsers);

			var currentUser = (GlbStaff)Env.CurrentUser;

			if (!users.Select(u => u.GS_Code).Contains(currentUser.GS_Code))
			{
				users.Add(currentUser);
			}

			var assignedUserCode = task.P9_GS_NKAssignedStaffMember;

			var result = users
				.Distinct()
				.Where(u => u.GS_Code != assignedUserCode)
				.Select(u => new AddAssistanceTaskForStaffMenuItemInfo(u.GS_Code, ResString.GetMultilingualString("E40AD801-A4B9-4B8A-B682-80C2F48C5803", "{0} - {1}", u.GS_Code, u.GS_FullName)))
				.ToList();

			return result;
		}

		static IReadOnlyCollection<AddAssistanceTaskForCapabilityMenuItemInfo> GetCapabilityCodesAndDescriptionsForAssistanceTask(BusinessObjectFactory factory, ProcessHeader workflow, IReadOnlyCollection<GlbStaff> primaryUsers, IReadOnlyCollection<GlbCapability> primaryCapabilities, IReadOnlyCollection<GlbStaff> secondaryUsers)
		{
			List<GlbCapability> addedCapabilities = new List<GlbCapability>();
			List<AddAssistanceTaskForCapabilityMenuItemInfo> result = new List<AddAssistanceTaskForCapabilityMenuItemInfo>();

			foreach (var capability in primaryCapabilities)
			{
				result.Add(new AddAssistanceTaskForCapabilityMenuItemInfo(capability.PK, ResString.GetMultilingualString("78EB7E8D-6E21-42ED-B550-6CB3AFDD0565", "{0} - {1} (Capability Channel)", capability.G4_Code, capability.G4_Description)));
			}
			addedCapabilities.AddRange(primaryCapabilities);

			var workflowCapabilites = GetWorkflowCapabilities(factory, workflow).Except(addedCapabilities).ToArray();

			foreach (var capability in workflowCapabilites)
			{
				result.Add(new AddAssistanceTaskForCapabilityMenuItemInfo(capability.PK, ResString.GetMultilingualString("2B434C24-D97D-4E78-B4E3-C1913B8C431E", "{0} - {1} (Workflow)", capability.G4_Code, capability.G4_Description)));
			}
			addedCapabilities.AddRange(workflowCapabilites);

			var secondaryCapabilties = GetSecondaryCapabilities(primaryUsers).Except(addedCapabilities).ToArray();

			foreach (var capability in secondaryCapabilties)
			{
				result.Add(new AddAssistanceTaskForCapabilityMenuItemInfo(capability.PK, ResString.GetMultilingualString("5BF000B4-1272-4BE1-A3D9-F943231C3CCD", "{0} - {1} (Team Capability)", capability.G4_Code, capability.G4_Description)));
			}
			addedCapabilities.AddRange(secondaryCapabilties);

			var tertiaryCapabilties = GetTertiaryCapabilities(secondaryUsers).Except(addedCapabilities).ToArray();

			foreach (var capability in tertiaryCapabilties)
			{
				result.Add(new AddAssistanceTaskForCapabilityMenuItemInfo(capability.PK, ResString.GetMultilingualString("6590AA3F-A86A-42D4-A754-B5269C249202", "{0} - {1} (Other)", capability.G4_Code, capability.G4_Description)));
			}
			addedCapabilities.AddRange(tertiaryCapabilties);

			var currentUserCapabilties = GetCurrentCapabilities().Except(addedCapabilities).ToArray();

			foreach (var capability in currentUserCapabilties)
			{
				result.Add(new AddAssistanceTaskForCapabilityMenuItemInfo(capability.PK, ResString.GetMultilingualString("D3CE1F17-B6A1-47F9-973E-E7DAB706A342", "{0} - {1} (Current User)", capability.G4_Code, capability.G4_Description)));
			}
			addedCapabilities.AddRange(currentUserCapabilties);

			return result;
		}

		static IReadOnlyCollection<GlbCapability> GetWorkflowCapabilities(BusinessObjectFactory factory, ProcessHeader workflow)
		{
			var capabilityPKs = workflow.Tasks
				.Where(t => t.P9_G4_RequiredCapability.IsValid)
				.Select(t => t.P9_G4_RequiredCapability)
				.Distinct();
			var capabilities = factory.Load<GlbCapability>(new ZQuery(GlbCapabilitySchema.PK, capabilityPKs));
			var capabilitiesSorted = capabilities.Where(c => c.G4_IsActive).OrderBy(c => c.G4_Code).ToArray();
			return capabilitiesSorted;
		}

		static IReadOnlyCollection<GlbCapability> GetSecondaryCapabilities(IReadOnlyCollection<GlbStaff> primaryUsers) => GetUserCapabilitiesSorted(primaryUsers);

		static IReadOnlyCollection<GlbCapability> GetTertiaryCapabilities(IReadOnlyCollection<GlbStaff> secondaryUsers) => GetUserCapabilitiesSorted(secondaryUsers);

		static IReadOnlyCollection<GlbCapability> GetCurrentCapabilities() => GetUserCapabilitiesSorted(new GlbStaff[] { (GlbStaff)Env.CurrentUser });

		static IReadOnlyCollection<GlbCapability> GetUserCapabilitiesSorted(IReadOnlyCollection<GlbStaff> users)
		{
			var capabilities = new List<GlbCapability>();

			foreach (var user in users)
			{
				capabilities.AddRange(user.Capabilities.Cast<GlbCapability>());
			}
			return capabilities.Distinct().Where(c => c.G4_IsActive).OrderBy(c => c.G4_Code).ToArray();
		}
	}
}
