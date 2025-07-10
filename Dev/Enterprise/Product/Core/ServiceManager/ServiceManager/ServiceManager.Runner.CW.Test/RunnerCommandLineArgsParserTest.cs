using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Enterprise.ServiceManager.Runner;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test
{
	public class RunnerCommandLineArgsParserTest : TestCase
	{
		public void TestParser_WithEmptySDir_SDirIsNull()
		{
			// Arrange, Act
			var parser = new RunnerCommandLineArgsParser(
				"ServerName",
				"DatabaseName",
				"-ProductKey:ProductKey",
				"-SDir:");

			// Assert
			AssertNull(parser.OptionSDir);
		}

		public void TestDefaultValues()
		{
			var parser = new RunnerCommandLineArgsParser(
				"ServerName",
				"DatabaseName",
				"-ProductKey:ProductKey");

			AssertValues(parser, new Dictionary<string, object>()
			{
				{ nameof(RunnerCommandLineArgsParser.ServerName), "ServerName" },
				{ nameof(RunnerCommandLineArgsParser.DatabaseName), "DatabaseName" },
				{ "-ProductKey", "ProductKey" },
			});
		}

		public void TestAllArgumentsAndOptions()
		{
			var parser = new RunnerCommandLineArgsParser(
				"ServerName",
				"DatabaseName",
				"-debug",
				"-SDir:ServerDirectory",
				"-NoUI",
				"-SingleRun",
				"-EnableConnectionPooling",
				"-ProductKey:ProductKey");

			AssertValues(parser, new Dictionary<string, object>()
			{
				{ nameof(RunnerCommandLineArgsParser.ServerName), "ServerName" },
				{ nameof(RunnerCommandLineArgsParser.DatabaseName), "DatabaseName" },
				{ nameof(RunnerCommandLineArgsParser.OptionDebug),  true },
				{ nameof(RunnerCommandLineArgsParser.OptionSDir),  "ServerDirectory" },
				{ nameof(RunnerCommandLineArgsParser.NoUIArgument),  true },
				{ nameof(RunnerCommandLineArgsParser.SingleRunArgument),  true },
				{ nameof(RunnerCommandLineArgsParser.EnableConnectionPooling),  true },
				{ nameof(RunnerCommandLineArgsParser.ProductKey), "ProductKey" },
			});
		}

		public void TestAllArgumentsAreMandatory()
		{
			AssertErrors([
				ErrorRequiredArgumentMissingForCommand,
				ErrorRequiredArgumentMissingForCommand,
				"-ProductKey is required",
			]);
		}

		public void TestProductKeyIsNotRequiredForDebug()
		{
			// Arrange, Act
			var parser = new RunnerCommandLineArgsParser("ServerName", "DatabaseName", "-debug");

			// Assert
			AssertEquals(parser.ProductKey, "EDIDAT");
		}

		public void TestDatabaseNameIsMandatory()
		{
			AssertErrors([
					ErrorRequiredArgumentMissingForCommand,
				],
			"ServerName", "-ProductKey:ProductKey");
		}

		public void TestExtraArgument()
		{
			AssertErrors([
					string.Format(ErrorUnrecognizedCommandOrArgument, "extraArgument")
				],
			"ServerName", "DatabaseName", "-ProductKey:ProductKey", "extraArgument");
		}

		public void TestArgumentValuesAreNotNullOrWhiteSpace()
		{
			AssertErrors([
					string.Format(ErrorOptionValueIsNullOrWhiteSpace, nameof(RunnerCommandLineArgsParser.ProductKey)),
					string.Format(ErrorArgumentValueIsNullOrWhiteSpace, nameof(RunnerCommandLineArgsParser.ServerName)),
					string.Format(ErrorArgumentValueIsNullOrWhiteSpace, nameof(RunnerCommandLineArgsParser.DatabaseName)),
				],
			"	  ", "  	  ", "-ProductKey:  	  ");
		}

		public void TestInvalidOption()
		{
			AssertErrors([
					string.Format(ErrorUnrecognizedCommandOrArgument, "-INVALID_OPTION:123")
				],
			"ServerName", "DatabaseName", "-debug", "-INVALID_OPTION:123");
		}

		void AssertValues(RunnerCommandLineArgsParser parser, IDictionary<string, object> values)
		{
			foreach (var entry in GetFuncs)
			{
				var key = entry.Key;
				var expectedValue = values.ContainsKey(key) ? values[key] : DefaultValues[key];

				AssertEquals($"For property \"{key}\"", expectedValue, entry.Value.Invoke(parser));
			}
		}

		static void AssertErrors(string[] expectedErrorMessages, params string[] args)
		{
			var exception = AssertExceptionThrown<CommandLineException>(() =>
			{
				// Act
				var parser = new RunnerCommandLineArgsParser(args);
			});

			// Assert
			var errorMessages = exception.Errors.Select(e => e.Message).ToArray();
			AssertArrayEqualsByElements(expectedErrorMessages, errorMessages);
		}

		static readonly string ErrorRequiredArgumentMissingForCommand = $"Required argument missing for command: '{RootCommand.ExecutableName}'.";
		const string ErrorUnrecognizedCommandOrArgument = "Unrecognized command or argument '{0}'.";
		const string ErrorArgumentValueIsNullOrWhiteSpace = "Argument value cannot be null or empty for argument: '{0}'.";
		const string ErrorOptionValueIsNullOrWhiteSpace = "Argument value cannot be null or empty for option: '{0}'.";

		readonly IDictionary<string, object> DefaultValues = new Dictionary<string, object>()
		{
			{ nameof(RunnerCommandLineArgsParser.OptionDebug), false },
			{ nameof(RunnerCommandLineArgsParser.OptionSDir), null },
			{ nameof(RunnerCommandLineArgsParser.OptionGrpcGuid), null },
			{ nameof(RunnerCommandLineArgsParser.NoUIArgument), false },
			{ nameof(RunnerCommandLineArgsParser.SingleRunArgument), false },
			{ nameof(RunnerCommandLineArgsParser.EnableConnectionPooling), false },
			{ nameof(RunnerCommandLineArgsParser.ServerName), null },
			{ nameof(RunnerCommandLineArgsParser.DatabaseName), null },
		};

		readonly IDictionary<string, Func<RunnerCommandLineArgsParser, object>> GetFuncs = new Dictionary<string, Func<RunnerCommandLineArgsParser, object>>()
		{
			{ nameof(RunnerCommandLineArgsParser.OptionDebug), (options) => options.OptionDebug },
			{ nameof(RunnerCommandLineArgsParser.OptionSDir), (options) => options.OptionSDir },
			{ nameof(RunnerCommandLineArgsParser.OptionGrpcGuid), (options) => options.OptionGrpcGuid },
			{ nameof(RunnerCommandLineArgsParser.NoUIArgument), (options) => options.NoUIArgument },
			{ nameof(RunnerCommandLineArgsParser.SingleRunArgument), (options) => options.SingleRunArgument },
			{ nameof(RunnerCommandLineArgsParser.EnableConnectionPooling), (options) => options.EnableConnectionPooling },
			{ nameof(RunnerCommandLineArgsParser.ServerName), (options) => options.ServerName },
			{ nameof(RunnerCommandLineArgsParser.DatabaseName), (options) => options.DatabaseName },
		};
	}
}
