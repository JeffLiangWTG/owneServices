using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiOrgOpportunityValueAnalysisCollection))]
	public class EdiOrgOpportunityValueAnalysisCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiOrgOpportunityValueAnalysisCollection>
	{
		public void TestPopulateDefaultValues()
		{
			EDIDataRegistry.Instance.OpportunityExchangeRateCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GlbCompany.CurrentCompany.PK.ToGuid());
			var uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var usdExchange = uSD.ExchangeRates.AddNew();
			usdExchange.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			usdExchange.RE_SellRate = 2;
			usdExchange.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			usdExchange.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
			Factory.Save();

			var regValue = new OpportunityValueAnalysisDefaultCollection();
			var value1 = regValue.AddNew();
			value1.Code = "C01";
			value1.Description = "DESC1";
			value1.ValueInUSD = 3m;
			EDIDataRegistry.Instance.OpportunityValueAnalysisDefaultRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);

			var opp = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var collection = opp.ValueAnalysisCollection;
			collection.PopulateDefaultValues();

			AssertEquals(1, collection.Count);
			AssertEquals(value1.Code, collection[0].EOV_ModuleCode);
			AssertEquals(value1.Description, collection[0].EOV_Calc_ModuleDescription);
			AssertEquals(0, collection[0].EOV_UserCount);
			AssertEquals(0m, collection.TotalForeignValue);

			collection[0].EOV_UserCount = 100;
			AssertEquals(300m, collection.TotalForeignValue);
			AssertEquals(150m, collection.TotalLocalValue);

			var value2 = regValue.AddNew();
			value2.Code = "D01";
			value2.Description = "DESC2";
			value2.ValueInUSD = 5m;
			EDIDataRegistry.Instance.OpportunityValueAnalysisDefaultRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);

			collection.PopulateDefaultValues();

			AssertEquals(2, collection.Count);
			AssertEquals(value1.Code, collection[0].EOV_ModuleCode);
			AssertEquals(value1.Description, collection[0].EOV_Calc_ModuleDescription);
			AssertEquals(100, collection[0].EOV_UserCount);

			AssertEquals(value2.Code, collection[1].EOV_ModuleCode);
			AssertEquals(value2.Description, collection[1].EOV_Calc_ModuleDescription);
			AssertEquals(0, collection[1].EOV_UserCount);

			collection[1].EOV_UserCount = 200;
			AssertEquals(1300m, collection.TotalForeignValue);
			AssertEquals(650m, collection.TotalLocalValue);
		}

		protected override EdiOrgOpportunityValueAnalysisCollection GetCollectionToTest()
		{
			var opp = Factory.New<EDIOrgOpportunity>();
			var collection = opp.ValueAnalysisCollection;
			return collection;
		}
	}
}
