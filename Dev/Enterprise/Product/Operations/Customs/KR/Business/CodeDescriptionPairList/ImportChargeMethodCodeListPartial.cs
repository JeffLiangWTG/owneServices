using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	partial class ImportChargeMethodCodeList
	{
		public static string[] GetAdditionalAmountList(ZString valuationCode)
		{
			var result = System.Array.Empty<string>();
			switch (valuationCode)
			{
				case ValuationCodeList.Codes.MethodOne:
					result = FormA_AdditionalTotalAmountList;
					break;
				case ValuationCodeList.Codes.MethodTwo:
				case ValuationCodeList.Codes.MethodThree:
					result = FormB_TwoOrThree_AdditionalTotalAmountList;
					break;
				case ValuationCodeList.Codes.MethodFive:
				case ValuationCodeList.Codes.MethodSix:
					result = new string[] { Codes.B502 };
					break;
			}
			return result;
		}

		public static string[] GetDeductionAmountList(ZString valuationCode)
		{
			var result = System.Array.Empty<string>();
			switch (valuationCode)
			{
				case ValuationCodeList.Codes.MethodOne:
					result = FormA_DeductionTotalAmountList;
					break;
				case ValuationCodeList.Codes.MethodTwo:
				case ValuationCodeList.Codes.MethodThree:
					result = FormB_TwoOrThree_DeductionTotalAmountList;
					break;
				case ValuationCodeList.Codes.MethodFourA:
				case ValuationCodeList.Codes.MethodFourB:
					result = FormB_FourAOrFourB_DeductionTotalAmountList;
					break;
			}
			return result;
		}

		public static string[] GetFreightList(ZString valuationCode)
		{
			var result = System.Array.Empty<string>();
			var stringCode = GetFreightCode(valuationCode);
			if (!string.IsNullOrEmpty(stringCode))
			{
				result = new string[] { stringCode };
			}
			return result;
		}

		public static string GetFreightCode(ZString valuationCode)
		{
			var result = string.Empty;
			switch (valuationCode)
			{
				case ValuationCodeList.Codes.MethodOne:
					result = Codes.A114;
					break;
				case ValuationCodeList.Codes.MethodTwo:
				case ValuationCodeList.Codes.MethodThree:
					result = Codes.B311;
					break;
				case ValuationCodeList.Codes.MethodFive:
				case ValuationCodeList.Codes.MethodSix:
					result = Codes.B501;
					break;
			}
			return result;
		}

		public static string[] GetInsuranceList(ZString valuationCode)
		{
			var result = System.Array.Empty<string>();
			var stringCode = GetInsuranceCode(valuationCode);
			if (!string.IsNullOrEmpty(stringCode))
			{
				result = new string[] { stringCode };
			}
			return result;
		}

		public static string GetInsuranceCode(ZString valuationCode)
		{
			var result = string.Empty;
			switch (valuationCode)
			{
				case ValuationCodeList.Codes.MethodOne:
					result = Codes.A116;
					break;
				case ValuationCodeList.Codes.MethodTwo:
				case ValuationCodeList.Codes.MethodThree:
					result = Codes.B313;
					break;
				case ValuationCodeList.Codes.MethodFive:
				case ValuationCodeList.Codes.MethodSix:
					result = Codes.B503;
					break;
			}
			return result;
		}

		public static CodeDescriptionPairList GetChargeMethodCodeListByValuationCode(string valuationCode)
		{
			switch (valuationCode)
			{
				case ValuationCodeList.Codes.MethodOne:
					return new ImportChargeMethodOneCodeList();
				case ValuationCodeList.Codes.MethodTwo:
				case ValuationCodeList.Codes.MethodThree:
					return new ImportChargeMethodTwoAndThreeCodeList();
				case ValuationCodeList.Codes.MethodFourA:
				case ValuationCodeList.Codes.MethodFourB:
					return new ImportChargeMethodFourCodeList();
				case ValuationCodeList.Codes.MethodFive:
				case ValuationCodeList.Codes.MethodSix:
					return new ImportChargeMethodFiveAndSixCodeList();
				default:
					return [];
			}
		}

		static string[] FormA_AdditionalTotalAmountList => new string[] { Codes.A104, Codes.A105, Codes.A106, Codes.A107, Codes.A108, Codes.A109, Codes.A110, Codes.A111, Codes.A112, Codes.A115 };
		static string[] FormA_DeductionTotalAmountList => new string[] { Codes.A118, Codes.A119, Codes.A120, Codes.A121 };
		static string[] FormB_TwoOrThree_AdditionalTotalAmountList => new string[] { Codes.B309, Codes.B310, Codes.B312 };
		static string[] FormB_TwoOrThree_DeductionTotalAmountList => new string[] { Codes.B303, Codes.B304, Codes.B305, Codes.B306, Codes.B307 };
		static string[] FormB_FourAOrFourB_DeductionTotalAmountList => new string[] { Codes.B404, Codes.B405, Codes.B406, Codes.B407, Codes.B408, Codes.B409, Codes.B410, Codes.B411 };
	}
}
