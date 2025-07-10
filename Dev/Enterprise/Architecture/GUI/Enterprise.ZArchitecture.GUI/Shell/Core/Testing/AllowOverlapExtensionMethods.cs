using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	public static class AllowOverlapExtensionMethods
	{
		public static void AllowOverlap(this Control control, Control overControl)
		{
#if DEBUG

			if (TestingState.IsRunningTests && !control.IsDisposed)
			{
				if (control == overControl)
				{
					throw new ArgumentException($"{control.Name} cannot overlap itself.");
				}

				var allowedOverlap = (List<Control>)control.GetUserData(AllowOverlapUserDataKey);
				if (allowedOverlap == null)
				{
					allowedOverlap = new List<Control>(3);
					control.SetUserData(AllowOverlapUserDataKey, allowedOverlap);
				}

				var alreadyCheckedControls = new HashSet<Control>();
				if (overControl.IsOverlapping(control, ref alreadyCheckedControls))
				{
					throw new ArgumentException($"{control.Name} cannot overlap {overControl.Name} as {overControl.Name} is already overlapping {control.Name}.");
				}

				allowedOverlap.Add(overControl);

				overControl.Disposed += ((s, arg) =>
				{
					allowedOverlap.Remove(overControl);
				});
			}

#endif

#if WINZOR
#pragma warning disable CW1040 // Check for mixed arithmatic between scaled and unscaled components

			control.ZIndex = Math.Max(control.ZIndex, overControl.ZIndexMax + 1);

#pragma warning restore CW1040
#endif
		}

#if DEBUG
		static bool IsOverlapping(this Control control, Control initialControl, ref HashSet<Control> alreadyCheckedControls)
		{
			if (control.IsDisposed)
			{
				return false;
			}
			var allowedOverlap = (List<Control>)control.GetUserData(AllowOverlapUserDataKey);
			if (allowedOverlap == null)
			{
				return false;
			}

			if (alreadyCheckedControls.Contains(control))
			{
				return false;
			}
			alreadyCheckedControls.Add(control);

			if (allowedOverlap.Contains(initialControl))
			{
				return true;
			}

			for (var i = 0; i < allowedOverlap.Count; i++)
			{
				if (allowedOverlap[i].IsOverlapping(initialControl, ref alreadyCheckedControls))
				{
					return true;
				}
			}
			return false;
		}

		internal static bool IsOverlapAllowed(this Control control, Control overControl)
		{
			var allowedOverlap = (List<Control>)control.GetUserData(AllowOverlapUserDataKey);
			return allowedOverlap != null && allowedOverlap.Contains(overControl);
		}

		static readonly int AllowOverlapUserDataKey = ControlExtensions.CreateUserDataKey();

#endif
	}
}
