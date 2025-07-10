using System.Globalization;
using System.Xml;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	class LicenceInstallationDetailsTest : LicenceSegmentTest
	{
		public void TestAddToLicenceNode()
		{
			string[] expectedAttributes = new string[] { "AMSMode" };

			XmlNode testNode = NodeOfInstallationDetails; // This calls AddToLicenceNode
			for (int i = 0; i < testNode.Attributes.Count; i++)
			{
				AssertEquals(string.Format(CultureInfo.CurrentCulture, "Node {0} should match the expected item in order", i), expectedAttributes[i], testNode.Attributes[i].Name);
			}
		}

		public void TestLoadFromLicenceNode()
		{
			LicenceInstallationDetails testInstallationDetails = new LicenceInstallationDetails();
			AssertEquals("OFF", testInstallationDetails.AMSMode);

			testInstallationDetails.LoadFromLicenceNode(NodeOfInstallationDetails);
			AssertEquals("ON", testInstallationDetails.AMSMode);
		}

		public void TestDeprecatedOnDemandModeFlag()
		{
			LicenceInstallationDetails installationDetails = new LicenceInstallationDetails();
			Assert(!((IExposeDeprecatedOnDemandModeFlag)installationDetails).OnDemandMode);

			XmlDocument document = new XmlDocument();
			XmlNode node = document.CreateNode(XmlNodeType.Element, "TestNode", "");
			XmlAttribute attribute = document.CreateAttribute("OnDemand");
			attribute.Value = bool.TrueString;
			node.Attributes.Append(attribute);
			installationDetails.LoadFromLicenceNode(node);
			Assert(((IExposeDeprecatedOnDemandModeFlag)installationDetails).OnDemandMode);

			node.RemoveAll();
			LicenceInstallationDetails anotherInstallationDetails = new LicenceInstallationDetails();
			AssertEquals("Precondition", 0, node.Attributes.Count);
			AssertEquals("Precondition", "OFF", anotherInstallationDetails.AMSMode);

			((IExposeDeprecatedOnDemandModeFlag)anotherInstallationDetails).OnDemandMode = true;
			anotherInstallationDetails.AddToLicenceNode(node);
			AssertEquals(true.ToString(), node.Attributes["OnDemand"].Value);
		}

		#region Implementation

		XmlNode NodeOfInstallationDetails
		{
			get
			{
				LicenceInstallationDetails testInstallDetails = new LicenceInstallationDetails();
				testInstallDetails.Set("ON");

				XmlDocument xmlDoc = new XmlDocument();
				XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, "TestNode", "");
				testInstallDetails.AddToLicenceNode(node);

				return node;
			}
		}

		#endregion
	}
}