using System;
using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class HeartbeatTestListener : BaseTestListener
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly HeartbeatTestListener Instance = new HeartbeatTestListener();

		HeartbeatTestListener()
		{ }

		public override void StartAllTests(DateTime startTime)
		{
			disabledTimers = EnvProxy.Instance.SemaphoreProvider.InternalHeartbeat.TemporarilyDisableTimers();
		}

		public override void BeforeEachTest(DateTime startTime)
		{
			EnvProxy.Instance.SemaphoreProvider.InternalHeartbeat.PumpIfNeeded();
		}

		public override void EndAllTests(DateTime endTime)
		{
			disabledTimers?.Dispose();
		}

		IDisposable disabledTimers;
	}
}
