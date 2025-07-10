using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	class BMBoardSectionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCardType()
		{
			var system = Factory.New<BMSystem>();
			var bucket = system.Components.AddNew();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;

			var section = system.Boards.AddNew().Sections.AddNew();
			section.MS_FC_Component = bucket.PK;
			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.CardType = "";
			AssertHasErrorContaining(sectionConfiguration.CardTypeInfo, MandatoryValidation.MustBeEntered);

			sectionConfiguration.CardType = "!";
			AssertHasErrorContaining(sectionConfiguration.CardTypeInfo, ListValidation.InvalidCodeError);

			sectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			AssertNoErrorContaining(sectionConfiguration.CardTypeInfo, ListValidation.InvalidCodeError);
			AssertNoErrorContaining(sectionConfiguration.CardTypeInfo, MandatoryValidation.MustBeEntered);

			var warningText = "Showing one card per workflow may not be useful on a Buffer section with resource channels since workflow cards do not identify each task the resources need to complete.";
			AssertNoWarning(sectionConfiguration.CardTypeInfo, warningText);

			section.MS_FC_Component = buffer.PK;
			AssertNoWarning(sectionConfiguration.CardTypeInfo, warningText);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.New<GlbStaff>().PK);
			sectionConfiguration.Validation.ValidateCardType();
			AssertHasWarning(sectionConfiguration.CardTypeInfo, warningText);

			sectionConfiguration.CardType = CardTypeList.Codes.Task;
			AssertNoWarning(sectionConfiguration.CardTypeInfo, warningText);

			FilterStripsTestHelper.AddFilterStrips(section.WorkflowFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.BufferZone
			});

			sectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			warningText = "Selected workflow filter(s) are not applicable when showing tickets for Job-Level Workflow. It might be one of these filters: Buffer Zone, Constraint Status, Job or Workflow, Lead Time, Current Component, Component Change Logs.";
			AssertHasWarning(sectionConfiguration.CardTypeInfo, warningText);

			sectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			AssertNoWarning(sectionConfiguration.CardTypeInfo, warningText);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
