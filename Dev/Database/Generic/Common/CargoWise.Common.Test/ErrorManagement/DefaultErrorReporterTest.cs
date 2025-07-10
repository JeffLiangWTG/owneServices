using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class DefaultErrorReporterTest : TestCase
	{
		[GuiTest]
		public void TestReport()
		{
			new DefaultErrorReporter().Report(null, "Some error", new Exception("Some error"));
			var coreAssembly = AssemblyLoader.LoadAssembly("Enterprise.ZArchitecture.Core");
			var exceptionReporterTestListenerType = coreAssembly.GetType("Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener");
			var exceptionReporterTestListener = exceptionReporterTestListenerType.GetProperty("Instance").GetValue(null);
			AssertEquals("Some error", ((IEnumerable<Exception>)exceptionReporterTestListener).Single().Message);
			exceptionReporterTestListenerType.GetMethod("Clear").Invoke(exceptionReporterTestListener, Array.Empty<object>());
		}
	}
}
