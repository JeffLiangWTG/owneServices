using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(AttributeMapItemCollection))]
	class AttributeMapItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AttributeMapItemCollection>
	{
		public void TestValidation()
		{
			var adAttributesForTest = ADAttributeList.Instance.GetPredefinedAttributes().ToList();
			adAttributesForTest.Add("manager");
			adAttributesForTest.Add("middleName");
			ADTestHelper.MockSearcherWithDomainAttributes(adAttributesForTest);

			var mapItems = GetCollectionToTest();
			mapItems.Add(new AttributeMapItem("GS_City", "l", true));
			mapItems.Add(new AttributeMapItem("GS_MobilePhone", "middleName", true));
			mapItems.Add(new AttributeMapItem("GS_FullName", "name", true));
			mapItems.Add(new AttributeMapItem("GS_State", "l", false));
			mapItems.Add(new AttributeMapItem("GG_Desc", "name", true));

			mapItems.RunPreSaveValidation();

			AssertHasError("GS_City", mapItems[GlbStaffSchema.GS_City].ActiveDirectoryAttributeNameInfo, "The 'l' attribute has been mapped to more than one column in GlbStaff table.");
			AssertHasError("GS_State", mapItems[GlbStaffSchema.GS_State].ActiveDirectoryAttributeNameInfo, "The 'l' attribute has been mapped to more than one column in GlbStaff table.");

			AssertEquals("GS_MobilePhone", false, mapItems[GlbStaffSchema.GS_MobilePhone].HasErrors);
			AssertEquals("GS_FullName", false, (mapItems[GlbStaffSchema.GS_FullName].HasErrors));
			AssertEquals("GG_Desc", false, (mapItems[GlbGroupSchema.GG_Desc].HasErrors));
		}

		#region Implementation

		protected override AttributeMapItemCollection GetCollectionToTest()
		{
			return new AttributeMapItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AttributeMapItem();
		}

		public override void TestDelete()
		{
			Assert("Does not allow delete", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("Does not allow remove", true);
		}

		protected override void SetUp()
		{
			ADTestHelper.MockSearcherWithDomainAttributes(null);
			base.SetUp();
		}
		#endregion
	}
}
