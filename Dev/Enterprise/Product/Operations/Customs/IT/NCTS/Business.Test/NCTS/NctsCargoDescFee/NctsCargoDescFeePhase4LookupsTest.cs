using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsCargoDescFeePhase4LookupsTest : BusinessObjectLookupsTestCase
{
	public void TestChargeTypeList()
	{
		var fee = Factory.New<NctsCargoDescFee>();
		var chargeTypeList = fee.Lookups.ChargeTypeList;
		AssertNotNull("Not null", chargeTypeList);
		AssertType<NctsCargoDescFeeChargeTypeList>("Type", chargeTypeList);
		AssertEquals("CodesAsString", "149", chargeTypeList.CodesAsString);
	}

	public void TestRateOverrideReasonList()
	{
		var fee = Factory.New<NctsCargoDescFee>();
		var rateOverrideReasonList = fee.ITLookups.RateOverrideReasonList;
		AssertNotNull("Not null", rateOverrideReasonList);
		AssertType<RateOverrideReasonList>("Type", rateOverrideReasonList);
		AssertEquals("CodesAsString", "ADD, OVR", rateOverrideReasonList.CodesAsString);
	}

	public void TestMethodOfPaymentList()
	{
		SetUpReferenceData();
		Factory.Save();

		var fee = Factory.New<NctsCargoDescFee>();
		var methodOfPaymentList = fee.ITLookups.MethodOfPaymentList;
		AssertNotNull("Not null", methodOfPaymentList);
		AssertEquals("CodesAsString", "A, B, C, D, E, F, G, H, J, K", methodOfPaymentList.CodesAsString);
	}

	public void TestMethodOfCalculationList()
	{
		var fee = Factory.New<NctsCargoDescFee>();
		var methodOfCalculation = fee.ITLookups.MethodOfCalculationList;
		AssertNotNull("MethodOfCalculationList", methodOfCalculation);
		AssertEquals("MethodOfCalculationList CodeAsString", "X, TNE", methodOfCalculation.CodesAsString);
	}

	void SetUpReferenceData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");
		SetUpMethodOfPayments("A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "M", "O", "P", "R", "T", "U", "V");

		void SetUpMethodOfPayments(params ZString[] codeList)
		{
			foreach (var item in codeList)
			{
				var cusCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, item, item, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode.PK, RefCusCodeListAttributeTypes.Codes.Category, UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			}
		}
	}
}
