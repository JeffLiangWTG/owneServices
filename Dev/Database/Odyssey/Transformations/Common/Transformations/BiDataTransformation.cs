using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public abstract class BiDataTransformation : DataTransformation
	{
		public abstract string BiDbName { get; }

		public abstract void RunBiTransformation(DbConnection biConnection, TransformationSection section, CancellationToken token);

		public void RunTransformWithSlowTransformReporter(TransformationSection transformationSection, DbConnection biConnection, CancellationToken token)
		{
			using (new SlowTransformReporter(UserDescription, transformationSection))
			using (((ICurrentDbControl)biConnection).UseDatabase(BiDbName))
			{
				this.RunBiTransformation(biConnection, transformationSection, token);
			}
		}

#if DEBUG
		// This is to allow tests to pass to expose the rest of the work required
		public void RunBiTransformation(DbConnection biConnection, CancellationToken token)
		{
			RunBiTransformation(biConnection, TransformationSection.OnlinePreUpgrade, token);
			RunBiTransformation(biConnection, TransformationSection.OfflinePreUpgrade, token);
			RunBiTransformation(biConnection, TransformationSection.OfflinePostUpgrade, token);
			RunBiTransformation(biConnection, TransformationSection.OnlinePostUpgrade, token);
		}
#endif
	}
}
