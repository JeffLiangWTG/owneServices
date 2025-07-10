using System;
using System.CommandLine;
using System.CommandLine.Binding;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;

namespace Enterprise.ServiceManager.Shared.Testing.CommandLine
{
	public static class CommandLineParseResultTest
	{
		public class ArgumentValueTest
		{
			[Test]
			public void TestStringArgumentValue()
			{
				AssertArgument(
					argument: new Argument<string>(name: "stringArgument"),
					expectedValue: "stringValue",
					args: "stringValue");
			}

			[Test]
			public void TestDefaultStringArgumentValue()
			{
				var argument = new Argument<string>(name: "stringArgument", getDefaultValue: () => "defaultStringValue");

				AssertArgument(argument, expectedValue: "defaultStringValue");
				AssertArgument(argument, expectedValue: "stringValue", args: "stringValue");
			}

			[Test]
			public void TestBoolArgumentValue()
			{
				AssertArgument(
					argument: new Argument<bool>(name: "boolArgument"),
					expectedValue: true,
					args: "true");
			}

			[Test]
			public void TestDefaultBoolArgumentValue()
			{
				var argument = new Argument<bool>(name: "boolArgument", getDefaultValue: () => true);

				AssertArgument(argument, expectedValue: true);
				AssertArgument(argument, expectedValue: false, args: "false");
			}

			[Test]
			public void TestDecimalArgumentValue()
			{
				AssertArgument(
					argument: new Argument<decimal>(name: "decimalArgument"),
					expectedValue: 123.45M,
					args: "123.45");
			}

			[Test]
			public void TestDefaultDecimalArgumentValue()
			{
				var argument = new Argument<decimal>(name: "decimalArgument", getDefaultValue: () => 123.45M);

				AssertArgument(argument, expectedValue: 123.45M);
				AssertArgument(argument, expectedValue: 456.78M, args: "456.78");
			}

			[Test]
			public void TestInvalidArgument()
			{
				// Arrange
				var argument = new Argument<string>(name: "stringArgument");
				var cmd = new RootCommand { argument };

				// Act
				var parseResult = new CommandLineParseResult(cmd.Parse("stringValue"));

				// Assert
				Assert.That(parseResult.GetValue(new Argument<string>(name: "invalidStringArgument")), Is.Null);
				Assert.That(parseResult.GetValue(new Argument<bool>(name: "invalidBoolArgument")), Is.False);
				Assert.That(parseResult.GetValue(new Argument<decimal>(name: "invalidDecimalArgument")), Is.EqualTo(0M));
			}

			[Test]
			public void TestFailureWithInvalidArgument()
			{
				// Arrange
				var argument = new Argument<int>(name: "intArgument");
				var cmd = new RootCommand { argument };

				// Act
				var parseResult = new CommandLineParseResult(cmd.Parse("123", "invalidArgumentValue"));

				// Assert
				Assert.That(parseResult.GetValue(argument), Is.EqualTo(123));

				Assert.That(parseResult.GetValue(new Argument<string>(name: "invalidArgumentValue")), Is.Null);
				Assert.That(parseResult.GetValue(new Argument<string>(name: "invalidStringArgument")), Is.Null);
				Assert.That(parseResult.GetValue(new Argument<bool>(name: "invalidBoolArgument")), Is.False);
				Assert.That(parseResult.GetValue(new Argument<int>(name: "invalidIntArgument")), Is.Zero);
			}

			[Test]
			public void TestFailureWithInvalidArgumentValue()
			{
				// Arrange
				var argument = new Argument<int>(name: "intArgument");
				var cmd = new RootCommand { argument };

				// Act
				var parseResult = new CommandLineParseResult(cmd.Parse("non-int"));

				// Assert
				Assert.Throws<InvalidOperationException>(() => parseResult.GetValue(argument));

				Assert.That(parseResult.GetValue(new Argument<string>(name: "invalidStringArgument")), Is.Null);
				Assert.That(parseResult.GetValue(new Argument<bool>(name: "invalidBoolArgument")), Is.False);
				Assert.That(parseResult.GetValue(new Argument<int>(name: "invalidIntArgument")), Is.Zero);
			}

			void AssertArgument<T>(Argument<T> argument, T expectedValue, params string[] args)
			{
				// Arrange
				var cmd = new RootCommand { argument };

				// Act
				var parseResult = new CommandLineParseResult(cmd.Parse(args));

				// Assert
				Assert.That(parseResult.GetValue(argument), Is.EqualTo(expectedValue));
			}
		}

		public class OptionValueTest
		{
			[Test]
			public void TestStringOptionValue()
			{
				AssertOption(
					option: new Option<string>(name: "-stringOption"),
					expectedValue: "stringValue",
					args: "-stringOption:stringValue");
			}

			[Test]
			public void TestDefaultStringOptionValue()
			{
				var option = new Option<string>(name: "-stringOption", getDefaultValue: () => "defaultStringValue");

				AssertOption(option, expectedValue: "defaultStringValue");
				AssertOption(option, expectedValue: "stringValue", args: "-stringOption:stringValue");
			}

			[Test]
			public void TestBoolOptionValue()
			{
				AssertOption(
					option: new Option<bool>(name: "-boolOption"),
					expectedValue: true,
					args: "-boolOption:true");
			}

			[Test]
			public void TestDefaultBoolOptionValue()
			{
				var option = new Option<bool>(name: "-boolOption", getDefaultValue: () => true);

				AssertOption(option, expectedValue: true);
				AssertOption(option, expectedValue: false, args: "-boolOption:false");
			}

			[Test]
			public void TestDecimalOptionValue()
			{
				AssertOption(
					option: new Option<decimal>(name: "-decimalOption"),
					expectedValue: 123.45M,
					args: "-decimalOption:123.45");
			}

			[Test]
			public void TestDefaultDecimalOptionValue()
			{
				var option = new Option<decimal>(name: "-decimalOption", getDefaultValue: () => 123.45M);

				AssertOption(option, expectedValue: 123.45M);
				AssertOption(option, expectedValue: 456.78M, args: "-decimalOption:456.78");
			}

			[Test]
			public void TestInvalidOption()
			{
				// Arrange
				var option = new Option<string>(name: "-stringOption");
				var cmd = new RootCommand { option };

				// Act
				var parseResult = new CommandLineParseResult(cmd.Parse("-stringOption:stringValue"));

				// Assert
				Assert.That(parseResult.GetValue(new Option<string>(name: "-invalidStringOption")), Is.Null);
				Assert.That(parseResult.GetValue(new Option<bool>(name: "-invalidBoolOption")), Is.False);
				Assert.That(parseResult.GetValue(new Option<decimal>(name: "-invalidDecimalOption")), Is.EqualTo(0M));
			}

			[Test]
			public void TestFailureWithInvalidOption()
			{
				// Arrange
				var option = new Option<int>(name: "-intOption");
				var cmd = new RootCommand { option };

				// Act
				var parseResult = new CommandLineParseResult(cmd.Parse("-intOption:123", "-invalidOption:value"));

				// Assert
				Assert.That(parseResult.GetValue(option), Is.EqualTo(123));

				Assert.That(parseResult.GetValue(new Option<string>(name: "-invalidOption")), Is.Null);
				Assert.That(parseResult.GetValue(new Option<string>(name: "-invalidStringOption")), Is.Null);
				Assert.That(parseResult.GetValue(new Option<bool>(name: "-invalidBoolOption")), Is.False);
				Assert.That(parseResult.GetValue(new Option<decimal>(name: "-invalidDecimalOption")), Is.EqualTo(0M));
			}

			[Test]
			public void TestFailureWithInvalidOptionValue()
			{
				// Arrange
				var option = new Option<int>(name: "-intOption");
				var cmd = new RootCommand { option };

				// Act
				var parseResult = new CommandLineParseResult(cmd.Parse("-intOption:non-int"));

				// Assert
				Assert.Throws<InvalidOperationException>(() => parseResult.GetValue(option));

				Assert.That(parseResult.GetValue(new Option<string>(name: "-invalidStringOption")), Is.Null);
				Assert.That(parseResult.GetValue(new Option<bool>(name: "-invalidBoolOption")), Is.False);
				Assert.That(parseResult.GetValue(new Option<decimal>(name: "-invalidDecimalOption")), Is.EqualTo(0M));
			}

			void AssertOption<T>(Option<T> option, T expectedValue, params string[] args)
			{
				// Arrange
				var cmd = new RootCommand { option };

				// Act
				var parseResult = new CommandLineParseResult(cmd.Parse(args));

				// Assert
				Assert.That(parseResult.GetValue(option), Is.EqualTo(expectedValue));
			}
		}

		public class ArgumentAndOptionValueTest
		{
			[Test]
			public void TestArgumentAndOptionValue()
			{
				// Arrange
				var argument = new Argument<string>(name: "stringArgument");
				var option = new Option<int>(name: "-intOption");
				var cmd = new RootCommand { argument, option };

				// Act
				var parseResult = new CommandLineParseResult(cmd.Parse("stringValue", "-intOption:123"));

				// Assert
				Assert.That(parseResult.GetValue(argument), Is.EqualTo("stringValue"));
				Assert.That(parseResult.GetValue(option), Is.EqualTo(123));
			}

			[Test]
			public void TestNonArgumentOrOption()
			{
				// Arrange
				var argument = new Argument<string>(name: "stringArgument");
				var option = new Option<int>(name: "-intOption");
				var cmd = new RootCommand { argument, option };

				// Act
				var parseResult = new CommandLineParseResult(cmd.Parse("stringValue", "-intOption:123"));

				// Assert
				var exception = Assert.Throws(Is.InstanceOf<ArgumentException>(), () => parseResult.GetValue(new TestValueDescriptor())) as ArgumentException;
				Assert.That(exception!.ParamName, Is.EqualTo("valueDescriptor"));
				Assert.That(exception!.Message, Does.Contain("Not an argument or option: 'TestName'"));
			}

			class TestValueDescriptor : IValueDescriptor<string>
			{
				public TestValueDescriptor()
				{
					ValueType = typeof(string);
				}

				public Type ValueType { get; }
				public string ValueName => "TestName";
				public bool HasDefaultValue => false;
				public object? GetDefaultValue() => null;
			}
		}
	}
}
