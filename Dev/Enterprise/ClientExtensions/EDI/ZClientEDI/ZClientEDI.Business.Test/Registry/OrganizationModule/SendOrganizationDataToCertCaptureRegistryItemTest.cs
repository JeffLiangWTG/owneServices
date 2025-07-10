using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(SendOrganizationDataToCertCaptureRegistryItem))]
	class SendOrganizationDataToCertCaptureRegistryItemTest : StronglyTypedRegistryItemTestCase<SendOrganizationDataToCertCapture>
	{
		protected override StronglyTypedRegistryItem<SendOrganizationDataToCertCapture, SendOrganizationDataToCertCapture> GetNewRegistryItem()
		{
			return new SendOrganizationDataToCertCaptureRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new SendOrganizationDataToCertCapture());
		}
	}
}
