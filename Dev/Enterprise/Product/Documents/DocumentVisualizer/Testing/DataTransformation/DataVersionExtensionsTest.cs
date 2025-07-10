using System.Xml.Linq;
using Enterprise.DocumentVisualizer.DataTransformation;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DataVersionExtensionsTest : TestCase
	{
		public void TestGetDataVersion()
		{
			var xmlDoc = XDocument.Parse(@"<Entity Type=""Shipment"" DataMajorVersion=""123456"" DataMinorVersion=""4"">
  <Id Type=""ZGuid"">d016070b-8005-4be5-ab3d-14a305ac6cd2</Id>
  <Property Name=""PortOfLoading"">
    <Value Type=""ZString"">AUSYD</Value>
  </Property>
  <Property Name=""PortOfDischarge"">
    <Value Type=""ZString"">NZAKL</Value>
  </Property>
</Entity>");

			AssertEquals("data version", "123456.4", xmlDoc.GetDataVersion().ToString());
		}

		public void TestGetDataVersion_NoVersionInXml()
		{
			var xmlDoc = XDocument.Parse(@"<Entity Type=""Shipment"">
  <Id Type=""ZGuid"">d016070b-8005-4be5-ab3d-14a305ac6cd2</Id>
  <Property Name=""PortOfLoading"">
    <Value Type=""ZString"">AUSYD</Value>
  </Property>
  <Property Name=""PortOfDischarge"">
    <Value Type=""ZString"">NZAKL</Value>
  </Property>
</Entity>");

			AssertEquals("data version", "0.0", xmlDoc.GetDataVersion().ToString());
		}

		public void TestGetDataVersion_MisformattedVersionInXml()
		{
			var xmlDoc = XDocument.Parse(@"<Entity Type=""Shipment"" DataMajorVersion=""abc"" DataMinorVersion=""xyz"">
  <Id Type=""ZGuid"">d016070b-8005-4be5-ab3d-14a305ac6cd2</Id>
  <Property Name=""PortOfLoading"">
    <Value Type=""ZString"">AUSYD</Value>
  </Property>
  <Property Name=""PortOfDischarge"">
    <Value Type=""ZString"">NZAKL</Value>
  </Property>
</Entity>");

			AssertEquals("data version", "0.0", xmlDoc.GetDataVersion().ToString());
		}

		public void TestGetDataVersion_NoXml()
		{
			XDocument xmlDoc = null;

			AssertEquals("data version", "0.0", xmlDoc.GetDataVersion().ToString());
		}

		public void TestGetDataVersion_NoVersonInXml()
		{
			XDocument xmlDoc = null;

			AssertEquals("data version", "0.0", xmlDoc.GetDataVersion().ToString());
		}
	}
}