using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZMacrosFindBoxTest : TestCaseWithDummy
	{
		public void TestAntlrDefaultValues() => CombineAssertions(() =>
		{
			using var findBox = new ZMacrosFindBox(MacroType.Antlr);
			AssertEquals("UseMcrEvaluator", true, findBox.UseMcrEvaluator);
			AssertEquals("DefaultCollectionIndex", 0, findBox.DefaultCollectionIndex);
			AssertEquals("OpeningMacroTag", string.Empty, findBox.OpeningMacroTag);
			AssertEquals("ClosingMacroTag", string.Empty, findBox.ClosingMacroTag);
		});

		[NUnit.Framework.ExpectNoExceptions]
		public void TestGetList()
		{
			using (var findBox = new ZMacrosFindBox())
			{
				AssertNull(findBox.List);
			}
		}
		public void TestMapTreeFormPopup()
		{
			using (var findBox = new ZMacrosFindBox(MacroType.Antlr))
			{
				var dummy = Factory.New<DummyWithWorkflow>();
				var task = dummy.WorkflowItems.Triggers.AddNew();
				findBox.Current = task;
				findBox.PropertyDescriptor = TypeDescriptor.GetProperties(task).Find("TriggerConditions+TriggerConditionValue", false);

				findBox.SelectFromPopupForm();
				AssertNotNull(findBox.mapTreePresenter);
			}
		}

		public void TestMapTreeFormPopupForUniversalTemplateTrigger()
		{
			using (var findBox = new ZMacrosFindBox(MacroType.Antlr))
			{
				var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
				var universalTrigger = universalTemplate.TemplateTriggers.AddNew();
				findBox.Current = universalTrigger;
				findBox.PropertyDescriptor = TypeDescriptor.GetProperties(universalTrigger).Find("TriggerConditions+TriggerConditionValue", false);

				findBox.SelectFromPopupForm();
				AssertNotNull(findBox.mapTreePresenter);
				AssertEquals(typeof(DummyEnterpriseBusinessObject), findBox.mapTreePresenter.ParentTypes.Single().BaseType);
			}
		}
	}
}
