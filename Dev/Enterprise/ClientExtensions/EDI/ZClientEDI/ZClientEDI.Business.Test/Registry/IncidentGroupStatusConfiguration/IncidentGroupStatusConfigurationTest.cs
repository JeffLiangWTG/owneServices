using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(IncidentGroupStatusConfiguration))]
	public class IncidentGroupStatusConfigurationTest : RegistryBusinessObjectTemplateTestCase<IncidentGroupStatusConfiguration>
	{
		#region Test Validation
		public void TestValidateSequence()
		{
			var groupType = new IncidentGroupType();
			groupType.GroupType = "NTT";

			groupType.IncidentGroupStatusConfigurations.RemoveAll();

			var groupStatus = groupType.IncidentGroupStatusConfigurations.AddNew();
			groupStatus.Sequence = -1;
			groupStatus.ValidateSequence();
			AssertHasError(groupStatus.SequenceInfo, "Sequence should be greater than 0.");

			groupStatus.Sequence = 0;
			groupStatus.ValidateSequence();
			AssertHasError(groupStatus.SequenceInfo, "Please enter a Sequence.");
		}

		public void TestValidateCode()
		{
			var groupType = new IncidentGroupType();
			groupType.GroupType = "NTT";

			var groupStatus = groupType.IncidentGroupStatusConfigurations.AddNew();
			groupStatus.Code = "";
			groupStatus.ValidateCode();
			AssertHasError(groupStatus.CodeInfo, MandatoryValidation.MustBeEnteredMessage(groupStatus.CodeInfo.Description));

			groupStatus.Code = "AB";
			groupStatus.ValidateCode();
			AssertHasError(groupStatus.CodeInfo, "Code should have 3 characters.");

			groupStatus.Code = "ABC";
			groupStatus.ValidateCode();
			var groupStatus2 = groupType.IncidentGroupStatusConfigurations.AddNew();
			groupStatus2.Code = "ABC";
			groupStatus2.ValidateCode();

			AssertHasError(groupStatus2.CodeInfo, PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(groupStatus2.CodeInfo.Description));
		}

		public void TestValidateTriggerOn()
		{
			var groupType = new IncidentGroupType();
			groupType.GroupType = "NTT";

			var groupStatus = groupType.IncidentGroupStatusConfigurations.AddNew();
			groupStatus.Code = "ACI";
			groupStatus.ControlIncidents = false;
			groupStatus.ValidateTriggerOn();
			AssertHasWarning(groupStatus.TriggerOnInfo, "Broadcast will not be sent");
		}

		#endregion

		#region Test Read-Only
		public void TestReadOnly()
		{
			var setup = (IncidentGroupStatusConfiguration)GetNewBusinessObject();

			setup.IsSystem = false;
			AssertEquals(false, setup.Sequence_ReadOnly);
			AssertEquals(false, setup.Code_ReadOnly);
			AssertEquals(true, setup.TriggerOn_ReadOnly);

			setup.IsSystem = true;
			AssertEquals(true, setup.Sequence_ReadOnly);
			AssertEquals(true, setup.Code_ReadOnly);
			AssertEquals(true, setup.TriggerOn_ReadOnly);
		}

		public void TestEnabledReadOnly()
		{
			var setup = (IncidentGroupStatusConfiguration)GetNewBusinessObject();

			setup.Code = "ABC";
			AssertEquals(false, setup.Enabled_ReadOnly);

			setup.Code = "INV";
			AssertEquals(@"The code is 'INV' that can not be deleted.", true, setup.Enabled_ReadOnly);
		}
		#endregion

		public void TestCanNotDeleteWhenIsSystem()
		{
			var setup = (IncidentGroupStatusConfiguration)GetNewBusinessObject();
			setup.IsSystem = false;
			AssertEquals(true, setup.CanDelete);

			setup.IsSystem = true;
			AssertEquals(false, setup.CanDelete);
			AssertEquals("System-defined setup(s) cannot be deleted.", setup.ReasonForNotAbleToDelete);
		}

		public void TestCanNotDeleteWhenIsInUse()
		{
			var setup = (IncidentGroupStatusConfiguration)GetNewBusinessObject();
			setup.IsSystem = false;
			AssertEquals(true, setup.CanDelete);
			AssertEquals(false, setup.CheckIsCodeInUse());

			var incidentGroupType = new IncidentGroupType();
			incidentGroupType.GroupType = "ABC";
			setup.ParentCode = incidentGroupType.GroupType;
			setup.Code = "INV";
			setup.SetOriginalCode("INV");

			var incidentManagementGroup = Factory.NewWithValidTestData<IncidentManagementGroup>();
			incidentManagementGroup.ING_Type = "ABC";
			incidentManagementGroup.ING_Status = "INV";

			Factory.Save();

			AssertEquals(false, setup.CanDelete);
			AssertEquals(true, setup.CheckIsCodeInUse());
			AssertEquals("The stage INV cannot be deleted as it is the active stage for existing Incident Management Groups. You may disable the stage, which will allow any user to manually set the active stage when opening an affected group.", setup.ReasonForNotAbleToDelete);
		}

		public void TestCanNotChangeCodeIfIsCodeInUse()
		{
			var setup = (IncidentGroupStatusConfiguration)GetNewBusinessObject();
			setup.IsSystem = false;

			var incidentGroupType = new IncidentGroupType();
			incidentGroupType.GroupType = "ABC";
			setup.ParentCode = incidentGroupType.GroupType;
			setup.Code = "INV";
			setup.SetOriginalCode("INV");

			var incidentManagementGroup = Factory.NewWithValidTestData<IncidentManagementGroup>();
			incidentManagementGroup.ING_Type = "ABC";
			incidentManagementGroup.ING_Status = "INV";

			Factory.Save();

			AssertEquals(true, setup.CheckIsCodeInUse());
			setup.Code = "INN";
			setup.ValidateCode();
			AssertHasError(setup.CodeInfo, "This code cannot be changed as it is in use by at least one record in the system.");
		}

		public void TestBusinessObjectDefault()
		{
			var setup = (IncidentGroupStatusConfiguration)GetNewBusinessObject();

			AssertEquals(IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, setup.TriggerOn);
			AssertEquals(false, setup.IsSystem);
			AssertEquals(true, setup.Enabled);
		}

		#region Implementation
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override IncidentGroupStatusConfiguration GetBusinessObjectToClone()
		{
			return new IncidentGroupStatusConfiguration(NewFallbackLevel(), Factory);
		}

		protected override IncidentGroupStatusConfiguration GetBusinessObjectToSerialise()
		{
			return new IncidentGroupStatusConfiguration(NewFallbackLevel(), Factory);
		}
		#endregion
	}
}
