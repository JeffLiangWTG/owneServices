using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class SuppressCheckControlModuleIdAttribute : Attribute
	{
		public static bool IsApplied(object component)
		{
			var control = component as Control;
			return control != null ? IsAppliedOnControl(control) : IsAppliedCore(component);
		}

		static bool IsAppliedOnControl(Control control)
		{
			var current = control;
			while (current != null)
			{
				if (IsAppliedCore(current))
				{
					return true;
				}
				current = current.Parent;
			}
			return false;
		}

		static bool IsAppliedCore(object component)
		{
			return component.GetType().GetCustomAttributes(typeof(SuppressCheckControlModuleIdAttribute), true).Length > 0;
		}
	}
}
