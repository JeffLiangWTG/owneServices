using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(AttributeMapItem))]
	class AttributeMapItemNonPersistentObjectTestCase : NonPersistentBusinessObjectTestCase
	{
	}

	class AttributeMapItemTest : TestCaseWithFactory
	{
		public void TestColumnNameInfoIsCorrespondingToColumnName()
		{
			var item = new AttributeMapItem(GlbStaffSchema.GS_MobilePhone, "test", true);
			AssertEquals(35, item.EnterpriseColumnNameInfo.MaxLength);
			AssertEquals(GlbStaffSchema.Constants.GS_MobilePhone, item.EnterpriseColumnNameInfo.Value);
		}

		public void TestConstructor()
		{
			var item = new AttributeMapItem(GlbStaffSchema.GS_WorkPhone, "FooBar", true);
			AssertEquals(GlbStaffSchema.Constants.GS_WorkPhone, item.EnterpriseColumnName);
			AssertEquals("FooBar", item.ActiveDirectoryAttributeName);
			Assert(item.IsSynced);
		}

		public void TestMatchesSchemaColumn()
		{
			var item = new AttributeMapItem(GlbStaffSchema.GS_LoginName, "blah", true);
			AssertEquals(true, item.Matches(GlbStaffSchema.GS_LoginName));
			AssertEquals(false, item.Matches(GlbStaffSchema.GS_Birthdate));
		}

		public void TestCloneItem()
		{
			var item = new AttributeMapItem(GlbStaffSchema.GS_LoginName, "FooBar", true);
			var clone = item.Clone();

			AssertEquals(item.EnterpriseColumnName, clone.EnterpriseColumnName);
			AssertEquals(item.ActiveDirectoryAttributeName, clone.ActiveDirectoryAttributeName);
		}

		public void TestEqualityComparer()
		{
			var item1 = new AttributeMapItem(GlbStaffSchema.GS_LoginName, "Property1", true);
			var item2 = new AttributeMapItem(GlbGroupSchema.GG_Desc, "Property2", true);
			var item3 = new AttributeMapItem(GlbStaffSchema.GS_LoginName, "Property1", true);
			var item4 = new AttributeMapItem(GlbStaffSchema.GS_LoginName, "Property4", true);
			var item5 = new AttributeMapItem(GlbStaffSchema.GS_LoginName, "Property4", false);

			var item7 = new AttributeMapItem(GlbStaffSchema.GS_EftWages, "Property4", true);
			var item8 = new AttributeMapItem(GlbStaffSchema.GS_EftWages, "Property4", false);

			var comparer = new AttributeMapItem.MapItemEqualityComparer();

			AssertEquals("Properties mismatched", false, comparer.Equals(item1, item2));
			AssertEquals("Both properties match", true, comparer.Equals(item1, item3));
			AssertEquals("Properties mismatched", false, comparer.Equals(item1, item4));
			AssertEquals("Same instance matches", true, comparer.Equals(item2, item2));
			AssertEquals("Properties mismatched", false, comparer.Equals(item2, item3));
			AssertEquals("Properties mismatched", false, comparer.Equals(item4, item5));
			AssertEquals("Properties mismatched", false, comparer.Equals(item7, item8));
		}

		public void TestEnterpriseFieldsShouldAllBeReadOnly()
		{
			foreach (AttributeMapItem item in AttributeMap.DefaultMap.MapItems)
			{
				Assert(item.EnterpriseColumnNameInfo.ReadOnly);
			}
		}

		public void TestADAttributesShouldAllBeReadOnly()
		{
			foreach (AttributeMapItem item in AttributeMap.DefaultMap.MapItems)
			{
				var readOnlyEnterpriseColumns = new string[] { GlbGroupSchema.Constants.GG_Desc, GlbStaffSchema.Constants.GS_LoginName };
				if (readOnlyEnterpriseColumns.Any(column => item.EnterpriseColumnName == column))
				{
					Assert(item.ActiveDirectoryAttributeNameInfo.ReadOnly);
				}
				else
				{
					AssertEquals(false, item.ActiveDirectoryAttributeNameInfo.ReadOnly);
				}
			}
		}

		public void TestAttributeValidation()
		{
			ADTestHelper.MockSearcherWithDomainAttributes(new[] { "name", "displayName" });

			var item1 = new AttributeMapItem(GlbStaffSchema.GS_FullName, "name", true);
			item1.RunPreSaveValidation();
			AssertHasWarning(item1.ActiveDirectoryAttributeNameInfo, "Mapping GS_FullName to 'name' attribute could affect the AD user login name including 'userPrincipalName' and 'sAMAccountName' attributes, potentially affecting the functionality of user logins and single-sign-on.");

			item1.ActiveDirectoryAttributeName = "xxxx";
			AssertHasError(item1.ActiveDirectoryAttributeNameInfo, "Enter a valid Active Directory Attribute.");

			item1.ActiveDirectoryAttributeName = "name";
			AssertHasWarning(item1.ActiveDirectoryAttributeNameInfo, "Mapping GS_FullName to 'name' attribute could affect the AD user login name including 'userPrincipalName' and 'sAMAccountName' attributes, potentially affecting the functionality of user logins and single-sign-on.");

			item1.ActiveDirectoryAttributeName = "displayName";
			Assert(!item1.HasErrors);
			Assert(!item1.HasWarnings);

			// name attribute cannot be mapped to other column except GS_FullName
			var item2 = new AttributeMapItem(GlbStaffSchema.GS_City, "name", true);
			item2.RunPreSaveValidation();
			Assert(item2.HasErrors);
			AssertHasError(item2.ActiveDirectoryAttributeNameInfo, "The 'name' attribute cannot be mapped to column other than GS_FullName or GG_Desc.");
		}
	}
}
