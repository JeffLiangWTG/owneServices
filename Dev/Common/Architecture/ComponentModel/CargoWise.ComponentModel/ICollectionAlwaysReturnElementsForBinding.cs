using System;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Implemented on a collection to present to .net data binding the ability to provide at least 1
	/// element at all times.
	/// </summary>
	public interface ICollectionAlwaysReturnElementsForBinding
	{
		IDisposable AlwaysReturnElementsForBinding();
		bool InAlwaysReturnElementsForBinding { get; }
	}
}
