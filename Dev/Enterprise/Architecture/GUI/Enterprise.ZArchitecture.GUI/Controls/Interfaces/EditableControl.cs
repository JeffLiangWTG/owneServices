using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	public static class EditableControl
	{
		#region Get

		public static IEditableControl Get(Control control)
		{
			return control as IEditableControl ?? GetDefaultImplementation(control);
		}

		static IEditableControl GetDefaultImplementation(Control control)
		{
			return new DefaultEditableControl(control);
		}

		class DefaultEditableControl : IEditableControl
		{
			public DefaultEditableControl(Control control)
			{
				this.Control = control;
			}

			public Control Control { get; private set; }

			public bool IsEditing
			{
				get { return HasModifiedDataBindingValueOrIsChildControlEditing(Control); }
			}
		}

		#endregion

		#region HasModifiedDataBindingValueOrIsChildControlEditing

		public static bool HasModifiedDataBindingValueOrIsChildControlEditing(Control control)
		{
			var result = HasModifiedDataBindingValue(control.DataBindings);
			if (!result)
			{
				foreach (Control childControl in control.Controls)
				{
					var editableControl = childControl as IEditableControl;
					if (editableControl != null)
					{
						if (editableControl.IsEditing)
						{
							result = true;
							break;
						}
					}
					else if (HasModifiedDataBindingValueOrIsChildControlEditing(childControl))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		public static bool HasModifiedDataBindingValue(BindingsCollection bindings)
		{
			foreach (Binding binding in bindings)
			{
				if ((bool)BindingModifiedField.GetValue(binding))
				{
					return true;
				}
			}
			return false;
		}

		static FieldInfo BindingModifiedField
		{
			get
			{
				if (bindingModifiedField == null)
				{
					bindingModifiedField = typeof(Binding).GetField("modified", BindingFlags.NonPublic | BindingFlags.Instance);
					if (bindingModifiedField == null)
					{
						bindingModifiedField = typeof(Binding).GetField("_modified", BindingFlags.NonPublic | BindingFlags.Instance);
					}
				}
				return bindingModifiedField;
			}
		}
		[SuppressThreadStaticFieldMessage]
		static FieldInfo bindingModifiedField;

		#endregion
	}
}
