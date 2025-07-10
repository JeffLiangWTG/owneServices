using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.ZArchitecture.Business
{
	public interface IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter : IAddInfoChildUniqueIndexFailureHandlerSupporter
	{
		string UniqueClusterIndexName { get; }
		IClusterKeyMaster ClusterKeyMaster { get; }
		SchemaIntColumn ClusterKeyColumn { get; }
	}
}
