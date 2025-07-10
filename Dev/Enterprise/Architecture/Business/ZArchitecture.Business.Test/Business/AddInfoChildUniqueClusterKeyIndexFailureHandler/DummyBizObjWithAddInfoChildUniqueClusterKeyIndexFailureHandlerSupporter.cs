using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyBizObjWithAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter : CargoWise.EntityFramework.Testing.DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter, IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter
	{
		public DummyBizObjWithAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DummyClusterKeyChildBizoWithAddInfoChildSupporter ClusterKeyMaster => Factory.Load<DummyClusterKeyChildBizoWithAddInfoChildSupporter>(Z0_Guid);

		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => ClusterKeyMaster;

		public string UniqueClusterIndexName => "DUMMY_CLUSTER_INDEX";
		IClusterKeyMaster IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.ClusterKeyMaster => ClusterKeyMaster;
		SchemaIntColumn IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.ClusterKeyColumn => DummyBizoSchema.Z0_Number;
	}
}
