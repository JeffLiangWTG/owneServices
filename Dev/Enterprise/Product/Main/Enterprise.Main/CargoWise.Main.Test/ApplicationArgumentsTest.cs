using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class ApplicationArgumentsTest : TestCase
	{
		public void TestNewArgumentAreDisabledByDefault()
		{
			ApplicationArguments arguments = new ApplicationArguments(System.Array.Empty<string>());
			foreach (string key in arguments.OptionalArgs.Keys)
			{
				Assert(key + " must be null or false", arguments[key] == null || (arguments[key] is bool && !(bool)arguments[key]) || key == ApplicationArguments.OptionServerDirectoryPath);
			}
		}

		public void TestOptionForceCW1HomeScreenIsDisabledByDefault()
		{
			ApplicationArguments arguments = new ApplicationArguments(System.Array.Empty<string>());

			var isDisabledByDefault = arguments[ApplicationArguments.OptionForceCW1HomeScreen];

			Assert($"Expected {ApplicationArguments.OptionForceCW1HomeScreen} to be false by default",
				isDisabledByDefault is bool && !(bool)isDisabledByDefault);
		}

		public void TestOptionForceCW1HomeScreenIsEnabledWhenPassed()
		{
			string[] args = { ApplicationArguments.OptionForceCW1HomeScreen };
			ApplicationArguments arguments = new ApplicationArguments(args);

			var isEnabled = arguments[ApplicationArguments.OptionForceCW1HomeScreen];

			Assert($"Expected {ApplicationArguments.OptionForceCW1HomeScreen} to be true when passed",
				isEnabled is bool && (bool)isEnabled);
		}
	}
}
