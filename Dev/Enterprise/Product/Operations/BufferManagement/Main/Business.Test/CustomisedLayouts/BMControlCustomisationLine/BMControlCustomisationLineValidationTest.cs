using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMControlCustomisationLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPropertySource()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;
			var line = customisation.CustomisationLines.AddNew();

			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			AssertNoErrors(line.PropertySourceInfo);

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowDetailedCard;
			line.Validation.ValidateAll();

			AssertHasError(line.PropertySourceInfo, "Workflow cards may not display properties from tasks.");

			line.PropertySource = PropertySourceList.Codes.Job;
			AssertNoErrors(line.PropertySourceInfo);

			line.PropertySource = PropertySourceList.Codes.Workflow;
			AssertNoErrors(line.PropertySourceInfo);

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;
			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			AssertHasError(line.PropertySourceInfo, "Workflow cards may not display properties from tasks.");

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			line.Validation.ValidateAll();
			AssertNoErrors(line.PropertySourceInfo);
		}

		public void TestOrientation()
		{
			var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var line = customisation.CustomisationLines.AddNew();
			line.Orientation = "Vertical";

			line.ControlType = PropertyTypeList.Codes.Text;
			line.IsReadOnly = false;
			AssertHasError(line.OrientationInfo, "Vertical orientation is only supported for read-only properties.");

			line.IsReadOnly = true;
			AssertNoErrors(line.OrientationInfo);

			line.ControlType = PropertyTypeList.Codes.Boolean;
			AssertHasError(line.OrientationInfo, "Vertical orientation is not supported for this property type.");
		}

		#region Control Type
		public void TestDurationControl_WhenNonDateTimePropertyUsed_ShouldAddValidationError()
		{
			var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = "EstimatedTimeToCompleteHours";

			line.ControlType = PropertyTypeList.Codes.Duration;
			AssertHasError(line.ControlTypeInfo, "Only duration Properties are valid for this selected Type, however Estimated Hours To Complete is a number Property.");

			line.ControlType = PropertyTypeList.Codes.DateTime;
			AssertHasError(line.ControlTypeInfo, "Only Date or Date Time Properties are valid for this selected Type, however Estimated Hours To Complete is a number Property.");

			line.ControlType = PropertyTypeList.Codes.Number;
			AssertNoErrors(line.ControlTypeInfo);

			customisation.FM_JobType = WorkflowDescriptors.OpportunityWorkflowDescriptorCode;
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = OrgOpportunitySchema.Constants.P8_RentalMultiplier;

			line.ControlType = PropertyTypeList.Codes.Duration;
			AssertHasError(line.ControlTypeInfo, "Only duration Properties are valid for this selected Type, however P8_RentalMultiplier is a number Property.");

			line.ControlType = PropertyTypeList.Codes.DateTime;
			AssertHasError(line.ControlTypeInfo, "Only Date or Date Time Properties are valid for this selected Type, however P8_RentalMultiplier is a number Property.");

			line.ControlType = PropertyTypeList.Codes.Number;
			AssertNoErrors(line.ControlTypeInfo);
		}

		public void TestNumericControl_WhenNonNumericPropertyUsed_ShouldAddValidationError()
		{
			var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = ProcessTasksSchema.Constants.P9_Status;

			line.ControlType = PropertyTypeList.Codes.Number;
			AssertHasError(line.ControlTypeInfo, "Only number Properties are valid for this selected Type, however Task Status is a text Property.");

			line.PropertyName = ProcessTasksSchema.Constants.P9_Sequence;
			AssertNoErrors(line.ControlTypeInfo);

			customisation.FM_JobType = WorkflowDescriptors.OpportunityWorkflowDescriptorCode;
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = OrgOpportunitySchema.Constants.P8_PackageType;

			line.ControlType = PropertyTypeList.Codes.Number;
			AssertHasError(line.ControlTypeInfo, "Only number Properties are valid for this selected Type, however P8_PackageType is a text Property.");

			line.PropertyName = OrgOpportunitySchema.Constants.P8_RentalMultiplier;
			AssertNoErrors(line.ControlTypeInfo);
		}

		public void TestBooleanControl_WhenNonBoolPropertyUsed_ShouldAddValidationError()
		{
			var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var line = customisation.CustomisationLines.AddNew();

			customisation.FM_JobType = WorkflowDescriptors.OpportunityWorkflowDescriptorCode;
			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = ProcessTasksSchema.Constants.P9_Description;

			line.ControlType = PropertyTypeList.Codes.Boolean;
			AssertHasError(line.ControlTypeInfo, "Only true/false Properties are valid for this selected Type, however Description is a text Property.");

			line.PropertyName = ProcessTasksSchema.Constants.P9_IsInterruptable;
			AssertNoErrors(line.ControlTypeInfo);

			customisation.FM_JobType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = OrgHeaderSchema.Constants.OH_FullName;
			line.ControlType = PropertyTypeList.Codes.Boolean;

			AssertHasError(line.ControlTypeInfo, "Only true/false Properties are valid for this selected Type, however OH_FullName is a text Property.");

			line.PropertyName = OrgHeaderSchema.Constants.OH_IsActive;
			AssertNoErrors(line.ControlTypeInfo);
		}

		public void TestTextControl_WhenNonTextPropertyUsedForJob_ShouldAddValidationError()
		{
			var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var line = customisation.CustomisationLines.AddNew();

			customisation.FM_JobType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode;
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = JobDeclarationSchema.Constants.JE_JS;

			line.ControlType = PropertyTypeList.Codes.Text;

			AssertHasError(line.ControlTypeInfo, "Only text Properties are valid for this selected Type, however JE_JS is a non-permissible Property.");
		}

		public void TestDurationControl_WhenNonDateTimePropertyUsedOnCustomField_ShouldAddValidationError()
		{
			var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<GetCustomField(Shmeckels)>";

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "Shmeckels";
			custom.XC_Type = AddOnColumnDataType.Codes.Integer;

			line.ControlType = PropertyTypeList.Codes.Duration;
			AssertHasError(line.ControlTypeInfo, "Only duration Properties are valid for this selected Type, however <GetCustomField(Shmeckels)> is a number Property.");

			line.ControlType = PropertyTypeList.Codes.DateTime;
			AssertHasError(line.ControlTypeInfo, "Only Date or Date Time Properties are valid for this selected Type, however <GetCustomField(Shmeckels)> is a number Property.");

			line.ControlType = PropertyTypeList.Codes.Number;
			AssertNoErrors(line.ControlTypeInfo);
		}

		public void TestNumericControl_WhenNonNumericPropertyUsedOnCustomField_ShouldAddValidationError()
		{
			var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<GetCustomField(Nickname)>";

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "Nickname";
			custom.XC_Type = AddOnColumnDataType.Codes.String;

			line.ControlType = PropertyTypeList.Codes.Number;
			AssertHasError(line.ControlTypeInfo, "Only number Properties are valid for this selected Type, however <GetCustomField(Nickname)> is a text Property.");

			line.ControlType = PropertyTypeList.Codes.Text;
			AssertNoErrors(line.ControlTypeInfo);
		}

		public void TestBooleanControl_WhenNonBoolPropertyUsedOnCustomField_ShouldAddValidationError()
		{
			var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var line = customisation.CustomisationLines.AddNew();

			customisation.FM_JobType = WorkflowDescriptors.OpportunityWorkflowDescriptorCode;
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<GetCustomField(Is the bread Helgas?)>";

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "Is the bread Helgas?";
			custom.XC_Type = AddOnColumnDataType.Codes.Boolean;

			line.ControlType = PropertyTypeList.Codes.Text;
			AssertHasError(line.ControlTypeInfo, "Only text Properties are valid for this selected Type, however <GetCustomField(Is the bread Helgas?)> is a true/false Property.");

			line.ControlType = PropertyTypeList.Codes.Boolean;
			AssertNoErrors(line.ControlTypeInfo);
		}

		#endregion

		#region Background color

		public void TestValidate_BackgroundColor()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = ProcessTasksSchema.Constants.P9_Description;
			line.IsReadOnly = false;

			line.BackgroundColor = Color.Transparent.Name;
			AssertHasError(line.BackgroundColorInfo, $"Transparent color is not supported on '{PropertyTypeList.Codes.Text}' type.");

			line.IsReadOnly = true;
			AssertNoError(line.BackgroundColorInfo, $"Transparent color is not supported on '{PropertyTypeList.Codes.Text}' type.");
		}

		public void TestValidateAll_BackgroundColor()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Workflow;
			line.PropertyName = ProcessHeaderSchema.Constants.FH_AgreedDeliveryDate;
			line.BackgroundColor = Color.Transparent.Name;

			CombineAssertions("WHEN setting TextBox-based-control background-color to transparent THEN should show error", () =>
			{
				foreach (var controlType in GetControlTypes(line))
				{
					AssertCustomisationLineValidateAll(line, controlType, isReadOnly: true);
					AssertCustomisationLineValidateAll(line, controlType, isReadOnly: false);
				}
			});
		}

		void AssertCustomisationLineValidateAll(BMControlCustomisationLine line, string controlType, bool isReadOnly)
		{
			using (line.GetValidationSuspender())
			{
				line.ControlType = controlType;
				line.IsReadOnly = isReadOnly;
			}

			line.Validation.ValidateAll();

			AssertBackgroundColorForTransparent(line, controlType, isReadOnly);
		}

		public void TestControlType_WhenSet_ShouldValidateBackgroundColor()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			line.IsReadOnly = false;
			line.PropertySource = PropertySourceList.Codes.Workflow;
			line.PropertyName = ProcessHeaderSchema.Constants.FH_AgreedDeliveryDate;
			line.BackgroundColor = Color.Transparent.Name;

			CombineAssertions("WHEN setting TextBox-based-control background-color to transparent THEN should show error", () =>
			{
				foreach (var controlType in GetControlTypes(line))
				{
					AssertCustomisationLineValidate(line, controlType, isReadOnly: true);
					AssertCustomisationLineValidate(line, controlType, isReadOnly: false);
				}
			});
		}

		void AssertCustomisationLineValidate(BMControlCustomisationLine line, string controlType, bool isReadOnly)
		{
			line.IsReadOnly = isReadOnly;
			line.ControlType = controlType;

			AssertBackgroundColorForTransparent(line, controlType, isReadOnly);
		}

		#endregion

		#region Validate Property Name

		public void TestValidatePropertyName()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_JobType = "ORG";
			var line = customisation.CustomisationLines.AddNew();

			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = "booo";
			AssertHasError(line.PropertyNameInfo, "Enter a valid Property Name.");

			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "booo";
			AssertHasError(line.PropertyNameInfo, "Property not found.");

			line.PropertyName = OrgHeaderSchema.Constants.OH_Code;
			AssertNoErrors(line.PropertyNameInfo);

			line.PropertyName = "OverallSalesRepStaff";
			AssertNoErrors(line.PropertyNameInfo);

			customisation.FM_JobType = ZString.Empty;
			line.PropertyName = "OverallSalesRepStaff";
			AssertHasError(line.PropertyNameInfo, "When property source is job, the job type should not empty.");
		}

		public void TestValidatePropertyName_WhenPropertySourceIsJob()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_JobType = "WKI";
			var line = customisation.CustomisationLines.AddNew();

			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "booo";
			AssertHasError(line.PropertyNameInfo, "Property not found.");

			line.PropertyName = WorkItemSchema.Constants.WKI_WorkItemNumber;
			AssertNoErrors(line.PropertyNameInfo);

			line.PropertyName = "AssignedToStaff.GS_EmailAddress";
			AssertNoErrors(line.PropertyNameInfo);

			line.PropertyName = "<AssignedToStaff.GS_EmailAddress>";
			AssertNoErrors(line.PropertyNameInfo);

			line.PropertyName = "<AssignedBranch.GB_City>";
			AssertNoErrors(line.PropertyNameInfo);

			line.PropertyName = "<AssignedDepartment.GE_CustomsBrokerage>";
			AssertNoErrors(line.PropertyNameInfo);

			customisation.FM_JobType = ZString.Empty;
			line.PropertyName = "<AssignedToStaff.GS_EmailAddress>";
			AssertHasError(line.PropertyNameInfo, "When property source is job, the job type should not empty.");
		}

		public void TestValidatePropertyName_WhenPropertySourceIsJob_ForOrgHeader()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_JobType = "ORG";
			var line = customisation.CustomisationLines.AddNew();

			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "booo";
			AssertHasError(line.PropertyNameInfo, "Property not found.");

			line.PropertyName = OrgHeaderSchema.Constants.OH_FullName;
			AssertNoErrors(line.PropertyNameInfo);

			line.PropertyName = "<Country.RN_AddressFormattingRule>";
			AssertNoErrors("GIVEN source=Job, WHEN validate, should not error: 'Property is not valid: Cannot make changes on element of type RefCountry.'", line.PropertyNameInfo);

			line.PropertyName = "<WorkflowItems.ETA>";
			AssertNoErrors("GIVEN source=Job, WHEN validate, should not error: 'Property is not valid: Property Enterprise.MasterFiles.Business.ProcessTask.ETA of type ZDateTime is read-only and cannot be used in current operation.'", line.PropertyNameInfo);

			line.PropertyName = "<AddressForSendingAPDocuments.CountryCode.RN_CountryDialingCode>";
			AssertNoErrors("GIVEN source=Job, WHEN validate, should not error: 'Property is not valid: Property Enterprise.MasterFiles.Business.OrgAddress.CountryCode of type Enterprise.MasterFiles.Business.RefCountry cannot be used in current operation - reference data cannot be changed here.'", line.PropertyNameInfo);
		}

		public void TestValidatePropertyName_WhenTypeIsNotValid()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_JobType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			var line = customisation.CustomisationLines.AddNew();

			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = OrgHeaderSchema.Constants.OH_FullName;
			line.ControlType = PropertyTypeList.Codes.Duration;
			AssertHasError(line.ControlTypeInfo, "Only duration Properties are valid for this selected Type, however OH_FullName is a text Property."); // No need to test when ControlType is set before PropertyName, because in UI when PropertyName set, the ControlType is auto set correctly.

			line.ControlType = PropertyTypeList.Codes.Text;
			AssertNoErrors(line.ControlTypeInfo);
		}

		void ValidatePropertyName_WhenPropertySourceIsJob_FirstAndFirstOrDefault(string propertyName)
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_JobType = "ORG";
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = propertyName;
			line.Validation.ValidateAll();
			AssertHasError(line.PropertyNameInfo, "Unable to render First or First Or Default macros on detailed cards.");

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowDetailedCard;
			line.Validation.ValidateAll();
			AssertHasError(line.PropertyNameInfo, "Unable to render First or First Or Default macros on detailed cards.");

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			line.Validation.ValidateAll();
			AssertNoErrors(line.PropertyNameInfo);

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;
			line.Validation.ValidateAll();
			AssertNoErrors(line.PropertyNameInfo);
		}

		public void TestValidatePropertyName_WhenPropertySourceIsJob_First()
		{
			ValidatePropertyName_WhenPropertySourceIsJob_FirstAndFirstOrDefault("<WorkflowItems.First(\"<P9_Description>\" == \"Task\").P9_Status>");
		}

		public void TestValidatePropertyName_WhenPropertySourceIsJob_FirstOrDefault()
		{
			ValidatePropertyName_WhenPropertySourceIsJob_FirstAndFirstOrDefault("<WorkflowItems.FirstOrDefault(\"<P9_Description>\" == \"Task\").P9_Status>");
		}

		public void TestValidatePropertyName_GetCustomField()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_JobType = "ORG";
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<GetCustomField(Shmeckel)>";
			line.Validation.ValidateAll();
			AssertHasError(line.PropertyNameInfo, "Property not found.");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "Shmeckel";
			custom.XC_Type = AddOnColumnDataType.Codes.Integer;

			line.Validation.ValidateAll();
			AssertNoErrors(line.PropertyNameInfo);

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowDetailedCard;
			line.Validation.ValidateAll();
			AssertNoErrors(line.PropertyNameInfo);

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			line.Validation.ValidateAll();
			AssertNoErrors(line.PropertyNameInfo);

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;
			line.Validation.ValidateAll();
			AssertNoErrors(line.PropertyNameInfo);

			line.PropertyName = "<GetCustomField(Shmeckel)";
			line.Validation.ValidateAll();
			AssertNoErrors("Should still correctly find the custom field, even with the missing >", line.PropertyNameInfo);

			line.PropertyName = "<GetCustomField(Shmeckel";
			line.Validation.ValidateAll();
			AssertHasError("Should not find the custom field, since ) is also missing", line.PropertyNameInfo, "Property not found.");
		}

		public void TestValidatePropertyName_GetCustomField_ComboBox()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_JobType = "ORG";
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<GetCustomField(Shmombo)>";

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "Shmombo";
			custom.XC_Type = AddOnColumnDataType.Codes.ComboBox;

			line.Validation.ValidateAll();
			AssertHasError(line.PropertyNameInfo, "Combo box custom fields are not supported.");
		}

		#endregion

		public void TestValidateIsReadOnly()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;

			var line = customisation.CustomisationLines.AddNew();
			AssertEquals(true, line.IsReadOnly);
			AssertNoErrors(line.IsReadOnlyInfo);

			line.IsReadOnly = false;
			AssertHasError(line.IsReadOnlyInfo, "Summary task card properties must be read-only.");

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;
			line.Validation.ValidateAll();
			AssertNoErrors(line.IsReadOnlyInfo);
		}

		#region Implementation

		void AssertBackgroundColorForTransparent(BMControlCustomisationLine line, string controlType, bool isReadOnly)
		{
			var message = $"ControlType={controlType}, ReadOnly={isReadOnly}";

			if (IsSupportTransparentBG(line))
			{
				AssertNoErrors(message, line.BackgroundColorInfo);
			}
			else
			{
				AssertHasError(message, line.BackgroundColorInfo, $"Transparent color is not supported on '{controlType}' type.");
			}
		}

		IEnumerable<string> GetControlTypes(BMControlCustomisationLine line)
		{
			return line
				.Lookups
				.ControlTypes
				.Cast<CodeDescriptionPair>()
				.Select(controlType => controlType.Code);
		}

		bool IsSupportTransparentBG(BMControlCustomisationLine line)
		{
			if (line.ControlType == PropertyTypeList.Codes.Text && line.IsReadOnly)
			{
				return true;
			}
			else if (supportedTransparentBGControl.Contains(line.ControlType.ToString()))
			{
				return true;
			}

			return false;
		}

		readonly IEnumerable<string> supportedTransparentBGControl = new[] { PropertyTypeList.Codes.Date, PropertyTypeList.Codes.DateTime, PropertyTypeList.Codes.Boolean };

		#endregion
	}
}
