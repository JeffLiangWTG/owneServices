using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(IncidentGroupType))]
	public class IncidentGroupTypeTest : RegistryBusinessObjectTemplateTestCase<IncidentGroupType>
	{
		public void TestGetClone()
		{
			var groupType = new IncidentGroupType();
			groupType.GroupType = "ATC";
			groupType.Description = "Test Only";

			var gtClone = (IncidentGroupType)groupType.Clone(groupType.CurrentFallbackLevel, groupType.Factory);
			AssertEquals(groupType.GroupType, gtClone.GroupType);
			AssertEquals(groupType.Description, gtClone.Description);
		}

		public void TestCanNotDeleteWhenIsSystem()
		{
			var groupType = GetBusinessObjectToClone();

			AssertEquals(true, groupType.CanDelete);

			groupType.IsSystem = true;
			AssertEquals("System defined group type should not be deleted.", false, groupType.CanDelete);
		}

		public void TestCanNotDeleteWhenIsInUse()
		{
			var groupType = GetBusinessObjectToClone();
			groupType.GroupType = "ABC";
			groupType.SetOriginalGroupType("ABC");
			groupType.IsSystem = false;
			AssertEquals(true, groupType.CanDelete);
			AssertEquals(false, groupType.IsGroupTypeInUse);

			var incidentManagementGroup = Factory.NewWithValidTestData<IncidentManagementGroup>();
			incidentManagementGroup.ING_Type = "ABC";

			Factory.Save();

			AssertEquals(false, groupType.CanDelete);
			AssertEquals(true, groupType.IsGroupTypeInUse);
			AssertEquals("The type ABC cannot be deleted as it is the active type for existing Incident Management Groups.", groupType.ReasonForNotAbleToDelete);
		}

		#region Validation
		public void TestValidateGroupType()
		{
			var collection = new IncidentGroupTypeCollection(NewFallbackLevel(), Factory);
			var groupType1 = collection.AddNew();

			groupType1.GroupType = "";
			AssertHasError(groupType1.GroupTypeInfo, MandatoryValidation.MustBeEnteredMessage(groupType1.GroupTypeInfo.Description));

			groupType1.GroupType = "AB";
			groupType1.ValidateGroupType();
			AssertHasError(groupType1.GroupTypeInfo, "The group type should have 3 characters");

			groupType1.GroupType = "ABC";
			var groupType2 = collection.AddNew();
			groupType2.GroupType = "ABC";
			AssertHasError(groupType2.GroupTypeInfo, PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(groupType2.GroupTypeInfo.Description));
		}
		#endregion

		public void TestCanNotChangeGroupTypeIfIsGroupTypeInUse()
		{
			var groupType = GetBusinessObjectToClone();
			groupType.GroupType = "ABC";
			groupType.IsSystem = false;
			groupType.SetOriginalGroupType("ABC");

			var incidentManagementGroup = Factory.NewWithValidTestData<IncidentManagementGroup>();
			incidentManagementGroup.ING_Type = "ABC";

			Factory.Save();

			AssertEquals(true, groupType.IsGroupTypeInUse);

			groupType.GroupType = "ACC";
			groupType.ValidateGroupType();
			AssertHasError(groupType.GroupTypeInfo, "This code cannot be changed as it is in use by at least one record in the system.");
		}

		#region Implementation
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override IncidentGroupType GetBusinessObjectToClone()
		{
			return new IncidentGroupType(NewFallbackLevel(), Factory);
		}

		protected override IncidentGroupType GetBusinessObjectToSerialise()
		{
			return new IncidentGroupType(NewFallbackLevel(), Factory);
		}
		#endregion
	}
}
