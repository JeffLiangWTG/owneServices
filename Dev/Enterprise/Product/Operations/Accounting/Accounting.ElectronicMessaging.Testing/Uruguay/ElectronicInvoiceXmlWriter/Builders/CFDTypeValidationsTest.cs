using System.Reflection;
using System.Xml;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	class CFDTypeValidationsTest : TestCaseWithFactory
	{
		//The test cases in this file are only to validate the modifications needed respect to the original xsd file provided by the government.
		public void TestSerieAndNroMandatory_ForFacturaAndTicket()
		{
			var electronicMessaging = Assembly.Load("Enterprise.Accounting.ElectronicMessaging");
			var cfeType = electronicMessaging.GetManifestResourceStream("Enterprise.Accounting.ElectronicMessaging.Uruguay.ElectronicInvoiceXmlWriter.Xsd.CFEType.xsd");

			var xsd = new XmlDocument();
			xsd.Load(cfeType);

			var root = xsd.DocumentElement;
			var nsmgr = new XmlNamespaceManager(xsd.NameTable);
			nsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

			var nodeIdDocFactura = root.SelectSingleNode("//xs:complexType[@name='IdDoc_Fact']", nsmgr);
			var nodeSerie = nodeIdDocFactura.SelectSingleNode("//xs:element[@name='Serie']", nsmgr);
			var nodeNro = nodeIdDocFactura.SelectSingleNode("//xs:element[@name='Nro']", nsmgr);
			var minOccursNodeSerie = nodeSerie.Attributes["minOccurs"].Value;
			var minOccursNodeNro = nodeNro.Attributes["minOccurs"].Value;

			AssertEquals("0", minOccursNodeSerie);
			AssertEquals("0", minOccursNodeNro);

			var nodeIdDoc_Tck = root.SelectSingleNode("//xs:complexType[@name='IdDoc_Tck']", nsmgr);
			nodeSerie = nodeIdDoc_Tck.SelectSingleNode("//xs:element[@name='Serie']", nsmgr);
			nodeNro = nodeIdDoc_Tck.SelectSingleNode("//xs:element[@name='Nro']", nsmgr);
			minOccursNodeSerie = nodeSerie.Attributes["minOccurs"].Value;
			minOccursNodeNro = nodeNro.Attributes["minOccurs"].Value;

			AssertEquals("0", minOccursNodeSerie);
			AssertEquals("0", minOccursNodeNro);
		}
	}
}
