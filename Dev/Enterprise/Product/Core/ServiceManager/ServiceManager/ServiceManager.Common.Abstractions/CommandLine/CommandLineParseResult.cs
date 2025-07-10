using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace ServiceManager.Common.Abstractions
{
	public class CommandLineParseResult
	{
		public CommandLineParseResult(ParseResult parseResult)
		{
			this.parseResult = parseResult;
		}

		public T? GetValue<T>(IValueDescriptor<T> valueDescriptor) => valueDescriptor switch
		{
			Option<T> option => parseResult.GetValueForOption(option),
			Argument<T> argument => parseResult.GetValueForArgument(argument),
			_ => throw new ArgumentOutOfRangeException(nameof(valueDescriptor),
				string.Format("Not an argument or option: '{0}'", valueDescriptor.ValueName))
		};

		readonly ParseResult parseResult;
	}
}
