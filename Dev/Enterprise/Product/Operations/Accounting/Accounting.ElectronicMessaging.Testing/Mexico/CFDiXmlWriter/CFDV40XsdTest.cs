using System.Reflection;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class cfdv40Test : TestCaseWithFactory
	{
		readonly ZString XsdResourceName = "Enterprise.Accounting.ElectronicMessaging.Mexico.CFDiXmlWriter.cfdv40.xsd";

		public void TestCfdv40_Attributes_HaveNoType()
		{
			using (var xsdStream = Assembly.Load("Enterprise.Accounting.ElectronicMessaging").GetManifestResourceStream(XsdResourceName))
			{
				var xsd = new XmlDocument();
				xsd.Load(xsdStream);

				var nsmgr = new XmlNamespaceManager(xsd.NameTable);
				nsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

				var nodes = new string[] { "LugarExpedicion", "ClaveProdServ", "Periodicidad", "Meses" };

				foreach (var node in nodes)
				{
					var xmlNode = xsd.DocumentElement.SelectSingleNode($"//xs:attribute[@name='{node}']", nsmgr);
					AssertNull(xmlNode.Attributes["name"].Value, xmlNode.Attributes["type"]);
				}
			}
		}
	}
}
