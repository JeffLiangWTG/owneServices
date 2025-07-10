using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrgDebtorGroupCodeListCollectionWrapper))]
	sealed class OrgDebtorGroupCodeListCollectionWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			OrgDebtorGroupCodeListCollectionWrapper bizO = new OrgDebtorGroupCodeListCollectionWrapper(Array.Empty<Guid>());
			return bizO;
		}

		public void TestOrgHeaderCodeListCollectionWrapper()
		{
			ZGuid guid1 = ZGuid.NewZGuid();
			ZGuid guid2 = ZGuid.NewZGuid();
			ZGuid guid3 = ZGuid.NewZGuid();
			ZGuid guid4 = ZGuid.NewZGuid();

			Guid[] value1 = new Guid[3];
			value1[0] = guid1.ToGuid();
			value1[1] = guid2.ToGuid();
			value1[2] = guid3.ToGuid();

			string value2 = guid1 + "," + guid3 + "," + guid4;

			OrgDebtorGroupCodeListCollectionWrapper bizO = new OrgDebtorGroupCodeListCollectionWrapper(value1);
			AssertEquals("ClientGuid[0]", guid1, bizO.OrgDebtorGroupCodeList[0].GroupGuid);
			AssertEquals("ClientGuid[1]", guid2, bizO.OrgDebtorGroupCodeList[1].GroupGuid);
			AssertEquals("ClientGuid[2]", guid3, bizO.OrgDebtorGroupCodeList[2].GroupGuid);

			bizO.OrgDebtorGroupCodeList.Remove(bizO.OrgDebtorGroupCodeList[1]);
			OrgDebtorGroupCodeListElement elem = bizO.OrgDebtorGroupCodeList.AddNew();
			elem.GroupGuid = guid4;
			AssertEquals("Value", value2, bizO.OrgDebtorGroupCodeList.ToString());
		}
	}
}
