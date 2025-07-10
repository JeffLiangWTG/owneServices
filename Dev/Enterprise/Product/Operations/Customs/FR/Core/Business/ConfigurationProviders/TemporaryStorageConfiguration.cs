using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business
{
	public sealed class TemporaryStorageConfiguration : EU.Business.CusTempStorage.TemporaryStorageConfiguration
	{
		protected override ITemporaryStorageHeaderValidationDecider GetValidationDeciderCore() => new FRTemporaryStorageHeaderValidationDecider();

		protected override EU.Business.CusTempStorage.TemporaryStorageBillConfiguration GetNewBillConfiguration() => new TemporaryStorageBillConfiguration();
	}
}
