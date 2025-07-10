using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public interface ICustomsOffice
	{
		ZDateTime ArrivalTime { get; }
		ZString OfficeCode { get; }
	}
}
