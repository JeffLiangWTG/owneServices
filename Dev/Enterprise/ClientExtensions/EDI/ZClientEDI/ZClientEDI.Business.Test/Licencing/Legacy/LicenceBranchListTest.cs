using System.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	class LicenceBranchListTest : TestCase
	{
		LicenceBranch GetNewLicenceBranch()
		{
			return new LicenceBranch();
		}

		[ExpectNoExceptions()]
		public void TestEmptyConstructor()
		{
			LicenceBranchList testLicenceBranchList = new LicenceBranchList();
		}

		public void TestConstructorWithLicenceBranchList()
		{
			LicenceBranchList testLicenceBranchList1 = new LicenceBranchList();
			testLicenceBranchList1.Add(GetNewLicenceBranch());
			testLicenceBranchList1.Add(GetNewLicenceBranch());
			testLicenceBranchList1.Add(GetNewLicenceBranch());

			LicenceBranchList testLicenceBranchList2 = new LicenceBranchList(testLicenceBranchList1);
			AssertEquals(3, testLicenceBranchList2.Count);
		}

		public void TestConstructorWithArray()
		{
			LicenceBranchList testLicenceBranchList = new LicenceBranchList(new LicenceBranch[] { GetNewLicenceBranch(), GetNewLicenceBranch() });
			AssertEquals(2, testLicenceBranchList.Count);
		}

		public void TestToAndFromXml()
		{
			LicenceBranch branch1 = new LicenceBranch();
			branch1.Set("AAA", "AAA Branch", "Address1", "Address2", "City", "State", "2000", "90251111", "90252222", "test@edi.com.au", "edi.com.au", "AUSYD");

			LicenceBranch branch2 = new LicenceBranch();
			branch2.Set("BBB", "BBB Branch", "Address1", "Address2", "City", "State", "2000", "90251111", "90252222", "test@edi.com.au", "edi.com.au", "AUSYD");

			LicenceBranch branch3 = new LicenceBranch();
			branch3.Set("CCC", "CCC Branch", "Address1", "Address2", "City", "State", "2000", "90251111", "90252222", "test@edi.com.au", "edi.com.au", "AUSYD");

			LicenceBranchList testList = new LicenceBranchList();
			testList.Add(branch1);
			testList.Add(branch2);
			testList.Add(branch3);

			XmlDocument xmlDoc = new XmlDocument();
			XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, "TestNode", "");

			testList.AddToLicenceNode(node);

			LicenceBranchList loadedList = new LicenceBranchList();
			loadedList.LoadFromLicenceNode(node);

			AssertEquals("Count", 3, loadedList.Count);
			AssertEquals("Branch1", branch1.Code, loadedList[0].Code);
			AssertEquals("Branch2", branch2.Code, loadedList[1].Code);
			AssertEquals("Branch3", branch3.Code, loadedList[2].Code);
		}
	}
}