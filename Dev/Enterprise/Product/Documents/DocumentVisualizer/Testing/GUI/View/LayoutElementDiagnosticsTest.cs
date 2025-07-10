using System.Drawing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Presentation;
using Moq;
using NUnit.Framework;
using Font = Enterprise.DocumentVisualizer.Core.Font;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class LayoutElementDiagnosticsTest : TestCase
	{
		#region TestRunTextDrawTest

		public void TestRunTextDrawTest_NoParams() => AssertRunTextDrawTest("@diagnostics.RunTextDrawTest()");
		public void TestRunTextDrawTest_Text() => AssertRunTextDrawTest("@diagnostics.RunTextDrawTest(\"other text\")");
		public void TestRunTextDrawTest_TextAndWrap() => AssertRunTextDrawTest("@diagnostics.RunTextDrawTest(\"other text\", false)");

		void AssertRunTextDrawTest(string macro)
		{
			var font = new Font("Arial", 8f, FontStyle.Regular, Color.Black, 0);
			var cell = new DummyCell
			{
				Format = new Format
				{
					Font = font
				}
			};

			var internals = new Mock<ILayoutElementInternals>();
			internals.SetupGet(i => i.Text).Returns("some text");
			internals.SetupGet(i => i.Cell).Returns(cell);
			internals.SetupGet(i => i.Font).Returns(font);
			internals.SetupGet(i => i.PaintArea).Returns(new RectangleF(0f, 0f, 110f, 11f));
			internals.SetupGet(i => i.LayoutArea).Returns(new RectangleF(0f, 0f, 120f, 21f));

			var diagnostics = new LayoutElementDiagnostics(internals.Object, _ => { });

			using (var scope = new MacroScope())
			{
				scope.SetVariable("diagnostics", diagnostics);

				var expr = macro.CreateExpression();
				var res = expr.Evaluate(scope);

				AssertNotNull("test has been created", res);
			}
		}

		#endregion

		#region TestEnableVisualCues

		public void TestEnableVisualCues()
		{
			bool? hasBeenEnabled = null;
			var internals = new Mock<ILayoutElementInternals>();

			var diagnostics = new LayoutElementDiagnostics(internals.Object, enable => hasBeenEnabled = enable);
			const string macro = "@diagnostics.EnableVisualCues()";

			using (var scope = new MacroScope())
			{
				scope.SetVariable("diagnostics", diagnostics);

				var expr = macro.CreateExpression();
				var res = expr.Evaluate(scope);

				Assert("visual cues have been set via macro", hasBeenEnabled.HasValue);
				Assert("visual cues value", hasBeenEnabled.Value);
			}
		}

		#endregion

		#region TestDisableVisualCues

		public void TestDisableVisualCues()
		{
			bool? hasBeenEnabled = null;
			var internals = new Mock<ILayoutElementInternals>();

			var diagnostics = new LayoutElementDiagnostics(internals.Object, enable => hasBeenEnabled = enable);
			const string macro = "@diagnostics.DisableVisualCues()";

			using (var scope = new MacroScope())
			{
				scope.SetVariable("diagnostics", diagnostics);

				var expr = macro.CreateExpression();
				var res = expr.Evaluate(scope);

				Assert("visual cues have been set via macro", hasBeenEnabled.HasValue);
				Assert("visual cues value", !hasBeenEnabled.Value);
			}
		}

		#endregion
	}
}
