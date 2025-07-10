using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business;

public class LineDefaultKey
{
	public LineDefaultKey(ZInt questionID, ZDateTime startDate)
	{
		this.QuestionID = questionID;
		this.StartDate = startDate;
	}

	public readonly ZInt QuestionID;
	public readonly ZDateTime StartDate;
}
