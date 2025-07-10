using System;
using System.Reflection;
using CargoWise.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ServiceManager.Logging.CW.Test.Extensions;

public class ServiceCollectionExtensionsTest : TestCase
{
	public void Test_RegisterNextLoggerServices_DefaultLogger()
	{
		using var provider = GetServiceProvider();
		var logger = provider.GetRequiredService<ILogger>();
		CheckLogger(logger, "ServiceManager.Logging.CW.LoggerNLogWrapper");
	}

	public void Test_RegisterNextLoggerServices_GenericLogger()
	{
		using var provider = GetServiceProvider();
		var logger = provider.GetRequiredService<ILogger<ServiceCollectionExtensionsTest>>();
		CheckGenericLogger(logger);
	}

	public void Test_RegisterNextLoggerServices_LoggerFactory()
	{
		using var provider = GetServiceProvider();
		var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
		var logger = loggerFactory.CreateLogger("Test Category");
		CheckLogger(logger, "Test Category");
	}

	void CheckGenericLogger<TCategory>(ILogger<TCategory> logger)
	{
		AssertType<Logger<TCategory>>("Logger type", logger);
		var value = GetPrivateField<ILogger>(logger, "_logger");
		CheckLogger(value, typeof(TCategory).FullName);
	}

	void CheckLogger(ILogger logger, string expectedCategoryName)
	{
		// check that the logger is of type MEL.Logger using Reflection
		var melAssembly = typeof(Microsoft.Extensions.Logging.LoggerFactory).Assembly;
		var loggerType = melAssembly.GetType("Microsoft.Extensions.Logging.Logger");
		var messageLoggerType = melAssembly.GetType("Microsoft.Extensions.Logging.MessageLogger");
		AssertNotNull(nameof(loggerType), loggerType);
		AssertNotNull(nameof(messageLoggerType), messageLoggerType);
		AssertType("Logger type", loggerType, logger);

		var messageLoggers = GetPublicProperty(logger, "MessageLoggers");
		// check that the MessageLoggers property is MessageLogger[]
		AssertType("MessageLoggers type", messageLoggerType!.MakeArrayType(), messageLoggers);
		// check that there is a single MessageLogger in the MessageLoggers array
		AssertEquals("MessageLoggers length", 1, (messageLoggers as Array)?.Length);
		var messageLogger = (messageLoggers as Array)?.GetValue(0);
		AssertNotNull("MessageLogger", messageLogger);
		// get the property Logger from the MessageLogger
		var loggerWrapper = GetPublicProperty<LoggerNLogWrapperWrapper>(messageLogger, "Logger");

		AssertEquals("CategoryName", expectedCategoryName, loggerWrapper?.CategoryName);
		using (Db.ClearServerDetailsTemporarily())
		{
			AssertEquals("We cannot log if DB is not yet initialized", false, loggerWrapper?.IsEnabled(LogLevel.Information));
		}
		AssertEquals("IsEnabled", true, loggerWrapper?.IsEnabled(LogLevel.Information));
	}

	static TField GetPrivateField<TField>(object obj, string name) where TField : class
	{
		var type = obj.GetType();
		// get the private field from the object
		var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
		AssertNotNull($"{type}.{name}", field);
		// get the value of the field
		var fieldValue = field!.GetValue(obj);
		AssertNotNull($"{type}.{name} value", fieldValue);
		// check that the value of the field implements TField
		var value = fieldValue as TField;
		AssertNotNull($"{type}.{name} should be an instance of {typeof(TField)} but is {value?.GetType()}", value);
		return value;
	}

	static TProperty GetPublicProperty<TProperty>(object obj, string name) where TProperty : class
	{
		var propertyValue = GetPublicProperty(obj, name);
		// check that the value of the field implements TProperty
		var value = propertyValue as TProperty;
		AssertNotNull($"{obj.GetType()}.{name} should be an instance of {typeof(TProperty)} but is {value?.GetType()}", value);
		return value;
	}

	static object GetPublicProperty(object obj, string name)
	{
		var type = obj.GetType();
		// get the public property from the object
		var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
		AssertNotNull($"{type}.{name}", property);
		// get the value of the property
		var propertyValue = property!.GetValue(obj);
		AssertNotNull($"{type}.{name} value", propertyValue);
		return propertyValue;
	}

	static ServiceProvider GetServiceProvider()
	{
		var services = new ServiceCollection();
		services.AddLogging(builder => builder.ClearProviders());
		services.RegisterNextLoggerServices();
		return services.BuildServiceProvider();
	}
}
