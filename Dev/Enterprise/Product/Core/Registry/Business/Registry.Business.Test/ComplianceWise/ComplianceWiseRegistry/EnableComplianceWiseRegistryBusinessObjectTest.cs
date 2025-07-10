using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EnableComplianceWiseRegistryBusinessObject))]
	sealed class EnableComplianceWiseRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<EnableComplianceWiseRegistryBusinessObject>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override EnableComplianceWiseRegistryBusinessObject GetBusinessObjectToClone()
		{
			return GetEnableComplianceWiseRegistryBusinessObject();
		}

		protected override EnableComplianceWiseRegistryBusinessObject GetBusinessObjectToSerialise()
		{
			return GetEnableComplianceWiseRegistryBusinessObject();
		}

		public static EnableComplianceWiseRegistryBusinessObject GetEnableComplianceWiseRegistryBusinessObject()
		{
			return ComplianceWiseRegistryHelper.SetValue(true);
		}
	}
}
