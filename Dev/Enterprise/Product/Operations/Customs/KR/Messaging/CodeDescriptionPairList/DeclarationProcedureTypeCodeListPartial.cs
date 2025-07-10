namespace Enterprise.Customs.KR.Messaging
{
	partial class DeclarationProcedureTypeCodeList
	{
		public static bool IsBondedFactoryProcedure(string code)
		{
			return code == Codes._12
				|| code == Codes._14
				|| code == Codes._27
				|| code == Codes._31;
		}

		public static bool IsDutyReductionOrInstallmentNotApplicable(string code)
		{
			return code == Codes._12
				|| code == Codes._14
				|| code == Codes._18
				|| code == Codes._27
				|| code == Codes._30
				|| code == Codes._31
				|| code == Codes._33
				|| code == Codes._35;
		}

		public static bool IsCheckDigitM(string code)
		{
			return code == Codes._12
				|| code == Codes._14
				|| code == Codes._18
				|| code == Codes._24
				|| code == Codes._27
				|| code == Codes._30
				|| code == Codes._31
				|| code == Codes._33
				|| code == Codes._35;
		}

		public static bool DoesRequireVesselOrFlightInformation(string code)
		{
			return code != Codes._13 &&
						 code != Codes._29 &&
						 code != Codes._36;
		}

		public static bool IsImportFromBondedAreaInKR(string code)
		{
			return code == Codes._13 ||
						 code == Codes._15 ||
						 code == Codes._28 ||
						 code == Codes._29 ||
						 code == Codes._33 ||
						 code == Codes._36;
		}

		public static bool IsApplicableForPaymentPostClearance(string code)
		{
			return code == Codes._11 ||
						 code == Codes._13 ||
						 code == Codes._15 ||
						 code == Codes._16 ||
						 code == Codes._29 ||
						 code == Codes._36;
		}

		public static bool IsApplicableForDefaultSupplierID(string code)
		{
			return code == Codes._13 ||
						 code == Codes._15 ||
						 code == Codes._18 ||
						 code == Codes._26 ||
						 code == Codes._28 ||
						 code == Codes._29 ||
						 code == Codes._32 ||
						 code == Codes._33 ||
						 code == Codes._34;
		}

		public static bool MustHavePaymentMethod00(string code)
		{
			return code == Codes._12
				|| code == Codes._14
				|| code == Codes._18
				|| code == Codes._20
				|| code == Codes._27
				|| code == Codes._30
				|| code == Codes._31
				|| code == Codes._33
				|| code == Codes._35
				|| code == Codes._37;
		}

		public static bool MustNotHavePaymentMethod00(string code)
		{
			return code == Codes._13
				|| code == Codes._15
				|| code == Codes._16
				|| code == Codes._17
				|| code == Codes._21
				|| code == Codes._26
				|| code == Codes._28
				|| code == Codes._36;
		}

		public static bool MustHavePaymentMethod01(string code)
		{
			return code == Codes._22 || code == Codes._23;
		}

		public static string ImpEntryNumberCheckDigit(string code, string tradeType)
		{
			string result = string.Empty;
			switch (code)
			{
				case Codes._12:
				case Codes._27:
				case Codes._31:
					result = Constants.EntryNumberCheckDigit.B;
					break;
				case Codes._18:
				case Codes._24:
				case Codes._30:
				case Codes._33:
					result = Constants.EntryNumberCheckDigit.S;
					break;
				case Codes._14:
				case Codes._35:
				case Codes._37:
					result = Constants.EntryNumberCheckDigit.F;
					break;
				case Codes._39:
					if (tradeType == ImportDealingTypeCodeList.Codes._69)
					{
						result = Constants.EntryNumberCheckDigit.H;
					}
					else
					{
						result = Constants.EntryNumberCheckDigit.M;
					}
					break;
				default:
					result = Constants.EntryNumberCheckDigit.M;
					break;
			}
			return result;
		}
	}
}
