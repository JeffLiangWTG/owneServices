using Enterprise.DocumentVisualizer.Presentation;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestsSubclassesOf(typeof(ICommandProvider), ExcludePrivate = true)]
	abstract class CommandProviderTest : TestCaseWithUXmlSupport
	{
		#region Tests

		[ExpectNoExceptions]
		public void TestCommands()
		{
			var module = CreateNewModule();

			foreach (var command in module.Commands)
			{
				AssertNotNull("command is not null", command);
				Assert("command has an id", !string.IsNullOrWhiteSpace(command.Id));
				AssertNoExceptionThrown("command does not blow up when calling IsEnabled", () => { var touch = command.IsEnabled; });
				AssertNoExceptionThrown("command does not blow up when calling IsVisible", () => { var touch = command.IsVisible; });
				AssertNoExceptionThrown("command does not blow up when calling Invoke()", () => { command.Invoke(); });
			}
		}

		#endregion

		#region Implementation

		protected abstract ICommandProvider CreateNewModule();

		#endregion
	}
}
