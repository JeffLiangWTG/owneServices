using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class GatewayProfitRedistributionApportionmentCriteriaTest : TestCaseWithFactory
	{
		public void TestImplementation()
		{
			AssertNoExceptionThrown(() => new GatewayProfitRedistributionApportionmentCriteria(Factory, null, null));

			var criteria = new GatewayProfitRedistributionApportionmentCriteria(Factory, null, null);
			AssertNull(criteria.ProfitShareRedistribution);
			AssertNull(criteria.ProfitApportionmentMethod);
			Assert(!criteria.ShareLosses);
			Assert(!criteria.IsChargeGroupProfitShared(null, null));

			var chargeCode = Factory.New<AccChargeCode>();
			Assert(!criteria.IsChargeGroupProfitShared(chargeCode, null));
			Assert(!criteria.IsChargeGroupProfitShared(null, new PaymentTermInfos()));
			Assert(!criteria.IsChargeGroupProfitShared(chargeCode, new PaymentTermInfos()));
		}

		public void TestIsChargeGroupProfitShared()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var paymentTerm = new PaymentTermInfos();

			var criteria = CreateCriteria(OrgProfitShareDetailsLookups.AgreementTypeCollectFreight);
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Assert(!criteria.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			paymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "FCA"));
			Assert(criteria.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			criteria = CreateCriteria(OrgProfitShareDetailsLookups.AgreementTypeUserDefined);
			Assert(!criteria.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			criteria = CreateCriteria(OrgProfitShareDetailsLookups.AgreementTypeUserDefined, new[] { chargeCode });
			Assert(criteria.IsChargeGroupProfitShared(chargeCode, paymentTerm));

			GatewayProfitRedistributionApportionmentCriteria CreateCriteria(string agreementType, AccChargeCode[] accChargeCodes = null)
			{
				return new GatewayProfitRedistributionApportionmentCriteria(Factory, null, null, agreementType, false, accChargeCodes);
			}
		}
	}
}
