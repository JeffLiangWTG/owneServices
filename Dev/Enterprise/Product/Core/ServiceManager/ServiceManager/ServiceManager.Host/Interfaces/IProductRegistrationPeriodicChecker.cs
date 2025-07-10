namespace Enterprise.ServiceManager.Host
{
	public interface IProductRegistrationPeriodicChecker
	{
		bool IsProductRegisteredAsNonTrialSystemOrUnknown();
	}
}
