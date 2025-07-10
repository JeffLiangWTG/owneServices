using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;

namespace Enterprise.ServiceManager.Shared.Testing.CommandLine
{
	public class CommandLineParserTest
	{
		[SetUp]
		public void SetUp()
		{
			parser = new CommandLineParser(
				CommandDescription,
				new Argument[]
				{
					doubleArgument,
					boolArgument,
					stringArgument
				},
				new Option[]
				{
					boolOption,
					intOption,
					stringOption
				});
		}

		public class ArgumentTest : CommandLineParserTest
		{
			[Test]
			public void TestWithNoArgumentsOrOptions()
			{
				AssertErrors();
			}

			[Test]
			public void TestWithoutRequiredArguments()
			{
				AssertErrors("-int:456", "-boolOption");
			}

			[Test]
			public void TestWithoutOptionalArguments()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: null,
					"123.45");
			}

			[Test]
			public void TestWithoutLastOptionalArguments()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: null,
					"123.45", "true");
			}

			[Test]
			public void TestExtraArguments()
			{
				AssertErrors("123.45", "false", "string1", "-int:456", "extraArgument");
			}

			[Test]
			public void TestBoolArgumentWithBlankValue()
			{
				AssertErrors("123.45", "", "string1");
			}

			[Test]
			public void TestBoolArgumentValidValue_true()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: null,
					"123.45", "true");
			}

			[Test]
			public void TestBoolArgumentValidValue_false()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: false,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: null,
					"123.45", "false");
			}

			[Test]
			public void TestBoolArgumentValidValue_True()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: null,
					"123.45", "True");
			}

			[Test]
			public void TestBoolArgumentValidValue_False()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: false,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: null,
					"123.45", "False");
			}

			[Test]
			public void TestBoolArgumentValidValue_TRUE()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: null,
					"123.45", "TRUE");
			}

			[Test]
			public void TestBoolArgumentValidValue_FALSE()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: false,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: null,
					"123.45", "FALSE");
			}

			[Test]
			public void TestBoolArgumentInvalidValue_yes()
			{
				AssertErrors("123.45", "yes");
			}

			[Test]
			public void TestBoolArgumentInvalidValue_Yes()
			{
				AssertErrors("123.45", "Yes");
			}

			[Test]
			public void TestBoolArgumentInvalidValue_N()
			{
				AssertErrors("123.45", "N");
			}

			[Test]
			public void TestBoolArgumentInvalidValue_NO()
			{
				AssertErrors("123.45", "NO");
			}

			[Test]
			public void TestNumericArgumentWithoutValue()
			{
				AssertErrors();
			}

			[Test]
			public void TestNumericArgumentWithBlankValue()
			{
				AssertErrors("");
			}

			[Test]
			public void TestNumericArgumentWithValidValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: DefaultStringOptionValue,
					"123.45");
			}

			[Test]
			public void TestNumericArgumentWithInvalidValue()
			{
				AssertErrors("123.45-double");
			}

			[Test]
			public void TestStringArgumentWithoutValue_DefaultValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "true");
			}

			[Test]
			public void TestStringArgumentWithBlankValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: "",
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "true", "");
			}

			[Test]
			public void TestStringArgumentWithValidValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: "string1",
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "true", "string1");
			}

			[Test]
			public void TestStringArgumentWithSpecialValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: SpecialStringValueVerbatim,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "true", SpecialStringValue);
			}

			// any value is valid for string argument, so no test case for invalid value
		}

		public class OptionTest : CommandLineParserTest
		{
			[Test]
			public void TestParser_WithNoOption_OptionIsNull()
			{
				// Arrange
				var commandParser = new CommandLineParser(
					CommandDescription,
					Array.Empty<Argument>(),
					new Option[]
					{
						optionalOption,
					});

				// Act
				var result = commandParser.Parse(optionalOption.Aliases.First());

				// Assert
				Assert.That(result.GetValue(optionalOption), Is.Null);
			}

			[Test]
			public void TestParser_WithEmptyOption_OptionIsNull()
			{
				// Arrange
				var commandParser = new CommandLineParser(
					CommandDescription,
					Array.Empty<Argument>(),
					new Option[]
					{
						optionalOption,
					});

				// Act
				var result = commandParser.Parse($"{optionalOption.Aliases.First()}:");

				// Assert
				Assert.That(result.GetValue(optionalOption), Is.Null);
			}

			[Test]
			public void TestMissingOptionsWithDefaultValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: "string2",
					"123.45", "-string:string2");
			}

			[Test]
			public void TestMissingOptionsWithoutDefaultValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: true,
					intOptionValue: 456,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "-boolOption", "-int:456");
			}

			[Test]
			public void TestInvalidOptionWithoutValue_WithoutOptionalArguments()
			{
				AssertErrors("123.45", "-invalidOption");
			}

			[Test]
			public void TestInvalidOptionWithoutValue_WithoutOptionalStringArgument()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: "-invalidOption",
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "true", "-invalidOption");
			}

			[Test]
			public void TestInvalidOptionWithoutValue_WithAllArguments()
			{
				AssertErrors("123.45", "true", "string1", "-invalidOption");
			}

			[Test]
			public void TestInvalidOptionWithValue_WithoutOptionalArguments()
			{
				AssertErrors("123.45", "-invalidOption:value");
			}

			[Test]
			public void TestInvalidOptionWithValue_WithoutOptionalStringArgument()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: "-invalidOption:value",
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "true", "-invalidOption:value");
			}

			[Test]
			public void TestInvalidOptionWithValue_WithAllArguments()
			{
				AssertErrors("123.45", "true", "string1", "-invalidOption:value");
			}

			[Test]
			public void TestBoolOptionWithoutValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: true,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "-boolOption");
			}

			[Test]
			public void TestBoolOptionWithBlankValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: true,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "-boolOption:");
			}

			[Test]
			public void TestBoolOptionValidValue_true()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: true,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "-boolOption:true");
			}

			[Test]
			public void TestBoolOptionValidValue_false()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: false,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "-boolOption:false");
			}

			[Test]
			public void TestBoolOptionInvalidValue_yes()
			{
				AssertErrors("123.45", "true", "string1", "-boolOption:yes");
			}

			[Test]
			public void TestBoolOptionInvalidValue_NO()
			{
				AssertErrors("123.45", "true", "string1", "-boolOption:NO");
			}

			[Test]
			public void TestNumericOptionWithoutValue()
			{
				AssertErrors("123.45", "-int");
			}

			[Test]
			public void TestNumericOptionWithBlankValue()
			{
				AssertErrors("123.45", "-int:");
			}

			[Test]
			public void TestNumericOptionWithValidValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: 456,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "-int:456");
			}

			[Test]
			public void TestNumericOptionWithInvalidValue()
			{
				AssertErrors("123.45", "-int:123-number");
			}

			[Test]
			public void TestStringOptionWithoutValue()
			{
				AssertErrors("123.45", "-string");
			}

			[Test]
			public void TestStringOptionWithBlankValue()
			{
				AssertErrors("123.45", "-string:");
			}

			[Test]
			public void TestStringOptionWithValidValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: "string2",
					"123.45", "-string:string2");
			}

			[Test]
			public void TestStringOptionWithSpecialValue()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: DefaultIntOptionValue,
					stringOptionValue: SpecialStringValueVerbatim,
					"123.45", $"-string:{SpecialStringValue}");
			}

			// any value is valid for string option, so no test case for invalid value
		}

		public class ArgumentAndOptionTest : CommandLineParserTest
		{
			[Test]
			public void TestWithAllArgumentsAndOptions()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: "string1",
					boolOptionValue: true,
					intOptionValue: 456,
					stringOptionValue: "string2",
					"123.45", "true", "string1", "-boolOption:true", "-int:456", "-string:string2");
			}

			[Test]
			public void TestWithPartialArgumentsAndPartialOptions()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: DefaultBoolArgumentValue,
					stringArgumentValue: DefaultStringArgumentValue,
					boolOptionValue: DefaultBoolOptionValue,
					intOptionValue: 456,
					stringOptionValue: DefaultStringOptionValue,
					"123.45", "-int:456");
			}

			[Test]
			public void TestWithArgumentsInTheBeginning()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: "string1",
					boolOptionValue: true,
					intOptionValue: 456,
					stringOptionValue: "string2",
					"123.45", "true", "string1", "-boolOption", "-int:456", "-string:string2");
			}

			[Test]
			public void TestWithArgumentsInTheMiddle()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: "string1",
					boolOptionValue: true,
					intOptionValue: 456,
					stringOptionValue: "string2",
					"-boolOption", "123.45", "true", "string1", "-int:456", "-string:string2");
			}

			[Test]
			public void TestWithArgumentsInTheEnd()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: "string1",
					boolOptionValue: true,
					intOptionValue: 456,
					stringOptionValue: "string2",
					"-string:string2", "-boolOption", "-int: 456", "123.45", "true", "string1");
			}

			[Test]
			public void TestWithMixedOrder()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: "string1",
					boolOptionValue: true,
					intOptionValue: 456,
					stringOptionValue: "string2",
					"123.45", "-int:456", "true", "-string:string2", "string1", "-boolOption");
			}
		}

		public class OneStringAsArgsTest : CommandLineParserTest
		{
			[Test]
			public void TestArgumentWithoutQuote()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: "string1",
					boolOptionValue: true,
					intOptionValue: 456,
					stringOptionValue: "string2",
					"123.45 -int:456 true -string:string2 string1 -boolOption");
			}

			[Test]
			public void TestArgumentWithQuoteSpacesAndTabs()
			{
				AssertValues(
					doubleArgumentValue: 123.45,
					boolArgumentValue: true,
					stringArgumentValue: "string1",
					boolOptionValue: true,
					intOptionValue: 456,
					stringOptionValue: "string2  	value",
					"123.45 -int:456  true \"-string:string2  	value\" string1 	-boolOption true");
			}
		}

		void AssertValues(double doubleArgumentValue, bool boolArgumentValue, string stringArgumentValue,
			bool boolOptionValue, int intOptionValue, string? stringOptionValue,
			params string[] args)
		{
			var stdout = "";
			var stderr = "";

			CommandLineParseResult? parseResult = null;

			using (var console = new ConsoleOutput())
			{
				parseResult = parser!.Parse(args);

				stdout = console.GetStdout();
				stderr = console.GetStderr();
			}

			AssertValues(parseResult, stdout, stderr,
				doubleArgumentValue, boolArgumentValue, stringArgumentValue,
				boolOptionValue, intOptionValue, stringOptionValue);
		}

		void AssertValues(double doubleArgumentValue, bool boolArgumentValue, string stringArgumentValue,
			bool boolOptionValue, int intOptionValue, string? stringOptionValue,
			string args)
		{
			var stdout = "";
			var stderr = "";

			CommandLineParseResult? parseResult;

			using (var console = new ConsoleOutput())
			{
				parseResult = parser!.Parse(args);

				stdout = console.GetStdout();
				stderr = console.GetStderr();
			}

			AssertValues(parseResult, stdout, stderr,
				doubleArgumentValue, boolArgumentValue, stringArgumentValue,
				boolOptionValue, intOptionValue, stringOptionValue);
		}

		void AssertValues(CommandLineParseResult parseResult, string stdout, string stderr,
			double doubleArgumentValue, bool boolArgumentValue, string stringArgumentValue,
			bool boolOptionValue, int intOptionValue, string? stringOptionValue)
		{
			Assert.That(parseResult.GetValue(doubleArgument), Is.EqualTo(doubleArgumentValue), "For Double Argument");
			Assert.That(parseResult.GetValue(boolArgument), Is.EqualTo(boolArgumentValue), "For Bool Argument");
			Assert.That(parseResult.GetValue(stringArgument), Is.EqualTo(stringArgumentValue), "For String Argument");

			Assert.That(parseResult.GetValue(boolOption), Is.EqualTo(boolOptionValue), "For Bool Option");
			Assert.That(parseResult.GetValue(intOption), Is.EqualTo(intOptionValue), "For Int Option");
			Assert.That(parseResult.GetValue(stringOption), Is.EqualTo(stringOptionValue), "For String Option");

			Assert.That(stdout, Is.EqualTo(""));
			Assert.That(stderr, Is.EqualTo(""));
		}

		void AssertErrors(params string[] args)
		{
			// Arrange
			var stderr = "";

			// Assert
			var exception = Assert.Throws<CommandLineException>(() =>
			{
				using (var console = new ConsoleOutput())
				{
					try
					{
						// Act
						parser!.Parse(args);
					}
					finally
					{
						stderr = console.GetStderr();
					}
				}
			});

			Assert.That(exception!.Errors.Count, Is.Not.Zero);

			var outputErrorLines = stderr.Split(
				[Environment.NewLine],
				StringSplitOptions.None);
			Assert.That(exception!.Errors.Count, Is.EqualTo(outputErrorLines.Length));
		}

		CommandLineParser? parser;

		const string CommandDescription = "Test Command";

		const string DefaultStringArgumentValue = "DefaultStringArgumentValue";
		const bool DefaultBoolArgumentValue = false;

		const int DefaultIntOptionValue = 123;
		const bool DefaultBoolOptionValue = false;
		const string? DefaultStringOptionValue = null;

		const string SpecialStringValue = "str\"ing2  \t test\\path";
		const string SpecialStringValueVerbatim = @"str""ing2  	 test\path";

		readonly Argument<double> doubleArgument = new Argument<double>(
			name: "doubleArgument",
			description: "Double Argument");

		readonly Argument<bool> boolArgument = new Argument<bool>(
			name: "boolArgument",
			description: "Boolean Argument");

		readonly Argument<string> stringArgument = new Argument<string>(
			name: "stringArgument",
			getDefaultValue: () => DefaultStringArgumentValue,
			description: "String Argument");

		readonly Option<bool> boolOption = new Option<bool>(
			name: "-boolOption",
			getDefaultValue: () => DefaultBoolOptionValue,
			description: "Boolean Option");

		readonly Option<int> intOption = new Option<int>(
			name: "-int",
			getDefaultValue: () => DefaultIntOptionValue,
			description: "Integer Option");

		readonly Option<string> stringOption = new Option<string>(
			name: "-string",
			description: "String Option");

		readonly Option<string> optionalOption = new Option<string>(
			name: "-optional",
			description: "Optional")
		{
			Arity = ArgumentArity.ZeroOrOne,
		};
	}

	class ConsoleOutput : IDisposable
	{
		public ConsoleOutput()
		{
			stdoutWriter = new StringWriter();
			originalStdout = Console.Out;
			Console.SetOut(stdoutWriter);

			stderrWriter = new StringWriter();
			originalStderr = Console.Error;
			Console.SetError(stderrWriter);
		}

		public string GetStdout()
		{
			return stdoutWriter.ToString().Trim();
		}

		public string GetStderr()
		{
			return stderrWriter.ToString().Trim();
		}

		public void Dispose()
		{
			Console.SetOut(originalStdout);
			stdoutWriter.Dispose();

			Console.SetError(originalStderr);
			stderrWriter.Dispose();
		}

		readonly StringWriter stdoutWriter;
		readonly TextWriter originalStdout;
		readonly StringWriter stderrWriter;
		readonly TextWriter originalStderr;
	}
}
