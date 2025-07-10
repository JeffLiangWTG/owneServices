using System;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Implement on a data source if one or more events are applicable.
	/// The DataSourcePosting event is used in the KBindingSource to know when to end all current edits.
	/// </summary>
	public interface IDataSourceEvents
	{
		event EventHandler DataSourcePosting;
	}
}
