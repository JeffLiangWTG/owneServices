using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public abstract class AuditDataTransformation : BiDataTransformation
	{
		public override string BiDbName => Db.AuditDatabaseName;
		public abstract void RunAuditTransformation(DbConnection auditConnection, TransformationSection section, CancellationToken token);

		public override void RunBiTransformation(DbConnection auditConnection, TransformationSection section, CancellationToken token)
		{
			RunAuditTransformation(auditConnection, section, token);
		}
	}
}
