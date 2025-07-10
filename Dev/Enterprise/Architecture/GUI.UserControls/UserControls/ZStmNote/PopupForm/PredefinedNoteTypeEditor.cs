using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	internal class PredefinedNoteTypeEditor : UITypeEditor
	{
#if !WINZOR

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.DropDown;

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider serviceProvider, object value)
		{
			EditorService = (IWindowsFormsEditorService)serviceProvider.GetService(typeof(IWindowsFormsEditorService));

			if (EditorService != null)
			{
				var noteTypesListBox = new ListBox();
				ControlDpiScalingHelper.SetHeight(ref noteTypesListBox, 300, true);
				noteTypesListBox.Sorted = true;
				noteTypesListBox.BorderStyle = BorderStyle.None;

				var selectedIndex = -1;
				var infos = typeof(PredefinedNoteTypes).GetProperties(BindingFlags.Static | BindingFlags.Public);

				foreach (var info in infos)
				{
					if (info.GetValue(null, null) is PredefinedNoteType noteType && noteType.IsTextOnly) // only add text notes
					{
						var index = noteTypesListBox.Items.Add(noteType.Description);
						if (value != null && noteType.ToString() == value.ToString())
						{
							selectedIndex = index;
						}
					}
				}

				noteTypesListBox.SelectedIndex = selectedIndex;
				noteTypesListBox.SelectedIndexChanged += new EventHandler(NoteTypesListBox_SelectedIndexChanged);
				EditorService.DropDownControl(noteTypesListBox);

				value = noteTypesListBox.SelectedItem;
			}

			return value;
		}

		void NoteTypesListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			((ListBox)sender).SelectedIndexChanged -= new EventHandler(NoteTypesListBox_SelectedIndexChanged);
			EditorService.CloseDropDown();
		}

		IWindowsFormsEditorService EditorService;

#endif
	}
}
