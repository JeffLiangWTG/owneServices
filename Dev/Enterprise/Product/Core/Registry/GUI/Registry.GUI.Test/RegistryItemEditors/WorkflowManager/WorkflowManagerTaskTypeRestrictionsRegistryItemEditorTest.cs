using System;
using CargoWise.Application;
using CargoWise.Integration;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WorkflowManagerTaskTypeRestrictionsRegistryItemEditor))]
	sealed class WorkflowManagerTaskTypeRestrictionsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestSetValueFromEditorPane()
		{
			using (ZForm form = new ZForm())
			{
				var editorPane = (WorkflowManagerTaskTypeRestrictionsControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(editorPane);
				form.Show();

				var workflowDescriptors = (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>();
				var collection = new TaskTypeRestrictionsCollection();

				var parent1 = collection.AddNew();
				var parent2 = collection.AddNew();
				var parent3 = collection.AddNew();

				parent1.WorkflowType = workflowDescriptors[0].Code;
				parent2.WorkflowType = workflowDescriptors[1].Code;
				parent3.WorkflowType = "!@#";

				var child1 = parent1.TaskTypesCollection.AddNew();
				var child2 = parent2.TaskTypesCollection.AddNew();
				var child3 = parent3.TaskTypesCollection.AddNew();

				child1.Code = "C1";
				child2.Code = "C2";
				child3.Code = "C3";

				Editor.SetValueFromEditorPane(editorPane, collection);
				AssertSetAndGetValuesAreTheSame(((IDataBoundControl)editorPane).DataSource);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new WorkflowManagerTaskTypeRestrictionsRegistryItemEditor(RegistryItem.DataType, null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WorkflowManagerTaskTypeRestrictionsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TaskAssignmentRestrictionsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		void AssertSetAndGetValuesAreTheSame(object getValue)
		{
			var workflowDescriptorList = (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>();
			var getCollection = (TaskTypeRestrictionsCollection)getValue;

			foreach (ICodeDescription task in workflowDescriptorList)
			{
				AssertEquals(string.Format("GetValue.GetDescriptionFromCode(\"{0}\")", task.Code), task.Description, ((ICodeDescriptionPairList)getCollection[0].WorkflowTypeList).GetDescriptionFromCode(task.Code));
			}

			AssertEquals(workflowDescriptorList[0].Code, getCollection[0].WorkflowType);
			AssertEquals(workflowDescriptorList[1].Code, getCollection[1].WorkflowType);
			AssertEquals("!@#", getCollection[2].WorkflowType);

			foreach (TaskTypeRestrictions element in getCollection)
			{
				foreach (RestrictedTaskTypes child in element.TaskTypesCollection)
				{
					AssertEquals(element.WorkflowType, child.WorkflowType);
				}
			}

			AssertEquals("C1", getCollection[0].TaskTypesCollection[0].Code);
			AssertEquals("C2", getCollection[1].TaskTypesCollection[0].Code);
			AssertEquals("C3", getCollection[2].TaskTypesCollection[0].Code);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new TaskTypeRestrictionsCollection();
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((WorkflowManagerTaskTypeRestrictionsControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
