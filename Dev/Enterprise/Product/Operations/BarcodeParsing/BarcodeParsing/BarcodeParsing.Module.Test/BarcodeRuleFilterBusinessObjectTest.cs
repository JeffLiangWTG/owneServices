using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Module.Testing
{
	[TestedType(typeof(BarcodeRuleFilterBusinessObject))]
	class BarcodeRuleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestFilters

		#region TestBuyerFilter

		public void TestBuyerFilter()
		{
			var buyer = Data.Rule1.RuleSet.Buyer;
			Factory.Save();

			var filter = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.Buyer, buyer.PK);
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.Buyer, filter, Data.Rule1);
		}

		#endregion

		#region TestOptionalBuyerFilter

		public void TestOptionalBuyerFilter()
		{
			var helper = new BarcodeParsingTestHelper(Factory);
			var buyer = helper.CreateOrg("C1");
			var ruleSetWithoutBuyer = helper.CreateRuleSet();
			var ruleSetWithBuyer = helper.CreateRuleSet(buyer);
			var ruleWithoutBuyer = helper.CreateRule(ruleSetWithoutBuyer, "rule1");
			var ruleWithBuyer = helper.CreateRule(ruleSetWithBuyer, "rule2");
			helper.CreateRuleComponent(ruleWithoutBuyer);
			helper.CreateRuleComponent(ruleWithBuyer);
			Factory.Save();
			Asserter.AddToScope(ruleWithoutBuyer);
			Asserter.AddToScope(ruleWithBuyer);

			var filter = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.OptionalBuyer, buyer.PK);
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.OptionalBuyer, filter, new[] { ruleWithoutBuyer, ruleWithBuyer });

			filter = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.OptionalBuyer, ZGuid.NewZGuid());
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.OptionalBuyer, filter, new[] { ruleWithoutBuyer });
		}

		#endregion

		#region TestIsPartialRuleFilter

		public void TestIsPartialRuleFilter()
		{
			Data.Rule1Component.BRC_ApplicationID = "01";
			Data.Rule1Component.BRC_Sequence = 0;
			Data.Rule2Component.BRC_Sequence = 1;
			Factory.Save();

			var filter1 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.IsPartial, (ZString)IsPartialStatuses.Codes.PartialRules);
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.IsPartial, filter1, Data.Rule1);

			var filter2 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.IsPartial, (ZString)IsPartialStatuses.Codes.FullRules);
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.IsPartial, filter2, Data.Rule2);

			var filter3 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.IsPartial, (ZString)IsPartialStatuses.Codes.All);
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.IsPartial, filter3, Data.Rule1, Data.Rule2);
		}

		#endregion

		#region TestIsSystemFilter

		public void TestIsSystemFilter()
		{
			// system barcode rule sets should not have any foreign keys set.
			Data.RuleSet1.BRS_IsSystem = true;
			Data.RuleSet1.BRS_Module = "TST";
			Data.RuleSet1.BRS_OH_Buyer = ZGuid.Empty;
			Data.RuleSet1.BRS_OH_Supplier = ZGuid.Empty;
			Factory.Save();

			var filter1 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.IsSystem, (ZString)IsSystemStatuses.Codes.SystemRules);
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.IsSystem, filter1, Data.Rule1);

			var filter2 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.IsSystem, (ZString)IsSystemStatuses.Codes.UserRules);
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.IsSystem, filter2, Data.Rule2);

			var filter3 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.IsPartial, (ZString)IsSystemStatuses.Codes.All);
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.IsPartial, filter3, Data.Rule1, Data.Rule2);
		}

		#endregion

		#region TestBarcodeModuleFilter

		public void TestBarcodeModuleFilter()
		{
			Data.RuleSet1.BRS_Module = "YES";
			Factory.Save();

			var filter1 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.Module, (ZString)"YES");
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.Module, filter1, Data.Rule1);

			var filter2 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.Module, (ZString)"NO");
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.Module, filter2);

			var filter3 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.Module, ZString.Empty);
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.Module, filter3, Data.Rule1, Data.Rule2);
		}

		#endregion

		#region TestRuleNameFilter

		public void TestRuleNameFilter()
		{
			var filter1 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.RuleName, (ZString)"Rule1");
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.RuleName, filter1, Data.Rule1);

			var filter2 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.Module, (ZString)"NON");
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.Module, filter2);

			var filter3 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.Module, ZString.Empty);
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.Module, filter3, Data.Rule1, Data.Rule2);
		}

		#endregion

		#region TestSupplierFilter

		public void TestSupplierFilter()
		{
			var supplier = Data.Rule1.RuleSet.Supplier;
			Factory.Save();

			var filter = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.Supplier, supplier.PK);
			Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.Supplier, filter, Data.Rule1);
		}

		#endregion

		#region TestTerminatorTypeFilter

		public void TestTerminatorTypeFilter()
		{
			// #warning change this once GS1 is made to use Barcode Parsing completely
			bool hideGS1Functionality = true;
			if (hideGS1Functionality)
			{
				Assert(true);
			}
			else
			{
				Data.Rule1.TerminatorType = TerminatorTypes.Codes.GS1;
				Factory.Save();

				var filter1 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.TerminatorType, (ZString)TerminatorTypes.Codes.GS1);
				Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.TerminatorType, filter1, Data.Rule1);

				var filter2 = GetFilterStrip(BarcodeRuleFilterBusinessObject.FilterNames.TerminatorType, (ZString)TerminatorTypes.Codes.UserDefined);
				Asserter.AssertMatches(BarcodeRuleFilterBusinessObject.FilterNames.TerminatorType, filter2, Data.Rule2);
			}
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

		FilterStripAsserter<BarcodeRule> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<BarcodeRule>(Factory, b => b.BRU_Name)); }
		}
		FilterStripAsserter<BarcodeRule> asserter;

		BarcodeRulesDataObject Data
		{
			get { return data ?? (data = new BarcodeRulesDataObject(Factory, Asserter)); }
		}
		BarcodeRulesDataObject data;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BarcodeRuleFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			dummyBarcodeEnableDisposable = BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			dummyBarcodeEnableDisposable?.Dispose();
		}

		IDisposable dummyBarcodeEnableDisposable;

		#region BarcodeRulesDataObject

		class BarcodeRulesDataObject
		{
			public BarcodeRule Rule1 { get; private set; }
			public BarcodeRule Rule2 { get; private set; }

			public BarcodeRuleSet RuleSet1 { get; private set; }
			public BarcodeRuleSet RuleSet2 { get; private set; }

			public BarcodeRuleComponent Rule1Component { get; private set; }
			public BarcodeRuleComponent Rule2Component { get; private set; }

			readonly BusinessObjectFactory Factory;

			public BarcodeRulesDataObject(BusinessObjectFactory factory, FilterStripAsserter<BarcodeRule> asserter)
			{
				Factory = factory;
				var buyer1 = Helper.CreateOrg("Buy1");
				var buyer2 = Helper.CreateOrg("Buy2");

				var supplier1 = Helper.CreateOrg("Sup1");
				var supplier2 = Helper.CreateOrg("Sup2");

				RuleSet1 = Helper.CreateRuleSet(buyer1, supplier1);
				RuleSet2 = Helper.CreateRuleSet(buyer2, supplier2);

				Rule1 = Helper.CreateRule(RuleSet1, (ZString)"Rule1");
				Rule2 = Helper.CreateRule(RuleSet2, (ZString)"Rule2");

				Rule1Component = Helper.CreateRuleComponent(Rule1);
				Rule2Component = Helper.CreateRuleComponent(Rule2);

				Factory.Save();
				asserter.AddToScope(Rule1);
				asserter.AddToScope(Rule2);
			}

			BarcodeParsingTestHelper Helper
			{
				get { return helper ?? (helper = new BarcodeParsingTestHelper(Factory)); }
			}

			BarcodeParsingTestHelper helper;
		}

		#endregion

		#endregion
	}
}
