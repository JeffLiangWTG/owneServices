using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace WTG.TestHelpers.Net
{
	sealed class TestServicePortsMemoryMappedFile : IDisposable
	{
		const string MutexName = "WTGTestServicePortsMutex";
		const string MemoryMap = "WTGTestServicePorts";

		public TestServicePortsMemoryMappedFile()
		{
			mutex = new Mutex(false, MutexName);
			file = MemoryMappedFile.CreateOrOpen(MemoryMap, UnusedPortLocator.MaxValidPort);
		}

		public bool AcquirePort(ushort port)
		{
			AcquireMutexIgnoringAbandoned();
			try
			{
				using var accessor = file.CreateViewAccessor();

				if (!accessor.ReadBoolean(port))
				{
					accessor.Write(port, value: true);
					acquiredPorts.Add(port);
					return true;
				}
				else
				{
					return false;
				}
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}

		public void ReleasePort(ushort port) => ReleasePorts(port);

		void ReleasePorts(params ushort[] ports)
		{
			AcquireMutexIgnoringAbandoned();
			try
			{
				using var accessor = file.CreateViewAccessor();

				foreach (var port in ports)
				{
					if (acquiredPorts.Contains(port))
					{
						acquiredPorts.Remove(port);
						if (accessor.ReadBoolean(port))
						{
							accessor.Write(port, value: false);
						}
					}
				}
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}

		void AcquireMutexIgnoringAbandoned()
		{
			try
			{
				mutex.WaitOne();
			}
			catch (AbandonedMutexException)
			{
				// AbandonedMutexException indicates that the thread that held the mutex terminated without releasing it, but we still
				// successfully acquired it. This can happen if another test runner timed out and was forcefully terminated by the build
				// process.
				//
				// Ignore it because:
				//  * We technically still hold the mutex and don't want to cause other test runners to time out or receive the same exception
				//    when we terminate.
				//  * We don't want this exception to cause superfluous test failures.
				//  * If the thread that held this lock has already terminated, then the risk of a race condition has already passed anyway.
				//  * The only risk of corrupted data is having superfluous ports claimed, and they will be cleaned up when the last process
				//    referencing the shared memory is terminated.
			}
		}

		public void Dispose()
		{
			if (!disposed)
			{
				if (acquiredPorts.Count > 0)
				{
					ReleasePorts(acquiredPorts.ToArray());
				}

				file.Dispose();
				mutex.Dispose();

				disposed = true;
			}
		}

		readonly MemoryMappedFile file;
		readonly Mutex mutex;
		readonly List<ushort> acquiredPorts = new List<ushort>();
		bool disposed;
	}
}
