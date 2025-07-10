using System;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	/// <summary>
	/// Ignores marking property as changed and overridden, which means that after user changes the property value
	/// - he/she/it won't be asked to save changes before sending message or printing
	/// - the overridden value won't be saved
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class IgnoreChangesAttribute : Attribute
	{
	}
}
