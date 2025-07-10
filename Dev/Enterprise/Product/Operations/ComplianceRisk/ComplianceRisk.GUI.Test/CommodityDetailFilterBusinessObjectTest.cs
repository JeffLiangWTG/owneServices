using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.GUI.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	[TestedType(typeof(CommodityDetailFilterBusinessObject))]
	public class CommodityDetailFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilterCommodityByRiskStatus()
		{
			var commodity1 = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodity1.CCD_RiskStatus = Codes.PotentialRisk;

			var commodity2 = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodity2.CCD_RiskStatus = Codes.Released;

			var filter = (ModuleTextFilter)FilterStripBizO["Risk Status"];
			filter.Property = Codes.PotentialRisk;
			filter.IsActive = true;

			AssertEquals("commodity1 should in query", true, commodity1.MatchesFilter(filter.Query));
			AssertEquals("commodity2 should not in query", false, commodity2.MatchesFilter(filter.Query));

			AssertContainsExactElementsInAnyOrder(new[]
			{
				Codes.NotChecked,
				Codes.Clear,
				Codes.PotentialRisk,
				Codes.Blocked,
				Codes.Released,
				Codes.PossibleRisk,
				Codes.HighRisk,
				Codes.NotAssessed,
			}, (filter.List as CodeDescriptionPairList).GetAllCodes());
		}

		public void TestFilterCommodityByHarmonizedCodes()
		{
			var commodity1 = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodity1.CCD_HarmonizedCode = "123456";

			var commodity2 = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodity2.CCD_HarmonizedCode = "112233";

			var filter = (ModuleTextFilter)FilterStripBizO["Harmonized Codes"];
			filter.Property = "123456";
			filter.IsActive = true;

			AssertEquals("commodity1 should in query", true, commodity1.MatchesFilter(filter.Query));
			AssertEquals("commodity2 should not in query", false, commodity2.MatchesFilter(filter.Query));
		}

		public void TestFilterCommodityByGoodsDescription()
		{
			var commodity1 = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodity1.CCD_Description = "test1";

			var commodity2 = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodity2.CCD_Description = "test2";

			var filter = (ModuleTextFilter)FilterStripBizO["Goods Description"];
			filter.Property = "test1";
			filter.IsActive = true;

			AssertEquals("commodity1 should in query", true, commodity1.MatchesFilter(filter.Query));
			AssertEquals("commodity2 should not in query", false, commodity2.MatchesFilter(filter.Query));
		}

		public void TestFilterCommodityByOriginOfGoods()
		{
			var commodity1 = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodity1.CCD_RN_NKOrigin = "AU";

			var commodity2 = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodity2.CCD_RN_NKOrigin = "US";

			var filter = (ModuleNkFilter)FilterStripBizO["Origin Of Goods"];
			filter.Property = "AU";
			filter.IsActive = true;

			AssertEquals("commodity1 should in query", true, commodity1.MatchesFilter(filter.Query));
			AssertEquals("commodity2 should not in query", false, commodity2.MatchesFilter(filter.Query));
		}

		#region Implementation

		FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = GetNewFilterStripBusinessObject();
				}
				return fFilterStripBizO;
			}
		}

		FilterStripBusinessObject fFilterStripBizO;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CommodityDetailFilterBusinessObject();
		}

		#endregion
	}
}
