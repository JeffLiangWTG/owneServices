using CargoWise.Licensing;

namespace Enterprise.ProductRegistration.Common
{
	public class RegisterRequest
	{
		public string ProductKey { get; set; }
		public DatabaseUniqueKey UniqueKey { get; set; }
		public string ProductVersion { get; set; }
	}
}
