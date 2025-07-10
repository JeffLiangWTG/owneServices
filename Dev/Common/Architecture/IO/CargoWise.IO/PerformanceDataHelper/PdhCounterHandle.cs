#if NETFRAMEWORK
using System.Runtime.ConstrainedExecution;
#endif
using Microsoft.Win32.SafeHandles;

namespace CargoWise.IO
{
	/// <summary>
	/// A safe handle around a counter handle.
	/// </summary>
	public class PdhCounterHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public PdhCounterHandle() : base(true)
		{
		}
		/// <summary>
		/// When overridden in a derived class, executes the code required to free the handle.
		/// </summary>
		/// <returns>
		/// true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false. In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.
		/// </returns>
#if NETFRAMEWORK
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
		protected override bool ReleaseHandle()
		{
			return IsClosed || PdhApi.RemoveCounter(this) == PdhStatus.CStatusValidData;
		}
	}
}
