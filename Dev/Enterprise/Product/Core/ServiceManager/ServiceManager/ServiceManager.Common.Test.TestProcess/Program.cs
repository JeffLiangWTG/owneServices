using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ServiceManager.Common.Test.TestProcess
{
	class Program
	{
		static int Main(string[] args)
		{
			while (Console.ReadLine() != "Ready")
			{
				Thread.Sleep(10);
			}

			return MaxPrivateMemory();
		}

		// Return the maximum private memory which could be used (bytes), or 1 (error).
		// Since we don't want allow it to allocate too much mem, we just return at 128MB
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1071:DoNotUseGCWaitForPendingFinalizersOrGetTotalMemory", Justification = "Testing")]
		static int MaxPrivateMemory()
		{
			const int error = 1;
			var mem = 0L;
			var list = new List<byte[]>();
			const int mb = 1024 * 1024;

			try
			{
				while (true)
				{
					list.Add(new byte[mb]);
					mem = Process.GetCurrentProcess().PrivateMemorySize64;

					if (mem > 128 * mb)
					{
						return (int)mem;
					}
				}
			}
			catch (OutOfMemoryException)
			{
				return (int)mem;
			}
			catch (Exception)
			{
				// ignored
			}
			finally
			{
				list.Clear();

				GC.Collect(); // for a unit test
				GC.WaitForPendingFinalizers(); // for a unit test
				GC.Collect(); // for a unit test
			}

			return error;
		}
	}
}
