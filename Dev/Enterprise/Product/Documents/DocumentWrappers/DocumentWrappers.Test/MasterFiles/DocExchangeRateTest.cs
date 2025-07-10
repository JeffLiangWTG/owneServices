using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocExchangeRate))]
	public class DocExchangeRateTest : DocumentWrapperTestCase
	{
		public void TestBuyRate()
		{
			AssertEquals(0m, Wrapper.BuyRate);

			Rate.JF_BaseRate = 0.8243m;
			AssertEquals(0.8243m, Wrapper.BuyRate);
		}

		public void TestLocalClientSellRate()
		{
			AssertEquals(0m, Wrapper.LocalClientSellRate);

			Rate.JF_BaseRate = 0.8243m;
			AssertEquals(0.8243m, Wrapper.LocalClientSellRate);
		}

		public void TestAgentSellRate()
		{
			AssertEquals(0m, Wrapper.AgentsSellRate);

			Rate.JF_BaseRate = 0.8243m;
			AssertEquals(0.8243m, Wrapper.AgentsSellRate);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocExchangeRate[] { Wrapper };
		}

		Job Header
		{
			get { return header ?? (header = Factory.NewJobForTesting<Job>()); }
		}
		Job header;

		ExchangeRate Rate
		{
			get { return rate ?? (rate = Header.ExchangeRates.AddNew()); }
		}
		ExchangeRate rate;

		DocExchangeRate Wrapper
		{
			get { return DocExchangeRate.New(Rate, Factory); }
		}

		#endregion
	}
}
