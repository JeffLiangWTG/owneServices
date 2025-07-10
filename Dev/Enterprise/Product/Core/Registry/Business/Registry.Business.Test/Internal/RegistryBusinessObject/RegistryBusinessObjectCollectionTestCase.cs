using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestsSubclassesOf(typeof(RegistryBusinessObjectCollection))]
	public abstract class RegistryBusinessObjectCollectionTestCase<T> : RegistryBusinessObjectCollectionTemplateTestCase<T> where T : RegistryBusinessObjectCollection
	{
		public void TestContainsCode()
		{
			AssertEquals("Precondition: Collection should not contain Code \"ABC\"", false, Collection.ContainsCode("ABC"));
			AssertEquals("Precondition: Collection should not contain Code \"XYZ\"", false, Collection.ContainsCode("XYZ"));
			AssertEquals("Precondition: Collection should not contain Code \"!@#\"", false, Collection.ContainsCode("!@#"));

			RegistryBusinessObject bizObj1 = Collection.AddNew();
			RegistryBusinessObject bizObj2 = Collection.AddNew();

			bizObj1.Code = "ABC";
			bizObj2.Code = "XYZ";

			AssertEquals("Collection should contain Code \"ABC\"", true, Collection.ContainsCode("ABC"));
			AssertEquals("Collection should contain Code \"XYZ\"", true, Collection.ContainsCode("XYZ"));
			AssertEquals("Collection should not contain Code \"!@#\"", false, Collection.ContainsCode("!@#"));
		}

		public void TestFindByCode()
		{
			AssertEquals("Precondition: FindByCode(\"ABC\") should be null", null, Collection.FindByCode("ABC"));
			AssertEquals("Precondition: FindByCode(\"XYZ\") should be null", null, Collection.FindByCode("XYZ"));
			AssertEquals("Precondition: FindByCode(\"!@#\") should be null", null, Collection.FindByCode("!@#"));

			RegistryBusinessObject bizObj1 = Collection.AddNew();
			RegistryBusinessObject bizObj2 = Collection.AddNew();

			bizObj1.Code = "ABC";
			bizObj2.Code = "XYZ";

			AssertEquals("FindByCode(\"ABC\") should be BizObj1", bizObj1, Collection.FindByCode("ABC"));
			AssertEquals("FindByCode(\"XYZ\") should be BizObj2", bizObj2, Collection.FindByCode("XYZ"));
			AssertEquals("FindByCode(\"!@#\") should be null", null, Collection.FindByCode("!@#"));
		}

		public virtual void TestGetCodeDescriptionPairList()
		{
			AssertEquals("GetCodeDescriptionPairList().Count", 0, Collection.GetCodeDescriptionPairList().Count);

			RegistryBusinessObject element1 = Collection.AddNew();
			RegistryBusinessObject element2 = Collection.AddNew();

			element1.Code = "E1";
			element2.Code = "E2";
			element1.Description = (NoResString)new ZString("Element 1").SubstringSafe(0, element1.Description_MaxLength);
			element2.Description = (NoResString)new ZString("Element 2").SubstringSafe(0, element2.Description_MaxLength);

			CodeDescriptionPairList list = Collection.GetCodeDescriptionPairList();
			AssertEquals("GetCodeDescriptionPairList().Count", 2, list.Count);
			AssertEquals("GetCodeDescriptionPairList()[0].Code", "E1", list[0].Code);
			AssertEquals("GetCodeDescriptionPairList()[0].Description", new ZString("Element 1").SubstringSafe(0, element1.Description_MaxLength), list[0].Description);
			AssertEquals("GetCodeDescriptionPairList()[1].Code", "E2", list[1].Code);
			AssertEquals("GetCodeDescriptionPairList()[1].Description", new ZString("Element 2").SubstringSafe(0, element1.Description_MaxLength), list[1].Description);
		}
	}
}
