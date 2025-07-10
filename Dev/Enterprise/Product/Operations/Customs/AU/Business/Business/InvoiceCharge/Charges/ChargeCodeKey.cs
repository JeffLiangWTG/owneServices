using System.Collections;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class ChargeCodeKey
	{
		public ChargeCodeKey()
		{
			chargeCodeKeyTable = new Hashtable();
			PopulateChargeCodeKeyTable();
		}

		public const string DutiableBuyingCommission = "DutiableBuyingCommission";
		public const string NonDutiableBuyingCommission = "NonDutiableBuyingCommission";
		public const string DutiableOtherCommission = "DutiableOtherCommission";
		public const string NonDutiableOtherCommission = "NonDutiableOtherCommission";
		public const string DutiableCommission = "DutiableCommission";
		public const string NonDutiableCommission = "NonDutiableCommission";
		public const string PackingCost = "PackingCost";
		public const string DutyGSTExWorksAmount = "DutyGSTExWorksAmount";
		public const string NonDutyGSTExWorksAmount = "NonDutyGSTExWorksAmount";
		public const string LandingCharges = "LandingCharges";
		public const string DutyGSTOtherCharges = "DutyGSTOtherCharges";
		public const string NonDutyGSTOtherCharges = "NonDutyGSTOtherCharges";
		public const string NonDutyNonGSTOtherCharges = "NonDutyNonGSTOtherCharges";
		public const string DutyGSTAdditionCharge = "DutyGSTAdditionCharge";
		public const string NonDutyGSTDeductionCharge = "NonDutyGSTDeductionCharge";
		public const string NonDutyNonGSTDeductionCharge = "NonDutyNonGSTDeductionCharge";
		public const string DutyGSTForeignInlandFreight = "DutyGSTForeignInlandFreight";
		public const string NonDutyGSTFIFT = "NonDutyGSTPreFOBFIFT";
		public const string OverseasFreight = "OverseasFreight";
		public const string OverseasInsurance = "OverseasInsurance";

		public ChargeCodeChargeKey this[string key] => chargeCodeKeyTable[key] as ChargeCodeChargeKey;

		readonly Hashtable chargeCodeKeyTable;
		void PopulateChargeCodeKeyTable()
		{
			chargeCodeKeyTable[DutiableBuyingCommission] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.BuyingCommission, true, true);
			chargeCodeKeyTable[NonDutiableBuyingCommission] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.BuyingCommission, false, false);
			chargeCodeKeyTable[DutiableOtherCommission] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.OtherCommission, true, true);
			chargeCodeKeyTable[NonDutiableOtherCommission] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.OtherCommission, false, false);
			chargeCodeKeyTable[DutiableCommission] = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Commission, true, true);
			chargeCodeKeyTable[NonDutiableCommission] = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Commission, false, false);

			chargeCodeKeyTable[PackingCost] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.PackingCost, true, true);
			chargeCodeKeyTable[DutyGSTExWorksAmount] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.ExWorks, true, true);
			chargeCodeKeyTable[NonDutyGSTExWorksAmount] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.ExWorks, false, true);
			chargeCodeKeyTable[LandingCharges] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.LandingCharges, false, false);

			chargeCodeKeyTable[DutyGSTOtherCharges] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.OtherCharges, true, true);
			chargeCodeKeyTable[NonDutyGSTOtherCharges] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.OtherCharges, false, true);
			chargeCodeKeyTable[NonDutyNonGSTOtherCharges] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.OtherCharges, false, false);
			chargeCodeKeyTable[DutyGSTAdditionCharge] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.AdditionCharge, true, true);

			chargeCodeKeyTable[NonDutyGSTDeductionCharge] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.DeductionCharge, false, true);
			chargeCodeKeyTable[NonDutyNonGSTDeductionCharge] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.DeductionCharge, false, false);

			chargeCodeKeyTable[DutyGSTForeignInlandFreight] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.ForeignInlandFreight, true, true);
			chargeCodeKeyTable[NonDutyGSTFIFT] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.ForeignInlandFreight, false, true);

			chargeCodeKeyTable[OverseasFreight] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.OverseasFreight, false, true);
			chargeCodeKeyTable[OverseasInsurance] = new ChargeCodeChargeKey(AUChargeCodeList.Codes.OverseasInsurance, false, true);
		}
	}
}
