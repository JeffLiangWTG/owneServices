using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	public sealed class ClusterKeyChildInfoTest : TestCaseWithFactory
	{
		public void TestLoadChildObjects()
		{
			var clusterKeyChildInfo = new ClusterKeyChildInfo(typeof(DummyClusterKeyChildBizo), DummyDependentBizoSchema.ZD1_Z0);

			var dummyParent = Factory.New<DummyClusterKeyParentBizo>();
			AssertEquals("LoadChildObjects count with no children", 0, clusterKeyChildInfo.LoadChildObjects(dummyParent).Count());

			var dummyChild1 = dummyParent.AddNewDependentBizo();

			CombineAssertions("LoadChildObjects (after adding one child)", () =>
			{
				var childObjects = clusterKeyChildInfo.LoadChildObjects(dummyParent);
				AssertEquals("Count", 1, childObjects.Count());
				AssertEquals("Contains dummyChild1?", true, childObjects.Contains(dummyChild1));
			});

			var dummyChild2 = dummyParent.AddNewDependentBizo();

			CombineAssertions("LoadChildObjects (after adding another child)", () =>
			{
				var childObjects = clusterKeyChildInfo.LoadChildObjects(dummyParent);
				AssertEquals("Count", 2, childObjects.Count());
				AssertEquals("Contains dummyChild2?", true, childObjects.Contains(dummyChild1));
				AssertEquals("Contains dummyChild2?", true, childObjects.Contains(dummyChild2));
			});
		}

		public static IEnumerable<IClusterKeyWorker> LoadChildClusterKeyEntities(IEnumerable<ClusterKeyChildInfo> clusterKeyChildList, EnterpriseBusinessObject parent)
		{
			var result = new List<IClusterKeyWorker>();
			clusterKeyChildList?.ForEach((c) => result.AddRange(c.LoadChildObjects(parent)));
			return result;
		}
	}
}
