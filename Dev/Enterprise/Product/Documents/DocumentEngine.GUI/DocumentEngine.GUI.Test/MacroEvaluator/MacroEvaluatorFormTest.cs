using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.DocumentEngine.MacroEvaluator;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[TestedType(typeof(MacroEvaluatorForm))]
	sealed class MacroEvaluatorFormTest : ZFormBasherTest
	{
		[DeveloperOnlyTest]
		public void TestCopyToClipboard()
		{
			using (var testForm = new MacroEvaluatorForm(MacroManagerForTest))
			{
				MacroManagerForTest.Macro = "Testing123";
				testForm.CopyToClipboard(this, new EventArgs());

				AssertEquals("Macro is copied to clipboard", "Testing123", SafeClipboard.GetText());
			}
		}

		[RequiresSTA]
		public void TestDataContextZDropEditAndDataContextLabelVisibility()
		{
			using (var testForm = new MacroEvaluatorForm(MacroManagerForTest))
			{
				Assert(MacroManagerForTest.HasDataContext);
				testForm.Show();

				var dropDown = testForm.Controls.Find("DataContextZDropEdit", true).First();
				Assert(dropDown.Visible);

				var label = testForm.Controls.Find("DataContextLabel", true).First();
				Assert(!label.Visible);
			}

			using (var testForm = new MacroEvaluatorForm(FilterMacroManagerForTest))
			{
				Assert(!FilterMacroManagerForTest.HasDataContext);
				testForm.Show();

				var dropDown = testForm.Controls.Find("DataContextZDropEdit", true).First();
				Assert(!dropDown.Visible);

				var label = testForm.Controls.Find("DataContextLabel", true).First();
				Assert(label.Visible);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new MacroEvaluatorForm(MacroManagerForTest);
		}

		MacroEvaluatorManager MacroManagerForTest
		{
			get
			{
				if (macroManagerForTest == null)
				{
					var dummy = Factory.New<DummyBODocSupportable>();
					var documentCommand = Factory.New<DocumentCommand>();
					documentCommand.Parent = dummy;
					macroManagerForTest = new MacroEvaluatorManager(Factory, dummy);
				}

				return macroManagerForTest;
			}
		}
		MacroEvaluatorManager macroManagerForTest;

		FilterEvaluatorManager FilterMacroManagerForTest
		{
			get
			{
				if (filterManagerForTest == null)
				{
					var dummy = Factory.New<DummyBODocSupportable>();
					var documentCommand = Factory.New<DocumentCommand>();
					documentCommand.Parent = dummy;
					filterManagerForTest = new FilterEvaluatorManager(Factory, dummy);
				}

				return filterManagerForTest;
			}
		}
		FilterEvaluatorManager filterManagerForTest;

		#endregion
	}
}
