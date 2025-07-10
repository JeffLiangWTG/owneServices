namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public interface ITemporaryStorageBillDetailTabLayoutProvider
	{
		bool IsSupportingDocumentsTabVisible { get; }
		bool IsAdditionalInformationTabVisible { get; }
	}
}
