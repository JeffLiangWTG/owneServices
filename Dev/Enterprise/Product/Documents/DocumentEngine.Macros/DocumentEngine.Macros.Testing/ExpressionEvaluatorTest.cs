using System;
using System.IO;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Macros.Testing
{
	sealed class ExpressionEvaluatorTest : TransactionedTestCase
	{
		#region TestExpressions

		public void TestExpressions()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertExpressions();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertExpressions();
			}
		}

		void AssertExpressions()
		{
			CombineAssertions(() =>
			{
				AssertExpression("\"a\" && 1", 1);
				AssertExpression("\"a\" || 1", "a");
				AssertExpression("1 && \"a\"", "a");
				AssertExpression("1 || \"a\"", 1);

				AssertExpression("true && 1", 1);
				AssertExpression("true || 1", true);
				AssertExpression("1 && true", true);
				AssertExpression("1 || true", 1);

				AssertExpression("\"a\" && \"b\"", "b");
				AssertExpression("\"a\" || \"b\"", "a");

				AssertExpression("1 && 2", 2);
				AssertExpression("1 || 2", 1);

				AssertExpression("true && true", true);
				AssertExpression("true && false", false);
				AssertExpression("false && true", false);
				AssertExpression("false && false", false);

				AssertExpression("true || true", true);
				AssertExpression("true || false", true);
				AssertExpression("false || true", true);
				AssertExpression("false || false", false);
			});
		}

		#endregion

		#region TestLogicalExpressions

		public void TestLogicalExpressions()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertLogicalExpressions();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertLogicalExpressions();
			}
		}

		void AssertLogicalExpressions()
		{
			AssertLogicalExpression("\"\" == \"\"", true);
			AssertLogicalExpression("'' == \"\"", true);
			AssertLogicalExpression("'aaa' == 'bbb'", false);
			AssertLogicalExpression("'AAA' == 'AAA'", true);
			AssertLogicalExpression("\"ABC\" == \"DEF\"", false);
			AssertLogicalExpression("\"AAA\" == \"AAA\"", true);
			AssertLogicalExpression("true == false", false);
			AssertLogicalExpression("true || false == true", true);
			AssertLogicalExpression("\"ABC\".Contains(\"B\")", true);
			AssertLogicalExpression("\"ABC\".substr(1, 1) == \"B\"", true);
			AssertLogicalExpression("\"ABC\".Substring(1, 1) == \"B\"", true);
			AssertLogicalExpression("\"ABC\".substr(0,2) == \"AB\"", true);
			AssertLogicalExpression("\"ABC\".Substring(0,2) == \"AB\"", true);
			AssertLogicalExpression("\"ABC\".indexOf('A') == 0", true);
			AssertLogicalExpression("\"ABC\".IndexOf('A') == 0", true);
			AssertLogicalExpression("\"US\" == \"US\"&&\"AAA, BBB, INB, CCC\".Contains(\", INB, \")", true);
			AssertLogicalExpression("\"$SEARU,$AIRUS\".indexOf(\"$AIRUS\") != -1", true);
			AssertLogicalExpression("\"$SEARU,$AIRUS\".IndexOf(\"$AIRUS\") != -1", true);
			AssertLogicalExpression("\"Indent Order Sign Off\".toLowerCase().indexOf(\"vendor\") == -1", true);
			AssertLogicalExpression("\"Indent Order Sign Off\".ToLower().IndexOf(\"vendor\") == -1", true);
			AssertLogicalExpression("\"ER\" == \"US\" && \"\".Contains(\",INB,\")", false);
			AssertLogicalExpression(" (\"Y\" == \"N\" && \"Local Cartage Advice\".StartsWith(\"Shipment\")) || \"Local Cartage Advice\".StartsWith(\"CFSShipment\")", false);
			AssertLogicalExpression("(\"Y\" == \"N\" && \"Local Cartage Advice\".StartsWith(\"Shipment\")) || \"Local Cartage Advice\".StartsWith(\"CFSShipment\")", false);
			AssertLogicalExpression("\"0\"==\"\" || \"0\"==\"0\"", true);
			AssertLogicalExpression("!\"Order Pre-Alert\".StartsWith(\"Bla\")", true);
			AssertLogicalExpression("\"NUMBER\"!=\"PERCENTAGE\" || (\"\"==\"Y\" && \"0\"==\"0\" && \"0\"==\"0\"  && \"0\"==\"0\"  && \"0\"==\"0\"  && \"0\"==\"0\"  && \"0\"==\"0\"  && \"0\"==\"0\" )", true);
		}

		#endregion

		#region TestExpressionsWithUnescapedQuotes

		public void TestExpressionsWithUnescapedQuotes()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertExpressionsWithUnescapedQuotes();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertExpressionsWithUnescapedQuotes();
			}
		}

		void AssertExpressionsWithUnescapedQuotes()
		{
			AssertLogicalExpression("\"a\"a\" == \"\"", false);
			AssertLogicalExpression("\"\" == \"a\"a\"", false);
			AssertLogicalExpression("\"a\"a\" == \"a\"a\"", true);
		}

		public void TestExpressionsWithUnescapedQuotes_InfiniteLoop()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var stringCausedInfiniteLoop = "\"AGT\"==\"DRT\"&&\"(DAP\". substr(0,1)==\"E\"||\"DAP\". substr(0,1)==\"F\")";
				AssertExceptionThrown<InvalidOperationException>("Exception should be thrown", () => stringCausedInfiniteLoop.EvaluateDocEngineExpression(false));
			}
		}

		public void TestEvaluateExpressionDoesNotLogToConsole()
		{
			var consolError = Console.Error;
			try
			{
				using (var writer = new StringWriter())
				{
					Console.SetError(writer);
					var badExpression = "$\"CRAP\" == \\n \"CRAP\"";
					badExpression.EvaluateDocEngineExpressionNoJS();
					AssertEquals("Expecting no logging to console", "", writer.ToString());
				}
			}
			finally
			{
				Console.SetError(consolError);
			}
		}

		#endregion

		#region TestSupportedMethods

		public void TestSupportedMethods()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSupportedMethods();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSupportedMethods();
			}
		}

		void AssertSupportedMethods()
		{
			AssertExpression("\"ABC\".substr(1)", "A");
			AssertExpression("\"ABC\".Substring(1)", "A");

			AssertExpression("\"ABC\".substr(1, 1)", "B");
			AssertExpression("\"ABC\".Substring(1, 1)", "B");

			AssertExpression("\"ABC\".indexOf('C')", 2);
			AssertExpression("\"ABC\".indexOf(\"C\")", 2);

			AssertExpression("\"ABC\".IndexOf('C')", 2);
			AssertExpression("\"ABC\".IndexOf(\"C\")", 2);

			AssertExpression("\"ABC\".Contains(\"B\")", true);

			AssertExpression("\"ABC\".ToLower()", "abc");

			AssertExpression("\"ABC\".startsWith(\"A\")", true);
			AssertExpression("\"ABC\".StartsWith(\"A\")", true);

			AssertExpression("\"ABC\".endsWith(\"C\")", true);
			AssertExpression("\"ABC\".EndsWith(\"C\")", true);
		}

		#endregion

		#region TestThrownOnEmptyDocEngineMacro

		public void TestThrownOnEmptyDocEngineMacro()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertThrownOnEmptyDocEngineMacro();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertThrownOnEmptyDocEngineMacro();
			}
		}

		void AssertThrownOnEmptyDocEngineMacro()
		{
			AssertExceptionThrown<InvalidOperationException>("Result of \"\" is not a True/False expression", () => string.Empty.EvaluateDocEngineExpression(false));
			AssertExceptionThrown<InvalidOperationException>("Result of  is not a True/False expression", () => ((string)null).EvaluateDocEngineExpression(false));
			AssertExceptionThrown<InvalidOperationException>("Result of  is not a True/False expression", () => new string(' ', 3).EvaluateDocEngineExpression(false));
		}

		#endregion

		#region TestThrownExceptionShowsOriginalMacroInText

		public void TestThrownExceptionShowsOriginalMacroInText()
		{
			AssertExceptionThrown<InvalidOperationException>("Result of \"1\" is not a True/False expression", () => "1".EvaluateDocEngineExpression(false));
		}

		#endregion

		#region TestDocEngineArithmeticExpression

		public void TestDocEngineArithmeticExpression()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertDocEngineArithmeticExpression();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertDocEngineArithmeticExpression();
			}
		}

		void AssertDocEngineArithmeticExpression()
		{
			AssertLogicalExpression("\"0\" * \"0\" == \"0\"", true);
			AssertLogicalExpression("0 * \'0\' == \"0\"", true);

			AssertLogicalExpression("\"4\" * \"3\" == \"12\"", true);
			AssertLogicalExpression("\"4\" * \"3\" > \"10\"", true);

			AssertLogicalExpression("\"49\" > \"300\"", true);
			AssertLogicalExpression("\"49\" > 300", false);

			AssertLogicalExpression("\"100\" + \"2\" == \"1002\"", true);
			AssertLogicalExpression("\"100\" - \"2\" == 98", true);
		}

		#endregion

		#region TestQuotedNumberComparison

		public void TestQuotedNumberComparison()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertQuotedNumberComparison();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertQuotedNumberComparison();
			}
		}

		public void AssertQuotedNumberComparison()
		{
			CombineAssertions(() =>
			{
				AssertExpression("\"12\" > \"3\"", false);
				AssertExpression("\"12\" > 3", true);
				AssertExpression("12 > 3", true);
				AssertExpression("12 > \"3\"", true);

				AssertExpression("\"4\" < \"30\"", false);
				AssertExpression("\"4\" < 30", true);
				AssertExpression("4 < 30", true);
				AssertExpression("4 < \"30\"", true);

				AssertExpression("\"4\" == \"4\"", true);
				AssertExpression("\"4\" == \"5\"", false);

				AssertExpression("\"4\" != \"4\"", false);
				AssertExpression("\"4\" != \"5\"", true);

				AssertExpression("\"4.000\" == \"4\"", false);
				AssertExpression("4.000 == \"4\"", true);
				AssertExpression("4.000 == 4", true);
			});
		}

		#endregion

		#region TestQuotedNumber

		public void TestQuotedNumber()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertQuotedNumber();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertQuotedNumber();
			}
		}

		void AssertQuotedNumber()
		{
			CombineAssertions(() =>
			{
				AssertLogicalExpression("\"8000\".Contains(\"8000\")", true);
				AssertLogicalExpression("\"8000\".Contains(\"9000\")", false);

				AssertLogicalExpression("\"1\" == \"1\"", true);
				AssertLogicalExpression("\"1\" != \"1\"", false);
				AssertLogicalExpression("\"1\" == \"2\"", false);
				AssertLogicalExpression("\"1\" != \"2\"", true);

				AssertLogicalExpression("\"1\" * \"9\" == \"9\"   ", true);
				AssertLogicalExpression("\"1\" * \"9\" == \"8\"   ", false);

				AssertLogicalExpression("\"12\" / \"3\" == \"4\"   ", true);
				AssertLogicalExpression("\"12\" / \"3\" == \"5\"   ", false);

				AssertLogicalExpression("\"12\" - \"3\" == \"9\"   ", true);
				AssertLogicalExpression("\"12\" - \"3\" == \"8\"   ", false);

				AssertLogicalExpression("\"49\" >= 30", true);
				AssertLogicalExpression("\"-49\" >= 300", false);
				AssertLogicalExpression("\"49.10\" >= 49", true);
				AssertLogicalExpression("\"-49.10\" < -49", true);

				AssertLogicalExpression("\"49.\" == 49", true);

				AssertExpression("\"12\"", "12");
				AssertExpression("\"-12\"", "-12");
				AssertExpression("\"12.\"", "12.");
				AssertExpression("\"-12.\"", "-12.");
				AssertExpression("\"12.69\"", "12.69");
				AssertExpression("\"-12.69\"", "-12.69");

				AssertExpression("\"-12E\"", "-12E");
			});
		}

		#endregion

		#region TestInvalidDocEngineMixedOldAndNewExpressions

		public void TestInvalidDocEngineMixedOldAndNewExpressions()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertInvalidDocEngineMixedOldAndNewExpressions();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertInvalidDocEngineMixedOldAndNewExpressions();
			}
		}

		void AssertInvalidDocEngineMixedOldAndNewExpressions()
		{
			CombineAssertions(() =>
			{
				AssertExpression("\"MSGBKRCTY=IMPUS\"&&\"SEA\"==\"AIR\"", false);
				AssertExpression("\"SEA\"==\"AIR\"&&\"MSGBKRCTY = IMPUS\"", "MSGBKRCTY = IMPUS");
				AssertExpression("\"MSGBKRCTY=IMPUS\"||\"SEA\"==\"AIR\"", "MSGBKRCTY=IMPUS");
				AssertExpression("\"SEA\"==\"AIR\"||\"MSGBKRCTY = IMPUS\"", false);
			});
		}

		#endregion

		#region Implementation

		void AssertExpression(string expression, object expected)
		{
			var jsEngineOnOff = RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value
				? "ON"
				: "OFF";

			var result = expression.Evaluate();

			Assert($"Expression: '{expression}' (JS engine: {jsEngineOnOff}) was executed correctly", result.IsRight);
			AssertEquals($"Expression: '{expression}' (JS engine: {jsEngineOnOff}) expected result",
				expected,
				result.Right);
		}

		void AssertLogicalExpression(string expression, object expected)
		{
			var jsEngineOnOff = RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value
				? "ON"
				: "OFF";

			AssertEquals($"Expression: '{expression}', JS engine: {jsEngineOnOff}",
				expected,
				expression.EvaluateDocEngineExpression(false));
		}

		#endregion
	}
}
