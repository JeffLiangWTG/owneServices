using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Moq;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public abstract class BackgroundApplicationStartupTaskTest<T> : AbstractApplicationStartupTaskTest<T> where T : BackgroundApplicationStartupTask
	{
		public virtual void TestErrorsAreReported()
		{
			var taskMock = new Mock<T> { CallBase = true };
			var task = taskMock.Object;
			taskMock.Setup(x => x.DoExecute()).Throws(new Exception("FAIL"));
			task.Execute(new CommandLineArguments(Array.Empty<string>(), new Hashtable()));
			var stopwatch = Stopwatch.StartNew();
			do
			{
				Thread.Sleep(10);
			}
			while (ExceptionReporterTestListener.Instance.Count == 0 && stopwatch.Elapsed < TimeSpan.FromSeconds(2));
			Assert("Expected an exception to be reported", ExceptionReporterTestListener.Instance.Count > 0);
			Assert(ExceptionReporterTestListener.Instance[0].FlattenInnerExceptions().Any(ex => ex.Message == "FAIL"));
			ExceptionReporterTestListener.Instance.Clear();
		}
	}
}
