using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.Testing.DataObjectCreators;
using Enterprise.Workflow.Integration;
using static Enterprise.UniversalDataBuss.Management.UniversalValidationRulesEvaluator;

namespace Enterprise.UniversalDataBuss.Testing
{
	public class UniversalValidationRuleTest : TestCaseWithFactory
	{
		IWorkflowTestHelper TestHelper { get; set; }

		Shipment CreateShipmentDataObjectWithRules(DataContextType contextType, string key, params string[] ruleCodes)
		{
			var dataContext = DataContextCreator.Create(contextType, key);
			var shipmentDataObject = UniversalShipmentCreator.Create(dataContext);
			var ruleDataObjects = new List<ValidationRule>();
			foreach (var code in ruleCodes)
			{
				ruleDataObjects.Add(ValidationRuleCreator.Create(code));
			}

			UniversalShipmentCreator.AddValidationRules(shipmentDataObject, ruleDataObjects.ToArray());
			return shipmentDataObject;
		}

		class RuleWrapper
		{
			public RuleWrapper(IReadOnlyUniversalValidationRule rule, string rulesSetCode, string evaluatedMessageMacro)
			{
				Rule = rule;
				RulesSetCode = rulesSetCode;
				EvaluatedMessageMacro = evaluatedMessageMacro;
			}

			public IReadOnlyUniversalValidationRule Rule { get; }
			public string RulesSetCode { get; }
			public string EvaluatedMessageMacro { get; }
		}

		RuleWrapper AddRuleToRuleSet(BusinessObject ruleSetBizo, string ruleCode, string ruleMacro, string statusLevel, string messageMacro, string evaluatedMessageMacro = null, bool isActive = true)
		{
			var rule = TestHelper.AddUniversalValidationRule(ruleSetBizo, ruleMacro, statusLevel, messageMacro, isActive);
			return new RuleWrapper(rule, ruleCode, evaluatedMessageMacro);
		}

		void AssertValidationRules(string assertionFailedMessage, UniversalProcessingResult processingResult, params RuleWrapper[] expectedRules)
		{
			CombineAssertions(assertionFailedMessage, () =>
			{
				if (expectedRules.Length == 0)
				{
					AssertNull(processingResult.SessionTracker.ValidationRuleCollection);
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, processingResult.Message.EM_Status);
				}
				else
				{
					AssertNotNull("Expecting a ValidationRuleCollection", processingResult.SessionTracker.ValidationRuleCollection);
					var failedRules = processingResult.SessionTracker.ValidationRuleCollection.ToDictionary(x => $"{x.Code}.{x.Sequence}");
					AssertEquals("Expected # of failed rules", expectedRules.Length, failedRules.Keys.Count);

					var hasError = false;
					foreach (var rule in expectedRules)
					{
						var key = $"{rule.RulesSetCode}.{rule.Rule.Sequence}";
						if (failedRules.TryGetValue(key, out var failedRule))
						{
							AssertEquals("Rule Message should be included", rule.EvaluatedMessageMacro ?? rule.Rule.MessageLog, failedRule.MessageLog);
							AssertEquals("Rule status shoudl be included", rule.Rule.IsError ? ValidationLevels.Error : ValidationLevels.Warning, failedRule.Result);
							hasError = hasError || rule.Rule.IsError;
						}
						else
						{
							Fail($"Expected rule {key} to fail with macro: {rule.Rule.BusinessRule}");
						}
					}

					if (hasError)
					{
						AssertEquals(EDIMessageStatusList.Codes.Discarded, processingResult.Message.EM_Status);
					}
					else
					{
						AssertEquals(EDIMessageStatusList.Codes.Warning, processingResult.Message.EM_Status);
					}
				}
			});
		}

		public void TestUniversalValidationSimpleRules()
		{
			var ruleCode1 = "R001";
			var ruleCode2 = "R002";
			var ruleSet1 = TestHelper.CreateUniversalValidationRuleSet(Factory, DataContextType.ForwardingShipment, ruleCode1, "");
			var rule1 = AddRuleToRuleSet(ruleSet1, ruleCode1, "JS_GoodsDescription == \"CRAP\"", Constants.ErrorStatus, "Oops I failed");
			var rule2 = AddRuleToRuleSet(ruleSet1, ruleCode1, "JS_GoodsDescription == \"APPLE\"", Constants.ErrorStatus, "I pass");
			var ruleSet2 = TestHelper.CreateUniversalValidationRuleSet(Factory, DataContextType.ForwardingShipment, ruleCode2, "");
			var rule3 = AddRuleToRuleSet(ruleSet2, ruleCode2, "@UXML.AdditionalTerms == \"Something\"", Constants.WarningStatus, "I fail with warning");
			var rule4 = AddRuleToRuleSet(ruleSet2, ruleCode2, "!IsInDatabase", Constants.WarningStatus, "I fail with warning");

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_GoodsDescription = "APPLE";

			Factory.Save();

			var shipmentDataObject = CreateShipmentDataObjectWithRules(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, ruleCode1, ruleCode2);

			var result = TestCaseWithFactoryAndMessagingHelpers.ProcessUniversalShipment(shipmentDataObject);
			AssertValidationRules("Rule2 is true, Rule1, 3 and 4 should be false", result, rule1, rule3, rule4);
		}

		public void TestRulesWithFalseCriteriaNotApplied()
		{
			var ruleCode1 = "R001";
			var ruleCode2 = "R002";
			var ruleSet1 = TestHelper.CreateUniversalValidationRuleSet(Factory, DataContextType.ForwardingShipment, ruleCode1, "@UXML.AdditionalTerms == \"Something false\"");
			var rule1 = AddRuleToRuleSet(ruleSet1, ruleCode1, "JS_GoodsDescription == \"CRAP\"", Constants.ErrorStatus, "Oops I failed");
			var ruleSet2 = TestHelper.CreateUniversalValidationRuleSet(Factory, DataContextType.ForwardingShipment, ruleCode2, "@UXML.AdditionalTerms == \"Something true\"");
			var rule2 = AddRuleToRuleSet(ruleSet2, ruleCode2, "JS_GoodsDescription == \"CRAP\"", Constants.WarningStatus, "I fail with warning");

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";

			Factory.Save();

			var shipmentDataObject = CreateShipmentDataObjectWithRules(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, ruleCode1, ruleCode2);
			shipmentDataObject.AdditionalTerms = "Something true";

			var result = TestCaseWithFactoryAndMessagingHelpers.ProcessUniversalShipment(shipmentDataObject);
			AssertValidationRules("Rule 1 should not be applied since the criteria is false", result, rule2);
		}

		public void TestRulesOriginalValue()
		{
			var ruleCode1 = "R001";
			var ruleCode2 = "R002";
			var ruleSet1 = TestHelper.CreateUniversalValidationRuleSet(Factory, DataContextType.ForwardingShipment, ruleCode1, "");
			var rule1 = AddRuleToRuleSet(ruleSet1, ruleCode1, "JS_AdditionalTerms == \"NEW VALUE\" && OriginalValue({JS_AdditionalTerms == \"OLD VALUE\"})", Constants.ErrorStatus, "Oops I failed");
			var ruleSet2 = TestHelper.CreateUniversalValidationRuleSet(Factory, DataContextType.ForwardingShipment, ruleCode2, "");
			var rule2 = AddRuleToRuleSet(ruleSet2, ruleCode2, "JS_AdditionalTerms == \"CRAP\"", Constants.WarningStatus, "I fail with warning");

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_AdditionalTerms = "OLD VALUE";
			Factory.Save();

			var shipmentDataObject = CreateShipmentDataObjectWithRules(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, ruleCode1, ruleCode2);
			shipmentDataObject.AdditionalTerms = "NEW VALUE";

			var result = TestCaseWithFactoryAndMessagingHelpers.ProcessUniversalShipment(shipmentDataObject);
			AssertValidationRules("Rule 1 should pass rule 2 should fail with warning", result, rule2);
		}

		public void TestMessageLogWithMacro()
		{
			var messageLog = "\"WARNING: \" + JS_AdditionalTerms";
			var expectedLog = "WARNING: NEW VALUE";

			var ruleCode1 = "R001";
			var ruleSet1 = TestHelper.CreateUniversalValidationRuleSet(Factory, DataContextType.ForwardingShipment, ruleCode1, "");
			var rule1 = AddRuleToRuleSet(ruleSet1, ruleCode1, "JS_AdditionalTerms == \"CRAP\"", Constants.WarningStatus, messageLog, expectedLog);

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_AdditionalTerms = "OLD VALUE";
			Factory.Save();

			var shipmentDataObject = CreateShipmentDataObjectWithRules(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, ruleCode1);
			shipmentDataObject.AdditionalTerms = "NEW VALUE";

			var result = TestCaseWithFactoryAndMessagingHelpers.ProcessUniversalShipment(shipmentDataObject);
			AssertValidationRules("Rule 1 should should fail with warning", result, rule1);
		}

		public void TestEmplyValidationRuleCollectionMatchesWithDataContext()
		{
			var ruleCode1 = "R001";
			var ruleCode2 = "R002";
			var ruleSet1 = TestHelper.CreateUniversalValidationRuleSet(Factory, DataContextType.ForwardingShipment, ruleCode1, "");
			var rule1 = AddRuleToRuleSet(ruleSet1, ruleCode1, "JS_GoodsDescription == \"CRAP\"", Constants.ErrorStatus, "Oops I failed");
			var ruleSet2 = TestHelper.CreateUniversalValidationRuleSet(Factory, DataContextType.ForwardingConsol, ruleCode2, "");
			var rule2 = AddRuleToRuleSet(ruleSet2, ruleCode2, "JS_GoodsDescription == \"CRAP\"", Constants.WarningStatus, "I fail with warning");

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";

			Factory.Save();

			var shipmentDataObject = CreateShipmentDataObjectWithRules(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);
			AssertEquals(0, shipmentDataObject.ValidationRuleCollection.Count);

			var result = TestCaseWithFactoryAndMessagingHelpers.ProcessUniversalShipment(shipmentDataObject);
			AssertValidationRules("Rule 2 should not be applied since it has different data context", result, rule1);
		}

		public void TestCompileError()
		{
			var ruleCode1 = "R001";
			var ruleSet1 = TestHelper.CreateUniversalValidationRuleSet(Factory, DataContextType.ForwardingShipment, ruleCode1, "");
			var rule1 = AddRuleToRuleSet(ruleSet1, ruleCode1, "JS_GoodsDescription == \"CRAP", Constants.ErrorStatus, "Oops I failed");

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";

			Factory.Save();

			var shipmentDataObject = CreateShipmentDataObjectWithRules(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, ruleCode1);

			var result = TestCaseWithFactoryAndMessagingHelpers.ProcessUniversalShipment(shipmentDataObject);
			AssertNull("Rules with complie errors are not evaluated", result.SessionTracker.ValidationRuleCollection);
		}

		public void TestRuntimeError()
		{
			var ruleCode1 = "R001";
			var ruleSet1 = TestHelper.CreateUniversalValidationRuleSet(Factory, DataContextType.ForwardingShipment, ruleCode1, "");
			var rule1 = AddRuleToRuleSet(ruleSet1, ruleCode1, "JS_GoodsDescri == \"CRAP\"", Constants.ErrorStatus, "Oops I failed");

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";

			Factory.Save();

			var shipmentDataObject = CreateShipmentDataObjectWithRules(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, ruleCode1);

			var result = TestCaseWithFactoryAndMessagingHelpers.ProcessUniversalShipment(shipmentDataObject);
			AssertNull("Rules with runtime errors are not evaluated", result.SessionTracker.ValidationRuleCollection);
		}

		protected override void SetUp()
		{
			TestHelper =  ObjectFactory.Get<IWorkflowTestHelper>();
			base.SetUp();
		}

		class Constants
		{
			public const string ErrorStatus = "ERR";
			public const string WarningStatus = "WRN";
		}
	}
}
