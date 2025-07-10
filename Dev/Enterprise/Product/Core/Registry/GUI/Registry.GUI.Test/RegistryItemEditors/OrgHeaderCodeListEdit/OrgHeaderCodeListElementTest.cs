using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrgHeaderCodeListElement))]
	sealed class OrgHeaderCodeListElementTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeaderCodeListCollection collection = new OrgHeaderCodeListCollection(Factory);
			OrgHeaderCodeListElement bizO = new OrgHeaderCodeListElement(ZGuid.Empty, collection);
			return bizO;
		}

		public void TestOrgHeaderCodeListElement()
		{
			OrgHeaderCodeListElement elem = (OrgHeaderCodeListElement)GetNewBusinessObject();
			var newOrg = Factory.New<OrgHeader>();
			newOrg.OH_Code = "TESTCODE";
			newOrg.OH_FullName = "FULL TEST NAME";
			Factory.Save();

			elem.ClientGuid = newOrg.PK;
			AssertEquals("ClientGuid", newOrg.PK, elem.ClientGuid);
			AssertEquals("ClientName", newOrg.OH_FullName, elem.ClientName);
			AssertEquals("OrgClient", newOrg.PK, elem.OrgClient.PK);

			elem.ClientGuid = ZGuid.Empty;
			AssertEquals("ClientGuid", ZGuid.Empty, elem.ClientGuid);
			AssertEquals("ClientName", "", elem.ClientName);
			AssertNull("OrgClient", elem.OrgClient);
		}
	}
}
