using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrgDebtorGroupCodeListCollection))]
	sealed class OrgDebtorGroupCodeListCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgDebtorGroupCodeListCollection>
	{
		public void TestToGuidArray()
		{
			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			Guid[] guidList = new Guid[] { guid1, guid2 };

			OrgDebtorGroupCodeListCollection collection = new OrgDebtorGroupCodeListCollection(guidList);
			Guid[] newList = collection.ToGuidArray();
			AssertNotNull("NewList", newList);
			AssertEquals("NewList.Length", 2, newList.Length);
			AssertCollectionContains("NewList should contain Guid1", guid1, newList);
			AssertCollectionContains("NewList should contain Guid2", guid2, newList);
		}

		public void TestToString()
		{
			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			Guid[] guidList = new Guid[] { guid1, guid2 };

			OrgDebtorGroupCodeListCollection collection = new OrgDebtorGroupCodeListCollection(guidList);
			string expectedString = guid1.ToString() + "," + guid2.ToString();
			Guid[] newList = collection.ToGuidArray();
			AssertNotNull("NewList", newList);
			AssertEquals("ToString", expectedString, collection.ToString());
		}

		public void TestOrgDebtorGroupCollection()
		{
			OrgDebtorGroup accountGroup1 = Factory.New<OrgDebtorGroup>();
			accountGroup1.OJ_Code = "WWW";
			accountGroup1.OJ_Desc = "World Wide Web";
			((ILightValidationInternals)accountGroup1).IsValid = true;

			OrgDebtorGroup accountGroup2 = Factory.New<OrgDebtorGroup>();
			accountGroup2.OJ_Code = "ABC";
			accountGroup2.OJ_Desc = "Alphabet";
			((ILightValidationInternals)accountGroup2).IsValid = false;

			Factory.Save();

			OrgDebtorGroupCodeListCollection codeListCollection = new OrgDebtorGroupCodeListCollection(Factory);
			AssertEquals(1, Factory.GetDatabaseCount(typeof(OrgDebtorGroup), new ZQuery(OrgDebtorGroupSchema.OJ_IsValid, ZBool.True)));

			codeListCollection.OrgDebtorGroupCollection.Load();
			AssertEquals("Should contain only 1 group", 1, codeListCollection.OrgDebtorGroupCollection.Count);
			AssertEquals("OJ_Code", "WWW", codeListCollection.OrgDebtorGroupCollection[0].OJ_Code);
			AssertEquals("OJ_Desc", "World Wide Web", codeListCollection.OrgDebtorGroupCollection[0].OJ_Desc);
		}

		#region overriden

		protected override OrgDebtorGroupCodeListCollection GetCollectionToTest()
		{
			return new OrgDebtorGroupCodeListCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgDebtorGroupCodeListElement(ZGuid.NewZGuid(), new OrgDebtorGroupCodeListCollection(Factory));
		}

		#endregion
	}
}
