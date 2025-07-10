using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.CommandLine
{
	public class HostCommandLineArgsParserTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestParser_WithEmptySDir_SDirIsNull()
		{
			// Arrange, Act
			var parser = new HostCommandLineArgsParser(
				"ServerName",
				"DatabaseName",
				"-SDir:");

			// Assert
			NUnit.Framework.Assert.That(parser.ServerDirectory, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			var parser = new HostCommandLineArgsParser(
				"ServerName",
				"DatabaseName");

			AssertValues(parser, new Dictionary<string, object>()
			{
				{ nameof(IServiceManagerHostOptions.OptionConsole), false },
				{ nameof(IServiceManagerHostOptions.OptionInstall), false },
				{ nameof(IServiceManagerHostOptions.OptionUninstall), false },
				{ nameof(IServiceManagerHostOptions.OptionStart), false },
				{ nameof(IServiceManagerHostOptions.OptionStop), false },
				{ nameof(IServiceManagerHostOptions.OptionQuiet), false },
				{ nameof(IServiceManagerHostOptions.Username), "" },
				{ nameof(IServiceManagerHostOptions.Password), "" },
				{ nameof(IServiceManagerHostOptions.Host), "." },
				{ nameof(IServiceManagerHostOptions.Automatic), false },
				{ nameof(IServiceManagerHostOptions.Config), "" },
				{ nameof(IServiceManagerHostOptions.RemoveDbRecord), false },
				{ nameof(IServiceManagerHostOptions.ServerName), "ServerName" },
				{ nameof(IServiceManagerHostOptions.DatabaseName), "DatabaseName" },
				{ "SDir", "" },
			});
		}

		[ExpectNoExceptions]
		public void TestAllOptions()
		{
			var parser = new HostCommandLineArgsParser(
				"-console",
				"-install",
				"-uninstall",
				"-start",
				"-stop",
				"-quiet",
				"-username:user",
				"-password:pass",
				"-host:HostName",
				"-automatic",
				"-config:ConfigValue",
				"-removeDbRecord",
				"-SDir:ServerDirectory",
				"ServerName",
				"DatabaseName");

			AssertValues(parser, new Dictionary<string, object>()
			{
				{ nameof(IServiceManagerHostOptions.OptionConsole), true },
				{ nameof(IServiceManagerHostOptions.OptionInstall), true },
				{ nameof(IServiceManagerHostOptions.OptionUninstall), true },
				{ nameof(IServiceManagerHostOptions.OptionStart), true },
				{ nameof(IServiceManagerHostOptions.OptionStop), true },
				{ nameof(IServiceManagerHostOptions.OptionQuiet), true },
				{ nameof(IServiceManagerHostOptions.Username), "user" },
				{ nameof(IServiceManagerHostOptions.Password), "pass" },
				{ nameof(IServiceManagerHostOptions.Host), "HostName" },
				{ nameof(IServiceManagerHostOptions.Automatic), true },
				{ nameof(IServiceManagerHostOptions.Config), "ConfigValue" },
				{ nameof(IServiceManagerHostOptions.RemoveDbRecord), true },
				{ nameof(IServiceManagerHostOptions.ServerName), "ServerName" },
				{ nameof(IServiceManagerHostOptions.DatabaseName), "DatabaseName" },
				{ "SDir", "ServerDirectory" },
			});
		}

		[ExpectNoExceptions]
		public void TestInstall()
		{
			var parser = new HostCommandLineArgsParser(
				"-install",
				"-automatic",
				"-username:user",
				"-password:pass",
				"-config:ConfigValue",
				"ServerName",
				"DatabaseName",
				"-quiet");

			AssertValues(parser, new Dictionary<string, object>()
			{
				{ nameof(IServiceManagerHostOptions.OptionInstall), true },
				{ nameof(IServiceManagerHostOptions.Automatic), true },
				{ nameof(IServiceManagerHostOptions.Username), "user" },
				{ nameof(IServiceManagerHostOptions.Password), "pass" },
				{ nameof(IServiceManagerHostOptions.Config), "ConfigValue" },
				{ nameof(IServiceManagerHostOptions.ServerName), "ServerName" },
				{ nameof(IServiceManagerHostOptions.DatabaseName), "DatabaseName" },
				{ nameof(IServiceManagerHostOptions.OptionQuiet), true },
			});
		}

		[ExpectNoExceptions]
		public void TestUninstall()
		{
			var parser = new HostCommandLineArgsParser(
				"-uninstall",
				"-automatic",
				"-username:user",
				"-password:pass",
				"-config:ConfigValue",
				"ServerName",
				"DatabaseName",
				"-removeDbRecord");

			AssertValues(parser, new Dictionary<string, object>()
			{
				{ nameof(IServiceManagerHostOptions.OptionUninstall), true },
				{ nameof(IServiceManagerHostOptions.Automatic), true },
				{ nameof(IServiceManagerHostOptions.Username), "user" },
				{ nameof(IServiceManagerHostOptions.Password), "pass" },
				{ nameof(IServiceManagerHostOptions.Config), "ConfigValue" },
				{ nameof(IServiceManagerHostOptions.ServerName), "ServerName" },
				{ nameof(IServiceManagerHostOptions.DatabaseName), "DatabaseName" },
				{ nameof(IServiceManagerHostOptions.RemoveDbRecord), true },
			});
		}

		[ExpectNoExceptions]
		public void TestStart()
		{
			var parser = new HostCommandLineArgsParser(
				"-start",
				"-host:HostName",
				"-username:user",
				"-password:pass",
				"ServerName",
				"DatabaseName");

			AssertValues(parser, new Dictionary<string, object>()
			{
				{ nameof(IServiceManagerHostOptions.OptionStart), true },
				{ nameof(IServiceManagerHostOptions.Host), "HostName" },
				{ nameof(IServiceManagerHostOptions.Username), "user" },
				{ nameof(IServiceManagerHostOptions.Password), "pass" },
				{ nameof(IServiceManagerHostOptions.ServerName), "ServerName" },
				{ nameof(IServiceManagerHostOptions.DatabaseName), "DatabaseName" },
			});
		}

		[ExpectNoExceptions]
		public void TestStop()
		{
			var parser = new HostCommandLineArgsParser(
				"-stop",
				"-host:HostName",
				"-username:user",
				"-password:pass",
				"ServerName",
				"DatabaseName");

			AssertValues(parser, new Dictionary<string, object>()
			{
				{ nameof(IServiceManagerHostOptions.OptionStop), true },
				{ nameof(IServiceManagerHostOptions.Host), "HostName" },
				{ nameof(IServiceManagerHostOptions.Username), "user" },
				{ nameof(IServiceManagerHostOptions.Password), "pass" },
				{ nameof(IServiceManagerHostOptions.ServerName), "ServerName" },
				{ nameof(IServiceManagerHostOptions.DatabaseName), "DatabaseName" },
			});
		}

		[ExpectNoExceptions]
		public void TestConsole()
		{
			var parser = new HostCommandLineArgsParser("-console", "ServerName", "DatabaseName");

			AssertValues(parser, new Dictionary<string, object>()
			{
				{ nameof(IServiceManagerHostOptions.OptionConsole), true },
				{ nameof(IServiceManagerHostOptions.ServerName), "ServerName" },
				{ nameof(IServiceManagerHostOptions.DatabaseName), "DatabaseName" },
			});
		}

		[ExpectNoExceptions]
		public void TestRun()
		{
			var parser = new HostCommandLineArgsParser("ServerName", "DatabaseName");

			AssertValues(parser, new Dictionary<string, object>()
			{
				{ nameof(IServiceManagerHostOptions.ServerName), "ServerName" },
				{ nameof(IServiceManagerHostOptions.DatabaseName), "DatabaseName" },
			});
		}

		public void TestAllArgumentsAreMandatory()
		{
			AssertErrors(new[]
			{
				ErrorRequiredArgumentMissingForCommand,
				ErrorRequiredArgumentMissingForCommand,
			});
		}

		public void TestDatabaseNameIsMandatory()
		{
			AssertErrors(new[]
			{
				ErrorRequiredArgumentMissingForCommand,
			},
			"ServerName");
		}

		public void TestExtraArgument()
		{
			AssertErrors(new[]
			{
				string.Format(ErrorUnrecognizedCommandOrArgument, "extraArgument"),
			},
			"ServerName", "DatabaseName", "extraArgument");
		}

		public void TestArgumentValuesAreNotNullOrWhiteSpace()
		{
			AssertErrors(new[]
			{
				string.Format(ErrorArgumentValueIsNullOrWhiteSpace, nameof(IServiceManagerHostOptions.ServerName)),
				string.Format(ErrorArgumentValueIsNullOrWhiteSpace, nameof(IServiceManagerHostOptions.DatabaseName)),
			},
			"	  ", "  	  ");
		}

		public void TestInvalidOption()
		{
			AssertErrors(new[]
			{
				string.Format(ErrorUnrecognizedCommandOrArgument, "-INVALID_OPTION:123"),
			},
			"ServerName", "DatabaseName", "-console", "-INVALID_OPTION:123");
		}

		void AssertValues(HostCommandLineArgsParser parser, IDictionary<string, object> values)
		{
			foreach (var entry in GetFuncs)
			{
				var key = entry.Key;
				var expectedValue = values.ContainsKey(key) ? values[key] : DefaultValues[key];

				NUnit.Framework.Assert.That(entry.Value.Invoke(parser), Is.EqualTo(expectedValue), $"For property \"{key}\"");
			}
		}

		void AssertErrors(string[] expectedErrorMessages, params string[] args)
		{
			var exception = AssertExceptionThrown<CommandLineException>(() =>
			{
				// Act
				var parser = new HostCommandLineArgsParser(args);
			});

			// Assert
			var errorMessages = exception.Errors.Select(e => e.Message).ToArray();
			NUnit.Framework.Assert.That(errorMessages, Is.EqualTo(expectedErrorMessages));
		}

		static readonly string ErrorRequiredArgumentMissingForCommand = $"Required argument missing for command: '{RootCommand.ExecutableName}'.";
		const string ErrorUnrecognizedCommandOrArgument = "Unrecognized command or argument '{0}'.";
		const string ErrorArgumentValueIsNullOrWhiteSpace = "Argument value cannot be null or empty for argument: '{0}'.";

		readonly IDictionary<string, object> DefaultValues = new Dictionary<string, object>()
		{
			{ nameof(IServiceManagerHostOptions.OptionConsole), false },
			{ nameof(IServiceManagerHostOptions.OptionInstall), false },
			{ nameof(IServiceManagerHostOptions.OptionUninstall), false },
			{ nameof(IServiceManagerHostOptions.OptionStart), false },
			{ nameof(IServiceManagerHostOptions.OptionStop), false },
			{ nameof(IServiceManagerHostOptions.OptionQuiet), false },
			{ nameof(IServiceManagerHostOptions.Username), "" },
			{ nameof(IServiceManagerHostOptions.Password), "" },
			{ nameof(IServiceManagerHostOptions.Host), "." },
			{ nameof(IServiceManagerHostOptions.Automatic), false },
			{ nameof(IServiceManagerHostOptions.Config), "" },
			{ nameof(IServiceManagerHostOptions.RemoveDbRecord), false },
			{ nameof(IServiceManagerHostOptions.ServerName), null },
			{ nameof(IServiceManagerHostOptions.DatabaseName), null },
			{ "SDir", "" },
		};

		readonly IDictionary<string, Func<IServiceManagerHostOptions, object>> GetFuncs = new Dictionary<string, Func<IServiceManagerHostOptions, object>>()
		{
			{ nameof(IServiceManagerHostOptions.OptionConsole), (options) => options.OptionConsole },
			{ nameof(IServiceManagerHostOptions.OptionInstall), (options) => options.OptionInstall },
			{ nameof(IServiceManagerHostOptions.OptionUninstall), (options) => options.OptionUninstall },
			{ nameof(IServiceManagerHostOptions.OptionStart), (options) => options.OptionStart },
			{ nameof(IServiceManagerHostOptions.OptionStop), (options) => options.OptionStop },
			{ nameof(IServiceManagerHostOptions.OptionQuiet), (options) => options.OptionQuiet },
			{ nameof(IServiceManagerHostOptions.Username), (options) => options.Username },
			{ nameof(IServiceManagerHostOptions.Password), (options) => options.Password },
			{ nameof(IServiceManagerHostOptions.Host), (options) => options.Host },
			{ nameof(IServiceManagerHostOptions.Automatic), (options) => options.Automatic },
			{ nameof(IServiceManagerHostOptions.Config), (options) => options.Config },
			{ nameof(IServiceManagerHostOptions.RemoveDbRecord), (options) => options.RemoveDbRecord },
			{ nameof(IServiceManagerHostOptions.ServerName), (options) => options.ServerName },
			{ nameof(IServiceManagerHostOptions.DatabaseName), (options) => options.DatabaseName },
			{ "SDir", (options) => (options as HostCommandLineArgsParser).ServerDirectory },
		};
	}
}
