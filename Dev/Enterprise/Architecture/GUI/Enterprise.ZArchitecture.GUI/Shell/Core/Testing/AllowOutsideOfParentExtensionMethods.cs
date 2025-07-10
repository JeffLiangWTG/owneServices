using System.Windows.Forms;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	public static class AllowOutsideOfParentExtensionMethods
	{
		public static void AllowOutsideOfParent(this Control control)
		{
			#if DEBUG

			if (TestingState.IsRunningTests && !control.IsDisposed)
			{
				control.SetUserData(AllowOutsideOfParentUserDataKey, true);
			}

			#endif
		}

		#if DEBUG

		internal static bool IsAllowedOutsideOfParent(this Control control)
		{
			return control.GetUserData(AllowOutsideOfParentUserDataKey) is bool value && value;
		}

		static readonly int AllowOutsideOfParentUserDataKey = ControlExtensions.CreateUserDataKey();

		#endif
	}
}
