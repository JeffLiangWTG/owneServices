using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public interface IPGAHeader
	{
		ZString GovAgencyIDCode { get; }
		IHasPGARequirements Parent { get; }
		void CopyPersistentValuesFrom(IPGAHeader source);
	}
}
