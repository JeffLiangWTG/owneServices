using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	partial class PenaltyExemptionReasonCodeList
	{
		public static bool IsLegalReasonCode(string code)
		{
			return code == Codes.A3
				|| code == Codes.A4
				|| code == Codes.A6
				|| code == Codes.B5
				|| code == Codes.B7
				|| code == Codes.B9;
		}

		public static CodeDescriptionPairList GetPenaltyExemption5UAOnlyCodeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Codes.A3, Descriptions.A3);
			list.AddPair(Codes.A4, Descriptions.A4);
			list.AddPair(Codes.A6, Descriptions.A6);
			list.AddPair(Codes.B5, Descriptions.B5);
			list.AddPair(Codes.B7, Descriptions.B7);
			list.AddPair(Codes.B9, Descriptions.B9);

			return list;
		}
	}
}
