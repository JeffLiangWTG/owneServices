using System.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Business
{
	public static class StatusColumnProvider
	{
		public static readonly SchemaStringColumn PlaceInQueue = new SchemaStringColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema, CustomTextFilterNameConstants.PlaceInQueue, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaStringColumn SecondsRunning = new SchemaStringColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema, CustomTextFilterNameConstants.SecondsRunning, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaStringColumn SecondsInQueue = new SchemaStringColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema,CustomTextFilterNameConstants.SecondsInQueue, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaStringColumn BindingTypes = new SchemaStringColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema,CustomTextFilterNameConstants.BindingTypes, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaStringColumn RegisteredOnHosts = new SchemaStringColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema,CustomTextFilterNameConstants.RegisteredOnHosts, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaStringColumn ProcessId = new SchemaStringColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema,CustomTextFilterNameConstants.ProcessId, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaIntColumn BindingsCount = new SchemaIntColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema,CustomTextFilterNameConstants.BindingsCount, 0, 0, false, true);
		public static readonly SchemaStringColumn StatusString = new SchemaStringColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema,CustomTextFilterNameConstants.StatusString, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaDateTimeColumn NextRunTime = new SchemaDateTimeColumn(StmScheduleTaskSchema.S5_NextScheduledPrintRunTimeUtc.TableSchema, CustomTextFilterNameConstants.NextRunTime,  1, SqlDbType.SmallDateTime, null, true);
		public static readonly SchemaDateTimeColumn LastRunTime = new SchemaDateTimeColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema, CustomTextFilterNameConstants.LastRunTime, 1, SqlDbType.SmallDateTime, null, true);
		public static readonly SchemaDateTimeColumn LastErrorTime = new SchemaDateTimeColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema, CustomTextFilterNameConstants.LastErrorTime, 1, SqlDbType.SmallDateTime, null, true);
		public static readonly SchemaIntColumn ErrorCountLast24Hours = new SchemaIntColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema, CustomTextFilterNameConstants.ErrorCountLast24Hours, 0, 0, false, true);
		public static readonly SchemaStringColumn MutuallyExclusiveGroup = new SchemaStringColumn(StmScheduleTaskSchema.S5_ScheduleType.TableSchema, CustomTextFilterNameConstants.MutuallyExclusiveGroup, 0, SqlDbType.Text, null, true, 100);
	}
}
