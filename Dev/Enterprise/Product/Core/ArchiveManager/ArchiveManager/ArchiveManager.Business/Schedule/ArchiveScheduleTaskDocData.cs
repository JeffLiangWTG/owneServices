using System;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ArchiveScheduleTaskDocData),
	Enterprise.Core.Constants.DocManagerCodes.ArchiveSchedule)]

namespace Enterprise.ArchiveManager.Business.Schedule
{
	class ArchiveScheduleTaskDocData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ArchiveScheduleTask); } }

		protected override Type CollectionType
			=> typeof(ArchiveScheduleTaskCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
			=> new ArchiveScheduleTaskCollection(factory);

		public override ModuleIdentifier ModuleID
			=> ModuleIDs.ArchiveSchedule;

		public override string ReferenceType
			=> Core.Constants.ReferenceTypes.GeneralReferenceTables;

		public override MultilingualString HumanReadableName
			=> ResString.GetMultilingualString("ace9df47-4396-40ab-80cc-9e91f5ff3616", "Archive Schedule");

		public override bool IsAllowedForUnallocatedeDocs
			=> false;
	}
}
