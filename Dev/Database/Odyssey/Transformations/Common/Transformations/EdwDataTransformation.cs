using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public abstract class EdwDataTransformation : BiDataTransformation
	{
		public override string BiDbName => Db.EdwDatabaseName;

		public abstract void RunEdwTransformation(DbConnection edwConnection, TransformationSection section, CancellationToken token);

		public override void RunBiTransformation(DbConnection biConnection, TransformationSection section, CancellationToken token)
		{
			RunEdwTransformation(biConnection, section, token);
		}
	}
}
