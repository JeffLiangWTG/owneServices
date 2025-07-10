namespace Enterprise.Customs.BR.Business
{
	public partial class MessageSubTypeList
	{
		public static bool RequiresTransportDetails(string code)
		{
			return code switch
			{
				Codes._01 or Codes._02 or Codes._03 or Codes._04 or Codes._05 or Codes._06 or
				Codes._07 or Codes._08 or Codes._09 or Codes._10 or Codes._12 or Codes._13 or
				Codes._14 or Codes._15 => true,
				_ => false,
			};
		}

		public static bool RequiresDepartureDetails(string code)
		{
			return code switch
			{
				Codes._01 or Codes._02 or Codes._03 or Codes._04 or Codes._05 or Codes._06 or
				Codes._07 or Codes._08 or Codes._09 or Codes._10 or Codes._12 => true,
				_ => false,
			};
		}

		public static bool MultimodalIsAvailable(string code)
		{
			return code switch
			{
				Codes._13 or Codes._14 or Codes._15 or Codes._16 or Codes._17 or Codes._18 or
				Codes._20 or Codes._21 => false,
				_ => true,
			};
		}

		public static bool CargoProvenanceIsAvailable(string code)
		{
			return code switch
			{
				Codes._16 or Codes._17 or Codes._18 or Codes._20 or Codes._21 => true,
				_ => false,
			};
		}

		public static bool RequiresGoodsApplication(string code)
		{
			return code switch
			{
				Codes._02 or Codes._03 or Codes._04 or Codes._05 or Codes._06 or Codes._07 or
				Codes._08 or Codes._09 or Codes._10 => false,
				_ => true,
			};
		}

		public static bool RequiresDispatchModality(string code)
		{
			return code switch
			{
				Codes._01 or Codes._02 or Codes._03 or Codes._04 or Codes._05 or Codes._06 or
				Codes._07 or Codes._08 or Codes._09 or Codes._10 or Codes._11 or Codes._12 or
				Codes._22 or Codes._23 or Codes._24 or Codes._25 or Codes._26 or Codes._27 or
				Codes._28 => true,
				_ => false,
			};
		}
	}
}
