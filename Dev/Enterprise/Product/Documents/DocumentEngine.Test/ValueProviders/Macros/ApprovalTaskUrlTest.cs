using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ApprovalTaskUrl))]
	sealed class ApprovalTaskUrlTest : ValueProviderTest
	{
		public void TestReplacementWithInvalidParameters()
		{
			var portalUrl = "https://myserver/Portals";
			var originalValue = GlowRegistry.Instance.GlowPortalsUri.Value;
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portalUrl);
			try
			{
				AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
				ValueProviderToTest.GetReplacement("<ApprovalTaskUrl( , foo)>", Report);
				AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
				AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
				AssertMultilineEquals(
					"report.ErrorManager.ToString()",
					@"Error in ApprovalTaskUrl Macro: TableCode not found
Error in ApprovalTaskUrl Macro: TaskPK not found",
					Report.ErrorManager.ToString("{1}", false),
					'\n');
				Report.ErrorManager.ClearErrors();
			}
			finally
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestReplacementWithInvalidEnvironment()
		{
			var originalValue = GlowRegistry.Instance.GlowPortalsUri.Value;
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			try
			{
				AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
				ValueProviderToTest.GetReplacement("<ApprovalTaskUrl(JS, 6E449683-C509-11CF-AAFA-00AA00B6015C)>", Report);
				AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
				AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
				AssertEquals(
					"report.ErrorManager.ToString()",
					"Error in ApprovalTaskUrl Macro: Please specify GLOW Portal URL in the Registry",
					Report.ErrorManager.ToString("{1}", false));
				Report.ErrorManager.ClearErrors();
			}
			finally
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("ApprovalTaskUrl", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<ApprovalTaskUrl meh>", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<ApprovalTaskUrl(X X,6E449683-C509-11CF-AAFA-00AA00B6015C)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<ApprovalTaskUrl(X9X, 6E449683-C509-11CF-AAFA-00AA00B6015C)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var portalUrl = "https://myserver/Portals";
			var originalValue = GlowRegistry.Instance.GlowPortalsUri.Value;
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portalUrl);
			try
			{
				var taskPK = Guid.NewGuid();
				AssertEquals(
					FormattableString.Invariant($"https://myserver/Portals/goto/approval-ZZ?taskPK={taskPK}"),
					ValueProviderToTest.GetReplacement(FormattableString.Invariant($"<ApprovalTaskUrl(ZZ, {taskPK})>"), Report));
			}
			finally
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestReplacementWithCorrectTrimmedUrl()
		{
			var portalUrl = "https://myserver/Portals/";
			var originalValue = GlowRegistry.Instance.GlowPortalsUri.Value;
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portalUrl);
			try
			{
				var taskPK = Guid.NewGuid();
				AssertEquals(
					FormattableString.Invariant($"https://myserver/Portals/goto/approval-ZZ?taskPK={taskPK}"),
					ValueProviderToTest.GetReplacement(FormattableString.Invariant($"<ApprovalTaskUrl(ZZ, {taskPK})>"), Report));
			}
			finally
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		protected override ValueProvider GetNewValueProvider() => new ApprovalTaskUrl();

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Task.P9_PK", "48f224b7-79c9-4d82-8524-7b7d0b66e281"));
		}

		public override void TestDocumentation()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myserver/Portals"))
			{
				base.TestDocumentation();
			}
		}
	}
}
