using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMControlCustomisationLine))]
	class BMControlCustomisationLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddingLines_ShouldSetTopToMakeThemStackNicely()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();

			var line1 = customisation.CustomisationLines.AddNew();
			line1.Top = 20;
			line1.Height = 20;

			var line2 = customisation.CustomisationLines.AddNew();
			AssertEquals(40, line2.Top);
			line2.Height = 40;

			var line3 = customisation.CustomisationLines.AddNew();
			AssertEquals(80, line3.Top);

			Factory.Save();

			var loadedCustomisation = new BusinessObjectFactory().Load<BMControlCustomisation>(customisation.PK);
			AssertEquals(false, loadedCustomisation.HasChanges);
		}

		[ExpectNoExceptions]
		public void TestDeleteLine_ShouldRefreshPreview()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();

			var previewReceiver = new Mock<IPreviewReceiver>(MockBehavior.Strict);

			previewReceiver.Setup(m => m.UpdatePreview());
			customisation.PreviewReceiver = previewReceiver.Object;
			line.Delete();
			previewReceiver.Verify(m => m.UpdatePreview(), Times.Once);
		}

		public void TestPropertySourceDescription()
		{
			var customisedLine = Factory.New<BMControlCustomisation>().CustomisationLines.AddNew();

			customisedLine.PropertySourceDescription = PropertySourceList.Descriptions.ProcessTask;
			AssertEquals(PropertySourceList.Codes.ProcessTask, customisedLine.PropertySource);

			customisedLine.PropertySourceDescription = PropertySourceList.Descriptions.Workflow;
			AssertEquals(PropertySourceList.Codes.Workflow, customisedLine.PropertySource);

			customisedLine.PropertySourceDescription = PropertySourceList.Descriptions.Job;
			AssertEquals(PropertySourceList.Codes.Job, customisedLine.PropertySource);
		}

		public void TestPropertyNameDescription_Workflow()
		{
			var customisedLine = Factory.New<BMControlCustomisation>().CustomisationLines.AddNew();
			customisedLine.PropertySource = PropertySourceList.Codes.Workflow;

			var pair = customisedLine.Lookups.PropertyNames.OfType<CodeDescriptionPair>().First();
			customisedLine.PropertyNameDescription = pair.Description;
			AssertEquals(pair.Code, customisedLine.PropertyName);
		}

		public void TestPropertyNameDescription_Job()
		{
			var customisedLine = Factory.New<BMControlCustomisation>().CustomisationLines.AddNew();
			customisedLine.PropertySource = PropertySourceList.Codes.Job;

			AssertEquals(0, customisedLine.Lookups.PropertyNames.Count);

			customisedLine.PropertyNameDescription = "Blah";
			AssertEquals("Should just set PropertyName directly for jobs - we can't get their descriptions at this time.", "Blah", customisedLine.PropertyName);
		}

		public void TestControlTypes()
		{
			var customisedLine = Factory.New<BMControlCustomisation>().CustomisationLines.AddNew();
			Assert(customisedLine.Lookups.ControlTypes.Cast<CodeDescriptionPair>().SequenceEqual(new PropertyTypeList().Cast<CodeDescriptionPair>()));
		}

		public void TestPropertySerialisation()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = ProcessTasksSchema.Constants.P9_Status;
			line.Left = 10;
			line.Top = 20;
			line.Width = 100;
			line.Height = 30;
			line.BackgroundColor = Color.Blue.Name;
			line.ForegroundColor = Color.HotPink.Name;
			line.IsBold = true;
			line.FontSize = 15;
			line.Orientation = "Vertical";

			Factory.Save();

			var loadedCustomisation = new BusinessObjectFactory().Load<BMControlCustomisation>(customisation.PK);
			var loadedLine = loadedCustomisation.CustomisationLines[0];
			AssertEquals(PropertySourceList.Codes.ProcessTask, loadedLine.PropertySource);
			AssertEquals(ProcessTasksSchema.Constants.P9_Status, loadedLine.PropertyName);
			AssertEquals(10, loadedLine.Left);
			AssertEquals(20, loadedLine.Top);
			AssertEquals(100, loadedLine.Width);
			AssertEquals(30, loadedLine.Height);
			AssertEquals(Color.Blue.Name, loadedLine.BackgroundColor);
			AssertEquals(Color.HotPink.Name, loadedLine.ForegroundColor);
			AssertEquals(true, loadedLine.IsBold);
			AssertEquals(15, loadedLine.FontSize);
			AssertEquals(BMBoardSectionOrientation.Vertical, loadedLine.OrientationValue);
		}

		public void TestOrientation_ShouldDefaultToHorizontal()
		{
			var line = Factory.New<BMControlCustomisation>().CustomisationLines.AddNew();
			AssertEquals(BMBoardSectionOrientation.Horizontal, line.OrientationValue);
		}

		public void TestPropertyName_ShouldSetLabel()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Workflow;
			line.PropertyName = ProcessHeaderSchema.FH_DateAcceptability.Name;

			AssertEquals("Date Acceptability", line.Label);

			line.PropertyName = "booo";

			AssertHasErrors(line.PropertyNameInfo);
			AssertEquals("Should not update label as PropertyName has errors", "Date Acceptability", line.Label);
		}

		public void TestPropertyName_ShouldSetLabel_WhenRenderingCustomField()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var customNumber = template.GenCustomColumnDefinitions.AddNew();
			customNumber.XC_Name = "schmumber";
			customNumber.XC_Type = AddOnColumnDataType.Codes.Integer;

			var customText = template.GenCustomColumnDefinitions.AddNew();
			customText.XC_Name = "schmext";
			customText.XC_Type = AddOnColumnDataType.Codes.String;

			var customDateTime = template.GenCustomColumnDefinitions.AddNew();
			customDateTime.XC_Name = "schmatetime";
			customDateTime.XC_Type = AddOnColumnDataType.Codes.Datetime;

			var customBoolean = template.GenCustomColumnDefinitions.AddNew();
			customBoolean.XC_Name = "schmoolean";
			customBoolean.XC_Type = AddOnColumnDataType.Codes.Boolean;

			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_JobType = "ORG";
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;

			line.PropertyName = "<GetCustomField(schmext)>";
			AssertEquals("schmext", line.Label);

			line.PropertyName = "<GetCustomField(schmumber)>";
			AssertEquals("schmumber", line.Label);

			line.PropertyName = "<GetCustomField(schmatetime)>";
			AssertEquals("schmatetime", line.Label);

			line.PropertyName = "<GetCustomField(schmoolean)>";
			AssertEquals("schmoolean", line.Label);
		}

		public void TestPropertyName_ShouldSetPropertyType()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Workflow;

			line.PropertyName = ProcessHeaderSchema.Constants.FH_CompletionStatement;
			AssertEquals(PropertyTypeList.Codes.Text, line.ControlType);

			line.PropertyName = ProcessHeaderSchema.Constants.FH_TimeDelayMinutes;
			AssertEquals(PropertyTypeList.Codes.Number, line.ControlType);

			line.PropertyName = ProcessHeaderSchema.Constants.FH_ReleaseDateTime;
			AssertEquals(PropertyTypeList.Codes.DateTime, line.ControlType);

			line.PropertyName = ProcessHeaderSchema.Constants.FH_IsActive;
			AssertEquals(PropertyTypeList.Codes.Boolean, line.ControlType);

			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = ProcessTasksSchema.Constants.P9_ActualDuration;
			AssertEquals(PropertyTypeList.Codes.Duration, line.ControlType);
		}

		public void TestPropertyName_ShouldSetPropertyType_WhenRenderingCustomFields()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var customNumber = template.GenCustomColumnDefinitions.AddNew();
			customNumber.XC_Name = "schmumber";
			customNumber.XC_Type = AddOnColumnDataType.Codes.Integer;

			var customText = template.GenCustomColumnDefinitions.AddNew();
			customText.XC_Name = "schmext";
			customText.XC_Type = AddOnColumnDataType.Codes.String;

			var customDateTime = template.GenCustomColumnDefinitions.AddNew();
			customDateTime.XC_Name = "schmatetime";
			customDateTime.XC_Type = AddOnColumnDataType.Codes.Datetime;

			var customBoolean = template.GenCustomColumnDefinitions.AddNew();
			customBoolean.XC_Name = "schmoolean";
			customBoolean.XC_Type = AddOnColumnDataType.Codes.Boolean;

			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_JobType = "ORG";
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;

			line.PropertyName = "<GetCustomField(schmext)>";
			AssertEquals(PropertyTypeList.Codes.Text, line.ControlType);

			line.PropertyName = "<GetCustomField(schmumber)>";
			AssertEquals(PropertyTypeList.Codes.Number, line.ControlType);

			line.PropertyName = "<GetCustomField(schmatetime)>";
			AssertEquals(PropertyTypeList.Codes.DateTime, line.ControlType);

			line.PropertyName = "<GetCustomField(schmoolean)>";
			AssertEquals(PropertyTypeList.Codes.Boolean, line.ControlType);
		}

		public void TestDisplaySequence()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			var line1 = customisation.CustomisationLines.AddNew();
			var line2 = customisation.CustomisationLines.AddNew();
			var line3 = customisation.CustomisationLines.AddNew();
			var line4 = customisation.CustomisationLines.AddNew();

			line4.Left = 0;
			line4.Top = 0;

			line3.Left = 50;
			line3.Top = 0;

			line2.Top = 100;
			line2.Left = 0;

			line1.Top = 100;
			line1.Left = 100;

			AssertEquals(1, line4.DisplaySequence);
			AssertEquals(2, line3.DisplaySequence);
			AssertEquals(3, line2.DisplaySequence);
			AssertEquals(4, line1.DisplaySequence);
		}

		public void TestGetBindingPath()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Bucket);
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var processTask = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "schmeckels";
			custom.XC_Type = AddOnColumnDataType.Codes.Integer;

			var customValue = Factory.New<GenCustomAddOnValue>();
			customValue.XV_ParentID = jobHeader.FH_ParentId;
			customValue.XV_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			customValue.XV_Name = custom.XC_Name;
			customValue.XV_Type = custom.XC_Type;
			customValue.XV_Data = "99";

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.System).Item2;
			var taskCardContent = new TaskCardContent(processTask, null);
			var factorylessCardContent = new FactorylessCardContent(workflow, processTask, viewModel, new CustomisedControlDataCache(), new TagDefinitionCache(Factory), new PopulateTaskCardStrategy());

			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = "<PropertyName>";
			var bindingPath = line.GetBindingPath(taskCardContent);
			AssertEquals("PropertyName", bindingPath);

			bindingPath = line.GetBindingPath(factorylessCardContent);
			AssertEquals(PropertySourceList.Codes.ProcessTask + "_PropertyName", bindingPath);

			line.PropertySource = PropertySourceList.Codes.Workflow;
			line.PropertyName = "<PropertyName>";
			bindingPath = line.GetBindingPath(taskCardContent);
			AssertEquals("ProcessHeader.PropertyName", bindingPath);

			bindingPath = line.GetBindingPath(taskCardContent, true);
			AssertEquals("ProcessHeader.ParentHeader.PropertyName", bindingPath);

			bindingPath = line.GetBindingPath(factorylessCardContent);
			AssertEquals(PropertySourceList.Codes.Workflow + "_PropertyName", bindingPath);

			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<PropertyName>";
			bindingPath = line.GetBindingPath(taskCardContent);
			AssertEquals("Task do not have the property", bindingPath, string.Empty);

			line.PropertyName = nameof(processTask.Parent.PK);
			bindingPath = line.GetBindingPath(taskCardContent);
			AssertEquals("Task has the property", bindingPath, "Parent.PK");

			line.PropertyName = "WorkflowItems.ProcessTaskNotifications.PQ_Calc_TriggerParty";
			bindingPath = line.GetBindingPath(taskCardContent);
			AssertEquals("Task has the property", bindingPath, "Parent.WorkflowItems.ProcessTaskNotifications.PQ_Calc_TriggerParty");

			bindingPath = line.GetBindingPath(factorylessCardContent);
			AssertEquals(PropertySourceList.Codes.Job + "_WorkflowItems_ProcessTaskNotifications_PQ_Calc_TriggerParty", bindingPath);

			line.PropertyName = "<GetCustomField(schmeckels)";
			bindingPath = line.GetBindingPath(taskCardContent);
			AssertEquals(bindingPath, "__SCHMECKELS__prop__ZInt");

			bindingPath = line.GetBindingPath(factorylessCardContent);
			AssertEquals(bindingPath, "JOB_GetCustomField(schmeckels)");
		}

		public void TestGetBindingPath_ParentIsNotCustomFieldProvider()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "GetCustomField(I saw my face in the mirror and felt fear.";
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Bucket);
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var processTask = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;

			Factory.Save();

			var taskCardContent = new TaskCardContent(processTask, null);
			AssertEquals(string.Empty, line.GetBindingPath(taskCardContent));
		}

		public void TestGetBindingPath_ForJobProperty_ForTaskWithOverriddenReferenceToParent_ShouldGetPropertyFromOverriddenParentType()
		{
			var dummy = Factory.New<DummySubclass>();
			var task = (TaskTop)dummy.WorkflowItems.Tasks.AddNew(typeof(TaskTop));

			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "Z0_Code";

			var taskCardContent = new TaskCardContent(task, null);
			var bindingPath = string.Empty;

			AssertNoExceptionThrown("There should not be an exception when trying to resolve which Parent property to use, we should just use the most specific one. SAD!", () => bindingPath = line.GetBindingPath(taskCardContent));
			AssertEquals("Parent.Z0_Code", bindingPath);
		}

		class TaskWithOverriddenParentMiddle : DummyProcessTask
		{
			protected TaskWithOverriddenParentMiddle(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new DummySubclass Parent => (DummySubclass)base.Parent; // The exception we're preventing can happen when Parent is overridden with a different type.
		}

		class TaskTop : TaskWithOverriddenParentMiddle
		{
			public TaskTop(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class DummySubclass : DummyWithWorkflow
		{
			public DummySubclass(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public void TestCustomisationLineReflectionHelper_WhenPropertyDoesNotExist_ShouldNotThrowExceptions()
		{
			PropertyInfo result = null;

			AssertNoExceptionThrown("We should have code that limits the recursiveness of this function, lest it start throwing exceptions or stack overflowing. SAD!",
				() => result = CustomisationLineReflectionHelper.GetPropertyInfo(typeof(DummyBusinessObject), "Shalala"));

			AssertNull(result);
		}

		public void TestGetProperty_WithMultipleResults_ShouldSelectCorrectProperty()
		{
			TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow);
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();

			Factory.Save();

			var taskCardContent = new TaskCardContent(task, null);
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<Validation>";
			string bindingPath = null;

			AssertExceptionThrown<AmbiguousMatchException>("Precondition: the Validation property should be overridden using the 'new' modifier, or this test is meaningless.", () => typeof(SalesEnquiry).GetProperty("Validation"));
			AssertNoExceptionThrown("An AmbiguousMatchException should not be thrown just because the selected property hides its base class's property. SAD!", () => bindingPath = line.GetBindingPath(taskCardContent));
			AssertEquals("Parent.Validation", bindingPath);
		}

		public void TestGetProperty_WithMultipleResults_WhenPropertyIsDeclaredInBaseClass_ShouldSelectCorrectProperty()
		{
			TypeDecider.AddSubstitution(typeof(SalesEnquiry), typeof(SalesEnquirySubclass));
			TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquirySubclass>(Factory, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow);
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();

			Factory.Save();

			var taskCardContent = new TaskCardContent(task, null);
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<Validation>";
			string bindingPath = null;

			AssertExceptionThrown<AmbiguousMatchException>("Precondition: the Validation property should be overridden using the 'new' modifier, or this test is meaningless.", () => typeof(SalesEnquirySubclass).GetProperty("Validation"));
			AssertNoExceptionThrown("An AmbiguousMatchException should not be thrown just because the selected property hides its base class's property, and when that property is declared in a base class. SAD!", () => bindingPath = line.GetBindingPath(taskCardContent));
			AssertEquals("Parent.Validation", bindingPath);
		}

		public void TestGetCustomProperty_WithMultipleResults_ShouldSelectCorrectProperty()
		{
			TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow);
			var customisation = Factory.New<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom1 = template1.GenCustomColumnDefinitions.AddNew();
			custom1.XC_Name = "schmeckels";
			custom1.XC_Type = AddOnColumnDataType.Codes.Integer;

			var custom1Value = Factory.New<GenCustomAddOnValue>();
			custom1Value.XV_ParentID = workflow.FH_ParentId;
			custom1Value.XV_ParentTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			custom1Value.XV_Name = custom1.XC_Name;
			custom1Value.XV_Type = custom1.XC_Type;
			custom1Value.XV_Data = "99";

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom2 = template2.GenCustomColumnDefinitions.AddNew();
			custom2.XC_Name = "schmeckels";
			custom2.XC_Type = AddOnColumnDataType.Codes.String;

			var custom2Value = Factory.New<GenCustomAddOnValue>();
			custom2Value.XV_ParentID = workflow.FH_ParentId;
			custom2Value.XV_ParentTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			custom2Value.XV_Name = custom2.XC_Name;
			custom2Value.XV_Type = custom2.XC_Type;
			custom2Value.XV_Data = "FLAKE";

			Factory.Save();

			var taskCardContent = new TaskCardContent(task, null);
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<GetCustomField(schmeckels)";
			string bindingPath = line.GetBindingPath(taskCardContent);
			AssertEquals(bindingPath, "__SCHMECKELS__prop__ZInt");
		}

		public void TestCustomisation_WhenAppliedToWrongJobType_WithComplexPropertyName_ShouldNotThrowExceptions()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code);

			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_JobType = "INQ";
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<Contact.Header.CallingStaff>"; // something that makes sense on a Sales Enquiry but is nonsense on an OrgHeader. Needs to have a "." in the property name.

			var taskCardContent = new TaskCardContent(task, null);
			string bindingPath = null;

			AssertNoExceptionThrown("Trying to get a complex property on a job type other than the type of the customisation should not throw uncaught exceptions. SAD!", () => bindingPath = line.GetBindingPath(taskCardContent));
			AssertEquals("The property doesn't exist on OrgHeader, so it should just return an empty string. SAD!", string.Empty, bindingPath);
		}

		public void TestTasksWithoutParent_ShouldNotThrowExceptions()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var taskWithParent = BMSTestHelper.CreateTask(workflow, staff.GS_Code);
			var taskStandAlone = Factory.NewWithValidTestData<ProcessTask>();
			var taskWithoutParent = BMSTestHelper.CreateTask(workflow, staff.GS_Code);
			taskWithoutParent.P9_ParentID = CargoWise.Types.ZGuid.Empty;

			var controlCustomisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			controlCustomisation.FM_JobType = "ORG";
			var customisationLine = controlCustomisation.CustomisationLines.AddNew();
			customisationLine.PropertySource = PropertySourceList.Codes.Job;
			customisationLine.PropertyName = "PK";

			var tasks = new[] { taskWithParent, taskStandAlone, taskWithoutParent };
			var workflows = new[] { workflow };

			AssertNoExceptionThrown(() => new CustomisedControlDataCache(controlCustomisation, new PropertyCache(), tasks, workflows, false, false));
		}

		public void TestTasksWithoutParent_CustomField()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var taskWithoutParent = BMSTestHelper.CreateTask(workflow, staff.GS_Code);
			taskWithoutParent.P9_ParentID = CargoWise.Types.ZGuid.Empty;

			var controlCustomisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			controlCustomisation.FM_JobType = "ORG";
			controlCustomisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;
			var customisationLine = controlCustomisation.CustomisationLines.AddNew();
			customisationLine.PropertySource = PropertySourceList.Codes.Job;
			customisationLine.PropertyName = "<GetCustomField(schmeckels)>";

			var taskCardContent = new TaskCardContent(taskWithoutParent, null);
			AssertEquals(string.Empty, customisationLine.GetBindingPath(taskCardContent));
		}

		#region Default Values

		public void TestLine_DefaultPlayButtonBehavior()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			AssertEquals("SAV", line.PlayButtonBehavior);
		}

		public void TestLine_DefaultSuspendButtonBehavior()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			AssertEquals("SAV", line.SuspendButtonBehavior);
		}

		public void TestLine_DefaultCloseTaskButtonBehavior()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			var line = customisation.CustomisationLines.AddNew();
			AssertEquals("SAV", line.CloseTaskButtonBehavior);
		}

		#endregion

		class SalesEnquirySubclass : SalesEnquiry
		{
			public SalesEnquirySubclass(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BMControlCustomisationLine(Factory.New<BMControlCustomisation>());
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "Alignment";
				yield return "AutoSize";
				yield return "BackgroundColor";
				yield return "BringToFront";
				yield return "CloseTaskButtonBehavior";
				yield return "ControlType";
				yield return "Font";
				yield return "FontSize";
				yield return "ForegroundColor";
				yield return "Height";
				yield return "IsBold";
				yield return "IsReadOnly";
				yield return "Label";
				yield return "Left";
				yield return "Orientation";
				yield return "PlayButtonBehavior";
				yield return "PropertyName";
				yield return "PropertySource";
				yield return "SuspendButtonBehavior";
				yield return "Top";
				yield return "Width";
			}
		}

		#endregion
	}
}
