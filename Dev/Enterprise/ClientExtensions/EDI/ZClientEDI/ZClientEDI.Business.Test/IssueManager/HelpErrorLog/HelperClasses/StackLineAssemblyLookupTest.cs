using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using static Enterprise.Client.EDI.IssueManager.Business.Test.AssignmentTestHelper;

namespace Enterprise.Client.EDI.IssueManager.Business.Tests
{
	class StackLineAssemblyLookupTest : TestCaseWithFactory
	{
		public void TestUpdateMissingAssemblyInformation()
		{
			AssertNoExceptionThrown(() =>
			{
				InsertTestData();

				var veryLongStackline = "AsyncTaskMethodB.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.AsyncStateMachineBox`1[[System.Threading.Task()";
				var veryLongStackline1 = "System.Reactive.Concurrency.ConcurrencyAbstractionLayerImpl.Timer.Tick(";
				var veryLongStackline2 = "System.Collections.Generic.List`1..ctor(";
				var veryLongStackline3 = "CargoWise.Glow.Index.Service.IndexDefinitionSqlAdapterGroup`1.<>c__DisplayClass2_0.<.ctor>b__0(";
				var veryLongStackline4 = ".ctor(";
				var veryLongStackline5 = "lambda_method149(";
				var veryLongStackline6 = "(";
				var veryLongStackline7 = ".(";
				var veryLongStackline8 = "System.Web.Http.Tracing.Tracers.RequestMessageHandlerTracer.<>c__DisplayClass2_0.<SendAsync>b__1(";

				var stacklineTests = new List<(IStackLine stackLine, string expectedAssembly, string expectedType, string expectedMethod)>
				{
					(new StackLine(veryLongStackline), "A.dll", "AsyncTaskMethodB.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.AsyncStateMachineBox`1[[System.Threadin", "Task"),
					(new StackLine(veryLongStackline1), "B.dll", "System.Reactive.Concurrency.ConcurrencyAbstractionLayerImpl.Timer", "Tick"),
					(new StackLine(veryLongStackline2), "C.dll", "System.Collections.Generic.List`1", ".ctor"),
					(new StackLine(veryLongStackline3), "D.dll", "CargoWise.Glow.Index.Service.IndexDefinitionSqlAdapterGroup`1.<>c__DisplayClass2_0", "<.ctor>b__0"),
					(new StackLine(veryLongStackline4), null, "", ".ctor"),
					(new StackLine(veryLongStackline5), null, "", "lambda_method149"),
					(new StackLine(veryLongStackline6), null, "", ""),
					(new StackLine(veryLongStackline7), null, "", ""),
					(new StackLine(veryLongStackline8), "E.dll", "System.Web.Http.Tracing.Tracers.RequestMessageHandlerTracer.<>c__DisplayClass2_0", "<SendAsync>b__1"),
				};
				var stackLines = stacklineTests.Select(e => e.stackLine);
				StackLineAssemblyLookup.UpdateMissingAssemblyInformation(stackLines);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);

				CombineAssertions(() =>
				{
					stacklineTests.ForEach(st =>
					{
						var (stackLine, expectedAssembly, expectedType, expectedMethod) = st;
						AssertEquals(expectedAssembly, stackLine.Assembly);
						AssertEquals(expectedType, stackLine.Type);
						AssertEquals(expectedMethod, stackLine.Method);
					});
				});
			});
		}

		static void InsertTestData()
		{
			var assemblyPk1 = AddPublishedAssembly("A.dll", "$code");
			var classPk1 = AddPublishedClass(assemblyPk1, "AsyncTaskMethodB.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.AsyncStateMachineBox`1[[System.Threadin");
			AddPublishedMethod(classPk1, "Task");

			var assemblyPk2 = AddPublishedAssembly("B.dll", "$code");
			var classPk2 = AddPublishedClass(assemblyPk2, "System.Reactive.Concurrency.ConcurrencyAbstractionLayerImpl.Timer");
			AddPublishedMethod(classPk2, "Tick");

			var assemblyPk3 = AddPublishedAssembly("C.dll", "$code");
			var classPk3 = AddPublishedClass(assemblyPk3, "System.Collections.Generic.List`1");
			AddPublishedMethod(classPk3, ".ctor");

			var assemblyPk4 = AddPublishedAssembly("D.dll", "$code");
			var classPk4 = AddPublishedClass(assemblyPk4, "CargoWise.Glow.Index.Service.IndexDefinitionSqlAdapterGroup`1.<>c__DisplayClass2_0");
			AddPublishedMethod(classPk4, "<.ctor>b__0");

			var assemblyPk5 = AddPublishedAssembly("E.dll", "$code");
			var classPk5 = AddPublishedClass(assemblyPk5, "System.Web.Http.Tracing.Tracers.RequestMessageHandlerTracer.<>c__DisplayClass2_0");
			AddPublishedMethod(classPk5, "<SendAsync>b__1");
		}
	}
}
