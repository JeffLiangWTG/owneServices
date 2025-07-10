using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport008DecQuestion
	{
		ZString QuestionID { get; }
		ZString Answer { get; }
	}
}
