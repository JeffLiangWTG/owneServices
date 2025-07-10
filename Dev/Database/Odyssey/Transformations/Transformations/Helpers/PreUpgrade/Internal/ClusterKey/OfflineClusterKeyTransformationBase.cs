using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Transformation.DataModification;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DbUpgrader.Transformations
{
	public interface IClusterKeyCreator
	{
		void Do(IUpgradeTaskWorkflowLogger logger, bool isOnline);
	}

	[CodeAlive("Base class for a standard transformation")]
	public abstract class OfflineClusterKeyTransformationBase : DataTransformation
	{
		protected sealed override void OfflinePreUpgradeTransform()
		{
			ClusterKeyCreator.Do(manager, isOnline: false);
		}

		protected abstract IClusterKeyCreator ClusterKeyCreator { get; }
	}
}
