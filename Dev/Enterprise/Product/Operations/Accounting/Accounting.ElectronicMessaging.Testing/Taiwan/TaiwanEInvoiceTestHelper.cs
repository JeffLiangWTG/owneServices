
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Taiwan
{
	public sealed class TaiwanEInvoiceTestHelper
	{
		public static string XmlFilesEmbeddedLocation => "Enterprise.Accounting.ElectronicMessaging.Testing.Taiwan.TestCases.";

		public static string GetEmbeddedResourceAsString(string resourceName)
			=> EInvoicingTestHelper.GetEmbeddedResourceAsUtf8String(resourceName, XmlFilesEmbeddedLocation).Replace("\t", "  ");
	}
}
