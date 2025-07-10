using System.IO;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport
{
	public abstract class JCDDependentDBObject
	{
		protected JCDDependentDBObject(JCDDBObjectInfo dbObject)
		{
			Argument.NotNull(dbObject, "JCDDBObjectInfo");
			DbObjectInfo = dbObject;
		}

		public JCDDBObjectInfo DbObjectInfo { get; }

		public virtual string Name => DbObjectInfo.Name;

		public virtual bool ShouldCreateOnSynchronize => !DbObjectInfo.IsObsolete;

		public abstract string CreateSQLText { get; }

		public virtual bool ShouldDropOnSynchronize => true;

		public abstract string CheckAndDropSQLText { get; }

		protected virtual string GetEmbeddedResourceText(string resourceName)
		{
			var sql = string.Empty;
			var assembly = Assembly.GetExecutingAssembly();
			using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
			{
				sql = reader.ReadToEnd();
			}
			return sql;
		}
	}

	public interface IDeleteDataWhenRemovePartition
	{
		void SetCommandForRemovePartition(DbCommand command, PartitionInfo pi);
	}
}
