using CargoWise.ComponentModel;

namespace Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice
{
	public interface IGlobalElectronicInvoiceBuilder
	{
		(GlobalElectronicInvoicing EInvoice, INotifications ValidationErrors, INotifications ValidationWarnings) Create();
	}
}