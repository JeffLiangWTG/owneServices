using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Presentation;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class CommandTest : TestCase
	{
		public void TestInvokeCommand_NoParameters()
		{
			var commandHasBeenInvoked = false;

			var command = new Command("id", "caption")
			{
				Invoker = _ => { commandHasBeenInvoked = true; },
				IsEnabled = () => true
			};

			using (var scope = new MacroScope(command))
			{
				var expr = "Invoke()".CreateExpression();

				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errros", "", expr.ToFormatString());
				Assert("result", (bool)result);
			}

			Assert("Command has been invoked", commandHasBeenInvoked);
		}

		public void TestInvokeCommand_WithMap()
		{
			var commandHasBeenInvoked = false;

			var command = new Command("id", "caption")
			{
				Invoker = _ => { commandHasBeenInvoked = true; },
				IsEnabled = () => true
			};

			using (var scope = new MacroScope(command))
			{
				var expr = "Invoke({Name: \"Alice\"})".CreateExpression();

				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errros", "", expr.ToFormatString());
				Assert("result", (bool)result);
			}

			Assert("Command has been invoked", commandHasBeenInvoked);
		}
	}
}
