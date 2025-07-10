using CargoWise.Types;

namespace Enterprise.Integration.Warehouse
{
	public interface IWhsInvoice
	{
		ZGuid ET_OH_Client { get; set; }
		ZGuid ET_WW { get; set; }
		ZDateTime ET_StorageFromDate { get; set; }
		ZDateTime ET_StorageToDate { get; set; }
		ZString ET_OffBandProcessingStatus { get; set; }
	}
}
