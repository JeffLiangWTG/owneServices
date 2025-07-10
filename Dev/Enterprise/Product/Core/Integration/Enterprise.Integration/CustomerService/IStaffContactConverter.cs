using CargoWise.Common;

namespace Enterprise.Integration.CustomerService
{
	public interface IStaffContactConverter
	{
		SecureQueryString CurrentStaffAndRegistrationToSecuredQueryString();
	}
}
