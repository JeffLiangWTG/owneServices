using System.Collections.Generic;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implemented on a control that requires custom behaviour to complete an edit.
	/// </summary>
	public interface IEndCurrentEdit
	{
		/// <summary>
		/// End the current edit in the control.
		/// </summary>
		void EndCurrentEdit();
	}

	public static class KEndCurrentEdit
	{
		public static void EndCurrentEdit(Control control)
		{
			IEndCurrentEdit endCurrentEdit = control as IEndCurrentEdit;
			if (endCurrentEdit != null)
			{
				endCurrentEdit.EndCurrentEdit();
			}
			else
			{
				DefaultEndCurrentEdit(control);
			}
		}

		static void DefaultEndCurrentEdit(Control control)
		{
			List<BindingManagerBase> managersDone = new List<BindingManagerBase>();
			foreach (Binding binding in control.DataBindings)
			{
				BindingManagerBase manager = binding.BindingManagerBase;
				if (manager != null && !managersDone.Contains(manager))
				{
					managersDone.Add(manager);
					manager.EndCurrentEdit();
				}
			}
		}
	}
}
