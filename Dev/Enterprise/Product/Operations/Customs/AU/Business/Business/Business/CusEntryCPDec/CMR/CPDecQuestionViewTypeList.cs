
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CPDecQuestionViewTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string All = "ALL";
			public const string Answered = "ANS";
			public const string Unanswered = "UAN";
		}

		public static class Descriptions
		{
			public const string All = "All CP Dec Questions";
			public const string Answered = "Answered CP Dec Questions Only";
			public const string Unanswered = "Unanswered CP Dec Questions Only";
		}

		public CPDecQuestionViewTypeList()
		{
			AddPair(Codes.All, Descriptions.All);
			AddPair(Codes.Answered, Descriptions.Answered);
			AddPair(Codes.Unanswered, Descriptions.Unanswered);
		}
	}
}
