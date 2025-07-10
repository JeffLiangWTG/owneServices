using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	class GoodsMeasureProviderTest : DataProviderTestCase<GoodsMeasureProvider>
	{
		public void TestIGoodsMeasure()
		{
			Assert("Should implement IGoodsMeasure", Provider is IGoodsMeasure);
		}

		public void TestGrossMass()
		{
			SetUpTestData();
			invoiceLine.JI_Weight = 12;
			invoiceLine.JI_WeightUQ = "KG";
			AssertEquals("GrossMass", 12m, Provider.GrossMass);
		}

		public void TestNetMass()
		{
			SetUpTestData();
			invoiceLine.JI_NetWeight = 12;
			invoiceLine.JI_NetWeightUQ = "KG";
			AssertEquals("NetMass", 12m, Provider.NetMass);
		}

		public void TestSupplementaryUnits()
		{
			SetUpTestData();
			invoiceLine.JI_CustomsSecondQuantity = 12;
			AssertEquals("SupplementaryUnits", 12m, Provider.SupplementaryUnits);
		}

		protected override GoodsMeasureProvider GetProvider()
		{
			SetUpTestData();
			return new GoodsMeasureProvider(entryLineWrapper.EntryLine);
		}

		void SetUpTestData()
		{
			if (entryLineWrapper == null)
			{
				var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
				entryLineWrapper = testBizObjs.entryLineWrapper;
				entryLineWrapper.Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				invoiceLine = entryLineWrapper.RandomInvoiceLine;
			}
		}

		EntryLineWrapper entryLineWrapper;
		JobComInvoiceLine invoiceLine;
	}
}
