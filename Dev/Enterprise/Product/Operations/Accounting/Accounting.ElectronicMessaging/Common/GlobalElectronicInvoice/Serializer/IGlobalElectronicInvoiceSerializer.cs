using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public interface IGlobalElectronicInvoiceSerializer
	{
		string Serialize(GlobalElectronicInvoicing eInvoice);
		GlobalElectronicInvoicing Deserialize(string serializedEInvoice);
	}
}
