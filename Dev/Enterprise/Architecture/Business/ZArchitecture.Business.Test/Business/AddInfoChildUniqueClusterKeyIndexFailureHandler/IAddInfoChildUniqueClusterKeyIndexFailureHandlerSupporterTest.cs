using CargoWise.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestsSubclassesOf(typeof(IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter))]
	public abstract class IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporterTestCase<T> : IAddInfoChildUniqueIndexFailureHandlerSupporterTestCase<T>
	where T : EnterpriseBusinessObject, IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter
	{
		public void TestIAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporterMembers()
		{
			var bizObj = (T)GetNewBusinessObject();
			IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter supporter = bizObj;
			AssertEquals("UniqueClusterIndexName", ExpectedUniqueClusterIndexName, supporter.UniqueClusterIndexName);
			AssertEquals("ClusterKeyMaster", GetParent(bizObj), supporter.ClusterKeyMaster);
			AssertEquals("ClusterKeyColumn", ExpectedClusterKeyColumn, supporter.ClusterKeyColumn);
		}

		protected abstract SchemaIntColumn ExpectedClusterKeyColumn { get; }
		protected abstract string ExpectedUniqueClusterIndexName { get; }
	}
}
