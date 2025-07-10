using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ParentAndChildCodeDescriptionBoolRegistryItemEditor))]
	sealed class ParentAndChildCodeDescriptionBoolRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestEditorPane()
		{
			using (ZForm form = new ZForm())
			{
				ParentAndChildCodeDescriptionBoolControl control = (ParentAndChildCodeDescriptionBoolControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(new ParentCodeDescriptionBoolCollection(), null);

				BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

				ZGroupBox parentGroupBox = (ZGroupBox)typeof(ParentAndChildCodeDescriptionBoolControl).GetField("ParentGroupBox", flags).GetValue(control);
				ZGroupBox childGroupBox = (ZGroupBox)typeof(ParentAndChildCodeDescriptionBoolControl).GetField("ChildGroupBox", flags).GetValue(control);
				CodeDescriptionBoolControl parentGridContainer = (CodeDescriptionBoolControl)typeof(ParentAndChildCodeDescriptionBoolControl).GetField("ParentGrid", flags).GetValue(control);
				CodeDescriptionBoolControl childGridContainer = (CodeDescriptionBoolControl)typeof(ParentAndChildCodeDescriptionBoolControl).GetField("ChildGrid", flags).GetValue(control);
				ZGrid parentGrid = (ZGrid)typeof(CodeDescriptionBoolControl).GetField("CodeDescriptionBoolGrid", flags).GetValue(parentGridContainer);
				ZGrid childGrid = (ZGrid)typeof(CodeDescriptionBoolControl).GetField("CodeDescriptionBoolGrid", flags).GetValue(childGridContainer);

				AssertEquals("ParentGroupBox.Text", "P1", parentGroupBox.Text);
				AssertEquals("ParentGroupBox.Text", "C1", childGroupBox.Text);
				AssertEquals("ParentGrid.Columns.Count", 3, parentGrid.Columns.Count);
				AssertEquals("ParentGrid.Columns[\"Bool\"].ColumnStyle.HeaderText", "P1B", parentGrid.Columns["Bool"].ColumnStyle.HeaderText);
				AssertEquals("ChildGrid.Columns.Count", 2, childGrid.Columns.Count);
				AssertNull("ChildGrid.Columns[\"Bool\"]", childGrid.Columns["Bool"]);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ParentAndChildCodeDescriptionBoolRegistryItemEditor(RegistryItem.DataType, RegistryItem.EditorInfo);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ParentAndChildCodeDescriptionBoolControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ParentAndChildCodeDescriptionBoolControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo(
				(NoResString)"P1", (NoResString)"C1",
				new CodeDescriptionBoolRegistryEditorInfo((NoResString)"P1B", true),
				new CodeDescriptionBoolRegistryEditorInfo((NoResString)"C1B", false));
			return new ParentAndChildCodeDescriptionBoolRegistryItem("", null, null, null, editorInfo, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			ParentCodeDescriptionBoolCollection collection = new ParentCodeDescriptionBoolCollection();
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
