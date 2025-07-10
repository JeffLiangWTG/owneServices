using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Logging;
using Moq;
using WTG.ApplicationLogging.Abstractions;

namespace Enterprise.ServiceManager.Shared.Testing;

public static class ApplicationLoggingTestHelper
{
	public static IDisposable Listen()
	{
		var listener = new ActivityListener();
		listener.Sample = (ref ActivityCreationOptions<ActivityContext> o) => ActivitySamplingResult.AllData;
		listener.ShouldListenTo = o => true;

		ActivitySource.AddActivityListener(listener);

		return listener;
	}

	public static IDisposable ListenFor(string name, string operationName, Action<Activity> activityStopped)
	{
		var listener = new ActivityListener();
		listener.Sample = (ref ActivityCreationOptions<ActivityContext> o) => ActivitySamplingResult.AllData;
		listener.ShouldListenTo = o => o.Name == name;
		listener.ActivityStopped = o =>
		{
			if (o.OperationName == operationName)
			{
				activityStopped(o);
			}
		};

		ActivitySource.AddActivityListener(listener);

		return listener;
	}

	public static IApplicationLoggerFactory MockLoggerFactory() =>
		MockLoggerFactory<IApplicationLoggerFactory>().Object;

	public static ICategorizedApplicationLoggerFactory MockCategorizedLoggerFactory()
	{
		var loggerFactoryMock = MockLoggerFactory<ICategorizedApplicationLoggerFactory>();

		loggerFactoryMock
			.Setup(o => o.CreateCategorizedLogger(It.IsAny<LoggerCategory>(), It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()))
			.Returns((LoggerCategory category, string name, IEnumerable<KeyValuePair<string, object>> properties) =>
			{
				var activitySource = new ActivitySource(name);
				return Mock.Of<IApplicationLogger>(o => o.ActivitySource == activitySource);
			});

		return loggerFactoryMock.Object;
	}

	static Mock<T> MockLoggerFactory<T>() where T : class, IApplicationLoggerFactory
	{
		var loggerFactoryMock = new Mock<T>();

		loggerFactoryMock
			.Setup(o => o.CreateApplicationLogger(It.IsAny<string>()))
			.Returns((string name) =>
			{
				var activitySource = new ActivitySource(name);

				return Mock.Of<IApplicationLogger>(o => o.ActivitySource == activitySource);
			});

		return loggerFactoryMock;
	}
}
