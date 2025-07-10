using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public interface IEMCSCancellation
	{
		ZString Reason { get; }
		ZString Information { get; }
	}
}
