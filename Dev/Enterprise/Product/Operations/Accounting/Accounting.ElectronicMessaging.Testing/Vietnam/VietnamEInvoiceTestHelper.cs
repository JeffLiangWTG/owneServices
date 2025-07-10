
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Vietnam
{
	public sealed class VietnamEInvoiceTestHelper
	{
		public static string XmlFilesEmbeddedLocation => "Enterprise.Accounting.ElectronicMessaging.Testing.Vietnam.EInvoice.TestCases.";

		public static string GetEmbeddedResourceAsString(string resourceName)
			=> EInvoicingTestHelper.GetEmbeddedResourceAsUtf8String(resourceName, XmlFilesEmbeddedLocation).Replace("\t", "  ").Trim();
	}
}
