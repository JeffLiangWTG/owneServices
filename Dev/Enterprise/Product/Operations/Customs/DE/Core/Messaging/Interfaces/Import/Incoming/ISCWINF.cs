using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ISCWINF : IDataProvider
	{
		string MRN { get; }

		string ReferenceNumber { get; }

		string CurrentProcedure { get; }
	}
}
