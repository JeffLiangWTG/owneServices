using System;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiOrgOpportunityValueAnalysis))]
	public class EdiOrgOpportunityValueAnalysisTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var opp = Factory.New<EDIOrgOpportunity>();
			var valueAnalysis = opp.ValueAnalysisCollection[0];
			AssertEquals("USD", valueAnalysis.EOV_Calc_ForeignValueCurrency);
			AssertEquals("AUD", valueAnalysis.EOV_Calc_LocalValueCurrency);
		}

		public void TestProperties()
		{
			EDIDataRegistry.Instance.OpportunityExchangeRateCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GlbCompany.CurrentCompany.PK.ToGuid());
			var uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var usdExchange = uSD.ExchangeRates.AddNew();
			usdExchange.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			usdExchange.RE_SellRate = 2m;
			usdExchange.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			usdExchange.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
			Factory.Save();

			var regValues = new OpportunityValueAnalysisDefaultCollection();
			var reg1 = regValues.AddNew();
			reg1.Code = "C01";
			reg1.ValueInUSD = 100m;
			reg1.Description = "Code 01";

			var reg2 = regValues.AddNew();
			reg2.Code = "C02";
			reg2.ValueInUSD = 200m;
			reg2.Description = "Code 02";

			EDIDataRegistry.Instance.OpportunityValueAnalysisDefaultRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValues);

			var opp = Factory.New<EDIOrgOpportunity>();
			AssertEquals(2, opp.ValueAnalysisCollection.Count);

			var analysis1 = opp.ValueAnalysisCollection[0];
			var analysis2 = opp.ValueAnalysisCollection[1];

			AssertEquals("C01", analysis1.EOV_ModuleCode);
			AssertEquals("Code 01", analysis1.EOV_Calc_ModuleDescription);
			AssertEquals(0, analysis1.EOV_UserCount);
			analysis1.EOV_UserCount = 20;
			AssertEquals(100m * 20m, analysis1.EOV_Calc_ForeignValue);
			AssertEquals((100m * 20m) / 2m, analysis1.EOV_Calc_LocalValue);

			AssertEquals("C02", analysis2.EOV_ModuleCode);
			AssertEquals("Code 02", analysis2.EOV_Calc_ModuleDescription);
			AssertEquals(0, analysis2.EOV_UserCount);
			analysis2.EOV_UserCount = 50;
			AssertEquals(200m * 50m, analysis2.EOV_Calc_ForeignValue);
			AssertEquals((200m * 50m) / 2m, analysis2.EOV_Calc_LocalValue);
		}
	}
}
