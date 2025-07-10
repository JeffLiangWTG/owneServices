using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveStageDescriptor
	{
		/// <summary>
		/// Name of the Stage, e.g. Standalone Asycuda Manifest Header Archive
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The PK column for the main archive record type, from which Archive Manager will search for related records using the relationships setup by the Setup() method.
		/// </summary>
		SchemaColumn MainArchivePKColumn { get; }

		/// <summary>
		/// The Natural Key column for the main archive record table. Return null if there is no single NK column, and Archive Manager will log messages using the PK instead.
		/// </summary>
		SchemaColumn MainArchiveNKColumn { get; }

		/// <summary>
		/// The date column to filter for, and is expected to be on the same table as the MainArchivePKColumn
		/// </summary>
		SchemaDateTimeColumn MainDateFilterColumn { get; }

		/// <summary>
		/// Setup archive system parameters, including relationships of the other records to be archived with the main archiveable record type.
		/// You can also perform any other setup tasks for this Archive Stage.
		/// </summary>
		/// <param name="system">Provides methods to call for setting up the archive sub system.</param>
		/// <param name="schedule">Schedule object</param>
		/// <param name="config">Provides the configuration info to help construct the filter, e.g. You can filter by the Date provided in config.</param>
		void Setup(IArchiveSystemSetup systemSetup, IArchiveSchedule schedule, IArchiveConfiguration config);

		void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config);

		/// <summary>
		/// Return a filter to be used by archive system to query for main archiveable records in the database.
		/// </summary>
		/// <param name="config">Provides the configuration info to help construct the filter, e.g. You can filter by the Date provided in config.</param>
		ZQuery GetMainArchiveableFilter(IArchiveConfiguration config);

		/// <summary>
		/// Override this method to perform additional preparation actions that persists before ArchiveActions are performed, or do nothing by returning null.
		/// </summary>
		/// <param name="logger">Logging object</param>
		/// <param name="set">an archive set that is about to be archived in an archive system</param>
		/// <param name="cache">an archive cache kept for the duration of the Archive System run. Use it for caching objects for fast performance</param>
		IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config);

		/// <summary>
		/// Override this method to perform additional archive actions in a transaction, or do nothing by returning null.
		/// </summary>
		/// <param name="logger">Logging object</param>
		/// <param name="set">an archive set that is about to be archived in an archive system</param>
		IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set);

		/// <summary>
		/// Override this method to do some tasks to finalise the archive stage, such as cleaning up orphan records or producing final archive report.
		/// </summary>
		/// <param name="logger">Logging object</param>
		/// <param name="schedule">Schedule object</param>
		/// <param name="config">Provides the configuration info to help construct the filter, e.g. You can filter by the Date provided in config.</param>
		void Finalise(IArchiveSystemDescriptor descriptor, IArchiveLogger logger, IArchiveSchedule schedule, IArchiveConfiguration config);

		/// <summary>
		/// Call this method to perform actions after an archive set was successfully archived. For example, increase count of archived records.
		/// </summary>
		/// <param name="set">an archive set that has been succesfully archived</param>
		void OnArchiveSetProcessed(IArchiveSet set);

		bool IsStageUsingTempTables { get; }
	}
}
