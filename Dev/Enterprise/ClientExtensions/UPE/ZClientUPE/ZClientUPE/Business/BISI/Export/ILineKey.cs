
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public interface ILineKey : IBusiness
	{
		ZString ShipmentRef { get; }
		ZString FlightNo { get; }
		ZDateTime ImportDate { get; }
		ZString ConsigneePostCode { get; }
	}
}
