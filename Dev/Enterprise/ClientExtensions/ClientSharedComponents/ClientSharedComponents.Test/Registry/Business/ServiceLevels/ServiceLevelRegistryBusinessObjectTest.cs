using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceLevelRegistryBusinessObject))]
	public class ServiceLevelRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateServiceLevel()
		{
			BizObj.RunPreSaveValidation();
			AssertEquals("ServiceLevel should have errors", true, BizObj.ServiceLevelInfo.HasErrors());

			BizObj.ServiceLevel = "x99";
			AssertEquals("ServiceLevel should have errors", true, BizObj.ServiceLevelInfo.HasErrors());

			BizObj.ServiceLevel = "STD";
			AssertEquals("ServiceLevel should not have any errors", false, BizObj.ServiceLevelInfo.HasErrors());
		}

		[ExpectException(typeof(RegistryValidationException))]
		public void TestDuplicateRegistryEntry()
		{
			ServiceLevelRegistryBusinessObjectCollection setupCollection = new ServiceLevelRegistryBusinessObjectCollection();
			ServiceLevelRegistryBusinessObject registrySetup = new ServiceLevelRegistryBusinessObject();
			registrySetup.ServiceLevel = "TSP";
			setupCollection.Add(registrySetup);
			Assert("Validation should pass", !registrySetup.HasErrors);

			ServiceLevelRegistryBusinessObject duplicateRegistry = new ServiceLevelRegistryBusinessObject();
			duplicateRegistry.ServiceLevel = "TSP";
			setupCollection.Add(duplicateRegistry);
		}

		#region Test ZPropertyInfos

		public void TestNewZPropertyInfos()
		{
			AssertZPropertyInfo(BizObj.ServiceLevelInfo, ServiceLevelRegistryBusinessObject.Schema.ServiceLevel, 3);
		}

		void AssertZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName)
		{
			AssertNotNull("ZPropertyInfo for " + propertyInfo.Name + " was null", propertyInfo);
			AssertEquals("PropertyInfo.Name", expectedName, propertyInfo.Name);
		}

		void AssertZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName, int expectedMaxLength)
		{
			AssertZPropertyInfo(propertyInfo, expectedName);
			AssertEquals("PropertyInfo.MaxLength", expectedMaxLength, propertyInfo.MaxLength);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.ServiceLevel = "STD";

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new ServiceLevelRegistryBusinessObject BizObj
		{
			get { return (ServiceLevelRegistryBusinessObject)base.BizObj; }
		}

		#endregion
	}
}
