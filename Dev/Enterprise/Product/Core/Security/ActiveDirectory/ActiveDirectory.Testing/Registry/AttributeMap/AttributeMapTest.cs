using System;
using System.Text.RegularExpressions;
using CargoWise.ActiveDirectory;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(AttributeMap))]
	class AttributeMapNonPersistentObjectTestCase : NonPersistentBusinessObjectTestCase
	{
	}

	[TestedType(typeof(AttributeMap))]
	class AttributeMapTest : RegistryBusinessObjectTemplateTestCase<AttributeMap>
	{
		public void TestDefaultMapFields()
		{
			AssertEquals(GlbStaffSchema.Constants.TableName, mapForTesting.MapItems[GlbStaffSchema.GS_LoginName].EnterpriseTableName);
			AssertEquals(GlbStaffSchema.GS_LoginName.Name, mapForTesting.MapItems[GlbStaffSchema.GS_LoginName].EnterpriseColumnName);
			AssertEquals(ADAttributes.UserPrincipalName, mapForTesting.MapItems[GlbStaffSchema.GS_LoginName].ActiveDirectoryAttributeName);

			AssertEquals(GlbStaffSchema.Constants.TableName, mapForTesting.MapItems[GlbStaffSchema.GS_IsActive].EnterpriseTableName);
			AssertEquals(GlbStaffSchema.GS_IsActive.Name, mapForTesting.MapItems[GlbStaffSchema.GS_IsActive].EnterpriseColumnName);
			AssertEquals(ADAttributes.UserAccountControl, mapForTesting.MapItems[GlbStaffSchema.GS_IsActive].ActiveDirectoryAttributeName);

			AssertEquals(GlbStaffSchema.Constants.TableName, mapForTesting.MapItems[GlbStaffSchema.GS_FullName].EnterpriseTableName);
			AssertEquals(GlbStaffSchema.GS_FullName.Name, mapForTesting.MapItems[GlbStaffSchema.GS_FullName].EnterpriseColumnName);
			AssertEquals(ADAttributes.DisplayName, mapForTesting.MapItems[GlbStaffSchema.GS_FullName].ActiveDirectoryAttributeName);

			AssertEquals(GlbGroupSchema.Constants.TableName, mapForTesting.MapItems[GlbGroupSchema.GG_Desc].EnterpriseTableName);
			AssertEquals(GlbGroupSchema.GG_Desc.Name, mapForTesting.MapItems[GlbGroupSchema.GG_Desc].EnterpriseColumnName);
			AssertEquals(ADAttributes.Name, mapForTesting.MapItems[GlbGroupSchema.GG_Desc].ActiveDirectoryAttributeName);
			AssertEquals(true, mapForTesting.MapItems[GlbGroupSchema.GG_Desc].IsSynced);

			AssertEquals(GlbStaffSchema.Constants.TableName, mapForTesting.MapItems[GlbStaffSchema.GS_WorkingLanguage].EnterpriseTableName);
			AssertEquals(GlbStaffSchema.GS_WorkingLanguage.Name, mapForTesting.MapItems[GlbStaffSchema.GS_WorkingLanguage].EnterpriseColumnName);
			AssertEquals(ADAttributes.PreferredLanguage, mapForTesting.MapItems[GlbStaffSchema.GS_WorkingLanguage].ActiveDirectoryAttributeName);
			AssertEquals(false, mapForTesting.MapItems[GlbStaffSchema.GS_WorkingLanguage].IsSynced);

			AssertEquals("CurrentDRMManager should not be sync for non-EDI client", false, mapForTesting.IsSynced(MasterFiles.Business.GlbStaff.Schema.CurrentDRMManager));

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				mapForTesting = new PropertyMapForTesting(AttributeMap.DefaultMap);
				AssertEquals("CurrentDRMManager should be sync for EDI client", true, mapForTesting.IsSynced(MasterFiles.Business.GlbStaff.Schema.CurrentDRMManager));
			}
		}

		public void TestActiveDirectoryPropertyFromSchema()
		{
			AssertEquals("userPrincipalName", mapForTesting.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_LoginName));
			AssertEquals("userAccountControl", mapForTesting.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_IsActive));
			AssertEquals("displayName", mapForTesting.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_FullName));
			AssertEquals("name", mapForTesting.GetActiveDirectoryAttributeFromSchema(GlbGroupSchema.GG_Desc));

			AssertExceptionThrown<NotSupportedException>(() => mapForTesting.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_IsDeveloper));
		}

		public void TestChangePropertyMapAndGetPropertyFromSchema()
		{
			var map = new AttributeMap();
			var mapItem = new AttributeMapItem(GlbStaffSchema.GS_LoginName, ADAttributes.UserPrincipalName, true);
			map.MapItems.Add(mapItem);

			AssertEquals("userPrincipalName", map.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_LoginName));

			mapItem.EnterpriseColumnName = "GS_IsActive";
			AssertEquals("userPrincipalName", map.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_IsActive));

			mapItem.EnterpriseColumnName = "GG_Desc";
			AssertEquals("userPrincipalName", map.GetActiveDirectoryAttributeFromSchema(GlbGroupSchema.GG_Desc));

			mapItem.ActiveDirectoryAttributeName = "SomethingCompletelyDifferent";
			AssertEquals("SomethingCompletelyDifferent", map.GetActiveDirectoryAttributeFromSchema(GlbGroupSchema.GG_Desc));

			mapItem.EnterpriseColumnName = "Blah";
			AssertEquals(true, mapItem.HasErrors);

			mapItem.EnterpriseColumnName = "GG_Desc";
			AssertEquals(false, mapItem.HasErrors);
		}

		public void TestGetClone()
		{
			var clone = mapForTesting.GetClone_Exposed();
			AssertEquals(mapForTesting.MapItems.Count, clone.MapItems.Count);

			for (int i = 0; i < mapForTesting.MapItems.Count; i++)
			{
				AssertEquals(mapForTesting.MapItems[i].EnterpriseTableName, clone.MapItems[i].EnterpriseTableName);
				AssertEquals(mapForTesting.MapItems[i].EnterpriseColumnName, clone.MapItems[i].EnterpriseColumnName);
				AssertEquals(mapForTesting.MapItems[i].ActiveDirectoryAttributeName, clone.MapItems[i].ActiveDirectoryAttributeName);
			}
		}

		public void TestToStringForSerialisation()
		{
			string mapAsString = mapForTesting.ToStringForSerialisation();

			var regex = new Regex(@"(.._\w*\|\w*\|[Y|N],)+");
			var match = regex.Match(mapAsString);
			Assert("Should match regex pattern", match.Success);
			AssertEquals("Should match entire string", mapAsString.Length, match.Groups[0].Value.Length);
		}

		public void TestEquals()
		{
			var map1 = AttributeMap.DefaultMap;
			var map2 = AttributeMap.DefaultMap;

			Assert(!Object.ReferenceEquals(map1, map2));
			Assert(map1.Equals(map2));
		}

		public void TestGetHashCode()
		{
			var map = AttributeMap.DefaultMap;
			var expectedHashCode = 0;
			foreach (AttributeMapItem item in mapForTesting.MapItems)
			{
				expectedHashCode ^= item.EnterpriseColumnName.GetHashCode() ^ item.ActiveDirectoryAttributeName.GetHashCode() ^ item.IsSynced.GetHashCode();
			}
			AssertEquals(expectedHashCode, map.GetHashCode());
		}

		public void TestIsSynced_ByColumnName()
		{
			AssertIsSynced((map, column) => map.IsSynced(column.Name));
		}

		public void TestIsSynced_BySchemaColumn()
		{
			AssertIsSynced((map, column) => map.IsSynced(column));
		}

		void AssertIsSynced(Func<AttributeMap, SchemaColumn, bool> isSynced)
		{
			var map = AttributeMap.DefaultMap;
			map.MapItems[GlbStaffSchema.GS_LoginName].IsSynced = true;
			map.MapItems[GlbStaffSchema.GS_IsActive].IsSynced = false;
			map.MapItems[GlbStaffSchema.GS_EmailAddress].IsSynced = true;
			map.MapItems[GlbStaffSchema.GS_Title].IsSynced = false;
			map.MapItems[GlbStaffSchema.GS_WorkPhone].IsSynced = false;
			map.MapItems[GlbGroupSchema.GG_Desc].IsSynced = false;

			Assert("GS_LoginName should be synced", isSynced(map, GlbStaffSchema.GS_LoginName));
			Assert("GS_IsActive should be not be synced", !isSynced(map, GlbStaffSchema.GS_IsActive));
			Assert("GS_EmailAddress should be synced", isSynced(map, GlbStaffSchema.GS_EmailAddress));
			Assert("GS_Title should not be synced", !isSynced(map, GlbStaffSchema.GS_Title));
			Assert("GS_WorkPhone should not be synced", !isSynced(map, GlbStaffSchema.GS_WorkPhone));
			Assert("GG_Desc should not be synced", !isSynced(map, GlbGroupSchema.GG_Desc));
		}

		#region Implementation

		protected override void SetUp()
		{
			ADTestHelper.MockSearcherWithDomainAttributes(null);
			mapForTesting = new PropertyMapForTesting(AttributeMap.DefaultMap);
			base.SetUp();
		}
		PropertyMapForTesting mapForTesting;

		class PropertyMapForTesting : AttributeMap
		{
			internal PropertyMapForTesting(AttributeMap source)
			{
				foreach (AttributeMapItem item in source.MapItems)
				{
					this.MapItems.Add(item.Clone());
				}
			}

			internal AttributeMap GetClone_Exposed() => (AttributeMap)GetClone(null, null);
		}

		protected override AttributeMap GetBusinessObjectToClone() => AttributeMap.DefaultMap;

		protected override AttributeMap GetBusinessObjectToSerialise() => AttributeMap.DefaultMap;

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		#endregion
	}
}
