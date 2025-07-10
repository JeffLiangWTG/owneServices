using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.Common;
using Microsoft.Win32;

namespace Enterprise.ZArchitecture.Core
{
	public class ZSystemInformation
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Path")]
		const string RegistryKeyNamePageFile = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";
		const string RegistryValueNamePageFile = "PagingFiles";

		public ZSystemInformation() : this(new SystemAdapter())
		{
		}

		internal ZSystemInformation(ISystemAdapter systemAdapter)
		{
			this.systemAdapter = systemAdapter ?? throw new ArgumentNullException(nameof(systemAdapter));
		}

		public static class MinimumRequirements
		{
			public const int TotalPhysicalMemoryInMB = 1024;
			public const int AvailableMemoryInMB = 512;
			public const int DiskFreeSpaceInMB = 500;
			public const int AvailablePageFileInMB = 512;
		}

		public static ZSystemInformation Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new ZSystemInformation();
				}
				return fInstance;
			}
		}

#if DEBUG
		public static IDisposable SetInstanceForTesting(ZSystemInformation systemInfo)
		{
			var original = fInstance;
			fInstance = systemInfo;
			return new DisposableAction(delegate
			{
				fInstance = original;
			});
		}

#endif
		[ThreadStatic]
		static ZSystemInformation fInstance;

		[DllImport("Kernel32.dll")]
		protected static extern bool GlobalMemoryStatusEx(ref MemoryStatusStructEx memoryStatus);

		[StructLayout(LayoutKind.Sequential)]
		protected struct MemoryStatusStructEx
		{
			public int dwLength;
			public int dwMemoryLoad;
			public ulong ullTotalPhys;
			public ulong ullAvailPhys;
			public ulong ullTotalPageFile;
			public ulong ullAvailPageFile;
			public ulong ullTotalVirtual;
			public ulong ullAvailVirtual;
			public ulong ullAvailExtendedVirtual;
		}

		[DllImport("psapi.dll")]
		protected static extern bool GetPerformanceInfo(ref PerformanceInformationStruct performanceInformation, int size);

		[StructLayout(LayoutKind.Sequential)]
		protected struct PerformanceInformationStruct
		{
			public int cb;
			public IntPtr CommitTotal;
			public IntPtr CommitLimit;
			public IntPtr CommitPeak;
			public IntPtr PhysicalTotal;
			public IntPtr PhysicalAvailable;
			public IntPtr SystemCache;
			public IntPtr KernelTotal;
			public IntPtr KernelPaged;
			public IntPtr KernelNonpaged;
			public IntPtr PageSize;
			public int HandleCount;
			public int ProcessCount;
			public int ThreadCount;
		}

		protected UInt64 BytesToMB(UInt64 bytes)
		{
			return (bytes / 1024 / 1024);
		}

		protected uint MBToGB(uint mBytes)
		{
			return (mBytes / 1024);
		}

		/// <summary>
		/// Values are in MB
		/// </summary>
		public UInt64 AvailablePhysicalMemory
		{
			get { return BytesToMB(MemoryStatus.ullAvailPhys); }
		}

		/// <summary>
		/// Values are in MB
		/// </summary>
		public UInt64 TotalPhysicalMemory
		{
			get { return BytesToMB(MemoryStatus.ullTotalPhys); }
		}

		/// <summary>
		/// Values are in MB
		/// </summary>
		public UInt64 AvailableVirtualMemory
		{
			get { return BytesToMB(MemoryStatus.ullAvailVirtual); }
		}

		/// <summary>
		/// Values are in MB
		/// </summary>
		public UInt64 TotalVirtualMemory
		{
			get { return BytesToMB(MemoryStatus.ullTotalVirtual); }
		}

		/// <summary>
		/// Values are in MB
		/// </summary>
		public UInt64 AvailablePageFileSize
		{
			get { return BytesToMB(MemoryStatus.ullAvailPageFile); }
		}

		/// <summary>
		/// Values are in MB
		/// </summary>
		public UInt64 TotalPageFileSize
		{
			get { return BytesToMB(MemoryStatus.ullTotalPageFile); }
		}

		/// <summary>
		/// Values are in MB
		/// </summary>
		public UInt64 CommittableAvailableSystemWide
		{
			get { return CommittLimitSystemWide - CommittedTotalSystemWide; }
		}

		/// <summary>
		/// Values are in MB
		/// </summary>
		public UInt64 CommittedTotalSystemWide
		{
			get { return BytesToMB((UInt64)(PerformanceInformation.CommitTotal.ToInt64() * PerformanceInformation.PageSize.ToInt64())); }
		}

		/// <summary>
		/// Values are in MB
		/// </summary>
		public UInt64 CommittLimitSystemWide
		{
			get { return BytesToMB((UInt64)(PerformanceInformation.CommitLimit.ToInt64() * PerformanceInformation.PageSize.ToInt64())); }
		}

		public bool IsPageFileEnable
		{
			get
			{
				var pagingFiles = (string[])Registry.GetValue(RegistryKeyNamePageFile, RegistryValueNamePageFile, null);

				if (pagingFiles == null)
				{
					return false;
				}

				foreach (var pagingFile in pagingFiles)
				{
					if (!string.IsNullOrEmpty(pagingFile))
					{
						return true;
					}
				}
				return false;
			}
		}

		MemoryStatusStructEx fMemoryStatus;
		MemoryStatusStructEx MemoryStatus
		{
			get
			{
				fMemoryStatus = new MemoryStatusStructEx();
				fMemoryStatus.dwLength = 64;//urgh
				FillMemoryStatusStruct(ref fMemoryStatus);
				return fMemoryStatus;
			}
		}

		protected virtual void FillMemoryStatusStruct(ref MemoryStatusStructEx memoryStatus)
		{
			if (!GlobalMemoryStatusEx(ref memoryStatus))
			{
				throw new Exception("Unable to retrieve memory status");
			}
		}

		PerformanceInformationStruct performanceInformation;
		PerformanceInformationStruct PerformanceInformation
		{
			get
			{
				performanceInformation = new PerformanceInformationStruct();
				FillPerformanceInformationStruct(ref performanceInformation);
				return performanceInformation;
			}
		}

		protected virtual void FillPerformanceInformationStruct(ref PerformanceInformationStruct performanceInformation)
		{
			if (!GetPerformanceInfo(ref performanceInformation, Marshal.SizeOf(performanceInformation)))
			{
				throw new Exception("Unable to retrieve performance information");
			}
		}

		public ulong AvailableFreeSpace
		{
			get { return AvailableFreeSpaceCore; }
		}

		protected virtual ulong AvailableFreeSpaceCore
		{
			get
			{
				if (systemAdapter.GetDiskFreeSpace(out var result))
				{
					return BytesToMB(result);
				}

				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		public bool IsThereSufficientFreeDiskSpace(out ulong availableFreeSpace)
		{
			availableFreeSpace = AvailableFreeSpace;
			return availableFreeSpace > MinimumRequirements.DiskFreeSpaceInMB;
		}

		readonly ISystemAdapter systemAdapter;

		public interface ISystemAdapter
		{
			bool GetDiskFreeSpace(out ulong freeBytesAvailable);
		}

		class SystemAdapter : ISystemAdapter
		{
			static string EnterpriseDiskDrive => Path.GetPathRoot(AssemblyLoader.GetBinPath());

			[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
			static extern bool GetDiskFreeSpaceEx(string directoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

			public bool GetDiskFreeSpace(out ulong freeBytesAvailable)
			{
				return GetDiskFreeSpaceEx(EnterpriseDiskDrive, out freeBytesAvailable, out _, out _);
			}
		}
	}
}
