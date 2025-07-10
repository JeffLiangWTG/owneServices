using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ServiceTaskCodeService : IService
	{
		public ServiceTaskCodeService(string serviceTaskCode)
		{
			ServiceTaskCode = serviceTaskCode;
		}

		public string ServiceTaskCode { get; private set; }
	}
}
