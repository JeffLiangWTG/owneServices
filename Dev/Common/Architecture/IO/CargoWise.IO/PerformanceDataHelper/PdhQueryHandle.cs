#if NETFRAMEWORK
using System.Runtime.ConstrainedExecution;
#endif
using Microsoft.Win32.SafeHandles;

namespace CargoWise.IO
{
	/// <summary>
	/// Represents a safe wrapper around a query handle.
	/// </summary>
	public class PdhQueryHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		public PdhQueryHandle() : base(true)
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
			return IsClosed || PdhApi.CloseQuery(this) == PdhStatus.CStatusValidData;
		}
	}
}
