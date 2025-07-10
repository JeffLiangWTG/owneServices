using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Common
{
	public enum JobObjectInfoType
	{
		AssociateCompletionPortInformation = 7,
		BasicLimitInformation = 2,
		BasicUIRestrictions = 4,
		EndOfJobTimeInformation = 6,
		ExtendedLimitInformation = 9,
		SecurityLimitInformation = 5,
		GroupInformation = 11
	}

	[StructLayout(LayoutKind.Sequential)]
	struct JOBOBJECT_BASIC_LIMIT_INFORMATION
	{
		public Int64 PerProcessUserTimeLimit;
		public Int64 PerJobUserTimeLimit;
		public Int16 LimitFlags;
		public UIntPtr MinimumWorkingSetSize;
		public UIntPtr MaximumWorkingSetSize;
		public Int16 ActiveProcessLimit;
		public Int64 Affinity;
		public Int16 PriorityClass;
		public Int16 SchedulingClass;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct IO_COUNTERS
	{
		public UInt64 ReadOperationCount;
		public UInt64 WriteOperationCount;
		public UInt64 OtherOperationCount;
		public UInt64 ReadTransferCount;
		public UInt64 WriteTransferCount;
		public UInt64 OtherTransferCount;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
	{
		public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
		public IO_COUNTERS IoInfo;
		public UIntPtr ProcessMemoryLimit;
		public UIntPtr JobMemoryLimit;
		public UIntPtr PeakProcessMemoryUsed;
		public UIntPtr PeakJobMemoryUsed;
	}

	public class JobObject : IJobObject
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		static extern IntPtr CreateJobObject([In, Optional] IntPtr security_attributes, string lpName);

		[DllImport("kernel32.dll")]
		static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

		IntPtr jobHandle;
		bool disposed;

		const int JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;
		const int JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK = 0x1000;
		const int JOB_OBJECT_LIMIT_PROCESS_MEMORY = 0x100;

		public JobObject(IHostRegistrySettings hostRegistry)
		{
			var processMemoryLimit = hostRegistry.ServiceTaskMemoryConstraint;

			jobHandle = CreateJobObject(IntPtr.Zero, "ProcessController_" + Guid.NewGuid().ToString());

			var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION { LimitFlags = JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK | JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE };

			// this will allow child processes to create further children that are not monitored.

			if (processMemoryLimit > 0)
			{
				info.LimitFlags |= JOB_OBJECT_LIMIT_PROCESS_MEMORY;
			}

			var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION { BasicLimitInformation = info };

			const uint Mega = 1024 * 1024;

			if (processMemoryLimit > 0)
			{
				if (processMemoryLimit < 4096)
				{
					extendedInfo.ProcessMemoryLimit = new UIntPtr(((uint)processMemoryLimit) * Mega);
				}
				else
				{
					extendedInfo.ProcessMemoryLimit = new UIntPtr(uint.MaxValue);
				}
			}

			var length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));

			var extendedInfoPtr = Marshal.AllocHGlobal(length);
			try
			{
				Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, true);

				if (!SetInformationJobObject(jobHandle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
				{
					throw new Win32Exception();
				}
			}
			finally
			{
				Marshal.FreeHGlobal(extendedInfoPtr);
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing) { }

			Close();
			disposed = true;
		}

		public void Close()
		{
			CloseHandle(jobHandle);
			jobHandle = IntPtr.Zero;
		}

		public bool AddProcess(IntPtr childProcessHandle)
		{
			var result = AssignProcessToJobObject(jobHandle, childProcessHandle);
			return result;
		}
	}
}
