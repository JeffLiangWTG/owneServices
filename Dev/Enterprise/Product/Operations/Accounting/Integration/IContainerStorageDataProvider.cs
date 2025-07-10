using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Integration
{
	public interface IContainerStorageDataProvider
	{
		ZString ContainerNumber { get; }
		ZDateTime PickupDate { get; }
		IJobInvoicingPlugIn[] OperationsJobs { get; }
		StmALog CreateEditLog(string reference);
	}
}
