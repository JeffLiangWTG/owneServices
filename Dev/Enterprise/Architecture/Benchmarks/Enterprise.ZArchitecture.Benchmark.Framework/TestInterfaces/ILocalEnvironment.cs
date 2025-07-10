using System;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Benchmark.Framework.TestInterfaces
{
	public interface ILocalEnvironment
	{
		DateTimeOffset GetLocalTime();

		string RuntimeVersion { get; }

		string Username { get; }

		string MachineName { get; }

		string DatabaseServerName { get; }

		string DatabaseFullVersionText { get; }

		(string name, int physicalCPUs, int physicalCores, int logicalCores) GetCPUDetails();

		string PowerProfileName { get; }
	}

	/// <summary>
	/// Standard local environment.
	/// </summary>
	public sealed class LocalEnvironment : ILocalEnvironment
	{
		public DateTimeOffset GetLocalTime() => DateTimeOffset.Now;

		public string RuntimeVersion
			=> System.Environment.Version.Major == 4
				? "net48"
				: $"net{System.Environment.Version.Major}.0";

		public string Username => System.Environment.UserName;

		public string MachineName => System.Environment.MachineName;

		public string DatabaseServerName => Db.DatabaseName;

		public string DatabaseFullVersionText => Db.Connection.ServerFullVersionText;

		public (string name, int physicalCPUs, int physicalCores, int logicalCores) GetCPUDetails()
		{
			string name = "";
			int physicalCPUs = 0;
			int physicalCores = 0;
			int logicalCores = 0;

			if (System.Environment.OSVersion.Platform != PlatformID.Win32NT)
			{
#pragma warning disable CW1161 // Res.GetString Analyzer: Only visible to developers; no translation required.
				return ("Only supported on Windows (until you implement Linux support!)", physicalCPUs, physicalCores, logicalCores);
#pragma warning restore CW1161 // Res.GetString Analyzer
			}

			using (var mosProcessor = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
			{
				foreach (var moProcessor in mosProcessor.Get().Cast<ManagementObject>())
				{
					name = moProcessor["Name"]?.ToString();
					if (!string.IsNullOrEmpty(name))
					{
						++physicalCPUs;
						physicalCores += (int)(uint)moProcessor["NumberOfCores"];
						logicalCores += (int)(uint)moProcessor["NumberOfLogicalProcessors"];
					}
				}
			}
			return (name, physicalCPUs, physicalCores, logicalCores);
		}

		public string PowerProfileName
		{
			get
			{
				// Shamelessly stolen from https://github.com/dotnet/BenchmarkDotNet/blob/master/src/BenchmarkDotNet/Helpers/PowerManagementHelper.cs
				// MIT licensed
				// Thanks BenchmarkDotnet!
				const uint ErrorMoreData = 234;
				const uint SuccessCode = 0;

				if (System.Environment.OSVersion.Platform != PlatformID.Win32NT)
				{
#pragma warning disable CW1161 // Res.GetString Analyzer: Only visible to developers; no translation required.
					return "Only supported on Windows (until you implement Linux support!)";
#pragma warning restore CW1161 // Res.GetString Analyzer
				}

				uint buffSize = 0;
				StringBuilder buffer = new StringBuilder();
				IntPtr activeGuidPtr = IntPtr.Zero;
				uint res = PowerGetActiveScheme(IntPtr.Zero, ref activeGuidPtr);
				if (res != SuccessCode)
				{
					return "UNKNOWN";
				}
				res = PowerReadFriendlyName(IntPtr.Zero, activeGuidPtr, IntPtr.Zero, IntPtr.Zero, buffer, ref buffSize);
				if (res == ErrorMoreData)
				{
					buffer.Capacity = (int)buffSize;
					res = PowerReadFriendlyName(IntPtr.Zero, activeGuidPtr, IntPtr.Zero, IntPtr.Zero, buffer, ref buffSize);
				}
				if (res != SuccessCode)
				{
					return "UNKNOWN";
				}

				return buffer.ToString();
			}
		}

		[DllImport("powrprof.dll", ExactSpelling = true)]
		static extern uint PowerGetActiveScheme(IntPtr userRootPowerKey, ref IntPtr activePolicyGuid);

		[DllImport("powrprof.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		static extern uint PowerReadFriendlyName(IntPtr rootPowerKey, IntPtr schemeGuid, IntPtr subGroupOfPowerSettingGuid, IntPtr powerSettingGuid, StringBuilder buffer, ref uint bufferSize);
	}

	/// <summary>
	/// Local environment which does not report database information.
	/// Useful for tests which do not touch the DB at all.
	/// Because Db.Connection can throw if not initialized correctly.
	/// </summary>
	public sealed class NoDatabaseLocalEnvironment : ILocalEnvironment
	{
		readonly ILocalEnvironment LocalEnvironment = new LocalEnvironment();

		public DateTimeOffset GetLocalTime() => LocalEnvironment.GetLocalTime();

		public string RuntimeVersion => LocalEnvironment.RuntimeVersion;

		public string Username => LocalEnvironment.Username;

		public string MachineName => LocalEnvironment.MachineName;

		public string DatabaseServerName => string.Empty;

		public string DatabaseFullVersionText => string.Empty;

		public (string name, int physicalCPUs, int physicalCores, int logicalCores) GetCPUDetails()
			=> LocalEnvironment.GetCPUDetails();

		public string PowerProfileName => LocalEnvironment.PowerProfileName;
	}

	/// <summary>
	/// Local environment which returns empty or zero for everything.
	/// </summary>
	public sealed class NullEnvironment : ILocalEnvironment
	{
		public DateTimeOffset GetLocalTime() => DateTimeOffset.MinValue;

		public string RuntimeVersion => string.Empty;

		public string Username => string.Empty;

		public string MachineName => string.Empty;

		public string DatabaseServerName => string.Empty;

		public string DatabaseFullVersionText => string.Empty;

		public (string name, int physicalCPUs, int physicalCores, int logicalCores) GetCPUDetails()
			=> (string.Empty, 0, 0, 0);

		public string PowerProfileName => string.Empty;
	}
}
