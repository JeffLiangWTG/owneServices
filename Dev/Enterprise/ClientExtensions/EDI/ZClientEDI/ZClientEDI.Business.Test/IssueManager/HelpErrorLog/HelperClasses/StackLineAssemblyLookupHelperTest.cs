using System.Collections.Generic;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Tests;

public sealed class StackLineAssemblyLookupHelperTest : TestCase
{
	public void TestGetFullMethodNameWithoutParameters()
	{
		AssertFullMethodName("", string.Empty);
		AssertFullMethodName(null, string.Empty);
		AssertFullMethodName("System.Reactive.Concurrency.ConcurrencyAbstractionLayerImpl.Timer.Tick(", "System.Reactive.Concurrency.ConcurrencyAbstractionLayerImpl.Timer.Tick");
		AssertFullMethodName("System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.AsyncStateMachineBox`1[[SecureWebService.WhsSecurityAccessWebServiceResponse, Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[Warehouse.RF.Core.Business.WebServices.SecureServiceManager.<GetAndHandleWebServiceResponse>d__133`1[[SecureWebService.WhsSecurityAccessWebServiceResponse, Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].ExecutionContextCallback(Object s)", "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.AsyncStateMachineBox`1[[SecureWebService.WhsSecurityAccessWebServiceResponse, Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[Warehouse.RF.Core.Business.WebServices.SecureServiceManager.<GetAndHandleWebServiceResponse>d__133`1[[SecureWebService.WhsSecurityAccessWebServiceResponse, Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].ExecutionContextCallback");
		AssertFullMethodName("lambda_method(Closure , Object , Object[] )", "lambda_method");
		AssertFullMethodName("(Closure , Object , Object[] )", string.Empty);
		AssertFullMethodName("className[assembly information].methodName[assembly information](Object)", "className[assembly information].methodName[assembly information]");
	}

	public void TestParseFullMethodName()
	{
		AssertParseFullMethodName("System.Reactive.Concurrency.ConcurrencyAbstractionLayerImpl.Timer.Tick", "System.Reactive.Concurrency.ConcurrencyAbstractionLayerImpl.Timer", "Tick");
		AssertParseFullMethodName("System.Collections.Generic.List`1..ctor", "System.Collections.Generic.List`1", ".ctor");
		AssertParseFullMethodName("CargoWise.Glow.Index.Service.IndexDefinitionSqlAdapterGroup`1.<>c__DisplayClass2_0.<.ctor>b__0", "CargoWise.Glow.Index.Service.IndexDefinitionSqlAdapterGroup`1.<>c__DisplayClass2_0", "<.ctor>b__0");
		AssertParseFullMethodName(".ctor", null, ".ctor");
		AssertParseFullMethodName("lambda_method149", null, "lambda_method149");
		AssertParseFullMethodName("", null, "");
		AssertParseFullMethodName(".", null, "");
		AssertParseFullMethodName("System.Web.Http.Tracing.Tracers.RequestMessageHandlerTracer.<>c__DisplayClass2_0.<SendAsync>b__1", "System.Web.Http.Tracing.Tracers.RequestMessageHandlerTracer.<>c__DisplayClass2_0", "<SendAsync>b__1");
		AssertParseFullMethodName("System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.AsyncStateMachineBox`1[[SecureWebService.WhsSecurityAccessWebServiceResponse, Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[Warehouse.RF.Core.Business.WebServices.SecureServiceManager.<GetAndHandleWebServiceResponse>d__133`1[[SecureWebService.WhsSecurityAccessWebServiceResponse, Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], Warehouse.RF.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].ExecutionContextCallback", "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.AsyncStateMachineBox`1", "ExecutionContextCallback");
		AssertParseFullMethodName("className[assembly information].methodName[assembly information]", "className", "methodName");
	}

	public void TestParseFullMethodName_ReportErrorWhenMultipleSiblingAssemblyInformation()
	{
		var exampleStackLine = "className[assembly information]partOfClassName[assembly information].methodName";
		var (className, methodName) = StackLineAssemblyLookupHelper.ParseFullMethodName(exampleStackLine);
		AssertEquals("className", className);
		AssertEquals("methodName", methodName);
		Assert(ErrorReporter.LastMessageReported == "In RemoveAssemblyInformation method, our truncate algorithm is not enough which make us missing some information, please check it.");
		ErrorReporter.Clear();
	}

	public void TestTruncate()
	{
		AssertTruncateResult(null, 2, null);
		AssertTruncateResult(string.Empty, 2, string.Empty);
		AssertTruncateResult("a", 2, "a");
		AssertTruncateResult("aaa", 2, "aa");
	}

	public void TestTruncateStackLine()
	{
		var result = StackLineAssemblyLookupHelper.TruncateStackLine(new string('a', AutoHelpErrorStackLineCount.Schema.HSL_StackLineMaxLength + 1));
		AssertEquals(new string('a', AutoHelpErrorStackLineCount.Schema.HSL_StackLineMaxLength), result);
	}

	public void TestTruncateAssembly()
	{
		var result = StackLineAssemblyLookupHelper.TruncateAssembly(new string('a', AutoHelpErrorStackLineCount.Schema.HSL_AssemblyMaxLength + 1));
		AssertEquals(new string('a', AutoHelpErrorStackLineCount.Schema.HSL_AssemblyMaxLength), result);
	}

	public void TestTruncateType()
	{
		var result = StackLineAssemblyLookupHelper.TruncateType(new string('a', StackLineAssemblyLookupHelper.ClassNameMaxLength + 1));
		AssertEquals(new string('a', StackLineAssemblyLookupHelper.ClassNameMaxLength), result);
	}

	public void TestTruncateMethod()
	{
		var result = StackLineAssemblyLookupHelper.TruncateType(new string('a', StackLineAssemblyLookupHelper.MethodNameMaxLength + 1));
		AssertEquals(new string('a', StackLineAssemblyLookupHelper.MethodNameMaxLength), result);
	}

	public void TestTruncateParameters()
	{
		var result = StackLineAssemblyLookupHelper.TruncateParameters(new string('a', AutoHelpErrorStackLineCount.Schema.HSL_ParametersMaxLength + 1));
		AssertEquals(new string('a', AutoHelpErrorStackLineCount.Schema.HSL_ParametersMaxLength), result);
	}

	public void TestIsWeightCalculableStackLine()
	{
		var stackLineTests = new List<(IStackLine stackLine, bool expectedResult)>
		{
			(new StackLine("System.Reactive.Concurrency.ConcurrencyAbstractionLayerImpl.Timer.Tick("), true),
			(new StackLine("xxxx"), false),
			(null, false),
		};

		CombineAssertions(() =>
		{
			stackLineTests.ForEach(stackLine =>
			{
				var isWeightCalculableStackLine = StackLineAssemblyLookupHelper.IsWeightCalculableStackLine(stackLine.stackLine);
				AssertEquals($"stack line: {stackLine.stackLine} useful result is: {stackLine.expectedResult}", stackLine.expectedResult, isWeightCalculableStackLine);
			});
		});
	}

	public void TestStackLineFieldRestrict()
	{
		AssertEquals(512, StackLineAssemblyLookupHelper.ClassNameMaxLength);
		AssertEquals(512, StackLineAssemblyLookupHelper.MethodNameMaxLength);
	}

	#region implementation

	void AssertFullMethodName(string stackLine, string expectedFullMethodName)
	{
		var fullMethodName = StackLineAssemblyLookupHelper.GetFullMethodNameWithoutParameters(stackLine);
		AssertEquals(expectedFullMethodName, fullMethodName);
	}

	void AssertParseFullMethodName(string fullMethodName, string expectedClassName, string expectedMethodName)
	{
		var actual = StackLineAssemblyLookupHelper.ParseFullMethodName(fullMethodName);
		var msg = $"Expected \"{expectedClassName}::{expectedMethodName}\" but was \"{actual.className}::{actual.methodName}\"";
		AssertEquals(msg, (expectedClassName, expectedMethodName), actual);
	}

	void AssertTruncateResult(string str, int length, string expectedTruncateResult)
	{
		var truncate = StackLineAssemblyLookupHelper.Truncate(str, length);
		AssertEquals(expectedTruncateResult, truncate);
	}

	#endregion
}
