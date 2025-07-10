using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(StaticControlCustomisation))]
	class StaticControlCustomisationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestControlTypes()
		{
			var customisedControl = Factory.New<BMControlCustomisation>().CustomisedControls.AddNew();
			Assert(customisedControl.Lookups.ControlTypes.Cast<CodeDescriptionPair>().SequenceEqual(new StaticControlTypeList().Cast<CodeDescriptionPair>()));
		}

		public void TestAttachedTagsIndicatorControlType_BackgroundShouldBeTransparent()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			var controlCustomisation = customisation.CustomisedControls.AddNew();
			controlCustomisation.ControlType = StaticControlTypeList.Codes.AttachedTagsIndicator;
			AssertEquals("Background should be Transparent", Color.Transparent.Name, controlCustomisation.BackgroundColor);
			controlCustomisation.BackgroundColor = Color.White.Name;
			AssertEquals("Background should still be Transparent", Color.Transparent.Name, controlCustomisation.BackgroundColor);
		}

		public void TestNotAvailableControlTypeShouldNotReportError()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, workflowType: WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow);

			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.FM_JobType = WorkflowDescriptors.DummyWorkflowDescriptorCode;
			var controlCustomisation = customisation.CustomisedControls.AddNew();
			controlCustomisation.ControlType = "ZZZ";

			ErrorReporter.Clear();
			AssertNoExceptionThrown(() => new CustomisedControlDataCache(customisation, new PropertyCache(), [task], [workflow], false, false));
			var exception = ExceptionReporterTestListener.Instance.FirstOrDefault()?.InnerException;
			AssertNull(exception);
		}

		public void TestNonReadOnlyControlTypeShouldReportError()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, workflowType: WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow);

			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.FM_JobType = WorkflowDescriptors.DummyWorkflowDescriptorCode;
			var controlCustomisation = customisation.CustomisedControls.AddNew();
			controlCustomisation.ControlType = StaticControlTypeList.Codes.WorkingStatusButton;

			new CustomisedControlDataCache(customisation, new PropertyCache(), [task], [workflow], false, false);

			var exception = ExceptionReporterTestListener.Instance.FirstOrDefault()?.InnerException;
			AssertNotNull(exception);
			AssertEquals("The control type [WRK] is not readonly but it is being used in a readonly context. Layout: Name: 316X6FL6K76VLJ8AVMEKZYJ5HXDGBX5KOYC16BGDXIA1HEOSLU3I25JESBQJCE4WCWHVGHDUGAVQXQYCIU3P2XQRP6J5T6BSNMZT, Job Type: DUM, Control Type: DHN",
				exception.Message);
			ErrorReporter.Clear();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<BMControlCustomisation>().CustomisedControls.AddNew();
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "Alignment";
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
				yield return "SuspendButtonBehavior";
				yield return "Top";
				yield return "Width";
			}
		}
	}
}
