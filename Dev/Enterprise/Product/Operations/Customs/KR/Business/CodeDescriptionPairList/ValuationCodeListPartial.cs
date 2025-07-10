using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	partial class ValuationCodeList
	{
		public static string GetValuationMethod(ZString code)
		{
			var result = ZString.Empty;
			switch (code)
			{
				case Codes.MethodOne:
					result = Constants.ValuationMethod.A;
					break;
				default:
					result = Constants.ValuationMethod.B;
					break;
			}

			return result;
		}

		public static bool IsValuationMethodTwoToThree(ZString code) => code == Codes.MethodTwo || code == Codes.MethodThree;
		public static bool IsValuationMethodFour(ZString code) => code == Codes.MethodFourA || code == Codes.MethodFourB;
		public static bool IsValuationMethodFiveToSix(ZString code) => code == Codes.MethodFive || code == Codes.MethodSix;
	}
}
