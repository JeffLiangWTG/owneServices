using System.Reflection;
using CargoWise.IO;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using XMLSchemaExtractor;

namespace XMLSchemaExtractorTest
{
	public class SchemaExtractorTest : TestCase
	{
		readonly string UniversalZipName = $"{UniversalXmlInfo.ZipFileName}.zip";

		readonly string NativeZipName = $"{NativeXmlInfo.ZipFileName}.zip";

		readonly static string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

		public void TestUniversalXmlSchemasGeneration()
		{
			var names = new[]
			{
				"UniversalActivity.xsd",
				"UniversalActivityRequest.xsd",
				"UniversalCommon.xsd",
				"UniversalDocumentRequest.xsd",
				"UniversalEvent.xsd",
				"UniversalInterchange.xsd",
				"UniversalInterchangeRequeueRequest.xsd",
				"UniversalResponse.xsd",
				"UniversalSchedule.xsd",
				"UniversalShipment.xsd",
				"UniversalShipmentRequest.xsd",
				"UniversalTransaction.xsd",
				"UniversalTransactionBatch.xsd",
				"UniversalTransactionBatchRequest.xsd",
			};

			using (var tempFileDirectory = new TempDirectory())
			{
				var expectDocData = false;
				var tempDirectoryName = tempFileDirectory.DirectoryName;

				var extractor = new XMLExtractor();
				extractor.GenerateUniversalXmlSchemas();
				 
				ZipCompression.Unzip(Path.Combine(binPath, UniversalZipName), tempDirectoryName);

				var actualFiles = Directory.GetFiles(tempDirectoryName).Select(f => Path.GetFileName(f));
				AssertContainsExactElementsInAnyOrder(names, actualFiles);

				AssertEquals("UniversalInterchange.xsd should be exported", true, File.Exists(Path.Combine(tempDirectoryName, "UniversalInterchange.xsd")));
				AssertEquals("UniversalEvent.xsd should be exported", true, File.Exists(Path.Combine(tempDirectoryName, "UniversalEvent.xsd")));
				AssertEquals("UniversalDocumentRequest.xsd should be exported", true, File.Exists(Path.Combine(tempDirectoryName, "UniversalDocumentRequest.xsd")));

				AssertContains("Check shipment xsd contents", @"<xs:element name=""UniversalShipment"" type=""UniversalShipmentData"" />", File.ReadAllText(tempDirectoryName + "\\UniversalShipment.xsd"));

				AssertContains("Check activity xsd contents", @"<xs:element name=""UniversalActivity"" type=""UniversalActivityData"" />", File.ReadAllText(tempDirectoryName + "\\UniversalActivity.xsd"));
				AssertContains("Check event xsd contents", @"<xs:element name=""UniversalEvent"" type=""UniversalEventData"" />", File.ReadAllText(tempDirectoryName + "\\UniversalEvent.xsd"));
				AssertContains("Check transaction xsd contents", @"<xs:element name=""UniversalTransaction"" type=""UniversalTransactionData"" />", File.ReadAllText(tempDirectoryName + "\\UniversalTransaction.xsd"));
				AssertContains("Check interchange xsd contents", @"<xs:element name=""UniversalInterchange"">", File.ReadAllText(tempDirectoryName + "\\UniversalInterchange.xsd"));

				var commonXsd = File.ReadAllText(tempDirectoryName + "\\UniversalCommon.xsd");
				AssertContains("Check common xsd contents", @"<xs:complexType name=""DataContext"">", commonXsd);
				AssertEquals($"Check common xsd should {(expectDocData ? "" : "not ")}contain DocData element", expectDocData, commonXsd.Contains(@"<xs:element name=""DocData"""));
				AssertContains("Check attached doc in common xsd", @"<xs:complexType name=""AttachedDocument"">", commonXsd);
				AssertContains("Check attached doc collection in common xsd", @"<xs:element name=""AttachedDocumentCollection"" minOccurs=""0"">", commonXsd);
			}
		}

		public void TestGenerateNativeXmlSchemas()
		{
			using (var tempFileDirectory = new TempDirectory())
			{
				var tempDirectoryName = tempFileDirectory.DirectoryName;

				var extractor = new XMLExtractor();
				extractor.GenerateNativeXmlSchemas();

				ZipCompression.Unzip(Path.Combine(binPath, NativeZipName), tempDirectoryName);

				var actualFiles = Directory.GetFiles(tempDirectoryName).Select(f => Path.GetFileName(f));

				AssertEquals("Native.xsd should be exported", true, File.Exists(Path.Combine(tempDirectoryName, "Native.xsd")));
				AssertEquals("NativeOrganization.xsd should be exported", true, File.Exists(Path.Combine(tempDirectoryName, "NativeOrganization.xsd")));
				AssertContains("Check native xsd contents", @"<xs:element name=""Native"">", File.ReadAllText(Path.Combine(tempDirectoryName, "Native.xsd")));
				AssertContains("Check organisation xsd contents", @"<xs:complexType name=""NativeOrganization"">", File.ReadAllText(Path.Combine(tempDirectoryName, "NativeOrganization.xsd")));
			}
		}
	}
}
