using CargoWise.Licensing;

namespace Enterprise.ProductRegistration.Common
{
	public class VerifyRequest
	{
		public string Key { get; set; }
		public DatabaseUniqueKey UniqueKey { get; set; }
		public string ProductVersion { get; set; }
	}
}
