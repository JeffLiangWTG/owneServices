using System;

namespace Enterprise.ZArchitecture.Core
{
	/// <summary>
	/// Base Data Concentrator. Contains templates to be implemented by specific importer/exporter types.
	/// </summary>
	public interface IDataTransfer
	{
		event ProcessedEventHandler Processed;
		event EventHandler ProcessCompleted;
	}
}
