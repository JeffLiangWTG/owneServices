using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(ServiceInstallInfo))]
	class ServiceInstallInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			// Arrange
			var info = new ServiceInstallInfoForTest(Factory);
			info.Automatic = false;

			// Act
			info.SetDefaultValuesExposed();

			// Assert
			Assert(info.Automatic);
		}
	}

	class ServiceInstallInfoForTest : ServiceInstallInfo
	{
		public void SetDefaultValuesExposed()
		{
			base.SetDefaultValues();
		}

		public ServiceInstallInfoForTest(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
