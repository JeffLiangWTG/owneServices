using System.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Business
{
	public static class StmServiceTaskMetadataColumnProvider
	{
		public static readonly SchemaStringColumn Description = new SchemaStringColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema, StmServiceTaskCustomFilterNameConstants.Description, 0, SqlDbType.Text, null, true, 80);
		public static readonly SchemaStringColumn Category = new SchemaStringColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema, StmServiceTaskCustomFilterNameConstants.Category, 0, SqlDbType.Text, null, true, 3);
		public static readonly SchemaStringColumn MutuallyExclusiveGroup = new SchemaStringColumn(StmServiceTaskSchema.SST_ServiceTaskCode.TableSchema, StmServiceTaskCustomFilterNameConstants.MutuallyExclusiveGroup, 0, SqlDbType.Text, null, true, 100);
	}
}
