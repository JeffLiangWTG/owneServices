using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrgDebtorGroupCodeListElement))]
	sealed class OrgDebtorGroupCodeListElementTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			OrgDebtorGroupCodeListCollection collection = new OrgDebtorGroupCodeListCollection(Factory);
			OrgDebtorGroupCodeListElement bizO = new OrgDebtorGroupCodeListElement(ZGuid.Empty, collection);
			return bizO;
		}

		public void TestOrgDebtorGroupCodeListElement()
		{
			OrgDebtorGroupCodeListElement elem = (OrgDebtorGroupCodeListElement)GetNewBusinessObject();
			OrgDebtorGroup group = Factory.New<OrgDebtorGroup>();
			group.OJ_Code = "WWW";
			group.OJ_Desc = "World Wide Web";
			Factory.Save();

			elem.GroupGuid = group.PK;
			AssertEquals("GroupGuid", group.PK, elem.GroupGuid);
			AssertEquals("GroupDescription", group.OJ_Desc, elem.GroupDescription);
			AssertEquals("Group", group.PK, elem.DebtorGroup.PK);
			AssertEquals("OrgClient", group.OJ_Code, elem.DebtorGroup.OJ_Code);

			elem.GroupGuid = ZGuid.Empty;
			AssertEquals("GroupGuid", ZGuid.Empty, elem.GroupGuid);
			AssertEquals("GroupDescription", "", elem.GroupDescription);
			AssertNull("DebtorGroup", elem.DebtorGroup);
		}
	}
}
