using System;

namespace Enterprise.VisualBoards.Business
{
	/// <summary>
	/// Denotes a filter which requires descendants to be refreshed along with the filtered item to which the filter is applied.
	/// This means all nested children will receive an update through <see cref="FilterManager.RefreshFilters(FiltersChangedEventArgs, bool)"/> when filters are added or removed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DescendantRefreshableFilterAttribute : Attribute
	{
	}
}
