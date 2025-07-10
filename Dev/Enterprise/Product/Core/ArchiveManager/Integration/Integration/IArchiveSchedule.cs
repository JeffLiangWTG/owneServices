using System;

namespace Enterprise.ArchiveManager.Integration
{
	/// <summary>
	/// Schedule object that provides schedule info and is also used as a storage area for data that persist between archive runs.
	/// </summary>
	public interface IArchiveSchedule
	{
		/// <summary>
		/// Get the watermark date for the archive run from the schedule.
		/// </summary>
		/// <param name="stageName">Archive stage name, as watermark date is unique per archive stage</param>
		IArchiveWatermark GetWatermark(string stageName);

		/// <summary>
		/// Sets the wartermark date for an archive stage run onto the schedule to keep.
		/// </summary>
		/// <param name="stageName"></param>
		/// <param name="watermark"></param>
		void SetWatermark(string stageName, IArchiveWatermark watermark);

		/// <summary>
		/// Attach an eDoc to the schedule object to retain.
		/// </summary>
		/// <param name="filepath">Path to the file to attach</param>
		/// <param name="eDocFileName">filename to retain in edoc</param>
		/// <param name="docType">eDoc doc type to use</param>
		/// <param name="description">eDoc description</param>
		void AttachEDoc(string filepath, string eDocFileName, string docType, string description);

		/// <summary>
		/// Get a PK associated with this schedule.
		/// </summary>
		Guid SchedulePK { get; }
	}
}
