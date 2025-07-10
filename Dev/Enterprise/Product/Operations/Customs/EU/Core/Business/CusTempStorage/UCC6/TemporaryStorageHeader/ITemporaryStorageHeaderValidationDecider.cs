namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public interface ITemporaryStorageHeaderValidationDecider
	{
		bool IsRule058Active { get; }

		bool IsPreLodgedStatusCheckActive { get; }

		bool IsCheckCRNAndMRNForTSAActive { get; }

		bool IsMessageTypeCheckActive { get; }

		bool IsAuthorizationUsageCheckActive { get; }
	}
}
