using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

class CusVehicleCollection : CusVehicleCollection<CusVehicle, JobComInvoiceLine>
{
	public CusVehicleCollection(JobComInvoiceLine master)
		: base(master)
	{
		RefreshMaxCount();
	}

	public void RefreshMaxCount()
	{
		var maxCount = Master.Declaration?.IsExport ?? false ? 1 : -1;
		this.EnableMaxCountValidation(maxCount, false, notificationType: NotificationType.MessageError, PassarValidationMessages.MessageNS30104_Vehicles);
	}
}
