using System;

namespace Enterprise.BufferManagement.Service.Shared
{
	public class AcceptabilityBandConfigurationDto
	{
		public Guid AcceptabilityBandId { get; set; }
		public string Name { get; set; } = null!;

		public string DisplayName { get; set; } = null!;
#nullable enable
		public string? DisplayUnits { get; set; }
#nullable disable
		public bool FilterByReleaseGroup { get; set; }
		public bool FilterByComponents { get; set; }

		public bool OverrideBoundaryValues { get; set; }

#nullable enable
		public AcceptabilityBandBoundariesDto? Boundaries { get; set; }
#nullable disable
	}
}
