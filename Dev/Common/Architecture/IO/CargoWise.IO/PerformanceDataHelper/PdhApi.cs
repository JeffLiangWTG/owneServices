using System;
#if NETFRAMEWORK
using System.Runtime.ConstrainedExecution;
#endif
using System.Runtime.InteropServices;
using System.Security;

namespace CargoWise.IO
{
	/// <summary>
	/// Static class containing some usefull PDH API's
	/// </summary>
	[SuppressUnmanagedCodeSecurity]
	public static class PdhApi
	{
		/// <summary>
		/// Creates a new query that is used to manage the collection of performance data.
		/// </summary>
		/// <param name="dataSource">The name of the log file from which to retrieve performance data. If NULL, performance data is collected from a real-time data source.</param>
		/// <param name="userDataPtr">User-defined value to associate with this query.</param>
		/// <param name="queryHandle">Handle to the query.</param>
		/// <returns>If the function succeeds, it returns ERROR_SUCCESS. If the function fails, the return value is a system error code or a PDH error code.</returns>
		[DllImport("pdh.dll", EntryPoint = "PdhOpenQuery", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern PdhStatus OpenQuery([In] string dataSource, [In] IntPtr userDataPtr, [Out] out PdhQueryHandle queryHandle);
		/// <summary>
		/// Closes all counters contained in the specified query, closes all handles related to the query, and frees all memory associated with the query.
		/// </summary>
		/// <param name="queryHandle">Handle to the query to close.</param>
		/// <returns>If the function succeeds, it returns ERROR_SUCCESS. Otherwise, the function returns a system error code or a PDH error code.</returns>
		[DllImport("pdh.dll", EntryPoint = "PdhCloseQuery", SetLastError = true)]
#if NETFRAMEWORK
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
		public static extern PdhStatus CloseQuery([In] PdhQueryHandle queryHandle);
		/// <summary>
		/// Adds the specified counter to the query.
		/// </summary>
		/// <param name="queryHandle">Handle to the query to which you want to add the counter.</param>
		/// <param name="fullCounterPath">The counter path.</param>
		/// <param name="userDataPtr">User-defined value. This value becomes part of the counter information.</param>
		/// <param name="counterHandle">Handle to the counter that was added to the query.</param>
		/// <returns>Returns ERROR_SUCCESS if the function succeeds. If the function fails, the return value is a system error code or a PDH error code. The following are possible values.</returns>
		[DllImport("pdh.dll", EntryPoint = "PdhAddCounter", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern PdhStatus AddCounter([In] PdhQueryHandle queryHandle, [In] string fullCounterPath, [In] IntPtr userDataPtr, [Out] out PdhCounterHandle counterHandle);
		/// <summary>
		/// Removes a counter from a query.
		/// </summary>
		/// <param name="counterHandle">Handle of the counter to remove from its query.</param>
		/// <returns>If the function succeeds, it returns ERROR_SUCCESS. If the function fails, the return value is a system error code or a PDH error code.</returns>
		[DllImport("pdh.dll", EntryPoint = "PdhRemoveCounter", SetLastError = true)]
#if NETFRAMEWORK
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
		public static extern PdhStatus RemoveCounter([In] PdhCounterHandle counterHandle);
		/// <summary>
		/// Collects the current raw data value for all counters in the specified query and updates the status code of each counter.
		/// </summary>
		/// <param name="queryHandle">Handle of the query for which you want to collect data.</param>
		/// <returns>If the function succeeds, it returns ERROR_SUCCESS. Otherwise, the function returns a system error code or a PDH error code.</returns>
		[DllImport("pdh.dll", EntryPoint = "PdhCollectQueryData", SetLastError = true)]
		public static extern PdhStatus CollectQueryData([In][Out] PdhQueryHandle queryHandle);
		/// <summary>
		/// Computes a displayable value for the specified counter.
		/// </summary>
		/// <param name="counterHandle">Handle of the counter for which you want to compute a displayable value.</param>
		/// <param name="format">Determines the data type of the formatted value. Specify one of the following values.</param>
		/// <param name="typePtr">The pointer to the counter type.</param>
		/// <param name="counterValue">The counter value</param>
		/// <returns>If the function succeeds, it returns ERROR_SUCCESS. If the function fails, the return value is a system error code or a PDH error code.</returns>
		[DllImport("pdh.dll", EntryPoint = "PdhGetFormattedCounterValue", SetLastError = true)]
		public static extern PdhStatus GetFormattedCounterValue([In] PdhCounterHandle counterHandle, [In] PdhCounterFormat format, [Out] IntPtr typePtr, [Out] out PdhFormatCounterValue counterValue);
	}
}
