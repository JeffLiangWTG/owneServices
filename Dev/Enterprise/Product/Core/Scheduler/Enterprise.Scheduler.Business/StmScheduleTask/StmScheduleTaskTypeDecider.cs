using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskTypeDecider : TypeDecider
	{
		public override Type GetTypeForNew()
		{
			return typeof(StmScheduleTask);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(StmScheduleTask);
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var tableCode = (string)row[StmScheduleTaskSchema.S5_ParentTableCode.Name];
#if DEBUG
			if (tableCode == "Z0")
			{
				return typeof(DummyStmScheduleTask);
			}
#endif
			var types = ObjectFactory.Get<Hashtable>("ScheduleTaskTypes");
			return (Type)types[tableCode] ?? typeof(StmScheduleTask);
		}
	}
}
