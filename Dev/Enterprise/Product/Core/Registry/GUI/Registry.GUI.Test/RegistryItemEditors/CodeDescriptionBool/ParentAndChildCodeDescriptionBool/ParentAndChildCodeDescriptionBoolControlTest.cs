using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ParentAndChildCodeDescriptionBoolControl))]
	sealed class ParentAndChildCodeDescriptionBoolControlTest : Testing.RegistryZUserControlTestCase
	{
		[RequiresSTA]
		public void TestCaptionsAndGrids()
		{
			using (ZForm form = new ZForm())
			{
				ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo(
					(NoResString)"P1", (NoResString)"C1",
					new CodeDescriptionBoolRegistryEditorInfo((NoResString)"P1B", false),
					new CodeDescriptionBoolRegistryEditorInfo((NoResString)"C1B", true));

				using (ParentAndChildCodeDescriptionBoolControl control = new ParentAndChildCodeDescriptionBoolControl(editorInfo))
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(GetNewBusinessEntity(), null);

					AssertEquals("ParentGroupBox.Text", "P1", control.ParentGroupBox.Text);
					AssertEquals("ParentGroupBox.Text", "C1", control.ChildGroupBox.Text);

					ZGrid parentGrid = GetGrid(control.ParentGrid);
					AssertEquals("ParentGrid.Columns.Count", 2, parentGrid.Columns.Count);
					AssertNull("ParentGrid.Columns[\"Bool\"]", parentGrid.Columns["Bool"]);

					ZGrid childGrid = GetGrid(control.ChildGrid);
					AssertEquals("ChildGrid.Columns.Count", 3, childGrid.Columns.Count);
					AssertEquals("ChildGrid.Columns[\"Bool\"].ColumnStyle.HeaderText", "C1B", childGrid.Columns["Bool"].ColumnStyle.HeaderText);
				}

				editorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo(
					(NoResString)"P2", (NoResString)"C2",
					new CodeDescriptionBoolRegistryEditorInfo((NoResString)"P2B", true),
					new CodeDescriptionBoolRegistryEditorInfo((NoResString)"C2B", false));

				using (ParentAndChildCodeDescriptionBoolControl control = new ParentAndChildCodeDescriptionBoolControl(editorInfo))
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(GetNewBusinessEntity(), null);

					AssertEquals("ParentGroupBox.Text", "P2", control.ParentGroupBox.Text);
					AssertEquals("ParentGroupBox.Text", "C2", control.ChildGroupBox.Text);

					ZGrid parentGrid = GetGrid(control.ParentGrid);
					AssertEquals("ParentGrid.Columns.Count", 3, parentGrid.Columns.Count);
					AssertEquals("ParentGrid.Columns[\"Bool\"].ColumnStyle.HeaderText", "P2B", parentGrid.Columns["Bool"].ColumnStyle.HeaderText);

					ZGrid childGrid = GetGrid(control.ChildGrid);
					AssertEquals("ChildGrid.Columns.Count", 2, childGrid.Columns.Count);
					AssertNull("ChildGrid.Columns[\"Bool\"]", childGrid.Columns["Bool"]);
				}
			}
		}

		public void TestGridSplitter()
		{
			ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo(
				(NoResString)"P1", (NoResString)"C1",
				new CodeDescriptionBoolRegistryEditorInfo((NoResString)"P1B", false),
				new CodeDescriptionBoolRegistryEditorInfo((NoResString)"C1B", true));

			using (ParentAndChildCodeDescriptionBoolControl control = new ParentAndChildCodeDescriptionBoolControl(editorInfo))
			{
				AssertEquals("GridSplitter.MinExtra", control.ChildGridPanel.Height - 100, control.GridSplitter.MinExtra);
				AssertEquals("GridSplitter.MinSize", control.ParentGroupBox.Height, control.GridSplitter.MinSize);
			}
		}

		public void TestParentGridReadOnly()
		{
			var editorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo((NoResString)"P1", (NoResString)"C1", new CodeDescriptionBoolRegistryEditorInfo((NoResString)"P1B", false),
				new CodeDescriptionBoolRegistryEditorInfo((NoResString)"C1B", true));

			Assert("EditorInfo.IsParentListReadOnly", !editorInfo.IsParentListReadOnly);
			AssertParentGridReadOnly(editorInfo);

			editorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo((NoResString)"P1", (NoResString)"C1", new CodeDescriptionBoolRegistryEditorInfo((NoResString)"P1B", false),
				new CodeDescriptionBoolRegistryEditorInfo((NoResString)"C1B", true), true);

			Assert("EditorInfo.IsParentListReadOnly", editorInfo.IsParentListReadOnly);
			AssertParentGridReadOnly(editorInfo);
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new ParentCodeDescriptionBoolCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			ParentAndChildCodeDescriptionBoolControl typeCastControl = (ParentAndChildCodeDescriptionBoolControl)control;
			return typeCastControl.ParentGrid.ReadOnly && typeCastControl.ChildGrid.ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo(
				(NoResString)"P1", (NoResString)"C1",
				new CodeDescriptionBoolRegistryEditorInfo((NoResString)"P1B", true),
				new CodeDescriptionBoolRegistryEditorInfo((NoResString)"C1B", true));
			return new ParentAndChildCodeDescriptionBoolControl(editorInfo);
		}

		ZGrid GetGrid(CodeDescriptionBoolControl control)
		{
			FieldInfo info = typeof(CodeDescriptionBoolControl).GetField("CodeDescriptionBoolGrid", BindingFlags.NonPublic | BindingFlags.Instance);
			return (ZGrid)info.GetValue(control);
		}

		void AssertParentGridReadOnly(ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo)
		{
			using (ZForm form = new ZForm())
			using (ParentAndChildCodeDescriptionBoolControl control = new ParentAndChildCodeDescriptionBoolControl(editorInfo))
			{
				form.Controls.Add(control);
				form.Show();

				IBusiness businessEntity = GetNewBusinessEntity();
				control.SetDataBinding(businessEntity, null);

				control.ReadOnly = true;
				Assert("Control ReadOnly", control.ReadOnly);
				AssertEquals("Parent Grid ReadOnly", editorInfo.IsParentListReadOnly || control.ReadOnly, control.ParentGrid.ReadOnly);

				control.ReadOnly = false;
				Assert("Control ReadOnly", !control.ReadOnly);
				AssertEquals("Parent Grid ReadOnly", editorInfo.IsParentListReadOnly || control.ReadOnly, control.ParentGrid.ReadOnly);
			}
		}

		#endregion
	}
}
