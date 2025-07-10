using CargoWise.EntityFramework;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StaffColumnToGroupDescriptionScimMappingCollection))]
	public class StaffColumnToGroupDescriptionScimMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<StaffColumnToGroupDescriptionScimMappingCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override StaffColumnToGroupDescriptionScimMappingCollection GetCollectionToTest()
		{
			return new StaffColumnToGroupDescriptionScimMappingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StaffColumnToGroupDescriptionScimMapping();
		}

		#endregion

		public void TestAllowNew()
		{
			Assert(Collection.AllowNew);
		}

		public void TestHasGroup()
		{
			var collection = new StaffColumnToGroupDescriptionScimMappingCollection();
			var data1 = collection.AddNew();
			data1.GroupDescriptionMapping = "Test1";
			data1.StaffColumnName = "GS_CanLogin";

			var data2 = collection.AddNew();
			data2.GroupDescriptionMapping = "Test2";
			data2.StaffColumnName = "GS_CanLogin";

			var group1 = Factory.New<IGlbGroup>();
			group1.GG_Desc = "tEsT1";

			var group2 = Factory.New<IGlbGroup>();
			group2.GG_Desc = "Test11";

			var group3 = Factory.New<IGlbGroup>();
			group3.GG_Desc = "qwerty";

			var group4 = Factory.New<IGlbGroup>();
			group4.GG_Desc = " Test2";

			AssertEquals(true, collection.HasGroup(group1));
			AssertEquals(false, collection.HasGroup(group2));
			AssertEquals(false, collection.HasGroup(group3));
			AssertEquals(false, collection.HasGroup(group4));
		}
	}
}
