using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Coalesce))]
	sealed class CoalesceTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<Coalesce(\"\", \"This is normal\")>");
			AssertIsResponsibleForReplacing("< Coalesce ( \"This has \" ,\"weird spacing\" ) >");
			AssertIsResponsibleForReplacing("<Coalesce(\"only one value\")>");
			AssertIsResponsibleForReplacing("<Coalesce(unquoted value 1 , unquoted value 2)>");
			AssertIsResponsibleForReplacing("<Coalesce(\"<Add(1, 2, random other stuff, \"1,000,000,005\")>\", \"some other value\")>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("abc", "<Coalesce(\"abc\", \"def\")>");
			AssertIsReplacedWith("def", "<Coalesce(\"\", \"def\")>");
		}

		#region ReplacementWithEvaluate

		public void TestReplacement_EmptyField()
		{
			var macro = "<Coalesce(\"<JS_AdditionalTerms>\", \"Replacement text\")>";

			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_AdditionalTerms = string.Empty;

			var (result, errors) = Evaluate(macro, (BusinessObject)shipment);
			AssertEquals("Replacement text", result);
			Assert("Replacement of empty field should not cause errors", !errors.Any());

			shipment.JS_AdditionalTerms = "Non-empty string";

			(result, _) = Evaluate(macro, (BusinessObject)shipment);
			AssertEquals("Non-empty string", result);
		}

		public void TestReplacement_MissingField()
		{
			var macro = "<Coalesce(\"<ThisDoesntExist>\", \"Replacement text\")>";

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();

			var (result, errors) = Evaluate(macro, shipment);
			AssertEquals("Replacement text", result);

			AssertEquals("Replacement should keep errors as information from field path evaluation", 1, errors.Count());
			AssertEquals("Field <ThisDoesntExist> not found on DataSource Type [ForwardingShipment].", errors.First().Message);
			AssertEquals(CargoWise.ComponentModel.NotificationType.Information, errors.First().Type);
		}

		public void TestReplacement_MissingFieldWithPrefix()
		{
			var macro = "<Coalesce(\"Some prefix<ThisDoesntExist>\", \"Replacement text\")>";

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();

			var (result, errors) = Evaluate(macro, shipment);
			AssertEquals("Replacement text", result);

			AssertEquals("Replacement should keep errors as information from field path evaluation", 1, errors.Count());
			AssertEquals("Field <ThisDoesntExist> not found on DataSource Type [ForwardingShipment].", errors.First().Message);
			AssertEquals(CargoWise.ComponentModel.NotificationType.Information, errors.First().Type);
		}

		public void TestReplacement_MissingFieldInReplacement()
		{
			var macro = "<Coalesce(\"\", \"<ThisDoesntExist>\")>";

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();

			var (result, errors) = Evaluate(macro, shipment);
			AssertEquals("", result);

			AssertEquals("Replacement with invalid field should cause errors", 1, errors.Count());
			AssertEquals("Field <ThisDoesntExist> not found on DataSource Type [ForwardingShipment].", errors.First().Message);
			AssertEquals(CargoWise.ComponentModel.NotificationType.Warning, errors.First().Type);
		}

		public void TestReplacement_NestedMacros()
		{
			var fieldPath = "<Add(1, 2, random other stuff, \"1,000,000,005\")>";
			var defaultValue = "Replacement text";
			var macro = $"<Coalesce(\"{fieldPath}\", \"{defaultValue}\")>";

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();

			var (result, errors) = Evaluate(macro, shipment);
			AssertEquals(defaultValue, result);

			AssertEquals("Replacement should keep errors as information from field path evaluation", 1, errors.Count());
			AssertEquals($@"Error in Add Macro: Invalid parameters provided: {fieldPath}, expected format: <Add(""value1"",""value2"",...)>. Input macro: [<Coalesce(""{fieldPath}"", ""{defaultValue}"")>]", errors.First().Message);
			AssertEquals(CargoWise.ComponentModel.NotificationType.Information, errors.First().Type);
		}

		(IZType result, IEnumerable<IReportError> errors) Evaluate(string macro, BusinessObject bizo)
		{
			return new WorkflowMacroEvaluator(null, new WorkflowMacroValidation()).EvaluateMacros(new[] { bizo }, macro);
		}

		public void TestInvalidParameters()
		{
			AssertInvalidParametersError("<Coalesce(\"Only 1 parameter\")>", "\"Only 1 parameter\"");
			AssertInvalidParametersError("<Coalesce(Missing quote\")>", "Missing quote\"");
			AssertInvalidParametersError("<Coalesce(Missing quotes)>", "Missing quotes");
			AssertInvalidParametersError("<Coalesce(\"ABC\", Missing quotes)>", "\"ABC\", Missing quotes");
		}

		void AssertInvalidParametersError(string macro, string parameters)
		{
			new MacroTranslator(Report).GetValue(macro, Passes.FirstPass);
			var expectedErrorMessage = string.Format(CultureInfo.InvariantCulture, @"Severity: [Warning (without error report)] Message: [Error in Coalesce Macro: Invalid parameters provided: {0}, expected format: {2}. Input macro: [{1}]]", parameters, macro, @"<Coalesce(""{fieldPath}"", ""{defaultValue}"")>");
			AssertEquals("report.ErrorManager.ToString()", expectedErrorMessage, Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		#endregion

		#region ReplacementWithLogWalker

		public void TestReplacement_InvalidFieldWithEmptyString()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;
			dummy.Z0_NVarCharMax = "Non-empty";

			AssertSetWithCoalesce(
				bizo: dummy,
				propertyInfo: dummy.Z0_NVarCharMaxInfo,
				fieldPath: "<ThisDoesntExist>",
				defaultValue: "",
				expectedValue: ZString.Empty,
				expectedWarnings: @"Field <ThisDoesntExist> not found on any of the DataSource Types: [DummyWithWorkflow], [DummyProcessTask], [ProcessTaskNotification], [StmALog], [TriggeringEventPropertyProvider`1].
FLD setting [Z0_NVarCharMax] with [<Coalesce(""<ThisDoesntExist>"", """")>] succeeded on 1 properties. Unchanged on 0. Failed on 0
Success logs:
Target Object: 'Dummy Business Object Default', Source: Non-empty, Target: ");
		}

		void AssertSetWithCoalesce<BizoT>(BizoT bizo, ZPropertyInfo propertyInfo, string fieldPath, string defaultValue, IZType expectedValue, string expectedWarnings)
			where BizoT : BusinessObject, IWorkflowProvider
		{
			var task = bizo.WorkflowItems.Triggers.AddNew();
			((IBaseTrigger)task).TriggerEventCode = Events.CustomisableEvent00.Code;

			var action = task.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = $"<{propertyInfo.Name}>";
			action.PQ_FieldValue = $"<Coalesce(\"{fieldPath}\", \"{defaultValue}\")>";

			if (bizo.GetLogs().DatabaseCount == 0)
			{
				bizo.GetLogs().AddNew(Events.CustomisableEvent00);
			}

			Factory.Save();

			var logWalkerOutput = MasterFilesTestHelper.RunLogWalker();

			AssertMultilineASCIIEquals(expectedWarnings, MasterFilesTestHelper.GetFldNotificationsFromLogWalker(logWalkerOutput));

			action.Parent.GetJob().Reload();
			AssertEquals(expectedValue, propertyInfo.Value);
		}

		#endregion

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals("ValueProviderToTest.ComponentType", VisualiserComponentTypes.TextEdit, ValueProviderToTest.ComponentType);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Coalesce();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("JS_ActualChargeable", ""));
		}
	}
}
