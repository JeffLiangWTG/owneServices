using System;
using CargoWise.EntityFramework;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(StmReportRunData),
	Enterprise.Core.Constants.DocManagerCodes.ReportStatistic)]

namespace Enterprise.Scheduler.Business
{
	internal class StmReportRunData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(StmReportRun);
		protected override Type CollectionType => typeof(StmReportRunCollection);
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new StmReportRunCollection(factory);
		}
		public override ModuleIdentifier ModuleID => ModuleIDs.ReportStatistics;
		public override string ReferenceType => Core.Constants.ReferenceTypes.All;
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("1c91a3a1-6a63-42b9-b33f-dc775f710053", "Report Statistic");
		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
