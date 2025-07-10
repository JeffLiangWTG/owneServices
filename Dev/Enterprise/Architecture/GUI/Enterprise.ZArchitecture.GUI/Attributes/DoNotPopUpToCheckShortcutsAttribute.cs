using System;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Apply this attribute to any MenuItem that does not contain sub-menu items that should be executed by keyboard shortcuts in modules.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DoNotPopUpToCheckShortcutsAttribute : Attribute
	{
	}
}
