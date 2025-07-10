using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Specifies that Controller doesn't support showing a form
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ControllerDoesNotSupportFormAttribute : Attribute
	{
		public static bool HasAttribute(ZController controller)
		{
			return controller.GetType().GetCustomAttributes(typeof(ControllerDoesNotSupportFormAttribute), false).Length > 0;
		}
	}
}
