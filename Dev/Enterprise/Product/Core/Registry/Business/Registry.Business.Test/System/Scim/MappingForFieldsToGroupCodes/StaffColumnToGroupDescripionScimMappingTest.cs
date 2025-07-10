using System.Linq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StaffColumnToGroupDescriptionScimMapping))]
	public class StaffColumnToGroupDescriptionScimMappingTest : RegistryBusinessObjectTemplateTestCase<StaffColumnToGroupDescriptionScimMapping>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override StaffColumnToGroupDescriptionScimMapping GetBusinessObjectToClone()
		{
			return new StaffColumnToGroupDescriptionScimMapping();
		}

		protected override StaffColumnToGroupDescriptionScimMapping GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion

		#region Test Validation

		public void TestValidateGroupDescriptionMapping()
		{
			var staffColumnToGroupDescriptionScimMapping = new StaffColumnToGroupDescriptionScimMapping();
			AssertNoErrors(staffColumnToGroupDescriptionScimMapping.GroupDescriptionMappingInfo);

			staffColumnToGroupDescriptionScimMapping.ValidateGroupDescriptionMapping();
			AssertHasError(staffColumnToGroupDescriptionScimMapping.GroupDescriptionMappingInfo, "Please enter a value.");

			var collection = new StaffColumnToGroupDescriptionScimMappingCollection();
			var staffColumnToGroupDescriptionScimMapping3 = collection.AddNew();
			staffColumnToGroupDescriptionScimMapping3.GroupDescriptionMapping = "group1";
			var staffColumnToGroupDescriptionScimMapping4 = collection.AddNew();
			staffColumnToGroupDescriptionScimMapping4.GroupDescriptionMapping = "group1";
			AssertHasError(staffColumnToGroupDescriptionScimMapping4.GroupDescriptionMappingInfo, "The Group Description Mapping has been duplicated and must be unique.");
		}

		public void TestValidateStaffColumnName()
		{
			var staffColumnToGroupDescriptionScimMapping = new StaffColumnToGroupDescriptionScimMapping();
			AssertNoErrors(staffColumnToGroupDescriptionScimMapping.StaffColumnNameInfo);

			staffColumnToGroupDescriptionScimMapping.ValidateStaffColumnName();
			AssertHasError(staffColumnToGroupDescriptionScimMapping.StaffColumnNameInfo, "Please enter a value.");

			var collection = new StaffColumnToGroupDescriptionScimMappingCollection();
			var staffColumnToGroupDescriptionScimMapping2 = collection.AddNew();
			staffColumnToGroupDescriptionScimMapping2.StaffColumnName = "TestA";
			AssertHasError(staffColumnToGroupDescriptionScimMapping2.StaffColumnNameInfo, "Enter a valid selection.");
			var staffColumnToGroupDescriptionScimMapping3 = collection.AddNew();
			staffColumnToGroupDescriptionScimMapping3.StaffColumnName = "GS_CanLogin";
			var staffColumnToGroupDescriptionScimMapping4 = collection.AddNew();
			staffColumnToGroupDescriptionScimMapping4.StaffColumnName = "GS_CanLogin";
			AssertHasError(staffColumnToGroupDescriptionScimMapping4.StaffColumnNameInfo, "The Staff Column Name has been duplicated and must be unique.");
		}

		public void TestStaffColumnList()
		{
			var staffColumnToGroupDescriptionScimMapping = new StaffColumnToGroupDescriptionScimMapping();
			var codes = staffColumnToGroupDescriptionScimMapping.Lookups.StaffList.ToArray().Select(i => i.Code);
			AssertContainsExactElementsInAnyOrder(new string[]
			{
				"GS_CanLogin", "GS_IsController", "GS_IsRobot", "GS_IsSalesRep", "GS_IsDriver", "GS_IsDevice", "IsDatabaseDeveloper", "IsReadOnlyDBUser", "IsBackupOperator"
			}, codes);
		}

		#endregion
	}
}
