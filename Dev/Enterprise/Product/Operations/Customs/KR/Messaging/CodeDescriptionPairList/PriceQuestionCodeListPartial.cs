using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	public partial class PriceQuestionCodeList : CodeDescriptionPairList
	{
		public static IEnumerable<string> MandatoryQuestionsForMethodOneFor5SM()
		{
			yield return Codes._5A;
			yield return Codes._6A;
			yield return Codes._6B;
			yield return Codes._7A;
			yield return Codes._7B;
			yield return Codes._8A;
			yield return Codes._8B;
			yield return Codes._8C;
			yield return Codes._8D;
			yield return Codes._9A;
			yield return Codes._9B;
			yield return Codes._10A;
			yield return Codes._10B;
			yield return Codes._10C;
			yield return Codes._10D;
			yield return Codes._11A;
			yield return Codes._11B;
			yield return Codes._11C;
			yield return Codes._11D;
		}

		public static IEnumerable<string> MandatoryQuestionsForMethodOneForIMP()
		{
			yield return Codes._7A;
			yield return Codes._8A;
			yield return Codes._8B;
			yield return Codes._9A;
			yield return Codes._9B;
		}

		public static IEnumerable<string> SubQuestionsOf5A()
		{
			yield return Codes._5B;
			yield return Codes._5C;
			yield return Codes._5D;
			yield return Codes._5EA;
			yield return Codes._5EB;
		}

		public static IEnumerable<string> SubQuestionsOf7A()
		{
			yield return Codes._7B;
			yield return Codes._7C;
			yield return Codes._7D;
			yield return Codes._7EA;
			yield return Codes._7EB;
		}
	}
}
