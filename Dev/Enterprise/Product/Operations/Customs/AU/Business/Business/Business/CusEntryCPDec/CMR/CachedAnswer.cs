using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business;

public class CachedAnswer
{
	public CachedAnswer(CMRCusEntryCPDec cpDec)
	{
		this.QuestionID = cpDec.ON_CPDecNum;
		this.StartDate = cpDec.ON_CPDecStartDate;
		this.Answer = cpDec.ON_AnswerCode;
		this.Permit = cpDec.ON_Permit;
	}

	public readonly ZInt QuestionID;
	public readonly ZDateTime StartDate;
	public readonly ZString Answer;
	public readonly ZString Permit;

	public static bool HasIdenticalAnswerPermit(CachedAnswer a, CachedAnswer b)
	{
		return a != null
			   && b != null
			   && a.QuestionID == b.QuestionID
			   && a.StartDate == b.StartDate
			   && string.Compare(a.Answer, b.Answer, true) == 0
			   && string.Compare(a.Permit, b.Permit, true) == 0;
	}
}
