using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Linq;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Common
{
	public class CommandLineParser
	{
		public CommandLineParser(string description, IEnumerable<Argument> arguments, IEnumerable<Option> options)
		{
			var rootCommand = new RootCommand(description);
			foreach (var argument in arguments)
			{
				rootCommand.Add(argument);
			}
			foreach (var option in options)
			{
				rootCommand.Add(option);
			}

			var commandLineBuilder = new CommandLineBuilder(rootCommand)
				.EnablePosixBundling(false)
				.UseTypoCorrections()
				.UseParseErrorReporting();

			parser = commandLineBuilder.Build();
		}

		public CommandLineParseResult Parse(params string[] args)
		{
			return ProcessParseResult(parser.Parse(args));
		}

		public CommandLineParseResult Parse(string args)
		{
			return ProcessParseResult(parser.Parse(args));
		}

		public static Argument<string> ArgumentServerName
		{
			get
			{
				var argument = new Argument<string>(name: "ServerName",
					description: "");
				argument.AddValidator(StringNotNullOrWhiteSpaceValidator);

				return argument;
			}
		}

		public static Argument<string> ArgumentDatabaseName
		{
			get
			{
				var argument = new Argument<string>(name: "DatabaseName",
					description: "");
				argument.AddValidator(StringNotNullOrWhiteSpaceValidator);

				return argument;
			}
		}

		public static void StringNotNullOrWhiteSpaceValidator(ArgumentResult result)
		{
			var value = result.GetValueOrDefault<string>();

			if (string.IsNullOrWhiteSpace(value))
			{
				result.ErrorMessage = string.Format("Argument value cannot be null or empty for argument: '{0}'.", result.Symbol.Name);
			}
		}

		public static void StringNotNullOrWhiteSpaceValidator(OptionResult result)
		{
			if (result == null)
			{
				throw new ArgumentNullException(nameof(result));
			}

			var value = result.GetValueOrDefault<string>();

			if (string.IsNullOrWhiteSpace(value))
			{
				result.ErrorMessage = string.Format("Argument value cannot be null or empty for option: '{0}'.", result.Symbol.Name);
			}
		}

		static CommandLineParseResult ProcessParseResult(ParseResult parseResult)
		{
			foreach (var error in parseResult.Errors)
			{
				Console.Error.WriteLine(error.Message);
			}

			if (parseResult.Errors.Count > 0)
			{
				throw new CommandLineException(parseResult.Errors.Select(e => new CommandLineParseError(e.SymbolResult.Symbol.Name, e.Message)));
			}

			return new CommandLineParseResult(parseResult);
		}

		readonly Parser parser;
	}
}
