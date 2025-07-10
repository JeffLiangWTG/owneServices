using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsCargoDescFee))]
	public class NctsCargoDescFeeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var fee = GetNewNctsCargoDescFee(Factory);
			AssertType<NctsCargoDescFeeValidation>(fee.Validation);
		}

		public void TestBFE_ChargeAmount_ReadOnly()
		{
			var fee = Factory.New<NctsCargoDescFee>();
			fee.BFE_RateOverrideReasonCode = ZString.Empty;
			Assert(fee.BFE_ChargeAmountInfo.ReadOnly);
			fee.BFE_RateOverrideReasonCode = "ADD";
			Assert(!fee.BFE_ChargeAmountInfo.ReadOnly);
		}

		public void TestBFE_RateOverrideReasonCode_Caption()
		{
			var fee = Factory.New<NctsCargoDescFee>();
			AssertEquals("Action", DataBoundResourceStrings.GetDataForProperty(fee.BFE_RateOverrideReasonCodeInfo).Caption);
		}

		public void TestLookups()
		{
			var fee = GetNewNctsCargoDescFee(Factory);
			AssertType<NctsCargoDescFeeLookups>(fee.Lookups);
		}

		public void TestChargeAmountRounder()
		{
			var fee = GetNewNctsCargoDescFee(Factory);
			AssertType<IntegerFeeRounder>(fee.ChargeAmountRounder);
		}

		public void TestChargeType()
		{
			var fee = GetNewNctsCargoDescFee(Factory);
			AssertEquals("Fee Code", fee.BFE_ChargeTypeInfo.HumanReadableName);
		}

		public void TestChargeAmount()
		{
			var fee = GetNewNctsCargoDescFee(Factory);
			AssertEquals("Amount", fee.BFE_ChargeAmountInfo.HumanReadableName);

			fee.BFE_ChargeAmount = 2.51m;
			AssertEquals(3m, fee.BFE_ChargeAmount);

			fee.BFE_ChargeAmount = 2.5m;
			AssertEquals(3m, fee.BFE_ChargeAmount);

			fee.BFE_ChargeAmount = 2.49m;
			AssertEquals(2m, fee.BFE_ChargeAmount);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewNctsCargoDescFee(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewNctsCargoDescFee(factory);

		NctsCargoDescFee GetNewNctsCargoDescFee(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = header.MovementHeader.GoodsItems.AddNew();
			return goodsItem.Fees.AddNew();
		}
	}
}
