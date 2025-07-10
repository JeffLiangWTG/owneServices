using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public interface IControlAccountProvider
	{
		ZGuid PK { get; }
		ZGuid GST { get; }
		void SetTransaction(AccTransactionHeader transaction);
	}
}
