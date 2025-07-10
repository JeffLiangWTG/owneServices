using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IEDIMessageContentFilter
	{
		ZString ECF_Name { get; }

		string GetUniversalShipmentPrimaryDataSource();
	}
}
