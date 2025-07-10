using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface ICusSealSequenceNumberGeneratorProvider
	{
		ShortSequenceNumberGenerator SequenceNumberGenerator { get; }
	}
}
