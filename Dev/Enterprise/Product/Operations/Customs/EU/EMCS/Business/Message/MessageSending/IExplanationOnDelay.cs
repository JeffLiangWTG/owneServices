using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public interface IExplanationOnDelay
	{
		ZString ExplanationCode { get; }
		ZString Information { get; }
		ZString MessageRole { get; }
	}
}
