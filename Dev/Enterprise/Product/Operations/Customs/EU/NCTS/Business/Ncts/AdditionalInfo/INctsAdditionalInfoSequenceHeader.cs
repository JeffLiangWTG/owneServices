using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsAdditionalInfoSequenceHeader
	{
		ShortSequenceNumberGenerator RefSequenceNumberGenerator { get; }
		ShortSequenceNumberGenerator InfSequenceNumberGenerator { get; }
		ShortSequenceNumberGenerator TraSequenceNumberGenerator { get; }
	}
}
