using System;
using System.Collections.Generic;
using System.Threading;

namespace Enterprise.ArchiveManager.Integration
{
	/// <summary>
	/// One of the stages within an Archive System's process. It describes how to archive one set of tables that follow the same archive logic.
	/// </summary>
	public interface IArchiveStage
	{
		/// <summary>
		/// Name of the stage to be displayed on the archiving log. e.g. Standalone Asycuda Manifest Header Archive
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Prepares the temporary tables etc for the Archive Stage. Called once at start of running this stage.
		/// The same config and logger object will be used for this Stage.
		/// </summary>
		/// <param name="config">Archive config object</param>
		/// <param name="schedule">Archive schedule object</param>
		/// <param name="logger">Archive logging object</param>
		void BeginRun(IArchiveConfiguration config, IArchiveSchedule schedule, IArchiveLogger logger);

		/// <summary>
		/// Loads the next set of record PKs to be archived off together as a single unit of transaction.
		/// </summary>
		/// <param name="watermark">This is normally the date of the record archived up to in the last run. Used to filter the current archive run record set, such that this run will not attempt to archive again those records already skipped in the last run.</param>
		/// <param name="schedule">Archive schedule object</param>
		/// <param name="stage">Archive stage object</param>
		/// <returns>An archive Set is the single unit of archiving in a transaction.</returns>
		IEnumerable<IArchiveSet> GetNextArchiveSet(IArchiveWatermark watermark, IArchiveSchedule schedule, IArchiveStage stage);
		List<(Guid, Guid, string)> GetNextArchiveSet();

		/// <summary>
		/// Runs the archive stage main process, to archive a single set to images.
		/// </summary>
		/// <param name="archiveSet">An archive set is the single unit of archiving, loaded first by the GetNextArchiveSet method.</param>
		/// <returns></returns>
		IArchiveStepResult ArchiveToImages(IArchiveSet archiveSet);

		bool ExecuteStage(IArchiveSystem system, IArchiveStage stage, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSchedule schedule, CancellationToken token);

		/// <summary>
		/// The number of archive sets an archive stage will attempt to load and process simultaneously.
		/// </summary>
		int BatchSize { get; }

		/// <summary>
		/// This is called at the end of the archive stage to remove all temporary tables created.
		/// </summary>
		void EndRun();
	}
}
