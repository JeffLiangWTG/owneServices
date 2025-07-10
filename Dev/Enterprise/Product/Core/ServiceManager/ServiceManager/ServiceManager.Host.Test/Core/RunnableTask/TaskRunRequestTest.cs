using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.ServiceManager.Host.Testing.Helpers;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using WTG.NUnit;

namespace Enterprise.ServiceManager.Host.Testing.Core.RunnableTask
{
	[TestsSubclassesOf(typeof(TaskRunRequest))]
	abstract class TaskRunRequestTest<T> where T : TaskRunRequest
	{
		public const string GuidRegExTemplate = "[0-9a-z]{8}-[0-9a-z]{4}-[0-9a-z]{4}-[0-9a-z]{4}-[0-9a-z]{12}";
		public const string TimeDurationRegExTemplate = @"[0-9]{2}\:[0-9]{2}\:[0-9]{2}\:[0-9]{2}\.[0-9]{3}";
		protected const string FormatLogMessagesTestCode = "TES";
		protected abstract T[] SameRequestList { get; }
		protected abstract T[] DifferentRequestList { get; }
		protected abstract Dictionary<LogMessageStage, (string regMessage, object[] parameters)> ExpectedLogMessages { get; }
		Dictionary<string, IRunnableServiceTask> taskDictionary;

		protected abstract T CreateRequest(string code);
		protected abstract T CreateRequest(string code, IStopwatch stepDurationStopwatch, IStopwatch totalDurationStopwatch);
		protected abstract T CreateRequest(string code, IStopwatch stepDurationStopwatch, IStopwatch totalDurationStopwatch, bool echoes);

		protected IRunnableServiceTask GetMockTask(string code)
		{
			if (taskDictionary.TryGetValue(code, out var oldResult))
			{
				return oldResult;
			}
			var randomFixedTime = new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.FromSeconds(0));
			var task = Mock.Of<IRunnableServiceTask>(x =>
				x.Code == code
				&& x.NextScheduledRunTime == randomFixedTime);
			taskDictionary.Add(code, task);
			return task;
		}

		[Test]
		public void TestTaskRunRequestEquality_Equal()
		{
			// Arrange
			// Act
			// Assert
			Assert.Multiple(() =>
			{
				Assert.That(SameRequestList.Length, Is.GreaterThanOrEqualTo(2), "SameRequestsList should contain at least 2 element");
				for (int i = 0; i < SameRequestList.Length; i++)
				{
					for (int j = i + 1; j < SameRequestList.Length; j++)
					{
						Assert.That(SameRequestList[j], Is.EqualTo(SameRequestList[i]), $"Request[{i}] should be same as Request[{j}]");
					}
				}
			});
		}

		[Test]
		public void TestTaskRunRequestEquality_NotEqual()
		{
			// Arrange
			var mockTask = Mock.Of<ITaskRunRequest>();

			// Act
			// Assert
			Assert.Multiple(() =>
			{
				Assert.That(DifferentRequestList.Length, Is.GreaterThanOrEqualTo(2), "DifferentRequestsList should contain at least 2 element");
				for (int i = 0; i < DifferentRequestList.Length; i++)
				{
					for (int j = i + 1; j < DifferentRequestList.Length; j++)
					{
						Assert.That(DifferentRequestList[j], Is.Not.EqualTo(DifferentRequestList[i]), $"Request[{i}] should be different from Request[{j}]");
					}
					Assert.That(mockTask, Is.Not.EqualTo(DifferentRequestList[i]).Using(CustomComparers.TypeComparison), $"Request[{i}] should be different from different type of request");
				}
			});
		}

		[Test]
		public void TestFormatLogMessages()
		{
			// Arrange
			var testList = Enum
				.GetValues(typeof(LogMessageStage))
				.Cast<LogMessageStage>()
				.ToList();
			var request = CreateRequest(FormatLogMessagesTestCode);

			Assert.Multiple(() =>
			{
				testList.ForEach(x =>
				{
					var expectMessage = ExpectedLogMessages[x];

					// Act
					try
					{
						var result = request.FormatRequestToLogMessage(x, expectMessage.parameters);

						// Assert
						var reg = new Regex(expectMessage.regMessage);
						Assert.That(result, Does.Match(reg), $"{expectMessage.regMessage} is not regular express of {result}");
					}
					catch (IndexOutOfRangeException)
					{
						Assert.Fail($"{Enum.GetName(typeof(LogMessageStage), x)} out of range for type {GetType().Name}");
					}
				});
			});
		}

		[Test]
		public void TestFormatLogMessage_LongStepDuration()
		{
			// Arrange
			stepDurationStopwatchMock
				.Setup(x => x.Elapsed)
				.Returns(new TimeSpan(2, 1, 1, 1));
			var request = CreateRequest(FormatLogMessagesTestCode, stepDurationStopwatchMock.Object, totalDurationStopwatchMock.Object, true);

			// Act
			var result = request.FormatRequestToLogMessage(LogMessageStage.DequeuedRequest, ExpectedLogMessages[LogMessageStage.DequeuedRequest]);

			// Assert
			var reg = new Regex(@".*is dequeued\. \[Time in the queue 02:01:01:01.000\]");
			Assert.That(result, Does.Match(reg));
		}

		[Test]
		public void TestFormatLogMessage_LongTotalProcessingDuration()
		{
			// Arrange
			totalDurationStopwatchMock
				.Setup(x => x.Elapsed)
				.Returns(new TimeSpan(2, 1, 1, 1));
			var request = CreateRequest(FormatLogMessagesTestCode, stepDurationStopwatchMock.Object, totalDurationStopwatchMock.Object, true);

			// Act
			var result = request.FormatRequestToLogMessage(LogMessageStage.RequestIsSentToRunner, ExpectedLogMessages[LogMessageStage.RequestIsSentToRunner].parameters);

			// Assert
			var reg = new Regex(@".*is sent to Runner with PID=1\. \[Total Processing Duration: 02:01:01:01.000\]");
			Assert.That(result, Does.Match(reg));
		}

		[Test]
		public void TestEnqueuedRequestCount()
		{
			// Arrange
			var request = CreateRequest(FormatLogMessagesTestCode);

			Assert.Multiple(() =>
			{
				for (var i = 1; i < 3; i++)
				{
					// Act
					var result = request.FormatRequestToLogMessage(LogMessageStage.EnqueuedRequest);

					// Assert
					var reg = new Regex($"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] [\\D]+ is enqueued {i} time");
					Assert.That(result, Does.Match(reg));
				}
			});
		}

		[Test]
		public void TestHandleUnableToRun()
		{
			Assert.Multiple(() =>
			{
				foreach (var (reason, logLevel, message) in UnableToRunReasonHelper.Source)
				{
					Test(reason);
				}
			});

			void Test(UnableToRunReason failureReason)
			{
				// Arrange
				var request = CreateRequest("TST");

				// Act
				request.OnUnableToRun(failureReason, false, true);

				// Assert
				Assert.DoesNotThrow(() =>
				{
					var mockTask = Mock.Get(request.Task);
					mockTask.Verify(l => l.HandleUnableToRun(failureReason, request, false, true), Times.Once);
				});
			}
		}

		[SetUp]
		public virtual void SetUp()
		{
			taskDictionary = new Dictionary<string, IRunnableServiceTask>();
			stepDurationStopwatchMock = new Mock<IStopwatch>();
			totalDurationStopwatchMock = new Mock<IStopwatch>();
		}

		Mock<IStopwatch> stepDurationStopwatchMock;
		Mock<IStopwatch> totalDurationStopwatchMock;
	}
}
