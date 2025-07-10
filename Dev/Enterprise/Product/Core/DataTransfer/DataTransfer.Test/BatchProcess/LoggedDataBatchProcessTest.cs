using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DataTransfer.BatchProcessor.Testing
{
	sealed class LoggedDataBatchProcessTest : BaseLoggedDataBatchProcessTestCase
	{
		public void TestHighWaterMarkNotSet()
		{
			ZDateTime highWaterMark = new ZDateTime(2005, 11, 17, 8, 42, 0);
			Process.HighWaterMark = highWaterMark;
			AssertEquals("HigWaterMark should be set", highWaterMark, Process.HighWaterMark);

			Process.HighWaterMark = ZDateTime.Invalid;
			AssertEquals("HigWaterMark should not change", highWaterMark, Process.HighWaterMark);

			SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
			AssertEquals("HigWaterMark should be Today", ZDateTime.UtcNow.Date, Process.HighWaterMark);
		}

		#region Implementation

		MockLoggedDataBatchProcess Process;

		protected override void SetUp()
		{
			base.SetUp();

			overridenClientHook = ClientHookLoader.Instance.OverrideClientHookForTest(TestClientHook.Instance);
			Process = new MockLoggedDataBatchProcess(new LoggingInformation());
		}

		protected override void TearDown()
		{
			overridenClientHook.Dispose();

			base.TearDown();
		}

		IDisposable overridenClientHook;

		#region TestClientHook

		public class TestClientHook : Enterprise.ZArchitecture.Modules.ClientHook
		{
			public override string ClientDisplayName
			{
				get { return "MEH"; }
			}

			public override Clients Client
			{
				get { return Clients.None; }
			}

			#region Instance

			public static TestClientHook Instance
			{
				get
				{
					if (fInstance == null)
					{
						fInstance = new TestClientHook();
					}
					return fInstance;
				}
			}

			static TestClientHook fInstance;

			#endregion
		}

		#endregion

		#region MockLoggedDataBatchProcess

		public class MockLoggedDataBatchProcess : LoggedDataBatchProcess
		{
			public MockLoggedDataBatchProcess(LoggingInformation logger)
				: base(logger)
			{
			}

			public new ZDateTime HighWaterMark
			{
				get { return base.HighWaterMark; }
				set { base.HighWaterMark = value; }
			}

			public new ILogBatchListenerProxy[] Listeners
			{
				get { return base.Listeners; }
			}
		}

		#endregion

		#endregion
	}
}
