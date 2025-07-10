using System;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.ActivityLogging;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.ActivityLogger
{
	[CodeAlive("This is used by Enterprise.ZArchitecture.Core.Testing.UnitTestListenersFactory.")]
	public sealed class ZFormActivityLoggerTestListener : BaseTestListener, IZFormActivityLoggerTestListener
	{
		bool isActivityLoggerInitiallyEnabled;
		public override void StartAllTests(DateTime startTime)
		{
			base.StartAllTests(startTime);

			isActivityLoggerInitiallyEnabled = ZFormActivityLogger.Instance.IsEnabled;
			if (isActivityLoggerInitiallyEnabled)
			{
				ZFormActivityLogger.Instance.DisableActivityLogger();
			}
		}

		public override void AfterEachTest(DateTime endTime)
		{
			if (ZFormActivityLogger.Instance.IsEnabled)
			{
				Assertion.Assert($"{nameof(ZFormActivityLogger)} is enabled in the current test, which is disallowed.", false);
			}

			base.AfterEachTest(endTime);
		}

		public override void EndTest(TestCase test, DateTime endTime)
		{
			if (ZFormActivityLogger.Instance.IsEnabled)
			{
				ZFormActivityLogger.Instance.DisableActivityLogger();
			}

			base.EndTest(test, endTime);
		}

		public override void EndAllTests(DateTime endTime)
		{
			if (isActivityLoggerInitiallyEnabled)
			{
				ZFormActivityLogger.Instance.EnableActivityLogger();
			}

			base.EndAllTests(endTime);
		}
	}
}
