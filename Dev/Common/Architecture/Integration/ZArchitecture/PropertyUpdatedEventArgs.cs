
namespace CargoWise.Integration
{
	using System;
	using CargoWise.Common;

	/// <summary>
	///		Defines arguments for <see cref="INotifyPropertyUpdated.PropertyUpdated"/> event.
	/// </summary>
	public class PropertyUpdatedEventArgs : EventArgs
	{
		/// <summary>
		///		Initializes a new instance of the <see cref="PropertyUpdatedEventArgs"/> class.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		public PropertyUpdatedEventArgs(string propertyName)
		{
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));
			PropertyName = propertyName;
		}

		/// <summary>
		///		Gets the name of the property that changed.
		/// </summary>
		public string PropertyName { get; private set; }
	}
}
