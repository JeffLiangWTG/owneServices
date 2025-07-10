using CargoWise.Application;
using Enterprise.Integration.Licensing;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public static class LicenceTypeChanger
	{
		public static void SetSystemLicence(string databaseType)
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = databaseType;
		}
	}
}
