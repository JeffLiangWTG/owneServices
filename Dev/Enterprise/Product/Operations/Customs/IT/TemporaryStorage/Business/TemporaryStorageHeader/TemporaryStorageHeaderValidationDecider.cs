using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageHeaderValidationDecider : ITemporaryStorageHeaderValidationDecider
{
	public bool IsRule058Active => false;

	public bool IsPreLodgedStatusCheckActive => false;

	public bool IsCheckCRNAndMRNForTSAActive => false;

	public bool IsMessageTypeCheckActive => false;

	public bool IsAuthorizationUsageCheckActive => false;
}
