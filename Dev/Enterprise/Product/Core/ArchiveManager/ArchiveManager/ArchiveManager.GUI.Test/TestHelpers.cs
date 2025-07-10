using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.GUI.Test
{
	public static class TestHelpers
	{
		public static IArchiveSystem GetArchiveSystem(string code)
			=> new ArchiveSystem(
				new ArchiveSystemDescriptorLoader()
					.Load()
					.Single(d => d.Code == code));

		public static void RunArchiveSystem(string code, ArchiveConfiguration configuration, TestArchiveLogger logger, ArchiveScheduleTask schedule)
		{
			var archiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());
			archiveManager.Run(code, configuration, logger, schedule, new CancellationToken());
		}

		public static IeDoc GetArchiveReportFromSchedule(ArchiveScheduleTask archiveSchedule, string reportDescription = "Archive Report")
			=> GetAllReportsFromSchedule(archiveSchedule)
				.FirstOrDefault(eDoc => eDoc.Description == reportDescription);

		public static IEnumerable<IeDoc> GetAllReportsFromSchedule(ArchiveScheduleTask archiveSchedule)
			=> ((IDocManagerSupport)archiveSchedule)
				.DocManagerInfo
				.Files
				.Cast<IeDoc>();

		public static ArchiveScheduleTask GetArchiveScheduleTask(BusinessObjectFactory factory, string archiveSystem)
		{
			var archiveScheduleTask = factory.New<ArchiveScheduleTask>();
			archiveScheduleTask.MaxRunDurationInMinutes = 1;
			archiveScheduleTask.S5_ScheduleType = archiveSystem;
			archiveScheduleTask.S5_ScheduleDescription = "Description";

			return archiveScheduleTask;
		}

		public static bool ArchiveStageHasRelationship(ArchiveStage stage, string parentNameOverride, SchemaColumn parentKeyColumnReferenceByChild, string childNameOverride, SchemaColumn childFKColumn, bool isReversed)
		{
			if (stage.ArchiveableRelationships.ContainsKey(parentNameOverride))
			{
				var resolver = new EnterpriseSchemaResolver();
				var childPKCol = resolver.GetPkColumn(childFKColumn.TableName);
				var parentPKCol = resolver.GetPkColumn(parentKeyColumnReferenceByChild.TableName);
				var relationship = new ArchiveableRelationship(parentNameOverride, parentPKCol, parentKeyColumnReferenceByChild, childNameOverride, childPKCol, childFKColumn, isReversed);
				var relationshipList = stage.ArchiveableRelationships[parentNameOverride];

				return relationshipList.Count > 0
					&& relationshipList.Exists((r) => r.ParentName == parentNameOverride && r.ParentKeyColumnReferencedByChild == parentKeyColumnReferenceByChild && r.ChildName == childNameOverride && r.ChildFKColumn == childFKColumn && r.IsReversed == isReversed);
			}

			return false;
		}
	}
}
