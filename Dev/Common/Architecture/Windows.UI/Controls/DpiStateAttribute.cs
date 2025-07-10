using System;
using static System.AttributeTargets;

namespace CargoWise.Windows.UI
{
	public enum DpiState
	{
		Unknown, // The value cannot be reliably determined

		ScaleX, // Scaled X value
		ScaleY, // Scaled Y value

		ScaledVariant, // Sometimes X, sometimes Y

		Unscaled, // Unscaled value
		Invalid, // Not a valid scaled or unscaled value
	}

	[AttributeUsage(Field | ReturnValue | Property | Parameter, AllowMultiple = false)]
	public sealed class DpiStateAttribute : Attribute
	{
		public DpiStateAttribute(DpiState state)
		{
			State = state;
		}

		public DpiState State { get; }
	}
}
