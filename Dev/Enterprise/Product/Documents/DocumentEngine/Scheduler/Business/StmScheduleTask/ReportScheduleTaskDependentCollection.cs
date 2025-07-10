
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ReportScheduleTaskDependentCollection : DependentBusinessObjectCollection<ReportScheduleTask, ReportCommand>
	{
		public ReportScheduleTaskDependentCollection(ReportCommand command)
			: base(command)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return StmScheduleTaskSchema.S5_ParentID; }
		}
	}
}
