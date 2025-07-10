using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Macros.Testing
{
	public abstract class TestCaseWithMacros : TestCase
	{
		#region Nested Members

		protected class MacroVariables : List<KeyValuePair<string, object>>
		{
			public void Add(string name, object value)
			{
				Add(new KeyValuePair<string, object>(name, value));
			}
		}

		protected class MacroRun
		{
			public object Data { get; set; }

			public MacroVariables Variables
			{
				get { return variables; }
			}

			readonly MacroVariables variables = new MacroVariables();

			public object ExpectedResult { get; set; }
			public string ExpectedNotifications { get; set; }
		}

		const int numberOfRuns = 2;

		#endregion

		protected virtual IEnumerable<IMacroLibrary> Libraries
		{
			get { yield return new StandardLibrary(); }
		}

		protected void AssertMacroRun(string macro, params MacroRun[] macroRuns)
		{
			var context = new MacroEvaluationContext();

			foreach (var library in Libraries)
			{
				context.Import(library);
			}

			IMacroExpression macroExpr = new MacroExpression(macro, context, null);

			if (macroExpr.Handler == null)
			{
				var errors = string.Join(", ", macroExpr.Errors.Select(err => err.Message));

				Fail(string.Concat("Macro could not be compiled. The following errors have ocurred: ", errors));
			}

			CombineAssertions(() =>
			{
				for (int i = 1; i <= numberOfRuns; i++)
				{
					foreach (var macroRun in macroRuns)
					{
						RunMacro(macro, macroRun, macroExpr);
					}
				}
			});
		}

		void RunMacro(string macro, MacroRun macroRun, IMacroExpression expression)
		{
			using (var scope = new MacroScope(macroRun.Data))
			{
				foreach (var variable in macroRun.Variables)
				{
					scope.SetVariable(variable.Key, variable.Value);
				}

				var result = expression.Evaluate(scope);

				var notifications = expression.Errors
					.Select(err => err.Message)
					.ToArray();

				if (!(macroRun.ExpectedResult is IMacroObject) && result is IMacroObject)
				{
					result = ((IMacroObject)result).Value;
				}

				if (macroRun.ExpectedResult is string && result is string)
				{
					AssertMultilineASCIIEquals(string.Format("{0} run result", macro),
						Convert.ToString(macroRun.ExpectedResult), Convert.ToString(result));
				}
				else if (macroRun.ExpectedResult is IEnumerable && result is IEnumerable)
				{
					AssertContainsExactElementsInAnyOrder(string.Format("{0} run result", macro),
						(IEnumerable)macroRun.ExpectedResult, (IEnumerable)result);
				}
				else
				{
					AssertEquals(string.Format("{0} run result", macro), macroRun.ExpectedResult, result);
				}

				if (!string.IsNullOrEmpty(macroRun.ExpectedNotifications) || notifications.Any())
				{
					AssertMultilineASCIIEquals(string.Format("{0} notifications", macro),
						macroRun.ExpectedNotifications, string.Join("\n\r", notifications));
				}
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			MacroExpressionTypeConverter.Instance.Clear();
		}
	}
}
