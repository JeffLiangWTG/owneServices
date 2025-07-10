using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public interface ICusSealSequenceNumberGeneratorProvider
	{
		ShortSequenceNumberGenerator SequenceNumberGenerator { get; }
	}
}
