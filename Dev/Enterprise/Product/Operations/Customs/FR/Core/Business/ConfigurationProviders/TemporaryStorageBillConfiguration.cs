using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business
{
	public class TemporaryStorageBillConfiguration : EU.Business.CusTempStorage.TemporaryStorageBillConfiguration
	{
		protected override EU.Business.CusTempStorage.ITemporaryStorageBillValidationDecider GetValidationDeciderCore() => new TemporaryStorageBillValidationDecider();
	}
}
