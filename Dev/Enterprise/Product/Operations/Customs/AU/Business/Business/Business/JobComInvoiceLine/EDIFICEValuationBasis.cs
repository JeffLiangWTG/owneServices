using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class EDIFICEValuationBasis
	{
		public static CodeDescriptionPairList Line_List
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.CustomType);

				list.AddPair(Line_Codes.UnrelatedTransation, "Unrelated transaction");
				list.AddPair(Line_Codes.RelatedTransaction, "Related transaction");
				list.AddPair(Line_Codes.UnrelatedRoyaltyTransaction, "Unrelated royalty transaction");
				list.AddPair(Line_Codes.RelatedRoyaltyTransaction, "Related royalty transaction");
				list.AddPair(Line_Codes.ComputedValuePercUplift, "Computed value with percentage uplift");

				return list;
			}
		}

		public static class Line_Codes
		{
			public const string UnrelatedTransation = "UT";
			public const string RelatedTransaction = "RT";
			public const string UnrelatedRoyaltyTransaction = "XUT";
			public const string RelatedRoyaltyTransaction = "XRT";
			public const string ComputedValuePercUplift = "CT";
		}
	}
}
