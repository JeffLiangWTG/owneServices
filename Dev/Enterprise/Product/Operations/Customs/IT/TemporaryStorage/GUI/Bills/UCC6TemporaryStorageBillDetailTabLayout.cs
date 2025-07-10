using Enterprise.Customs.EU.TemporaryStorage.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

sealed class UCC6TemporaryStorageBillDetailTabLayout : ITemporaryStorageBillDetailTabLayoutProvider
{
	public bool IsSupportingDocumentsTabVisible => false;

	public bool IsAdditionalInformationTabVisible => false;
}
