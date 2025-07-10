using System;
using System.Drawing;
using Enterprise.Client.JAS.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Client.JAS.Registry.GUI
{
	partial class CognosModeMappingControl : RegistryBusinessObjectTemplateZUserControl
	{
		public CognosModeMappingControl()
		{
			InitializeComponent();
			InitialiseButtons();

			SetDataSourceBinding(MapButton, "IsEnabledForBinding", CognosModeMapping.Schema.IsValidModeSelected);
			SetDataSourceBinding(UnmapButton, "IsEnabledForBinding", CognosModeMapping.Schema.IsValidModeSelected);
		}

		void MapButton_Click(object sender, EventArgs e)
		{
			HandleMapButtonClick();
		}

		void UnmapButton_Click(object sender, EventArgs e)
		{
			HandleUnmapButtonClick();
		}

		void HandleMapButtonClick()
		{
			GlbDepartment[] departments = AvailableDeptGrid.GetSelectedElements<GlbDepartment>();
			if (departments.Length > 0)
			{
				CurrentDataItem.MapDepartments(departments);
			}
		}

		void HandleUnmapButtonClick()
		{
			GlbDepartment[] departments = SelectedDeptGrid.GetSelectedElements<GlbDepartment>();
			if (departments.Length > 0)
			{
				CurrentDataItem.UnmapDepartments(departments);
			}
		}

		void InitialiseButtons()
		{
			MapButton.Text = "\u25B2";
			MapButton.Font = new Font("Arial", 8.50F);

			UnmapButton.Text = "\u25BC";
			UnmapButton.Font = new Font("Arial", 8.50F);
		}

		new CognosModeMapping CurrentDataItem
		{
			get { return (CognosModeMapping)base.CurrentDataItem; }
		}
	}
}
