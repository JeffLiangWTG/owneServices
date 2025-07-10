using System;
using System.Collections.Generic;
using Dat.Integration;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing;

sealed class TestRunnerTestListenerTests : TestCase
{
	public void TestGetTestResultCalledBeforeStartTestInvoked()
	{
		var testRunnerTestListener = new TestRunnerTestListener(new ErrorDescriptionListFactory());

		testRunnerTestListener.BeforeEachTest(DateTime.UtcNow);
		var testIdentifier = new TestIdentifier("scopeName", "elementName", "targetName");
		var testDescriptor = new TestDescriptor(
			testIdentifier,
			projectDefinedCapabilityRequirements: 0,
			DatTestFlags.Default,
			capabilityRequirements: [],
			overridableProperties: new Dictionary<string, string>());

		AssertEquals(testRunnerTestListener.GetTestResult(testDescriptor).TestDescriptor, testDescriptor);
	}

	public void TestGetTestResultCalledAfterStartTestInvoked()
	{
		var testRunnerTestListener = new TestRunnerTestListener(new ErrorDescriptionListFactory());

		testRunnerTestListener.BeforeEachTest(DateTime.UtcNow);
		testRunnerTestListener.StartTest(new TestRunnerTest(), DateTime.UtcNow);
		var testIdentifier = new TestIdentifier("scopeName", "elementName", "targetName");
		var testDescriptor = new TestDescriptor(
			testIdentifier,
			projectDefinedCapabilityRequirements: 0,
			DatTestFlags.Default,
			capabilityRequirements: [],
			overridableProperties: new Dictionary<string, string>());

		AssertEquals(testRunnerTestListener.GetTestResult(testDescriptor).TestDescriptor, testDescriptor);
	}
}
