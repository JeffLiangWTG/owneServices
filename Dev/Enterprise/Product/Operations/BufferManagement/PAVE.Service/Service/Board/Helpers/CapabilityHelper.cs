using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.PAVE.Common.Model;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Service
{
	internal static class CapabilityHelper
	{
		#region Model To DTO

		internal static IEnumerable<CapabilityDTO> ToCapabilityDTOs(this IEnumerable<Capability> capabilities)
		{
			return capabilities.Select(capability => ToCapabilityDTO(capability));
		}

		static CapabilityDTO ToCapabilityDTO(Capability capability)
		{
			return new CapabilityDTO
			{
				PK = capability.PK,
				Code = capability.Code,
				Name = capability.Name
			};
		}

		#endregion

		#region BusinessObject to DTO

		internal static IEnumerable<CapabilityDTO> ToCapabilitiesDTO(this IEnumerable<ProcessTask> tasks)
		{
			var capabilityPKs = tasks
				.Where(t => t.P9_G4_RequiredCapability.IsValid)
				.Select(t => t.P9_G4_RequiredCapability)
				.Distinct();

			var factory = tasks.FirstOrDefault()?.Factory;

			if (factory == null)
			{
				return null;
			}

			var capabilities = factory.Load<GlbCapability>(new ZQuery(GlbCapabilitySchema.PK, capabilityPKs));

			return capabilities.Select(capability => new CapabilityDTO()
			{
				PK = capability.PK.ToGuid(),
				Code = capability.G4_Code,
				Name = capability.G4_Description,
			}).ToArray();
		}

		#endregion
	}
}
