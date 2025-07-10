using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Malaysia;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Malaysia
{
	public class MalaysiaEInvoiceDocType_RegistryDescriptorTest : TestCaseWithFactory
	{
		public void TestGetDefaultValue()
		{
			AssertEquals("INV", RegistryDescriptor.GetDefaultValue(string.Empty));
		}

		public void TestGetDefaultTypeCaption()
		{
			AssertEquals("Electronic Invoice Document Type", RegistryDescriptor.GetDefaultTypeCaption());
		}

		MalaysiaEInvoiceDocType_RegistryDescriptor RegistryDescriptor => registryDescriptor ?? (registryDescriptor = new MalaysiaEInvoiceDocType_RegistryDescriptor());
		MalaysiaEInvoiceDocType_RegistryDescriptor registryDescriptor;
	}
}
