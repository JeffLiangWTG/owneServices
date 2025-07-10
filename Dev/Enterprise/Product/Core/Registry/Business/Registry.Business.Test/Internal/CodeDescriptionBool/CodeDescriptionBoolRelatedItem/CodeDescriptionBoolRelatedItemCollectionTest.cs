using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolRelatedItemCollection))]
	sealed class CodeDescriptionBoolRelatedItemCollectionTest : RegistryBusinessObjectCollectionTestCase<CodeDescriptionBoolRelatedItemCollection>
	{
		public void TestAddSetsParent()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();

			var bool1 = collection.AddNew();
			AssertEquals(collection, bool1.Parent);

			var bool2 = new CodeDescriptionBoolRelatedItem();
			collection.Add(bool2);
			AssertEquals(collection, bool2.Parent);
		}

		public void TestRelatedItemDescription()
		{
			var opportunitySources = new CodeDescriptionBoolRelatedItemCollection();
			opportunitySources.Add("WEB", (NoResString)"Website", true);
			Assert(opportunitySources[0].RelatedItemDescription.Length >= 0);
		}

		public void TestGetRelatedItemList()
		{
			CodeDescriptionBoolCollection relatedCollection = new CodeDescriptionBoolCollection();
			relatedCollection.Add("R1", (NoResString)"Desc R1", true);
			relatedCollection.Add("R2", (NoResString)"Desc R2", false);
			relatedCollection.Add("R3", (NoResString)"Desc R3", true);
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, relatedCollection);

			CodeDescriptionBoolRelatedItemCollection collection = new CodeDescriptionBoolRelatedItemCollection();
			CodeDescriptionBoolRelatedItem multilingaulBool1 = collection.AddNew();
			multilingaulBool1.Code = "aaa";
			multilingaulBool1.Description = (NoResString)"Desc AAA";
			multilingaulBool1.RelatedItemCode = "CL2";
			CodeDescriptionBoolRelatedItem multilingualBool2 = collection.AddNew();
			multilingualBool2.Code = "bBb";
			multilingualBool2.Description = (NoResString)"Desc BBB";

			CodeDescriptionPairList list1 = collection.GetRelatedItemList("AAA");
			AssertEquals(3, list1.Count);
			AssertEquals(true, list1.ContainsCode("R1"));
			AssertEquals(true, list1.ContainsCode("R2"));
			AssertEquals(true, list1.ContainsCode("R3"));

			CodeDescriptionPairList list2 = collection.GetActiveRelatedItemList("AAA");
			AssertEquals(2, list2.Count);
			AssertEquals(true, list2.ContainsCode("R1"));
			AssertEquals(false, list2.ContainsCode("R2"));
			AssertEquals(true, list2.ContainsCode("R3"));

			CodeDescriptionPairList list3 = collection.GetActiveRelatedItemList("BBB");
			AssertEquals(0, list3.Count);

			CodeDescriptionPairList list4 = collection.GetActiveRelatedItemList("CCC");
			AssertEquals(0, list4.Count);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CodeDescriptionBoolRelatedItemCollection GetCollectionToTest()
		{
			return new CodeDescriptionBoolRelatedItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionBoolRelatedItem();
		}

		public void TestInterfaceMembers()
		{
			CodeDescriptionBoolRelatedItem element1 = Collection.AddNew();
			CodeDescriptionBoolRelatedItem element2 = Collection.AddNew();

			element1.Code = "ABC";
			element2.Code = "XYZ";
			element1.Description = (NoResString)"Desc ABC";
			element2.Description = (NoResString)"Desc XYZ";
			element1.RelatedItemCode = "ABCD";
			element2.RelatedItemCode = "XYZ0";

			ICodeDescriptionBoolRelatedItemList relatedItemList = Collection;

			AssertEquals("RelatedItemList[0]", element1, relatedItemList[0]);
			AssertEquals("RelatedItemList[1]", element2, relatedItemList[1]);

			ICodeDescriptionBoolList boolList = Collection;

			AssertEquals("BoolList.ContainsCode(\"ABC\")", true, boolList.ContainsCode("ABC"));
			AssertEquals("BoolList.ContainsCode(\"XYZ\")", true, boolList.ContainsCode("XYZ"));
			AssertEquals("BoolList.ContainsCode(\"!@#\")", false, boolList.ContainsCode("!@#"));
		}
	}
}
