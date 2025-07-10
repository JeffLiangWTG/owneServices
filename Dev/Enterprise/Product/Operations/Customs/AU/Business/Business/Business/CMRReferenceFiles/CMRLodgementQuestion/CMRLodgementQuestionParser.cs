using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRLodgementQuestionParser
	{
		public CMRLodgementQuestionParser(ZString code)
		{
			if (!code.IsEmpty)
			{
				int separatorIndex = code.IndexOf("-");
				if (separatorIndex > 0)
				{
					string questionIDString = code.SubstringSafe(0, separatorIndex);
					string startDateString = code.SubstringSafe(separatorIndex + 1);
					if (ZInt.TryParse(questionIDString, out QuestionID) && ZDateTime.TryParseISO8601Date(startDateString, out StartDate))
					{
						IsCompleteCode = true;
					}
				}
				else
				{
					ZInt.TryParse(code, out QuestionID);
				}
			}
			IsEmptyCode = code.IsEmpty;
		}

		public readonly bool IsEmptyCode;
		public readonly bool IsCompleteCode;

		public readonly ZInt QuestionID;
		public readonly ZDateTime StartDate;
	}
}
