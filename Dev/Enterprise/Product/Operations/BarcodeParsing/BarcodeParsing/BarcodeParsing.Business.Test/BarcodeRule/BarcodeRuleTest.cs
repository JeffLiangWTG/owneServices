using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(BarcodeRule))]
	class BarcodeRuleTest : BarcodeParsingBusinessObjectTestCase
	{
		#region Related Entities

		#region TestComponents

		public void TestComponents()
		{
			var rule = Helper.CreateRule();
			AssertEquals(typeof(BarcodeRuleComponentCollection), rule.Components.GetType());
			AssertEquals(true, rule.IsRegisteredEditableChildObject(rule.Components));
		}

		#endregion

		#region TestRuleSet

		public void TestRuleSet()
		{
			var rule = Factory.New<BarcodeRule>();
			AssertNull(rule.RuleSet);

			var ruleSet = Helper.CreateRuleSet();
			rule.BRU_BRS_RuleSet = ruleSet.PK;
			AssertEquals(ruleSet, rule.RuleSet);
		}

		#endregion

		#endregion

		#region Properties

		#region TestBRU_Terminator

		public void TestBRU_Terminator()
		{
			var rule = Helper.CreateRule();
			AssertEquals("\"\"", rule.BRU_Terminator);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertEquals(BarcodeRule.GS1Terminator, rule.BRU_Terminator);

			rule.TerminatorType = TerminatorTypes.Codes.UserDefined;
			AssertEquals("\"\"", rule.BRU_Terminator);

			rule.BRU_Terminator = "1";
			AssertEquals("\"1\"", rule.BRU_Terminator);

			rule.BRU_Terminator = "\"1\"";
			AssertEquals("\"1\"", rule.BRU_Terminator);

			rule.BRU_Terminator = "\"2\"";
			AssertEquals("\"2\"", rule.BRU_Terminator);

			rule.BRU_Terminator = "";
			AssertEquals("\"\"", rule.BRU_Terminator);

			rule.BRU_Terminator = "\"\"";
			AssertEquals("\"\"\"\"", rule.BRU_Terminator);

			rule.BRU_Terminator = "12345678";
			AssertEquals("\"12345678\"", rule.BRU_Terminator);

			// Setting ther terminator should trim one quote from each end before wrapping again to account
			// for cases where the user edits the terminator when it is at max length and removes one quote.
			rule.BRU_Terminator = "\"12345678";
			AssertEquals("\"12345678\"", rule.BRU_Terminator);

			// Setting ther terminator should trim one quote from each end before wrapping again to account
			// for cases where the user edits the terminator when it is at max length and removes one quote.
			rule.BRU_Terminator = "12345678\"";
			AssertEquals("\"12345678\"", rule.BRU_Terminator);
		}

		#endregion

		#region TestBRU_TerminatorInfo

		public void TestBRU_TerminatorInfo()
		{
			var rule = Helper.CreateRule();
			AssertEquals("Precondition", false, rule.BRU_TerminatorInfo.ReadOnly);
			AssertEquals("Precondition", TerminatorTypes.Codes.UserDefined, rule.TerminatorType);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertEquals(true, rule.BRU_TerminatorInfo.ReadOnly);
		}

		#endregion

		#region TestEmptyTerminatorWithQuotes

		public void TestEmptyTerminatorWithQuotes()
		{
			AssertEquals("\"\"", BarcodeRule.EmptyTerminatorWithQuotes);
		}

		#endregion

		#region TestGS1Terminator

		public void TestGS1Terminator()
		{
			AssertEquals("\u001d", BarcodeRule.GS1Terminator);
		}

		#endregion

		#region TestModule

		public void TestModule()
		{
			var rule = (BarcodeRule)GetNewBusinessObject();
			AssertEquals("", rule.Module);

			var ruleSet = Helper.CreateRuleSet();
			rule.BRU_BRS_RuleSet = ruleSet.PK;
			AssertEquals("DUM", rule.Module);
		}

		#endregion

		#region TestReadOnly

		public void TestReadOnly()
		{
			var rule = Helper.CreateRule();
			AssertEquals("RuleSet is not system defined, should be editable.", false, rule.ReadOnly);

			rule.RuleSet.BRS_IsSystem = true;
			AssertEquals("RuleSet is system defined, should be read-only.", true, rule.ReadOnly);
		}

		#endregion

		#region TestTerminatorType

		public void TestTerminatorType()
		{
			var rule = Helper.CreateRule();
			AssertEquals("Terminator Type is User Defined by default.", TerminatorTypes.Codes.UserDefined, rule.TerminatorType);
			AssertEquals("Terminator is empty by default.", "\"\"", rule.BRU_Terminator);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertEquals("DB Terminator should not wrap the GS1 Character in quotes.", BarcodeRule.GS1Terminator, rule.BRU_Terminator);

			rule.TerminatorType = TerminatorTypes.Codes.UserDefined;
			AssertEquals("Setting Terminator Type to User Defined should remove GS1 Terminator.", "\"\"", rule.BRU_Terminator);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			Factory.Save();
			var reloadedRule = new BusinessObjectFactory().Load<BarcodeRule>(rule.PK);
			AssertEquals(TerminatorTypes.Codes.GS1, reloadedRule.TerminatorType);
		}

		#endregion

		#region TestTerminatorTypeDescription

		public void TestTerminatorTypeDescription()
		{
			var rule = Helper.CreateRule();
			AssertEquals("Terminator Type is User Defined by default.", TerminatorTypes.Codes.UserDefined, rule.TerminatorType);
			AssertEquals("Terminator is UserDefined by default.", TerminatorTypes.Descriptions.UserDefined, rule.TerminatorTypeDescription);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertEquals("DB Terminator should be GS1 Description", TerminatorTypes.Descriptions.GS1, rule.TerminatorTypeDescription);
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsAllComponentsFixedLength

		public void TestIsAllComponentsFixedLength()
		{
			var rule = Helper.CreateRule();
			AssertEquals("If no components, AreAllComponentsFixedLength should be false.", false, rule.IsAllComponentsFixedLength);

			var component1 = Helper.CreateRuleComponent(rule, fixedLength: 1);
			AssertEquals("All components have fixed length, AreAllComponentsFixedLength should be true.", true, rule.IsAllComponentsFixedLength);

			var component2 = Helper.CreateRuleComponent(rule, minLength: 1, maxLength: 2);
			AssertEquals("One component does not have fixed length, AreAllComponentsFixedLength should be false.", false, rule.IsAllComponentsFixedLength);

			component2.BRC_MaxLength = 1;
			AssertEquals("All components have fixed length, AreAllComponentsFixedLength should be true.", true, rule.IsAllComponentsFixedLength);
		}

		#endregion

		#region TestIsAllComponentsExceptLastFixedLength

		public void TestIsAllComponentsExceptLastFixedLength()
		{
			var rule = Helper.CreateRule();
			AssertEquals("If no components, AreAllComponentsExceptLastFixedLength should be false.", false, rule.IsAllComponentsExceptLastFixedLength);

			var component1 = Helper.CreateRuleComponent(rule, fixedLength: 1);
			AssertEquals("All components have fixed length, AreAllComponentsExceptLastFixedLength should be true.", true, rule.IsAllComponentsExceptLastFixedLength);

			var component2 = Helper.CreateRuleComponent(rule, minLength: 1, maxLength: 2);
			AssertEquals("One component does not have fixed length, since it is the only component AreAllComponentsExceptLastFixedLength should be true.", true, rule.IsAllComponentsExceptLastFixedLength);

			Helper.CreateRuleComponent(rule, minLength: 5, maxLength: 10);
			AssertEquals("One component before the last component does not have fixed length, AreAllComponentsFixedLength should be false.", false, rule.IsAllComponentsExceptLastFixedLength);
		}

		#endregion

		#region TestIsGS1

		public void TestIsGS1()
		{
			var rule = Helper.CreateRule();
			AssertEquals("A rule should not be a GS1 terminated rule by default.", false, rule.IsGS1);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertEquals("rule.IsGS1", true, rule.IsGS1);

			rule.TerminatorType = TerminatorTypes.Codes.UserDefined;
			AssertEquals("rule.IsGS1", false, rule.IsGS1);

			rule.IsGS1 = true;
			AssertEquals("rule.TerminatorType", TerminatorTypes.Codes.GS1, rule.TerminatorType);

			rule.IsGS1 = false;
			AssertEquals("rule.TerminatorType", TerminatorTypes.Codes.UserDefined, rule.TerminatorType);
		}

		#endregion

		#region TestIsPartialRule

		public void TestIsPartialRule()
		{
			var rule1 = Helper.CreateRule();
			AssertEquals("Empty rule should default to being a full rule.", false, rule1.IsPartialRule);

			var rule2 = Helper.CreateRule();
			var component1 = Factory.New<BarcodeRuleComponent>();
			component1.BRC_BRU_Rule = rule2.PK;
			component1.BRC_Sequence = 1;
			AssertEquals("The only component has a sequence greater than zero, should be full rule.", false, rule2.IsPartialRule);

			var rule3 = Helper.CreateRule();
			component1.BRC_BRU_Rule = rule3.PK;
			component1.BRC_Sequence = 0;
			AssertEquals("The only component has a sequence of zero, should be partial rule.", true, rule3.IsPartialRule);

			var rule4 = Helper.CreateRule();
			var component2 = Factory.New<BarcodeRuleComponent>();
			component1.BRC_BRU_Rule = rule4.PK;
			component2.BRC_BRU_Rule = rule4.PK;
			AssertEquals("There is more than one component, should be full rule.", false, rule4.IsPartialRule);
		}

		#endregion

		#region TestIsPartialRule_RefreshesComponentsCollection

		public void TestIsPartialRule_RefreshesComponentsCollection()
		{
			var rule = Helper.CreateRule();
			rule.IsPartialRule = false;

			int listChangedHitCount = 0;
			((IBusinessObjectCollection)rule.Components).ListChanged += (sender, e) => listChangedHitCount++;
			rule.IsPartialRule = true;
			AssertEquals("Setting Is Partial Rule, should refresh the Components Collection.", 1, listChangedHitCount);
		}

		#endregion

		#region TestIsPartialRule_SetsSequenceOnComponents

		public void TestIsPartialRule_SetsSequenceOnComponents()
		{
			var rule = Helper.CreateRule();
			var component1 = Helper.CreateRuleComponent(rule);
			var component2 = Helper.CreateRuleComponent(rule);
			AssertEquals("Precondition", new ZShort(1), component1.BRC_Sequence);
			AssertEquals("Precondition", new ZShort(2), component2.BRC_Sequence);

			rule.IsPartialRule = true;
			AssertEquals(new ZShort(0), component1.BRC_Sequence);
			AssertEquals(new ZShort(0), component2.BRC_Sequence);

			rule.IsPartialRule = false;
			AssertEquals(new ZShort(1), component1.BRC_Sequence);
			AssertEquals(new ZShort(2), component2.BRC_Sequence);
		}

		#endregion

		#endregion

		#region LoadMatchingRules

		#region TestLoadMatchingRulesChecksForGS1Flag

		public void TestLoadMatchingRulesChecksForGS1Flag()
		{
			var data = new BarcodeMatchingData(Helper);
			data.CreateDataInDB();

			// Should match most detailed non GS1 rule (Buyer+Supplier+Product)
			var rulesMatched1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1Supplier1Part1.PK }, rulesMatched1.Select(r => r.PK));

			// Should match most detailed GS1 rule (Buyer+Supplier+Product)
			var rulesMatched2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: true);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleGS1WithBuyer1Supplier1Part1.PK }, rulesMatched2.Select(r => r.PK));

			data.RuleGS1WithBuyer1Supplier1Part1.Delete();
			Factory.Save();

			// Shold match next GS1 less detailed rule
			var rulesMatched3 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: true);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleGS1WithBuyer1NoSupplier.PK }, rulesMatched3.Select(r => r.PK));

			// delete last GS1 rule
			data.RuleGS1WithBuyer1NoSupplier.Delete();
			Factory.Save();
			// no rules should match as there are only non GS1 rules left
			var rulesMatched4 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: true);
			AssertEquals(0, rulesMatched4.Count());
		}

		#endregion

		#region TestLoadMatchingRules_BuyerAndSupplier

		public void TestLoadMatchingRules_BuyerAndSupplier()
		{
			var data = new BarcodeMatchingData(Helper);
			data.CreateDataInDB();

			// Should first match rules where both buyer & supplier are the same
			var rulesMatched1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1Supplier1.PK }, rulesMatched1.Select(r => r.PK));

			var rulesMatchWithCode1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1Supplier1.PK }, rulesMatchWithCode1.Select(r => r.PK));

			data.RuleWithBuyer1Supplier1.Delete();
			Factory.Save();

			// Should fall back to rules where buyer is the same and there is no supplier
			var rulesMatched2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplier.PK }, rulesMatched2.Select(r => r.PK));

			var rulesMatchWithCode2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplier.PK }, rulesMatchWithCode2.Select(r => r.PK));

			data.RuleWithBuyer1NoSupplier.Delete();
			Factory.Save();

			// Should fall back to rules where supplier is the same and there is no buyer
			var rulesMatched3 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatched3.Select(r => r.PK));

			var rulesMatchWithCode3 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatchWithCode3.Select(r => r.PK));

			data.RuleWithSupplier1NoBuyer.Delete();
			Factory.Save();

			// Should fall back to rules where there is no buyer and no supplier
			var rulesMatched4 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatched4.Select(r => r.PK));

			var rulesMatchWithCode4 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatchWithCode4.Select(r => r.PK));

			data.RuleWithNoBuyerNoSupplier.Delete();
			Factory.Save();

			// There are no rules left
			var rulesMatched5 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, ZGuid.Empty, isGS1: false);
			AssertEquals(0, rulesMatched5.Count());

			var rulesMatchWithCode5 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, ZGuid.Empty, isGS1: false);
			AssertEquals(0, rulesMatchWithCode5.Count());
		}

		#endregion

		#region TestLoadMatchingRules_Buyer

		public void TestLoadMatchingRules_Buyer()
		{
			var data = new BarcodeMatchingData(Helper);
			data.CreateDataInDB();

			// Should first match rules where buyer is the same and there is no supplier
			var rulesMatched1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, ZGuid.Empty, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplier.PK }, rulesMatched1.Select(r => r.PK));

			var rulesMatchedWithCode1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, "", ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplier.PK }, rulesMatchedWithCode1.Select(r => r.PK));

			data.RuleWithBuyer1NoSupplier.Delete();
			Factory.Save();

			// Should fall back to rules where there is no buyer and no supplier
			var rulesMatched2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, ZGuid.Empty, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatched2.Select(r => r.PK));

			var rulesMatchedWithCode2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, "", ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatchedWithCode2.Select(r => r.PK));

			data.RuleWithNoBuyerNoSupplier.Delete();
			Factory.Save();

			// There are no rules left
			var rulesMatched3 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, ZGuid.Empty, ZGuid.Empty, isGS1: false);
			AssertEquals(0, rulesMatched3.Count());

			var rulesMatchedWithCode3 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, "", ZGuid.Empty, isGS1: false);
			AssertEquals(0, rulesMatchedWithCode3.Count());
		}

		#endregion

		#region TestLoadMatchingRules_Supplier

		public void TestLoadMatchingRules_Supplier()
		{
			var data = new BarcodeMatchingData(Helper);
			data.CreateDataInDB();

			// Should first match rules where supplier is the same and there is no buyer
			var rulesMatched1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, data.Supplier1.PK, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatched1.Select(r => r.PK));

			var rulesMatchedWithCode1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", data.Supplier1.OH_Code, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatchedWithCode1.Select(r => r.PK));

			data.RuleWithSupplier1NoBuyer.Delete();
			Factory.Save();

			// Should fall back to rules where there is no buyer and no supplier
			var rulesMatched2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, data.Supplier1.PK, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatched2.Select(r => r.PK));

			var rulesMatchedWithCode2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", data.Supplier1.OH_Code, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatchedWithCode2.Select(r => r.PK));

			data.RuleWithNoBuyerNoSupplier.Delete();
			Factory.Save();

			// There are no rules left
			var rulesMatched3 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, data.Supplier1.PK, ZGuid.Empty, isGS1: false);
			AssertEquals(0, rulesMatched3.Count());

			var rulesMatchedWithCode3 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", data.Supplier1.OH_Code, ZGuid.Empty, isGS1: false);
			AssertEquals(0, rulesMatchedWithCode3.Count());
		}

		#endregion

		#region TestLoadMatchingRules_NoBuyerOrSupplier

		public void TestLoadMatchingRules_NoBuyerOrSupplier()
		{
			var data = new BarcodeMatchingData(Helper);
			data.CreateDataInDB();

			// Should first match rules where there is no buyer and no supplier
			var rulesMatched1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatched1.Select(r => r.PK));

			var rulesMatchedWithCode1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", "", ZGuid.Empty, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatchedWithCode1.Select(r => r.PK));

			data.RuleWithNoBuyerNoSupplier.Delete();
			Factory.Save();

			// There are no rules left
			var rulesMatched2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, isGS1: false);
			AssertEquals(0, rulesMatched2.Count());

			var rulesMatchedWithCode2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", "", ZGuid.Empty, isGS1: false);
			AssertEquals(0, rulesMatchedWithCode2.Count());
		}

		#endregion

		#region TestLoadMatchingRules_RelatedEntity

		public void TestLoadMatchingRules_RelatedEntity()
		{
			var data = new BarcodeMatchingData(Helper);
			data.CreateDataInDB();

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.IsRelatedEntityAvailable = true;
			dummy.RelatedEntityRequirements = RelatedEntityRequirements.MustHaveBuyer;

			// Should match rules where both buyer & supplier and related entity are the same
			var rulesMatched1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1Supplier1Part1.PK }, rulesMatched1.Select(r => r.PK));

			var rulesMatchWithCode1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1Supplier1Part1.PK }, rulesMatchWithCode1.Select(r => r.PK));

			// Should match rules where the supplier is the same *only*. Since buyer is not provided, we ignore related entity
			var rulesMatched2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatched2.Select(r => r.PK));

			var rulesMatchWithCode2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatchWithCode2.Select(r => r.PK));

			// Should match rules where all fields are empty, we ignore related entity if buyer is not provided
			var rulesMatched3 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, ZGuid.Empty, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatched3.Select(r => r.PK));

			var rulesMatchWithCode3 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", "", data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatchWithCode3.Select(r => r.PK));

			// Should match rules where both buyer & related entity are the same
			var rulesMatched4 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, ZGuid.Empty, data.Part2.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplierPart2.PK }, rulesMatched4.Select(r => r.PK));

			var rulesMatchWithCode4 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, "", data.Part2.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplierPart2.PK }, rulesMatchWithCode4.Select(r => r.PK));

			data.RuleWithBuyer1Supplier1Part1.Delete();
			Factory.Save();

			// Should fall back to rules where the buyer & supplier are the same and there is no related entity
			var rulesMatched5 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1Supplier1.PK }, rulesMatched5.Select(r => r.PK));

			var rulesMatchWithCode5 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1Supplier1.PK }, rulesMatchWithCode5.Select(r => r.PK));

			data.RuleWithBuyer1Supplier1.Delete();
			Factory.Save();

			// Should fall back to rules where the buyer & related entity are the same and there is no supplier
			var rulesMatched6 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplierPart1.PK }, rulesMatched6.Select(r => r.PK));

			var rulesMatchWithCode6 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplierPart1.PK }, rulesMatchWithCode6.Select(r => r.PK));

			data.RuleWithBuyer1NoSupplierPart1.Delete();
			Factory.Save();

			// Should fall back to rules where the buyer is the same and there is no supplier and related entity
			var rulesMatched7 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplier.PK }, rulesMatched7.Select(r => r.PK));

			var rulesMatchWithCode7 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplier.PK }, rulesMatchWithCode7.Select(r => r.PK));

			// Should match rules where the buyer is the same and there is no supplier and related entity
			var rulesMatched8 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, ZGuid.Empty, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplier.PK }, rulesMatched8.Select(r => r.PK));

			var rulesMatchWithCode8 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, "", data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplier.PK }, rulesMatchWithCode8.Select(r => r.PK));

			data.RuleWithBuyer1NoSupplier.Delete();
			Factory.Save();

			// Should fall back to rules where the buyer is the same and there is no supplier and related entity
			var rulesMatched9 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatched9.Select(r => r.PK));

			var rulesMatchWithCode9 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatchWithCode9.Select(r => r.PK));

			data.RuleWithSupplier1NoBuyer.Delete();
			Factory.Save();

			// Should fall back to rules where the buyer, supplier & related entity are all empty
			var rulesMatched10 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatched10.Select(r => r.PK));

			var rulesMatchWithCode10 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoBuyerNoSupplier.PK }, rulesMatchWithCode10.Select(r => r.PK));
		}

		#endregion

		#region TestLoadMatchingRules_RelatedEntity_WhereBuyerIsNotRequired

		public void TestLoadMatchingRules_RelatedEntity_WhereBuyerIsNotRequired()
		{
			var data = new BarcodeMatchingData(Helper);
			data.CreateDataInDB();

			data.RuleWithBuyer1Supplier1Part1.Delete();
			data.RuleWithBuyer1Supplier1.Delete();
			Factory.Save();

			// Should Match rules where supplier & related entity are the same and there is no buyer, since buyer is not required for the related entity with the 'FOR' module
			var rulesMatched1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyerPart1.PK }, rulesMatched1.Select(r => r.PK));

			var rulesMatchedWithCode1 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyerPart1.PK }, rulesMatchedWithCode1.Select(r => r.PK));

			// Should Match rules where buyer & related entity are the same and there is no supplier
			var rulesMatched2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplierPart1.PK }, rulesMatched2.Select(r => r.PK));

			var rulesMatchedWithCode2 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithBuyer1NoSupplierPart1.PK }, rulesMatchedWithCode2.Select(r => r.PK));

			data.RuleWithBuyer1NoSupplierPart1.Delete();
			Factory.Save();

			// Should fall back to rules where the supplier & related entity are the same and there is no buyer, since buyer is not required for the related entity with the 'FOR' module
			var rulesMatched3 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyerPart1.PK }, rulesMatched3.Select(r => r.PK));

			var rulesMatchedWithCode3 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyerPart1.PK }, rulesMatchedWithCode3.Select(r => r.PK));

			data.RuleWithSupplier1NoBuyerPart1.Delete();
			data.RuleWithBuyer1NoSupplier.Delete(); // prevent fall back to rules where the buyer is the same and there is no supplier & related entity
			Factory.Save();

			// Should fall back to rules where the supplier is the same and there is no buyer and related entity
			var rulesMatched4 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatched4.Select(r => r.PK));

			var rulesMatchedWithCode4 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatchedWithCode4.Select(r => r.PK));

			// Should fall back to rules where the supplier is the same and there is no buyer and related entity
			var rulesMatched5 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatched5.Select(r => r.PK));

			var rulesMatchedWithCode5 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithSupplier1NoBuyer.PK }, rulesMatchedWithCode5.Select(r => r.PK));

			data.RuleWithSupplier1NoBuyer.Delete();
			Factory.Save();

			// Should fall back to rules where the related entity is the same and there is no buyer and supplier, since buyer is not required for the related entity with the 'FOR' module
			var rulesMatched6 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoSupplierNoBuyerPart1.PK }, rulesMatched6.Select(r => r.PK));

			var rulesMatchedWithCode6 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoSupplierNoBuyerPart1.PK }, rulesMatchedWithCode6.Select(r => r.PK));

			// Should fall back to rules where the related entity is the same and there is no buyer and supplier, since buyer is not required for the related entity with the 'FOR' module
			var rulesMatched7 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, data.Supplier1.PK, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoSupplierNoBuyerPart1.PK }, rulesMatched7.Select(r => r.PK));

			var rulesMatchedWithCode7 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", data.Supplier1.OH_Code, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoSupplierNoBuyerPart1.PK }, rulesMatchedWithCode7.Select(r => r.PK));

			// Should fall back to rules where the related entity is the same and there is no buyer and supplier, since buyer is not required for the related entity with the 'FOR' module
			var rulesMatched8 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.PK, ZGuid.Empty, data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoSupplierNoBuyerPart1.PK }, rulesMatched8.Select(r => r.PK));

			var rulesMatchedWithCode8 = BarcodeRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, data.Buyer1.OH_Code, "", data.Part1.PK, isGS1: false);
			AssertContainsExactElementsInAnyOrder(new[] { data.RuleWithNoSupplierNoBuyerPart1.PK }, rulesMatchedWithCode8.Select(r => r.PK));
		}

		#endregion

		#region class BarcodeMatchingData

		class BarcodeMatchingData
		{
			public BarcodeMatchingData(BarcodeParsingTestHelper helper)
			{
				Helper = helper;
			}

			readonly BarcodeParsingTestHelper Helper;

			public OrgHeader Buyer1 { get; private set; }
			public OrgHeader Buyer2 { get; private set; }
			public OrgHeader Supplier1 { get; private set; }
			public OrgHeader Supplier2 { get; private set; }

			public OrgSupplierPart Part1 { get; private set; }
			public OrgSupplierPart Part2 { get; private set; }

			// no parts
			public BarcodeRule RuleWithBuyer1Supplier1 { get; private set; }
			public BarcodeRule RuleWithBuyer1NoSupplier { get; private set; }
			public BarcodeRule RuleWithBuyer1Supplier2 { get; private set; }
			public BarcodeRule RuleWithSupplier1NoBuyer { get; private set; }
			public BarcodeRule RuleWithBuyer2Supplier1 { get; private set; }
			public BarcodeRule RuleWithNoBuyerNoSupplier { get; private set; }

			// with parts
			public BarcodeRule RuleWithBuyer1Supplier1Part1 { get; private set; }
			public BarcodeRule RuleWithBuyer1Supplier1Part2 { get; private set; }
			public BarcodeRule RuleWithBuyer1NoSupplierPart1 { get; private set; }
			public BarcodeRule RuleWithBuyer1NoSupplierPart2 { get; private set; }
			public BarcodeRule RuleWithBuyer1Supplier2Part1 { get; private set; }
			public BarcodeRule RuleWithBuyer1Supplier2Part2 { get; private set; }
			public BarcodeRule RuleWithSupplier1NoBuyerPart1 { get; private set; }
			public BarcodeRule RuleWithSupplier1NoBuyerPart2 { get; private set; }
			public BarcodeRule RuleWithNoSupplierNoBuyerPart1 { get; private set; }
			public BarcodeRule RuleWithNoSupplierNoBuyerPart2 { get; private set; }

			//
			public BarcodeRule RuleGS1WithBuyer1NoSupplier { get; private set; }
			public BarcodeRule RuleGS1WithBuyer1Supplier1Part1 { get; private set; }

			public void CreateDataInDB()
			{
				Buyer1 = Helper.CreateOrg("Buyer1");
				Buyer2 = Helper.CreateOrg("Buyer2");

				Supplier1 = Helper.CreateOrg("Supplier1");
				Supplier2 = Helper.CreateOrg("Supplier2");

				Part1 = Helper.CreatePart("P1", Buyer1);
				Part2 = Helper.CreatePart("P2", Buyer1);

				var ruleSet1 = Helper.CreateRuleSet(Buyer1, Supplier1);
				var ruleSet2 = Helper.CreateRuleSet(Buyer1);
				var ruleSet3 = Helper.CreateRuleSet(Buyer1, Supplier2);
				var ruleSet4 = Helper.CreateRuleSet(null, Supplier1);
				var ruleSet5 = Helper.CreateRuleSet(Buyer2, Supplier1);
				var ruleSet6 = Helper.CreateRuleSet();
				var ruleSet7 = Helper.CreateRuleSet(Buyer1, Supplier1, Part1);
				var ruleSet8 = Helper.CreateRuleSet(Buyer1, Supplier1, Part2);
				var ruleSet9 = Helper.CreateRuleSet(Buyer1, null, Part1);
				var ruleSet10 = Helper.CreateRuleSet(Buyer1, null, Part2);
				var ruleSet11 = Helper.CreateRuleSet(Buyer1, Supplier2, Part1);
				var ruleSet12 = Helper.CreateRuleSet(Buyer1, Supplier2, Part2);
				var ruleSet13 = Helper.CreateRuleSet(null, Supplier1, Part1);
				var ruleSet14 = Helper.CreateRuleSet(null, Supplier1, Part2);
				var ruleSet15 = Helper.CreateRuleSet(null, null, Part1);
				var ruleSet16 = Helper.CreateRuleSet(null, null, Part2);

				RuleWithBuyer1Supplier1 = Helper.CreateRule(ruleSet1);
				RuleWithBuyer1NoSupplier = Helper.CreateRule(ruleSet2);
				RuleWithBuyer1Supplier2 = Helper.CreateRule(ruleSet3);
				RuleWithSupplier1NoBuyer = Helper.CreateRule(ruleSet4);
				RuleWithBuyer2Supplier1 = Helper.CreateRule(ruleSet5);
				RuleWithNoBuyerNoSupplier = Helper.CreateRule(ruleSet6);
				RuleWithBuyer1Supplier1Part1 = Helper.CreateRule(ruleSet7);
				RuleWithBuyer1Supplier1Part2 = Helper.CreateRule(ruleSet8);
				RuleWithBuyer1NoSupplierPart1 = Helper.CreateRule(ruleSet9);
				RuleWithBuyer1NoSupplierPart2 = Helper.CreateRule(ruleSet10);
				RuleWithBuyer1Supplier2Part1 = Helper.CreateRule(ruleSet11);
				RuleWithBuyer1Supplier2Part2 = Helper.CreateRule(ruleSet12);
				RuleWithSupplier1NoBuyerPart1 = Helper.CreateRule(ruleSet13);
				RuleWithSupplier1NoBuyerPart2 = Helper.CreateRule(ruleSet14);
				RuleWithNoSupplierNoBuyerPart1 = Helper.CreateRule(ruleSet15);
				RuleWithNoSupplierNoBuyerPart2 = Helper.CreateRule(ruleSet16);
				RuleGS1WithBuyer1NoSupplier = Helper.CreateRule(ruleSet2, isGS1: true);
				RuleGS1WithBuyer1Supplier1Part1 = Helper.CreateRule(ruleSet7, isGS1: true);

				Helper.CreateRuleComponent(RuleWithBuyer1Supplier1);
				Helper.CreateRuleComponent(RuleWithBuyer1NoSupplier);
				Helper.CreateRuleComponent(RuleWithBuyer1Supplier2);
				Helper.CreateRuleComponent(RuleWithSupplier1NoBuyer);
				Helper.CreateRuleComponent(RuleWithBuyer2Supplier1);
				Helper.CreateRuleComponent(RuleWithNoBuyerNoSupplier);
				Helper.CreateRuleComponent(RuleWithBuyer1Supplier1Part1);
				Helper.CreateRuleComponent(RuleWithBuyer1Supplier1Part2);
				Helper.CreateRuleComponent(RuleWithBuyer1NoSupplierPart1);
				Helper.CreateRuleComponent(RuleWithBuyer1NoSupplierPart2);
				Helper.CreateRuleComponent(RuleWithBuyer1Supplier2Part1);
				Helper.CreateRuleComponent(RuleWithBuyer1Supplier2Part2);

				Helper.CreateRuleComponent(RuleWithSupplier1NoBuyerPart1);
				Helper.CreateRuleComponent(RuleWithSupplier1NoBuyerPart2);
				Helper.CreateRuleComponent(RuleWithNoSupplierNoBuyerPart1);
				Helper.CreateRuleComponent(RuleWithNoSupplierNoBuyerPart2);

				Helper.CreateRuleComponent(RuleGS1WithBuyer1NoSupplier);
				Helper.CreateRuleComponent(RuleGS1WithBuyer1Supplier1Part1);

				Buyer1.Factory.Save();
			}
		}

		#endregion

		#endregion

		#region SetDefaultValues

		public void TestSetDefaultValues()
		{
			var rule = Factory.New<BarcodeRule>();
			var row = ((IBusinessObjectInternals)rule).Row;
			AssertEquals("\"\"", row[BarcodeRuleSchema.Constants.BRU_Terminator]);
		}

		#endregion

		#region TestCloningCopiesComponents

		public void TestCloningCopiesComponents()
		{
			var ruleSet = Helper.CreateRuleSet();
			var rule = Helper.CreateRule(ruleSet);
			rule.Components.AddNew();
			AssertEquals(true, rule.SupportsClone());

			var clone = (BarcodeRule)rule.Clone();
			AssertEquals(1, clone.Components.Count);
			AssertNotEquals("Should not clone foreign key to rule set.", rule.BRU_BRS_RuleSet, clone.BRU_BRS_RuleSet);
			AssertEquals("Foreign key for cloned item should be empty", ZGuid.Empty, clone.BRU_BRS_RuleSet);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var barcodeRule = Factory.New<BarcodeRule>();
			AssertEquals("Barcode Rule", barcodeRule.HumanReadableName);

			barcodeRule.BRU_Name = "boo";
			AssertEquals("Barcode Rule boo", barcodeRule.HumanReadableName);
		}

		#endregion

		// interfaces

		#region IBarcodeRule Members

		public void TestIBarcodeRule()
		{
			var rule = Helper.CreateRule();
			rule.BRU_RuleNumber = 3;
			rule.BRU_Name = "ABC";
			rule.BRU_Terminator = "|";
			rule.IsPartialRule = true;

			var component = Helper.CreateRuleComponent(rule);
			IBarcodeRule iRule = rule;
			AssertEquals("iRule.RuleNumber", (short)3, iRule.RuleNumber);
			AssertEquals("iRule.RuleName", "ABC", iRule.RuleName);
			AssertEquals("iRule.Terminator", "|", iRule.Terminator);
			AssertEquals("iRule.IsPartialRule", true, iRule.IsPartialRule);
			AssertContainsExactElementsInAnyOrder(new[] { component }, iRule.Components);
		}

		#endregion
	}
}
