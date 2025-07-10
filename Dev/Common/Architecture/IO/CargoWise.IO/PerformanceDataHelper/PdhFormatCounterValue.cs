using System.Runtime.InteropServices;

namespace CargoWise.IO
{
	/// <summary>
	/// Contains the computed value of the counter and its status.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct PdhFormatCounterValue
	{
		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		// private UIntPtr cStatus;
		/// <summary>
		/// Indicates if the counter value is valid.
		/// </summary>
		public PdhCounterStatus CStatus;
		/// <summary>
		/// The computed counter value.
		/// </summary>
		public double DoubleValue;
	}
}
