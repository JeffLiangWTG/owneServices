using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using CusGuaranteeRule = Enterprise.Customs.Business.CusGuaranteeRule;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(GuaranteesFilterStripBusinessObject))]
	sealed class GuaranteesFilterStripBusinessObjectTest : Customs.Module.Testing.GuaranteesFilterStripBusinessObjectAbstractTest
	{
		public void TestLookups()
		{
			var filter = new GuaranteesFilterStripBusinessObject();
			AssertType<GuaranteesFilterLookups>(filter.Lookups);
		}

		public void TestGuaranteeRuleFilter()
		{
			var guaranteeHeader1 = AddGuaranteeHeader("G1");
			_ = AddGuaranteeRule(guaranteeHeader1, "TSP", "ES009999AAAAA");
			var guaranteeHeader2 = AddGuaranteeHeader("G2");
			_ = AddGuaranteeRule(guaranteeHeader2, "TSP", "XXXXXXXXXXXXX");
			_ = AddGuaranteeRule(guaranteeHeader2, "XXX", "ES009999AAAAA");
			Factory.Save();
			var filter = new GuaranteesFilterStripBusinessObject();
			var guaranteeRuleFilter = (CusAuthorisationsRuleModuleFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.GuaranteeRule];
			AssertNotNull(guaranteeRuleFilter);
			guaranteeRuleFilter.Property1 = "TSP";
			guaranteeRuleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			guaranteeRuleFilter.Property2 = "ES009999AAAAA";
			guaranteeRuleFilter.IsActive = true;
			var guaranteesCollection = new CusGuaranteeHeaderCollectionFiltered(Factory) { AdditionalFilter = filter.Filter };
			AssertEquals("Only guarantees with guarantee rule where CPR_RuleCode is 'TSP' AND CPR_ValueFrom is 'ES009999AAAAA' match filters.", 1, guaranteesCollection.Count);
			AssertEquals($"Only {nameof(guaranteeHeader1)} matches guarantee type filter.", guaranteeHeader1.PK, guaranteesCollection[0].PK);
		}

		CusGuaranteeHeader AddGuaranteeHeader(ZString guaranteeNumber)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = guaranteeNumber;
			return guaranteeHeader;
		}

		static CusGuaranteeRule AddGuaranteeRule(BaseCusGuaranteeHeader guaranteeHeader, ZString code, ZString valueFrom)
		{
			var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.CPR_RuleCode = code;
			guaranteeRule.CPR_ValueFrom = valueFrom;
			return guaranteeRule;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => GetEUGuaranteesFilterStripBusinessObject();

		public GuaranteesFilterStripBusinessObject GetEUGuaranteesFilterStripBusinessObject() => new ();
	}
}
