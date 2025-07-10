using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.IO;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;

namespace Enterprise.DbUpgrader.Transformations.Test.MasterFiles
{
	abstract class UpdateUsExportStatementRegistryDataTransformationTest : RegistryDataTransformationTestCase
	{
		protected abstract string XmlBeforeTransformationName { get; }

		protected abstract string XmlAfterTransformationName { get; }

		protected override void PrepareTestData()
		{
			var inputExportStatementRegistryItem = GetInputExportStatementRegistryItem();
			Helper.InsertStmDataRow(UsExportStatementRegistryItem, "BIN", inputExportStatementRegistryItem);
		}

		protected override void AssertTransformationResults()
		{
			var expectedXml = GetExpectedExportStatementRegistryItem();
			var actualXml = GetActualExportStatementRegistryItem();

			AssertXMLEquals(expectedXml, actualXml);
		}

		byte[] GetInputExportStatementRegistryItem()
		{
			using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(XmlBeforeTransformationName))
			{
				if (resourceStream == null)
				{
					throw new FileNotFoundException(
						$"Test input data is not available. Please check that assembly manifest resource exists: {XmlBeforeTransformationName}.");
				}

				var xmlWriterSettings = new XmlWriterSettings
				{
					Encoding = Utf16Encoding,
					OmitXmlDeclaration = false,
					Indent = false
				};
				using (var memoryStream = new MemoryStream())
				using (var xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
				{
					XDocument.Load(resourceStream).Save(xmlWriter);
					xmlWriter.Flush();
					memoryStream.Seek(0, SeekOrigin.Begin);
					return memoryStream.ToByteArray();
				}
			}
		}

		string GetExpectedExportStatementRegistryItem()
		{
			using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(XmlAfterTransformationName))
			{
				if (resourceStream == null)
				{
					throw new FileNotFoundException(
						$"Expected test data is not available. Please check that assembly manifest resource exists: {XmlAfterTransformationName}.");
				}

				var document = XDocument.Load(resourceStream);
				return GetXmlString(document);
			}
		}

		string GetActualExportStatementRegistryItem()
		{
			var dataBytes = Helper.GetStmDataValue(UsExportStatementRegistryItem);
			using (var memoryStream = new MemoryStream(dataBytes))
			using (var streamReader = new StreamReader(memoryStream, Utf16Encoding))
			{
				var document = XDocument.Load(streamReader);
				return GetXmlString(document);
			}
		}

		static string GetXmlString(XDocument document)
		{
			var stringBuilder = new StringBuilder();
			using (var stringWriter = new StringWriter(stringBuilder))
			{
				document.Save(stringWriter);
				stringWriter.Flush();
			}
			return stringBuilder.ToString();
		}

		const string UsExportStatementRegistryItem = "ExportStatementSetting";

		static readonly Encoding Utf16Encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
	}
}
