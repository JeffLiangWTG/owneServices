using System;

namespace CargoWise.NetworkVisualisation.Integration
{
	[Flags]
	public enum EntityState
	{
		None = 0,
		Inactive = 1 << 0,
		Fixed = 1 << 1,
		Approved = 1 << 2,
		NotApproved = 1 << 3,
		HasErrors = 1 << 4,
		HasWarnings = 1 << 5,
		HasMessages = 1 << 6,
	}
}
