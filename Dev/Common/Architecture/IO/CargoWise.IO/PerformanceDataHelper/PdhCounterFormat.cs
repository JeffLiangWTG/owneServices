using System;

namespace CargoWise.IO
{
	/// <summary>
	/// Contains the data types of the formatted counter value.
	/// </summary>
	[Flags]
	public enum PdhCounterFormat : uint
	{
		/// <summary>
		/// Return data as a long integer.
		/// </summary>
		Long = 0x00000100,
		/// <summary>
		/// Return data as a double-precision floating point real.
		/// </summary>
		@Double = 0x00000200,
		/// <summary>
		/// Return data as a long integer.
		/// </summary>
		Large = 0x00000400,
		/// <summary>
		/// Do not apply the counter's default scaling factor.
		/// </summary>
		NoScale = 0x00001000,
		/// <summary>
		/// Multiply the actual value by 1,000.
		/// </summary>
		Multiply1000 = 0x00002000,
		/// <summary>
		/// Counter values greater than 100 (for example, counter values measuring the processor load on multiprocessor computers) will not be reset to 100. The default behavior is that counter values are capped at a value of 100.
		/// </summary>
		NoCap100 = 0x00008000,
	}
}
