using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ExportVehicleNoWrapper : NonPersistentBusinessObject
	{
		public ExportVehicleNoWrapper(IExportVehicleNo vehicleNo)
		{
			this.VehicleNo = vehicleNo;
		}
		public IExportVehicleNo VehicleNo { get; }

		public ZString InvoiceLineNo { get; set; }
	}
}
