using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	class FRTemporaryStorageHeaderValidationDecider : ITemporaryStorageHeaderValidationDecider
	{
		public bool IsRule058Active => false;

		public bool IsPreLodgedStatusCheckActive => true;

		public bool IsCheckCRNAndMRNForTSAActive => true;

		public bool IsMessageTypeCheckActive => true;

		public bool IsAuthorizationUsageCheckActive => true;
	}
}
