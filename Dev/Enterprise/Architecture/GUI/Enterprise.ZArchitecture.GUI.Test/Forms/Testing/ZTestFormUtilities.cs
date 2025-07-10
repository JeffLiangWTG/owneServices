using System.Collections.Generic;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public static class ZTestFormUtilities
	{
		#region Get Controls Recursively

		static public T[] GetControlsRecursively<T>(Form form) where T : Control
		{
			return GetControlsRecursivelyFromControl<T>(form).ToArray();
		}

		static List<T> GetControlsRecursivelyFromControl<T>(Control parentControl) where T : Control
		{
			var result = new List<T>();
			foreach (Control ctrl in parentControl.Controls)
			{
				if (typeof(T).IsInstanceOfType(ctrl))
				{
					result.Add((T)ctrl);
				}

				result.AddRange(GetControlsRecursivelyFromControl<T>(ctrl));
			}
			return result;
		}

		#endregion
	}
}
