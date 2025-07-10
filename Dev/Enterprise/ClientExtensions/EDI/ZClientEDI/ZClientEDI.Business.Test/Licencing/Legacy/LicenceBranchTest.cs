using System.Globalization;
using System.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	class LicenceBranchTest : TestCase
	{
		public void TestAddToLicenceNode()
		{
			string[] expectedAttributes = new string[] {
				"Code", "BranchName", "Address1", "Address2", "City", "State", "PostCode", "Phone", "Fax", "Email", "WebAddress", "HomePortNK"
			};

			XmlNode testNode = NodeOfBranchDetails; // This calls AddToLicenceNode
			for (int i = 0; i < testNode.Attributes.Count; i++)
			{
				AssertEquals(string.Format(CultureInfo.CurrentCulture, "Node {0} should match the expected item in order", i), expectedAttributes[i], testNode.Attributes[i].Name);
			}
		}

		public void TestLoadFromLicenceNode()
		{
			LicenceBranch testBranchDetails = new LicenceBranch();
			testBranchDetails.LoadFromLicenceNode(NodeOfBranchDetails);

			AssertEquals("ABC", testBranchDetails.Code);
			AssertEquals("ABC Branch", testBranchDetails.BranchName);
			AssertEquals("Address1", testBranchDetails.Address1);
			AssertEquals("Address2", testBranchDetails.Address2);
			AssertEquals("City", testBranchDetails.City);
			AssertEquals("State", testBranchDetails.State);
			AssertEquals("2000", testBranchDetails.PostCode);
			AssertEquals("90251111", testBranchDetails.Phone);
			AssertEquals("90252222", testBranchDetails.Fax);
			AssertEquals("test@edi.com.au", testBranchDetails.Email);
			AssertEquals("edi.com.au", testBranchDetails.WebAddress);
			AssertEquals("AUSYD", testBranchDetails.HomePortNK);
		}

		XmlNode NodeOfBranchDetails
		{
			get
			{
				LicenceBranch branch = new LicenceBranch();
				branch.Set("ABC", "ABC Branch", "Address1", "Address2", "City", "State", "2000", "90251111", "90252222", "test@edi.com.au", "edi.com.au", "AUSYD");

				XmlDocument xmlDoc = new XmlDocument();
				XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, "TestNode", "");
				branch.AddToLicenceNode(node);

				return node;
			}
		}
	}
}