using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class ModuleIDEditor : UITypeEditor
	{
		public ModuleIDEditor()
		{
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider serviceProvider, object value)
		{
			EditorService = (IWindowsFormsEditorService)serviceProvider.GetService(typeof(IWindowsFormsEditorService));
			if (EditorService != null)
			{
				var modulesListBox = new ListBox();
				modulesListBox.Sorted = true;
				modulesListBox.BorderStyle = BorderStyle.None;

				var selectedIndex = -1;

				foreach (var iD in GetModuleIDs())
				{
					var index = modulesListBox.Items.Add(iD);
					if (value != null && iD.ToString() == value.ToString())
					{
						selectedIndex = index;
					}
				}

				modulesListBox.SelectedIndex = selectedIndex;
				modulesListBox.SelectedIndexChanged += new EventHandler(ModulesListBox_SelectedValueChanged);
				EditorService.DropDownControl(modulesListBox);

				value = modulesListBox.SelectedItem;
			}

			return value;
		}

		IWindowsFormsEditorService EditorService;

		void ModulesListBox_SelectedValueChanged(object sender, EventArgs e)
		{
			((ListBox)sender).SelectedValueChanged -= new EventHandler(ModulesListBox_SelectedValueChanged);
			EditorService.CloseDropDown();
		}

		protected virtual IEnumerable<ModuleIdentifier> GetModuleIDs()
		{
			return ModuleIDs.AllExcludingClientModules;
		}
	}
}