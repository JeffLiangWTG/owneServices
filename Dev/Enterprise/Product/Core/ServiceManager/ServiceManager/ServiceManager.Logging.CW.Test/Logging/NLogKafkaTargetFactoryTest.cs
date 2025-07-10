using System;
using System.Collections.Generic;
using Enterprise.Registry.Business;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NUnit.Framework;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Shared.Testing.Logging
{
	public class NLogKafkaTargetFactoryTest : TransactionedTestCase
	{
		public void TestCommonParams()
		{
			// Arrange
			var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
				{
					new SystemDefinableCodeDescriptionBoolWithExtraBool
					{
						Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true,
					},
				};
			newValue.SetDefaultCode(LoggingMethods.KAF, true);
			SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			var factory = new NLogKafkaTargetFactory();
			const string expectedLayout = "JsonLayout=message-${message}|eventTime-${date:universalTime=true:format=yyyy-MM-dd\\THH\\:mm\\:ss.fffK}";
			const int expectedLayoutMaxRecursionLimit = 3;
			const bool expectedLayoutIncludeAllProperties = true;
			const bool defaultTargetTimeout = true;
			const int defaultTargetBufferSize = 200;
			const int defaultTargetFlushTimeout = 1000;
			const string defaultTargetName = "kafka";

			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Act
				var target = (BufferingTargetWrapper)factory.GetOrCreateTarget();
				var kafkaTarget = (TargetWithLayout)target.WrappedTarget;
				var layout = (JsonLayout)kafkaTarget?.Layout;

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals(nameof(target.SlidingTimeout), defaultTargetTimeout, target.SlidingTimeout);
					AssertEquals(nameof(target.BufferSize), defaultTargetBufferSize, target.BufferSize);
					AssertEquals(nameof(target.FlushTimeout), defaultTargetFlushTimeout, target.FlushTimeout);
					AssertEquals(nameof(target.Name), defaultTargetName, target.Name);
					AssertEquals(nameof(layout), expectedLayout, layout.ToString());
					AssertEquals(nameof(layout.MaxRecursionLimit), expectedLayoutMaxRecursionLimit, layout.MaxRecursionLimit);
					AssertEquals(nameof(layout.IncludeEventProperties), expectedLayoutIncludeAllProperties, layout.IncludeEventProperties);
				});

				kafkaTarget.Dispose();
			}
		}

		static class Source
		{
			public static IEnumerable<(string expected, LogEventInfo logEventInfo)> LogEvents()
			{
				yield return ("{ \"message\": \"message\", \"eventTime\": \"2006-12-23T20:22:22.000Z\" }",
					new LogEventInfo
					{
						Level = LogLevel.Info,
						Message = "message",
						TimeStamp = new DateTime(2006, 12, 23, 20, 22, 22, DateTimeKind.Utc),
					});

				yield return ("{ \"message\": \"message\", \"eventTime\": \"2006-12-23T20:22:22.123Z\", \"property1\": \"value1\", \"property2\": \"value2\" }",
					new LogEventInfo
					{
						Message = "message",
						TimeStamp = new DateTime(2006, 12, 23, 20, 22, 22, 123, DateTimeKind.Utc),
						Properties =
						{
							["property1"] = "value1",
							["property2"] = "value2",
						},
					});

				yield return ("{ \"message\": \"message\", \"eventTime\": \"2006-12-23T20:22:22.000Z\", \"property1\": \"value1\", \"property2\": \"value2\", \"nestedProperty\": {\"name\":\"name\", \"hostname\":\"hostname\", \"pid\":10} }",
					new LogEventInfo
					{
						Message = "message",
						TimeStamp = new DateTime(2006, 12, 23, 20, 22, 22, DateTimeKind.Utc),
						Properties =
						{
							["property1"] = "value1",
							["property2"] = "value2",
							["nestedProperty"] = new
							{
								name = "name",
								hostname = "hostname",
								pid = 10,
							},
						},
					});

				yield return ("{ \"message\": \"message\", \"eventTime\": \"2006-12-23T20:22:22.000Z\", \"level1\": {\"level1\":{\"level2\":{\"level3\":\"{ inlinedValue = { value = value } }\"}}} }",
					new LogEventInfo
					{
						Message = "message",
						TimeStamp = new DateTime(2006, 12, 23, 20, 22, 22, DateTimeKind.Utc),
						Properties =
						{
							["level1"] = new
							{
								level1 = new
								{
									level2 = new
									{
										level3 = new
										{
											inlinedValue = new
											{
												value = "value",
											},
										},
									},
								},
							},
						},
					});
			}
		}

		public void TestRender()
		{
			// Arrange
			var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
				{
					new SystemDefinableCodeDescriptionBoolWithExtraBool
					{
						Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true,
					},
				};
			newValue.SetDefaultCode(LoggingMethods.KAF, true);
			SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				var factory = new NLogKafkaTargetFactory();
				var target = (BufferingTargetWrapper)factory.GetOrCreateTarget();
				var kafkaTarget = (TargetWithLayout)target.WrappedTarget;
				var layout = kafkaTarget.Layout;

				foreach (var testcase in Source.LogEvents())
				{
					// Act
					var result = layout.Render(testcase.logEventInfo);

					// Assert
					AssertEquals(testcase.expected, result);
				}

				kafkaTarget.Dispose();
			}
		}

		public void TestReturnsBufferedTargetIfExists()
		{
			// Arrange
			var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true,
				},
			};
			newValue.SetDefaultCode(LoggingMethods.KAF, true);

			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue))
			{
				using var target = new BufferingTargetWrapper { Name = "kafka" };
				LogManager.Configuration.AddTarget(target);

				// Act
				var result = new NLogKafkaTargetFactory().GetOrCreateTarget();

				// Assert
				AssertType<BufferingTargetWrapper>(result);
				AssertEquals(target, result);
			}
		}
	}
}
