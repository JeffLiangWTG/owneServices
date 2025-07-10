using System;

namespace Enterprise.BufferManagement.Service.Shared
{
	public class AcceptabilityBandResultDto
	{
		public Guid AcceptabilityBandId { get; set; }

		public string DisplayName { get; set; } = null!;

		public decimal? Result { get; set; }

		public string DisplayUnits { get; set; }

		public DateTime? CalculatedAtUtc { get; set; }

		public int? CalculationDurationInSeconds { get; set; }

		public AcceptabilityBandBoundariesDto Boundaries { get; set; }
	}
}
