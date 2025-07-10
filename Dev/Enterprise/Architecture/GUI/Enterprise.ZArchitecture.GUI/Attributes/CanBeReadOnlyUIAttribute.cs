using System;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Specifies that this UI component can be made ReadOnly
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class CanBeReadOnlyUIAttribute : Attribute { }
}
