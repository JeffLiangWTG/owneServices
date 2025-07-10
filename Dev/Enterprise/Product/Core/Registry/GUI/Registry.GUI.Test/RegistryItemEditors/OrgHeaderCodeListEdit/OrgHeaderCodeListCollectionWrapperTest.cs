using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrgHeaderCodeListCollectionWrapper))]
	sealed class OrgHeaderCodeListCollectionWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeaderCodeListCollectionWrapper bizO = new OrgHeaderCodeListCollectionWrapper(Array.Empty<Guid>());
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

			OrgHeaderCodeListCollectionWrapper bizO = new OrgHeaderCodeListCollectionWrapper(value1);
			AssertEquals("ClientGuid[0]", guid1, bizO.OrgHeaderCodeList[0].ClientGuid);
			AssertEquals("ClientGuid[1]", guid2, bizO.OrgHeaderCodeList[1].ClientGuid);
			AssertEquals("ClientGuid[2]", guid3, bizO.OrgHeaderCodeList[2].ClientGuid);

			bizO.OrgHeaderCodeList.Remove(bizO.OrgHeaderCodeList[1]);
			OrgHeaderCodeListElement elem = bizO.OrgHeaderCodeList.AddNew();
			elem.ClientGuid = guid4;
			AssertEquals("Value", value2, bizO.OrgHeaderCodeList.ToString());
		}
	}
}
