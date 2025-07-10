namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageBillConfiguration
	{
		public ITemporaryStorageBillValidationDecider GetValidationDecider() => GetValidationDeciderCore();
		protected virtual ITemporaryStorageBillValidationDecider GetValidationDeciderCore() => new TemporaryStorageBillValidationDecider();
	}
}
