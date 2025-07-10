using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Registry.GUI
{
	[TestedType(typeof(CognosModeMappingControl))]
	class CognosModeMappingControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		public void TestMapAndUnmapButtons()
		{
			AssertEquals("Should be initialised in the constructor", "\u25B2", ModeMappingControl.MapButton.Text);
			AssertEquals("Should be initialised in the constructor", "Arial", ModeMappingControl.MapButton.Font.Name);
			AssertEquals("Should be initialised in the constructor", 8.50F, ModeMappingControl.MapButton.Font.Size);
			AssertEquals("Should be initialised in the constructor", "\u25BC", ModeMappingControl.UnmapButton.Text);
			AssertEquals("Should be initialised in the constructor", "Arial", ModeMappingControl.UnmapButton.Font.Name);
			AssertEquals("Should be initialised in the constructor", 8.50F, ModeMappingControl.UnmapButton.Font.Size);
		}

		public void TestMapDepartments()
		{
			CognosModeMappingForTest modeMapping = new CognosModeMappingForTest();
			modeMapping.SelectedMode = "AI";
			AssertNull("Pre-condition", modeMapping.LastDepartmentsMapped);
			using (ZForm form = new ZForm(modeMapping))
			{
				form.Controls.Add(ModeMappingControl);
				form.Show();
				ModeMappingControl.SetDataBinding(modeMapping, "");
				ModeMappingControl.MapButton.PerformClick();
				AssertNull("Should still be null, no available departments selected", modeMapping.LastDepartmentsMapped);
				ModeMappingControl.AvailableDeptGrid.Select(0);
				GlbDepartment selectedDept = (GlbDepartment)ModeMappingControl.AvailableDeptGrid.SelectedElements[0];
				ModeMappingControl.MapButton.PerformClick();
				AssertEquals("The selected department should be mapped", 1, modeMapping.LastDepartmentsMapped.Length);
				AssertEquals("The selected department should be mapped", selectedDept, modeMapping.LastDepartmentsMapped[0]);
			}
		}

		public void TestUnmapDepartments()
		{
			CognosModeMappingForTest modeMapping = new CognosModeMappingForTest();
			modeMapping.SelectedMode = "AI";
			AssertNull("Pre-condition", modeMapping.LastDepartmentsUnmapped);
			using (ZForm form = new ZForm(modeMapping))
			{
				form.Controls.Add(ModeMappingControl);
				form.Show();
				ModeMappingControl.SetDataBinding(modeMapping, "");
				ModeMappingControl.UnmapButton.PerformClick();
				AssertNull("Should still be null, no mapped departments selected", modeMapping.LastDepartmentsUnmapped);
				ModeMappingControl.AvailableDeptGrid.Select(1);
				ModeMappingControl.MapButton.PerformClick();
				Application.DoEvents();
				ModeMappingControl.SelectedDeptGrid.Select(0);
				GlbDepartment selectedDept = (GlbDepartment)ModeMappingControl.SelectedDeptGrid.SelectedElements[0];
				ModeMappingControl.UnmapButton.PerformClick();
				AssertEquals("The selected department should be unmapped", 1, modeMapping.LastDepartmentsUnmapped.Length);
				AssertEquals("The selected department should be unmapped", selectedDept, modeMapping.LastDepartmentsUnmapped[0]);
			}
		}

		public void TestButtonsEnabledDependingOnSelectedMode()
		{
			using (ZForm form = new ZForm())
			{
				CognosModeMapping modeMapping = new CognosModeMapping();
				form.Controls.Add(ModeMappingControl);
				form.Show();
				ModeMappingControl.SetDataBinding(modeMapping, "");
				Assert("Invalid mode selected, should be disabled", !ModeMappingControl.MapButton.Enabled);
				Assert("Invalid mode selected, should be disabled", !ModeMappingControl.UnmapButton.Enabled);
				modeMapping.SelectedMode = "AI";
				Assert("Valid mode selected, should be enabled", ModeMappingControl.MapButton.Enabled);
				Assert("Valid mode selected, should be enabled", ModeMappingControl.UnmapButton.Enabled);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			ModeMappingControl = new CognosModeMappingControl();
		}

		protected override void TearDown()
		{
			ModeMappingControl.Dispose();
			base.TearDown();
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CognosModeMapping();
		}

		CognosModeMappingControl ModeMappingControl;
		#endregion
		#region class CognosModeMappingForTest
		class CognosModeMappingForTest : CognosModeMapping
		{
			public override void MapDepartments(params GlbDepartment[] departments)
			{
				LastDepartmentsMapped = departments;
				base.MapDepartments(departments);
			}

			public override void UnmapDepartments(params GlbDepartment[] departments)
			{
				LastDepartmentsUnmapped = departments;
				base.UnmapDepartments(departments);
			}

			public GlbDepartment[] LastDepartmentsMapped;
			public GlbDepartment[] LastDepartmentsUnmapped;
		}
		#endregion
	}
}
