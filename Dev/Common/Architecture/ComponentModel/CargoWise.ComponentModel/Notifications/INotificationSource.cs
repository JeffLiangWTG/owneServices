using System.Collections.Generic;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// An object that has notifications on it.
	/// </summary>
	public interface INotificationSource
	{
		/// <summary>
		/// Get the notifications on the object.
		/// </summary>
		IEnumerable<INotification> Notifications { get; }
	}
}
