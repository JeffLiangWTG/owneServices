using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Tests
{
	public sealed class StackLineTest : TestCase
	{
		public void TestStackLineSingleParamConstructor_CanNotUseNullParam()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new StackLine(null));
		}

		public void TestStackLineSingleParamConstructor_AutoParseTypeAndMethod()
		{
			var stackLineTests = new List<(string stackLine, string expectedType, string expectedMethod)> {
				("System.Reactive.Concurrency.ConcurrencyAbstractionLayerImpl.Timer.Tick(", "System.Reactive.Concurrency.ConcurrencyAbstractionLayerImpl.Timer", "Tick"),
				("System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.AsyncStateMachineBox`1[[SecureWebService.WhsSecurityAccessWebServiceResponse, Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[Warehouse.RF.Core.Business.WebServices.SecureServiceManager.<GetAndHandleWebServiceResponse>d__133`1[[SecureWebService.WhsSecurityAccessWebServiceResponse, Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].ExecutionContextCallback(Object s)", "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.AsyncStateMachineBox`1", "ExecutionContextCallback"),
				("lambda_method(Closure , Object , Object[] )", "", "lambda_method"),
				("(Closure , Object , Object[] )", "", ""),
				("", "", ""),
			};
			CombineAssertions(() =>
			{
				stackLineTests.ForEach(test => AssertStackLineAutoParse(test.stackLine, test.stackLine, test.expectedType, test.expectedMethod));
			});
		}

		public void TestStackLineSingleParamConstructors_Truncate()
		{
			var stackLineTests = new List<(string stackLine, string expectedStackLine, string expectedType, string expectedMethod)>
			{
				(
					$"{new string('x', 897)}.y(a",
					$"{new string('x', 897)}.y(",
					$"{new string('x', StackLineAssemblyLookupHelper.ClassNameMaxLength)}",
					"y"
				),
				(
					$"x.{new string('y', 899)}(",
					$"x.{new string('y', 898)}",
					"x",
					new string('y', StackLineAssemblyLookupHelper.MethodNameMaxLength)
				),
				(
					new string('x', 901),
					new string('x', AutoHelpErrorStackLineCount.Schema.HSL_StackLineMaxLength),
					"",
					""
				),
			};
			CombineAssertions(() =>
			{
				stackLineTests.ForEach(test => AssertStackLineAutoParse(test.stackLine, test.expectedStackLine, test.expectedType, test.expectedMethod));
			});
		}

		public void TestStackLineConstructor_Truncate()
		{
			var stackLineTests =
				new List<(string stackLine, string expectedStackLine, string type, string expectedType, string method,
					string expectedMethod, string assembly, string expectedAssembly, string parameter, string
					expectedParameter)>()
				{
					(
						new string('a', 901),
						new string('a', AutoHelpErrorStackLineCount.Schema.HSL_StackLineMaxLength),
						new string('b', 513),
						new string('b', StackLineAssemblyLookupHelper.ClassNameMaxLength),
						new string('c', 513),
						new string('c', StackLineAssemblyLookupHelper.MethodNameMaxLength),
						new string('d', 261),
						new string('d', AutoHelpErrorStackLineCount.Schema.HSL_AssemblyMaxLength),
						new string('e', 1025),
						new string('e', AutoHelpErrorStackLineCount.Schema.HSL_ParametersMaxLength)
					),
				};
			CombineAssertions(() =>
			{
				stackLineTests.ForEach(test =>
				{
					var stackLine = new StackLine(test.assembly, test.type, test.method, test.parameter, test.stackLine);
					AssertEquals(test.expectedAssembly, stackLine.Assembly);
					AssertEquals(test.expectedType, stackLine.Type);
					AssertEquals(test.expectedMethod, stackLine.Method);
					AssertEquals(test.expectedParameter, stackLine.Parameters);
					AssertEquals(test.expectedStackLine, stackLine.FullStackLine);
				});
			});
		}

		public void TestStackLineProperty_Truncate()
		{
			var stackLine = new StackLine(new string('x', 901));
			stackLine.Type = new string('a', 513);
			stackLine.Method = new string('b', 513);
			stackLine.Assembly = new string('c', 261);

			AssertEquals(new string('a', StackLineAssemblyLookupHelper.ClassNameMaxLength), stackLine.Type);
			AssertEquals(new string('b', StackLineAssemblyLookupHelper.MethodNameMaxLength), stackLine.Method);
			AssertEquals(new string('c', AutoHelpErrorStackLineCount.Schema.HSL_AssemblyMaxLength), stackLine.Assembly);
		}

		#region implementation

		void AssertStackLineAutoParse(string fullStackLine, string expectedStackLine, string expectedType, string expectedMethod)
		{
			var stackLine = new StackLine(fullStackLine);
			AssertEquals($"expect stack line is {expectedStackLine}, but is {stackLine.FullStackLine}", expectedStackLine, stackLine.FullStackLine);
			AssertEquals($"expected Type is {expectedType}, but is {stackLine.Type}", expectedType, stackLine.Type);
			AssertEquals($"expected Method is {expectedMethod}, but is {stackLine.Method}", expectedMethod, stackLine.Method);
		}

		#endregion
	}
}
