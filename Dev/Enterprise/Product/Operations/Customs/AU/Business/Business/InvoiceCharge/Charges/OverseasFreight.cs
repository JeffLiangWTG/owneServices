namespace Enterprise.Customs.AU.Declaration.Business
{
	partial class EdificeIncoTermAndCustomsChargeFactory
	{
		public static Common.CustomsChargeCode EdificeOverseasFreight => new Common.CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Descriptions.OverseasFreight)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true
		};
	}
}
