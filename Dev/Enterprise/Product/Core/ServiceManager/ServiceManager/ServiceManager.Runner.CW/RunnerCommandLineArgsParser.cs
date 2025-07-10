using System.CommandLine;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	class RunnerCommandLineArgsParser
	{
		public RunnerCommandLineArgsParser(params string[] args)
		{
			optionProductKey = CreateRequiredOptionWithDebugDefault<string>("-ProductKey", string.Empty, "EDIDAT");

			parser = new CommandLineParser(
				description: "Process Controller Runner",
				[
					argumentServerName,
					argumentDatabaseName
				],
				[
					optionDebug,
					optionSDir,
					optionGrpcGuid,
					optionNoUIArgument,
					optionSingleRunArgument,
					optionEnableConnectionPooling,
					optionProductKey
				]);

			parseResult = parser.Parse(args);
		}

		public bool OptionDebug => parseResult.GetValue(optionDebug);
		public string? OptionSDir => parseResult.GetValue(optionSDir);
		public string? OptionGrpcGuid => parseResult.GetValue(optionGrpcGuid);
		public bool NoUIArgument => parseResult.GetValue(optionNoUIArgument);
		public bool SingleRunArgument => parseResult.GetValue(optionSingleRunArgument);
		public bool EnableConnectionPooling => parseResult.GetValue(optionEnableConnectionPooling);
		public string ProductKey => parseResult.GetValue(optionProductKey)!;
		public string? ServerName => parseResult.GetValue(argumentServerName);
		public string? DatabaseName => parseResult.GetValue(argumentDatabaseName);

		readonly Option<bool> optionDebug = new(name: "-debug",
			getDefaultValue: () => false,
			description: "");
		readonly Option<string> optionGrpcGuid = new(name: "-grpc",
			description: "");
		readonly Option<string> optionSDir = new(name: "-SDir",
			description: "")
			{ Arity = ArgumentArity.ZeroOrOne, };
		readonly Option<bool> optionNoUIArgument = new("-NoUI",
			getDefaultValue: () => false,
			description: "");
		readonly Option<bool> optionSingleRunArgument = new("-SingleRun",
			getDefaultValue: () => false,
			description: "");
		readonly Option<bool> optionEnableConnectionPooling = new("-EnableConnectionPooling",
			getDefaultValue: () => false,
			description: "");
		readonly Option<string> optionProductKey;
		readonly Argument<string> argumentServerName = CommandLineParser.ArgumentServerName;
		readonly Argument<string> argumentDatabaseName = CommandLineParser.ArgumentDatabaseName;

		readonly CommandLineParser parser;
		readonly CommandLineParseResult parseResult;

		Option<T> CreateRequiredOptionWithDebugDefault<T>(string name, string description, T defaultValue)
		{
			var option = new Option<T>(name: name,
				description: "",
				getDefaultValue: () => defaultValue);

			option.AddValidator(CommandLineParser.StringNotNullOrWhiteSpaceValidator);
			option.AddValidator(o =>
			{
				if (o.Tokens.Count == 0 && (o.Parent == null || !o.Parent.GetValueForOption(optionDebug)))
				{
					o.ErrorMessage = $"{name} is required";
				}
			});

			return option;
		}
	}
}
