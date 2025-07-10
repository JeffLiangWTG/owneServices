namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public sealed class TemporaryStorageHeaderValidationDecider : ITemporaryStorageHeaderValidationDecider
	{
		public bool IsRule058Active => true;

		public bool IsPreLodgedStatusCheckActive => true;

		public bool IsCheckCRNAndMRNForTSAActive => true;

		public bool IsMessageTypeCheckActive => true;

		public bool IsAuthorizationUsageCheckActive => true;
	}
}
