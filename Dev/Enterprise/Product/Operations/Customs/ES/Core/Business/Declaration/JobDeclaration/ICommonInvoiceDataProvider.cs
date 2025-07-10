using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public interface ICommonInvoiceDataProvider : EU.Business.Declaration.ICommonInvoiceDataProvider
	{
		ZPropertyInfo ZG_DestinationStateInfo { get; }
		bool DestinationStateIsCanaryIsland { get; }
	}
}
