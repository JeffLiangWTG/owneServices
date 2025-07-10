using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiOrgOpportunityEx))]
	public class EdiOrgOpportunityExTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var opp = Factory.New<EDIOrgOpportunity>();
			AssertEquals("USD", opp.OrgOpportunityEx.EOM_RX_NKLifetimeValueCurrency);
			AssertEquals("AUD", opp.OrgOpportunityEx.EOM_Calc_LifetimeValueOver3YearsLocalCurrency);
		}

		public void TestEOM_Calc_LifetimeValueOver3YearsLocal()
		{
			EDIDataRegistry.Instance.OpportunityExchangeRateCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GlbCompany.CurrentCompany.PK.ToGuid());
			var uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var usdExchange = uSD.ExchangeRates.AddNew();
			usdExchange.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			usdExchange.RE_SellRate = 2;
			usdExchange.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			usdExchange.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
			Factory.Save();

			var opp = Factory.New<EDIOrgOpportunity>();
			opp.OrgOpportunityEx.EOM_LifetimeValueOver3Years = 100m;
			AssertEquals(50m, opp.OrgOpportunityEx.EOM_Calc_LifetimeValueOver3YearsLocal);
		}
	}
}
