using System.IO;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Italy
{
	public sealed class ItalyEInvoiceTestHelper
	{
		public static string XmlFilesEmbeddedLocation => "Enterprise.Accounting.ElectronicMessaging.Testing.Italy.FatturaElettronicaXmlWriter.Validation.TestFiles.";

		public static Stream GetEmbeddedResourceAsStream(string resourceName)
			=> EInvoicingTestHelper.GetEmbeddedResourceAsStream(resourceName, XmlFilesEmbeddedLocation);
	}
}
