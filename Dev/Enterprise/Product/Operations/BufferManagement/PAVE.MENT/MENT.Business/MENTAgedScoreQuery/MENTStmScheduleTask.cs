using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTStmScheduleTask : StmScheduleTask
	{
		public MENTStmScheduleTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override StmScheduleTaskValidation GetNewValidation()
		{
			return new MENTStmScheduleTaskValidation(this);
		}

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			S5_ParentTableCode = MENTAgedScoreQuerySchema.Constants.Prefix;
			S5_ParentID = Guid.NewGuid();

			HasChanges = false;
		}
#endif
		#endregion
	}
}
