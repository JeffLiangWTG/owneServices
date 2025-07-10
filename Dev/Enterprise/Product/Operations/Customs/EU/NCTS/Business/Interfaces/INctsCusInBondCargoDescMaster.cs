using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Interfaces
{
	public interface INctsCusInBondCargoDescMaster
	{
		ShortSequenceNumberGenerator LineNumberGenerator { get; }

		NctsHeader Header { get; }
	}
}
