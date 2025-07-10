using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(SendOrganizationDataToCertCapture))]
	public class SendOrganizationDataToCertCaptureTest : RegistryBusinessObjectTemplateTestCase<SendOrganizationDataToCertCapture>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override SendOrganizationDataToCertCapture GetBusinessObjectToClone()
		{
			return new SendOrganizationDataToCertCapture();
		}

		protected override SendOrganizationDataToCertCapture GetBusinessObjectToSerialise()
		{
			return new SendOrganizationDataToCertCapture();
		}
	}
}
