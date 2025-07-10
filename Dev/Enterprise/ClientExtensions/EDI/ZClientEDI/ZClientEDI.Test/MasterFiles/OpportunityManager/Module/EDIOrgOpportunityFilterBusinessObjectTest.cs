using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDIOrgOpportunityFilterBusinessObject))]
	public class EDIOrgOpportunityFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestValueTypes()
		{
			EDIOrgOpportunityFilterBusinessObject filterBizo = new EDIOrgOpportunityFilterBusinessObject();
			EDIOrgOpportunityValue oppValue = Factory.NewWithValidTestData<EDIOrgOpportunityValue>();
			foreach (CodeDescriptionPair pair in oppValue.Lookups.ValueTypes)
			{
				AssertEquals(pair.Description, filterBizo.ValueTypes.GetDescriptionFromCode(pair.Code));
			}
		}

		public void TestGlobalReachFilter()
		{
			var opp1 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opp1.P8_OpportunityID = "O00000011";
			opp1.OrgOpportunityEx.EOM_GlobalPotential = 0;
			var opp2 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opp2.P8_OpportunityID = "O00000022";
			opp2.OrgOpportunityEx.EOM_GlobalPotential = 5;
			var opp3 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opp3.P8_OpportunityID = "O00000033";
			opp3.OrgOpportunityEx.EOM_GlobalPotential = 10;
			var opp4 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp4.P8_OpportunityID = "O00000044";
			Factory.Save();
			var opps = new OpportunityManager.Business.EDIOrgOpportunityCollection(Factory);
			var filterBizObj = new EDIOrgOpportunityFilterBusinessObject();
			AssertEquals(typeof(ModuleNumberRangeFilter), filterBizObj["Global Reach"].GetType());
			var globalReachFilter = (ModuleNumberRangeFilter)filterBizObj["Global Reach"];
			AssertEquals("Global Reach", globalReachFilter.MultilingualDescription.ToString());
			AssertEquals((byte)0, globalReachFilter.Decimals);
			globalReachFilter.IsActive = true;
			globalReachFilter.Property1 = 5;
			globalReachFilter.Property2 = 15;
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { opp2.P8_OpportunityID, opp3.P8_OpportunityID }, opps.Select(x => x.P8_OpportunityID));
			globalReachFilter.Property1 = ZDecimal.Zero;
			globalReachFilter.Property2 = 5;
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { opp1.P8_OpportunityID, opp2.P8_OpportunityID, opp4.P8_OpportunityID }, opps.Select(x => x.P8_OpportunityID));
			globalReachFilter.Property1 = ZDecimal.Zero;
			globalReachFilter.Property2 = ZDecimal.Zero;
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { opp1.P8_OpportunityID, opp4.P8_OpportunityID }, opps.Select(x => x.P8_OpportunityID));
			globalReachFilter.Property1 = 5;
			globalReachFilter.Property2 = 5;
			opps.Load(filterBizObj.Filter);
			AssertEquals(1, opps.Count);
			AssertEquals(opp2.P8_OpportunityID, opps[0].P8_OpportunityID);
		}

		public void TestLifetimeFilter()
		{
			var opp1 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opp1.P8_OpportunityID = "O00000011";
			opp1.OrgOpportunityEx.EOM_LifetimeValueOver3Years = 0;
			var opp2 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opp2.P8_OpportunityID = "O00000022";
			opp2.OrgOpportunityEx.EOM_LifetimeValueOver3Years = 5;
			var opp3 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opp3.P8_OpportunityID = "O00000033";
			opp3.OrgOpportunityEx.EOM_LifetimeValueOver3Years = 10;
			var opp4 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp4.P8_OpportunityID = "O00000044";
			Factory.Save();
			var opps = new OpportunityManager.Business.EDIOrgOpportunityCollection(Factory);
			var filterBizObj = new EDIOrgOpportunityFilterBusinessObject();
			AssertEquals(typeof(ModuleNumberRangeFilter), filterBizObj["Lifetime (3 CLV)"].GetType());
			var lifetimeFilter = (ModuleNumberRangeFilter)filterBizObj["Lifetime (3 CLV)"];
			AssertEquals("Lifetime (3 CLV)", lifetimeFilter.MultilingualDescription.ToString());
			lifetimeFilter.IsActive = true;
			lifetimeFilter.Property1 = 5;
			lifetimeFilter.Property2 = 15;
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { opp2.P8_OpportunityID, opp3.P8_OpportunityID }, opps.Select(x => x.P8_OpportunityID));
			lifetimeFilter.Property1 = ZDecimal.Zero;
			lifetimeFilter.Property2 = 5;
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { opp1.P8_OpportunityID, opp2.P8_OpportunityID, opp4.P8_OpportunityID }, opps.Select(x => x.P8_OpportunityID));
			lifetimeFilter.Property1 = ZDecimal.Zero;
			lifetimeFilter.Property2 = ZDecimal.Zero;
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { opp1.P8_OpportunityID, opp4.P8_OpportunityID }, opps.Select(x => x.P8_OpportunityID));
			lifetimeFilter.Property1 = 5;
			lifetimeFilter.Property2 = 5;
			opps.Load(filterBizObj.Filter);
			AssertEquals(1, opps.Count);
			AssertEquals(opp2.P8_OpportunityID, opps[0].P8_OpportunityID);
		}

		public void TestContractCurrencyFilter()
		{
			var opp1 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opp1.P8_OpportunityID = "O00000011";
			opp1.P8_RX_NKEstimatedValueCurrency = "USD";
			var opp2 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opp2.P8_OpportunityID = "O00000022";
			opp2.P8_RX_NKEstimatedValueCurrency = "AUD";
			var opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_OpportunityID = "O00000033";
			Factory.Save();
			var opps = new OpportunityManager.Business.EDIOrgOpportunityCollection(Factory);
			var filterBizObj = new EDIOrgOpportunityFilterBusinessObject();
			AssertEquals(typeof(ModuleNkFilter), filterBizObj["Contract Currency"].GetType());
			var currencyFilter = (ModuleNkFilter)filterBizObj["Contract Currency"];
			AssertEquals("Contract Currency", currencyFilter.MultilingualDescription.ToString());
			currencyFilter.IsActive = true;
			currencyFilter.Property = "";
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { opp1.P8_OpportunityID, opp2.P8_OpportunityID, opp3.P8_OpportunityID }, opps.Select(x => x.P8_OpportunityID));
			currencyFilter.Property = "USD";
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { opp1.P8_OpportunityID, opp3.P8_OpportunityID }, opps.Select(x => x.P8_OpportunityID));
			currencyFilter.Property = "AUD";
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { opp2.P8_OpportunityID }, opps.Select(x => x.P8_OpportunityID));
		}

		public void TestEDISpecificFilterMultilingualDescriptionsShouldChange()
		{
			var opp1 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opp1.P8_OpportunityID = "O00000011";
			Factory.Save();
			var opps = new OpportunityManager.Business.EDIOrgOpportunityCollection(Factory);
			var filterBizObj = new EDIOrgOpportunityFilterBusinessObject();
			AssertEquals(typeof(ModuleNumberRangeFilter), filterBizObj["Current"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterBizObj["Potential"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterBizObj["Total Estimated Value (p.a)"].GetType());
			var localReachFilter = (ModuleNumberRangeFilter)filterBizObj["Potential"];
			AssertEquals("Local Reach", localReachFilter.MultilingualDescription.ToString());
			AssertEquals((byte)0, localReachFilter.Decimals);
			var contractedFilter = (ModuleNumberRangeFilter)filterBizObj["Current"];
			AssertEquals("Contracted", contractedFilter.MultilingualDescription.ToString());
			AssertEquals((byte)0, contractedFilter.Decimals);
			var contractFilter = (ModuleNumberRangeFilter)filterBizObj["Total Estimated Value (p.a)"];
			AssertEquals("Contract (p.a)", contractFilter.MultilingualDescription.ToString());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIOrgOpportunityFilterBusinessObject();
		}
	}
}
