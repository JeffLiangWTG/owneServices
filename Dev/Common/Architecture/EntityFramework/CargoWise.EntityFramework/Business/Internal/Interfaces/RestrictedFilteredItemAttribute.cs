using System;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Specifies that elements can be accessed from looukups (e.g. from find-box by F3) only if they match filter on corresponding collection.
	/// It will prevent opening and editing elements that exist in different companies or countries and should not be accessed in current context.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class RestrictedFilteredItemAttribute : Attribute
	{
	}
}
