using System.Collections.Generic;
using System.Xml.Linq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(XsltDataTransformationForTest))]
	sealed class XsltDataTransformationFunctionalityTest : XsltDataTransformationTest
	{
		protected override XDocument Source
		{
			get
			{
				return  XDocument.Parse(
@"<Entity Type=""Shipment"">
  <Id Type=""ZGuid"">d016070b-8005-4be5-ab3d-14a305ac6cd2</Id>
  <Property Name=""PortOfLoading"">
    <Value Type=""ZString"">AUSYD</Value>
  </Property>
  <Property Name=""PortOfDischarge"">
    <Value Type=""ZString"">NZAKL</Value>
  </Property>  
  <Property Name=""ReleaseType"" State=""Added"">
    <Value Type=""ZString"">AAA</Value>
  </Property>
</Entity>");
			}
		}

		protected override XDocument ExpectedResult
		{
			get
			{
				return XDocument.Parse(
@"<Entity Type=""Shipment"">
  <Property Name=""PortOfLoading"">AUSYD</Property>
  <Property Name=""PortOfDischarge"">NZAKL</Property>
  <Property Name=""ReleaseType"">AAA</Property>
</Entity>");
			}
		}

		protected override IEnumerable<string> ExpectedNotifications
		{
			get { yield return "test message"; }
		}
	}
}
