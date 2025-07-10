
using System.IO;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Romania
{
	public sealed class RomaniaEInvoiceTestHelper
	{
		public static string XmlFilesEmbeddedLocation => "Enterprise.Accounting.ElectronicMessaging.Testing.Romania.UniversalEventXml.";

		public static string ZippedXmlFilesEmbeddedLocation => "Enterprise.Accounting.ElectronicMessaging.Testing.Romania.UniversalEventXml.ZippedXmlFiles.";

		public static string GetEmbeddedResourceAsString(string resourceName)
			=> EInvoicingTestHelper.GetEmbeddedResourceAsUtf8String(resourceName, XmlFilesEmbeddedLocation).Replace("\t", "  ");

		public static Stream GetEmbeddedZippedXmlFileAsStream(string resourceName)
			=> EInvoicingTestHelper.GetEmbeddedResourceAsStream(resourceName, ZippedXmlFilesEmbeddedLocation);
	}
}
