using CargoWise.Types;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Module.Testing
{
	[TestedType(typeof(BarcodeValidationRuleFilterBusinessObject))]
	class BarcodeValidationRuleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestFilters

		#region TestBuyerFilter

		public void TestBuyerFilter()
		{
			var buyer1 = Helper.CreateOrg("Buy1");
			var buyer2 = Helper.CreateOrg("Buy2");
			var supplier1 = Helper.CreateOrg("Sup1");
			var supplier2 = Helper.CreateOrg("Sup2");
			var ruleSet1 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet2 = Helper.CreateRuleSet(buyer2, supplier2);
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);

			Factory.Save();
			Asserter.AddToScope(rule1);
			Asserter.AddToScope(rule2);

			var filter = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.Buyer, buyer1.PK);
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.Buyer, filter, rule1);
		}

		#endregion

		#region TestOptionalBuyerFilter

		public void TestOptionalBuyerFilter_FilterByBuyerPK()
		{
			var helper = new BarcodeParsingTestHelper(Factory);
			var buyer = helper.CreateOrg("C1");
			var ruleSetWithoutBuyer = helper.CreateRuleSet();
			var ruleSetWithBuyer = helper.CreateRuleSet(buyer);
			var ruleWithoutBuyer = helper.CreateValidationRule(ruleSetWithoutBuyer, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer = helper.CreateValidationRule(ruleSetWithBuyer, DummyTargetFields.Codes.TargetField2);

			Factory.Save();
			Asserter.AddToScope(ruleWithoutBuyer);
			Asserter.AddToScope(ruleWithBuyer);

			var filter = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.OptionalBuyer, buyer.PK);
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.OptionalBuyer, filter, new[] { ruleWithoutBuyer, ruleWithBuyer });
		}

		public void TestOptionalBuyerFilter_NotFilterByBuyerPK()
		{
			var helper = new BarcodeParsingTestHelper(Factory);
			var buyer = helper.CreateOrg("C1");
			var ruleSetWithoutBuyer = helper.CreateRuleSet();
			var ruleSetWithBuyer = helper.CreateRuleSet(buyer);
			var ruleWithoutBuyer = helper.CreateValidationRule(ruleSetWithoutBuyer, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer = helper.CreateValidationRule(ruleSetWithBuyer, DummyTargetFields.Codes.TargetField2);

			Factory.Save();
			Asserter.AddToScope(ruleWithoutBuyer);
			Asserter.AddToScope(ruleWithBuyer);

			var filter = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.OptionalBuyer, ZGuid.NewZGuid());
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.OptionalBuyer, filter, new[] { ruleWithoutBuyer });
		}

		#endregion

		#region TestSupplierFilter

		public void TestSupplierFilter()
		{
			var buyer1 = Helper.CreateOrg("Buy1");
			var buyer2 = Helper.CreateOrg("Buy2");
			var supplier1 = Helper.CreateOrg("Sup1");
			var supplier2 = Helper.CreateOrg("Sup2");
			var ruleSet1 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet2 = Helper.CreateRuleSet(buyer2, supplier2);
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);

			Factory.Save();
			Asserter.AddToScope(rule1);
			Asserter.AddToScope(rule2);

			var filter = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.Supplier, supplier1.PK);
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.Supplier, filter, rule1);
		}

		#endregion

		#region TestModuleFilter

		public void TestModuleFilter()
		{
			var buyer1 = Helper.CreateOrg("Buy1");
			var buyer2 = Helper.CreateOrg("Buy2");
			var supplier1 = Helper.CreateOrg("Sup1");
			var supplier2 = Helper.CreateOrg("Sup2");
			var ruleSet1 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet2 = Helper.CreateRuleSet(buyer2, supplier2);
			ruleSet1.BRS_Module = "YES";

			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);

			Factory.Save();
			Asserter.AddToScope(rule1);
			Asserter.AddToScope(rule2);

			var filter1 = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.Module, (ZString)"YES");
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.Module, filter1, rule1);

			var filter2 = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.Module, (ZString)"NO");
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.Module, filter2);

			var filter3 = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.Module, ZString.Empty);
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.Module, filter3, rule1, rule2);
		}

		#endregion

		#region TestFormatFilter

		public void TestFormatFilter()
		{
			var buyer1 = Helper.CreateOrg("Buy1");
			var buyer2 = Helper.CreateOrg("Buy2");
			var supplier1 = Helper.CreateOrg("Sup1");
			var supplier2 = Helper.CreateOrg("Sup2");
			var ruleSet1 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet2 = Helper.CreateRuleSet(buyer2, supplier2);
			ruleSet1.BRS_Module = "YES";

			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			rule1.BVR_Format = "YES";

			Factory.Save();
			Asserter.AddToScope(rule1);
			Asserter.AddToScope(rule2);

			var filter = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.Format, (ZString)"YES");
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.Format, filter, rule1);

			var filter2 = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.Format, (ZString)"NO");
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.Format, filter2);

			var filter3 = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.Format, ZString.Empty);
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.Format, filter3, rule1, rule2);
		}

		#endregion

		#region TestTargetFieldFilter

		public void TestTargetFieldFilter()
		{
			var buyer1 = Helper.CreateOrg("Buy1");
			var buyer2 = Helper.CreateOrg("Buy2");
			var supplier1 = Helper.CreateOrg("Sup1");
			var supplier2 = Helper.CreateOrg("Sup2");
			var ruleSet1 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet2 = Helper.CreateRuleSet(buyer2, supplier2);

			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			rule1.BVR_TargetField = "YES";

			Factory.Save();
			Asserter.AddToScope(rule1);
			Asserter.AddToScope(rule2);

			var filter = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.TargetField, (ZString)"YES");
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.TargetField, filter, rule1);

			var filter2 = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.TargetField, (ZString)"NO");
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.TargetField, filter2);

			var filter3 = GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.TargetField, ZString.Empty);
			Asserter.AssertMatches(BarcodeValidationRuleFilterBusinessObject.FilterNames.TargetField, filter3, rule1, rule2);
		}

		public void TestTargetFieldFilter_LookupMatchesModule()
		{
			var filter = GetNewFilterStripBusinessObject();
			using (BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(filter.Factory))
			{
				var moduleFilter = (ModuleTextFilter)filter[BarcodeValidationRuleFilterBusinessObject.FilterNames.Module];
				var targetFieldFilter = (ModuleTextFilter)filter[BarcodeValidationRuleFilterBusinessObject.FilterNames.TargetField];

				moduleFilter.IsActive = true;
				targetFieldFilter.IsActive = true;

				AssertEquals("Initial lookup should be empty.", 0, targetFieldFilter.List.Count);

				moduleFilter.Property = DummyBarcodeParsingConsumer.Module;

				var consumer = DummyBarcodeParsingConsumer.GetDummy(filter.Factory);
				AssertContainsExactElementsInAnyOrder(consumer.TargetFields, targetFieldFilter.List);
			}
		}

		public void TestTargetFieldFilter_Validator()
		{
			var buyer1 = Helper.CreateOrg("Buy1");
			var supplier1 = Helper.CreateOrg("Sup1");
			var ruleSet1 = Helper.CreateRuleSet(buyer1, supplier1);
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);

			Factory.Save();
			Asserter.AddToScope(rule1);

			var filter = (ModuleTextFilter)GetFilterStrip(BarcodeValidationRuleFilterBusinessObject.FilterNames.TargetField, ZString.Empty);
			filter.Validation.ValidateProperty();
			AssertNoError(filter.PropertyInfo, "Target Field cannot be entered without a module.");

			filter.Property = "ABC";
			AssertHasError(filter.PropertyInfo, "Target Field cannot be entered without a module.");
		}

		#endregion

		#region GetFilterStrip

		ModuleFilter GetFilterStrip(ZString moduleFilterName, IZType filterPropertyToFind)
		{
			var filterStripBizO = GetNewFilterStripBusinessObject();

			if (filterPropertyToFind is ZGuid)
			{
				((ModuleGuidFilter)filterStripBizO[moduleFilterName]).Property = (ZGuid)filterPropertyToFind;
				((ModuleGuidFilter)filterStripBizO[moduleFilterName]).IsActive = true;
			}
			else if (filterPropertyToFind is ZString)
			{
				((ModuleTextFilter)filterStripBizO[moduleFilterName]).Property = (ZString)filterPropertyToFind;
				((ModuleTextFilter)filterStripBizO[moduleFilterName]).IsActive = true;
			}

			return filterStripBizO[moduleFilterName];
		}

		#endregion

		#endregion

		#region Implementation

		FilterStripAsserter<BarcodeValidationRule> Asserter => asserter ?? (asserter = new FilterStripAsserter<BarcodeValidationRule>(Factory, b => b.BVR_TargetField));

		FilterStripAsserter<BarcodeValidationRule> asserter;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new BarcodeValidationRuleFilterBusinessObject();

		BarcodeParsingTestHelper Helper => helper ?? (helper = new BarcodeParsingTestHelper(Factory));

		BarcodeParsingTestHelper helper;

		#endregion
	}
}
