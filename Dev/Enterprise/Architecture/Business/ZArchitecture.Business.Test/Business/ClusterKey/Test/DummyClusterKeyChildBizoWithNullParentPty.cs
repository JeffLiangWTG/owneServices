using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;

namespace Enterprise.ZArchitecture.Business.Test.Business.ClusterKey.Test
{
	sealed class DummyClusterKeyChildBizoWithNullParentPty : DummyClusterKeyChildBizo
	{
		public DummyClusterKeyChildBizoWithNullParentPty(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZPropertyInfo ZD1_Z0Info => null;
	}
}
