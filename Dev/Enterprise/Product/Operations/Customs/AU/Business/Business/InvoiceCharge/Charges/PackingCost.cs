namespace Enterprise.Customs.AU.Declaration.Business
{
	partial class EdificeIncoTermAndCustomsChargeFactory
	{
		public static Common.CustomsChargeCode EdificePackingCost => new Common.CustomsChargeCode(AUChargeCodeList.Codes.PackingCost, CustomsChargeTypeList.Descriptions.PackingCost)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true
		};
	}
}
