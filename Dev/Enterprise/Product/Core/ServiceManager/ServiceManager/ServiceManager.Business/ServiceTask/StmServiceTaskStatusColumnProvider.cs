using System.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Business
{
	public static class StmServiceTaskStatusColumnProvider
	{
		public static readonly SchemaStringColumn PlaceInQueue = new SchemaStringColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema, StmServiceTaskCustomFilterNameConstants.PlaceInQueue, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaStringColumn SecondsRunning = new SchemaStringColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema, StmServiceTaskCustomFilterNameConstants.SecondsRunning, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaStringColumn SecondsInQueue = new SchemaStringColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema,StmServiceTaskCustomFilterNameConstants.SecondsInQueue, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaStringColumn BindingTypes = new SchemaStringColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema,StmServiceTaskCustomFilterNameConstants.BindingTypes, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaStringColumn RegisteredOnHosts = new SchemaStringColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema,StmServiceTaskCustomFilterNameConstants.RegisteredOnHosts, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaStringColumn ProcessId = new SchemaStringColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema,StmServiceTaskCustomFilterNameConstants.ProcessId, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaIntColumn BindingsCount = new SchemaIntColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema,StmServiceTaskCustomFilterNameConstants.BindingsCount, 0, 0, false, true);
		public static readonly SchemaIntColumn RunningCount = new SchemaIntColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema,StmServiceTaskCustomFilterNameConstants.RunningCount, 0, 0, false, true);
		public static readonly SchemaStringColumn StatusString = new SchemaStringColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema,StmServiceTaskCustomFilterNameConstants.StatusString, 0, SqlDbType.Text, null, true, 100);
		public static readonly SchemaDateTimeColumn NextRunTime = new SchemaDateTimeColumn(StmServiceTaskSchema.SST_NextRunTime.TableSchema, StmServiceTaskCustomFilterNameConstants.NextRunTime,  1, SqlDbType.SmallDateTime, null, true);
		public static readonly SchemaDateTimeColumn LastRunTime = new SchemaDateTimeColumn(StmServiceTaskSchema.SST_LastRunTime.TableSchema, StmServiceTaskCustomFilterNameConstants.LastRunTime, 1, SqlDbType.SmallDateTime, null, true);
		public static readonly SchemaDateTimeColumn LastErrorTime = new SchemaDateTimeColumn(StmServiceTaskSchema.SST_NextRunTime.TableSchema, StmServiceTaskCustomFilterNameConstants.LastErrorTime, 1, SqlDbType.SmallDateTime, null, true);
		public static readonly SchemaIntColumn ErrorCountLast24Hours = new SchemaIntColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema, StmServiceTaskCustomFilterNameConstants.ErrorCountLast24Hours, 0, 0, false, true);
	}
}
