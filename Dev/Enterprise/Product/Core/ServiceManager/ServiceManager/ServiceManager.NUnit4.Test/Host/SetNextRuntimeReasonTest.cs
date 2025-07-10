using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	public class SetNextRuntimeReasonTest
	{
		static readonly IEnumerable<(SetNextRuntimeReason reason, LogLevel logLevel, string messag)> ExpectingValues = new[]
		{
			(SetNextRuntimeReason.LoadingInitialValue, LogLevel.Debug, "we are loading the initial value from the database"),
			(SetNextRuntimeReason.UpdateReceivedFromPeerController, LogLevel.Debug, "an update was received from a peer controller"),
			(SetNextRuntimeReason.ScheduledToRun, LogLevel.Debug, "the task was scheduled to run"),
			(SetNextRuntimeReason.SetRequestReceived, LogLevel.Debug, "a set request was received"),
			(SetNextRuntimeReason.FailedRunAttempt, LogLevel.Information, "there was a failed run attempt"),
			(SetNextRuntimeReason.FailedToCalculate, LogLevel.Information, "we failed to calculate the next run time"),
			(SetNextRuntimeReason.LoadingFromLocalXml, LogLevel.Information, "a newer value was in the persisted XML file"),
		};

		[TestCaseSource(nameof(ExpectingValues))]
		public void TestNextRunReasonShowing((SetNextRuntimeReason reason, LogLevel logLevel, string message) tuple)
		{
			Assert.That(tuple.logLevel, Is.EqualTo(tuple.reason.LogInfo().LogLevel), "type should be equal");
			Assert.That(tuple.message, Is.EqualTo(tuple.reason.LogInfo().Message), "message should be equal");
		}

		[Test]
		public void TestAllNextRunReasonTested()
		{
			// Arrange
			var testedValues = ExpectingValues.Select(tuple => tuple.reason);
			var allValues = Enum.GetValues(typeof(SetNextRuntimeReason)).Cast<SetNextRuntimeReason>();

			// Act
			var result = allValues.Except(testedValues);

			// Assert
			Assert.That(result, Is.Empty, "SetNextRuntimeReasonTest is not test all enum");
		}
	}
}
