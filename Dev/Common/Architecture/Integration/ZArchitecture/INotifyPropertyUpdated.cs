
namespace CargoWise.Integration
{
	using System;
	using System.ComponentModel;

	/// <summary>
	///		Notifies clients that a property value has changed.
	/// </summary>
	/// <remarks>
	///		M.K:	"We do not use the <see cref="INotifyPropertyChanged"/> interface because it can be used for other purposes, be implemented on other objects, 
	///				and therefore lead (to small but existent) performance decreases. Also this interface is used by .NET system binding mechanism, and 
	///				I would not want to mess with that."
	/// </remarks>
	public interface INotifyPropertyUpdated
	{
		/// <summary>
		///		Occurs when a property value changes.
		/// </summary>
		event EventHandler<PropertyUpdatedEventArgs> PropertyUpdated;
	}
}
