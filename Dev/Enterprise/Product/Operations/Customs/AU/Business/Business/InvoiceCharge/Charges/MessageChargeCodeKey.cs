using System.Collections;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class MessageChargeCodeKey
	{
		public MessageChargeCodeKey()
		{
			messageChargeKeyTable = new Hashtable();
			PopulateChargeCodeKeyTable();
		}

		public const string DutiableBuyingCommissionExcluded = "DutiableBuyingCommissionExcluded";
		public const string NonDutiableBuyingCommissionIncluded = "NonDutiableBuyingCommissionIncluded";
		public const string DutiableOtherCommissionExcluded = "DutiableOtherCommissionExcluded";
		public const string NonDutiableOtherCommissionIncluded = "NonDutiableOtherCommissionIncluded";
		public const string DutiableCommissionExcluded = "DutiableCommissionExcluded";
		public const string NonDutiableCommissionIncluded = "NonDutiableCommissionIncluded";
		public const string PackingCostExcluded = "PackingCostExcluded";
		public const string DutyGSTExWorksAmountExcluded = "DutyGSTExWorksAmountExcluded";
		public const string NonDutyGSTExWorksAmountIncluded = "NonDutyGSTExWorksAmountIncluded";
		public const string LandingChargesIncluded = "LandingChargesIncluded";
		public const string DutyGSTOtherChargesExcluded = "DutyGSTOtherChargesExcluded";
		public const string NonDutyGSTOtherChargesIncluded = "NonDutyGSTOtherChargesIncluded";
		public const string NonDutyNonGSTOtherChargesIncluded = "NonDutyNonGSTOtherChargesIncluded";
		public const string DutyGSTAdditionChargeExcluded = "DutyGSTAdditionChargeExcluded";
		public const string NonDutyGSTDeductionChargeIncluded = "NonDutyGSTDeductionChargeIncluded";
		public const string NonDutyNonGSTDeductionChargeIncluded = "NonDutyNonGSTDeductionChargeIncluded";
		public const string DutyGSTForeignInlandFreightExcluded = "DutyGSTForeignInlandFreightExcluded";
		public const string NonDutyGSTForeignInlandFreightIncluded = "NonDutyGSTForeignInlandFreightIncluded";
		public const string NonDutyNonGSTDiscountExcluded = "NonDutyDiscountExcluded";

		public MessageChargeKey this[string key] => messageChargeKeyTable[key] as MessageChargeKey;

		readonly Hashtable messageChargeKeyTable;
		void PopulateChargeCodeKeyTable()
		{
			messageChargeKeyTable[DutiableBuyingCommissionExcluded] = new MessageChargeKey(AUChargeCodeList.Codes.BuyingCommission, true, true, false);
			messageChargeKeyTable[NonDutiableBuyingCommissionIncluded] = new MessageChargeKey(AUChargeCodeList.Codes.BuyingCommission, false, false, true);
			messageChargeKeyTable[DutiableOtherCommissionExcluded] = new MessageChargeKey(AUChargeCodeList.Codes.OtherCommission, true, true, false);
			messageChargeKeyTable[NonDutiableOtherCommissionIncluded] = new MessageChargeKey(AUChargeCodeList.Codes.OtherCommission, false, false, true);
			messageChargeKeyTable[DutiableCommissionExcluded] = new MessageChargeKey(CustomsChargeTypeList.Codes.Commission, true, true, false);
			messageChargeKeyTable[NonDutiableCommissionIncluded] = new MessageChargeKey(CustomsChargeTypeList.Codes.Commission, false, false, true);

			messageChargeKeyTable[PackingCostExcluded] = new MessageChargeKey(AUChargeCodeList.Codes.PackingCost, true, true, false);
			messageChargeKeyTable[DutyGSTExWorksAmountExcluded] = new MessageChargeKey(AUChargeCodeList.Codes.ExWorks, true, true, false);
			messageChargeKeyTable[NonDutyGSTExWorksAmountIncluded] = new MessageChargeKey(AUChargeCodeList.Codes.ExWorks, false, true, true);
			messageChargeKeyTable[LandingChargesIncluded] = new MessageChargeKey(AUChargeCodeList.Codes.LandingCharges, false, false, true);

			messageChargeKeyTable[DutyGSTOtherChargesExcluded] = new MessageChargeKey(AUChargeCodeList.Codes.OtherCharges, true, true, false);
			messageChargeKeyTable[NonDutyGSTOtherChargesIncluded] = new MessageChargeKey(AUChargeCodeList.Codes.OtherCharges, false, true, true);
			messageChargeKeyTable[NonDutyNonGSTOtherChargesIncluded] = new MessageChargeKey(AUChargeCodeList.Codes.OtherCharges, false, false, true);
			messageChargeKeyTable[DutyGSTAdditionChargeExcluded] = new MessageChargeKey(AUChargeCodeList.Codes.AdditionCharge, true, true, false);

			messageChargeKeyTable[NonDutyGSTDeductionChargeIncluded] = new MessageChargeKey(AUChargeCodeList.Codes.DeductionCharge, false, true, true);
			messageChargeKeyTable[NonDutyNonGSTDeductionChargeIncluded] = new MessageChargeKey(AUChargeCodeList.Codes.DeductionCharge, false, false, true);

			messageChargeKeyTable[DutyGSTForeignInlandFreightExcluded] = new MessageChargeKey(AUChargeCodeList.Codes.ForeignInlandFreight, true, true, false);
			messageChargeKeyTable[NonDutyGSTForeignInlandFreightIncluded] = new MessageChargeKey(AUChargeCodeList.Codes.ForeignInlandFreight, false, true, true);

			messageChargeKeyTable[NonDutyNonGSTDiscountExcluded] = new MessageChargeKey(AUChargeCodeList.Codes.Discount, false, false, false);
		}
	}
}
