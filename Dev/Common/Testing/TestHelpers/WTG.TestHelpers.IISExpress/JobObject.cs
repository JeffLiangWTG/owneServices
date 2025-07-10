using System;
using System.ComponentModel;
using System.Diagnostics;
#if NETFRAMEWORK
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;
using static WTG.TestHelpers.IISExpress.NativeMethods;

namespace WTG.TestHelpers.IISExpress
{
	sealed class JobObject : IDisposable
	{
		public JobObject()
		{
			jobHandle = CreateJobObjectW(IntPtr.Zero, null);

			var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION()
			{
				BasicLimitInformation =
					{
						// This will allow child processes to explicitly create further children that are not monitored.
						// By default, child processes will become part of the job.
						LimitFlags = JOB_OBJECT_LIMIT_BREAKAWAY_OK | JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE,
					},
			};

			var length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
			var extendedInfoPtr = IntPtr.Zero;

#if NETFRAMEWORK
			RuntimeHelpers.PrepareConstrainedRegions();
#endif
			try
			{
				extendedInfoPtr = Marshal.AllocHGlobal(length);
				Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, fDeleteOld: false);

				if (!SetInformationJobObject(jobHandle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
				{
					throw new Win32Exception();
				}
			}
			finally
			{
				if (extendedInfoPtr == IntPtr.Zero)
				{
					Marshal.FreeHGlobal(extendedInfoPtr);
				}
			}
		}

		public void Dispose()
		{
			if (jobHandle != null)
			{
				jobHandle.Dispose();
				jobHandle = null;
			}
		}

		public bool AddProcess(Process childProcess)
		{
			if (childProcess == null)
			{
				throw new ArgumentNullException(nameof(childProcess));
			}

			var result = AssignProcessToJobObject(jobHandle, childProcess.Handle);
			return result;
		}

		JobObjectSafeHandle jobHandle;
	}
}
