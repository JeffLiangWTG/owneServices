using System;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ExpressionEvaluatorTest : TransactionedTestCase
	{
		#region TestSimpleExpressions

		public void TestSimpleExpressions()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSimpleExpressions();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSimpleExpressions();
			}
		}

		void AssertSimpleExpressions()
		{
			AssertEquals(true, ExpressionEvaluator.Evaluate("1 == 1", false));
			AssertEquals(true, ExpressionEvaluator.Evaluate("2 == 1 + 1", false));
			AssertEquals(false, ExpressionEvaluator.Evaluate("3 < 2", false));
			AssertEquals(false, ExpressionEvaluator.Evaluate("1 == 3", false));
			AssertEquals(true, ExpressionEvaluator.Evaluate("1 != 2 && 3 < 4", false));
			AssertEquals(false, ExpressionEvaluator.Evaluate("1 < 1", false));
		}

		#endregion

		#region TestStringExpressions

		public void TestStringExpressions()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertStringExpressions();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertStringExpressions();
			}
		}

		void AssertStringExpressions()
		{
			AssertEquals(true, ExpressionEvaluator.Evaluate("\"\\n\" != \"\"", false));
			AssertEquals(true, ExpressionEvaluator.Evaluate("\"\\r\" != \"\"", false));
			AssertEquals(true, ExpressionEvaluator.Evaluate("\"\nDesc \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nB \n\nB \n\nMarks \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nA \n\nB \n\nB \n\" != \"\"", false));
		}

		#endregion

		#region TestStringExpressionsWithReplacements

		public void TestStringExpressionsWithReplacements()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertStringExpressionsWithReplacements();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertStringExpressionsWithReplacements();
			}
		}

		void AssertStringExpressionsWithReplacements()
		{
			AssertEquals(true, ExpressionEvaluator.Evaluate("1 &gt; 0", false));
			AssertEquals(true, ExpressionEvaluator.Evaluate("1 &lt; 2", false));
			AssertEquals(false, ExpressionEvaluator.Evaluate("1 &lt; 0", false));
			AssertEquals(false, ExpressionEvaluator.Evaluate("1 &gt; 2", false));
			AssertEquals(true, ExpressionEvaluator.Evaluate("\"some\\\"thing\" == \"some\\\"thing\"", false));
			AssertEquals(false, ExpressionEvaluator.Evaluate("\"some\\\"thing\" == \"something\"", false));
		}

		#endregion

		#region TestStringExpressionsWithRogueDoubleQuotes

		public void TestStringExpressionsWithRogueDoubleQuotes()
		{
			try
			{
				using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertStringExpressionsWithRogueDoubleQuotes();
				}

				using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertStringExpressionsWithRogueDoubleQuotes();
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		void AssertStringExpressionsWithRogueDoubleQuotes()
		{
			AssertEquals(true, ExpressionEvaluator.Evaluate("\"\"1\"\" == \"\"1\"\"   ", false));              // ""1"" == ""1""
			AssertEquals(false, ExpressionEvaluator.Evaluate("\"1\"\" == \"\"1\"\"   ", false));               // "1"" == ""1""
			AssertEquals(false, ExpressionEvaluator.Evaluate("\"\"1\"\" != \"\"1\"\"", false));                // ""1"" != ""1""
			AssertEquals(true, ExpressionEvaluator.Evaluate("\"1\"\" != \"\"1\"\"", false));                   // "1"" != ""1""
			AssertEquals(true, ExpressionEvaluator.Evaluate("\"1\"\" == \"1\" || \"2\" == \"2\"", false));     // "1"" == "1" || "2" == "2"
			AssertEquals(false, ExpressionEvaluator.Evaluate("\"1\"\" == \"1\" || \"2\" == \"\"2\"", false));  // "1"" == "1" || "2" == ""2"
			AssertEquals(true, ExpressionEvaluator.Evaluate("  \"\"1\"\" == \"\"1\"\" && \"2\"\" == \"2\"\"", false)); // ""1"" == ""1"" && "2"" == "2""
			AssertEquals(false, ExpressionEvaluator.Evaluate("  \"\"1\"\" == \"\"1\"\" && \"2\" == \"2\"\"", false));  // ""1"" == ""1"" && "2" == "2""
			AssertEquals(true, ExpressionEvaluator.Evaluate("\"\"1\"\" &gt; \"\"0\"\"", false));                       // ""1"" > ""0""
			AssertEquals(false, ExpressionEvaluator.Evaluate("(\"\"1\"\" &lt; \"\"0\"\")", false));                    // (""1"" < ""0"")
			AssertEquals(true, ExpressionEvaluator.Evaluate("(\"1\"\" == \"1\" || \"2\" == \"2\") && \"1\" == \"1\"", false));     // ("1"" == "1" || "2" == "2") && "1" == "1"
			AssertEquals(false, ExpressionEvaluator.Evaluate("(\"1\"\" == \"1\" || \"2\" == \"\"2\") && \"1\" == \"1\"", false));  // ("1"" == "1" || "2" == ""2") && "1" == "1"
		}

		#endregion

		#region TestStringExpressionsWithEscapedSlashes_ShouldEvaluateCorrectly

		public void TestStringExpressionsWithEscapedSlashes_ShouldEvaluateCorrectly()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertStringExpressionsWithEscapedSlashes_ShouldEvaluateCorrectly();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertStringExpressionsWithEscapedSlashes_ShouldEvaluateCorrectly();
			}
		}

		void AssertStringExpressionsWithEscapedSlashes_ShouldEvaluateCorrectly()
		{
			AssertEquals(true, ExpressionEvaluator.Evaluate(@"""\\something\\"" == ""\\something\\""", false));
			AssertEquals(true, ExpressionEvaluator.Evaluate(@"""\\something"" == ""\\something""", false));
			AssertEquals(false, ExpressionEvaluator.Evaluate(@"""\\something\\"" == ""\\something\\else""", false));
		}

		#endregion
	}
}
