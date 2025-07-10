using System;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Applied to a base container control class (UserControl/Form) that is inherited to provide controls.
	/// Currently this is used to determine when to stop inheritance fallback when looking for resource
	/// strings whose key has the container control name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ContainerControlBaseClassAttribute : Attribute
	{
	}
}
