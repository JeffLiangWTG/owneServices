using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICWSINF : IDataProvider
	{
		string CurrentProcedure { get; }
	}
}
