using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Client.MFI.Data;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.CaroTrans.Import.Testing
{
	[TestTimeZone]
	sealed class TestCarotransManifestDataFileReader : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateXmlResultSize20Container()
		{
			AssertProcessOceanBills(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateXmlResultSize40Container()
		{
			AssertProcessOceanBills(false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateXmlResult()
		{
			var testObject = new CarotransManifestDataFileReader(data);
			var consols = new Xsd.Consols();
			consols.Consol = testObject.ProcessImportManifest();
			using (var stream = new MemoryStream())
			{
				var streamWriter = new StreamWriter(stream);
				var writer = new XmlTextWriter(streamWriter);
				writer.Formatting = Formatting.Indented;
				new XmlValueObjectSerializer(typeof(Xsd.Consols)).Serialize(writer, consols);
				streamWriter.Flush();
				writer.Flush();
				var actualXml = Encoding.UTF8.GetString(stream.ToArray());
				var resultXml = new XmlDocument();
				resultXml.PreserveWhitespace = false;
				resultXml.LoadXml(actualXml);
				var validator = new XmlValidator(FreightXmlSchemaDefinitions.Instance.ConsolsSchema);
				var notify = new NotificationBuffer();
				validator.Validate(resultXml.InnerXml, notify);
				AssertEquals("Consol Xml Document does not validate against Consol schema " + System.Environment.NewLine + notify.AsString, false, notify.HasErrors);
			}
		}

		void AssertProcessOceanBills(bool isSize20)
		{
			if (isSize20)
			{
				data = data.Replace("40S", "20S");
			}

			var testObject = new CarotransManifestDataFileReader(data);
			var consols = new Xsd.Consols();
			consols.Consol = testObject.ProcessImportManifest();
			using (var stream = new MemoryStream())
			{
				var textWriter = new StreamWriter(stream);
				var writer = new XmlTextWriter(textWriter);
				writer.Formatting = Formatting.Indented;
				new XmlValueObjectSerializer(typeof(Xsd.Consols)).Serialize(writer, consols);
				writer.Flush();
				var actualXml = StreamConverter.StreamToString(stream);
				var expectedXml = File.ReadAllText(BaseSourcePath + @"\Enterprise\ClientExtensions\MFI\ZClientMFI\ZClientMFI.Test\CaroTrans\Import\TestFiles\CaroTransExpectedResultsTestFile_Whidbey.xml");
				expectedXml = expectedXml.Replace("\t", "  ");
				if (isSize20)
				{
					expectedXml = expectedXml.Replace("42G0", "22G0");
				}

				AssertXMLEquals("Generated xml should be correct", expectedXml, actualXml);
				writer.Close();
				textWriter.Close();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override void SetUp()
		{
			base.SetUp();
			data = File.ReadAllText(BaseSourcePath + @"Enterprise\ClientExtensions\MFI\ZClientMFI\ZClientMFI.Test\CaroTrans\Import\TestFiles\CaroTransTestFile.dat");
		}

		string data;
	}
}
