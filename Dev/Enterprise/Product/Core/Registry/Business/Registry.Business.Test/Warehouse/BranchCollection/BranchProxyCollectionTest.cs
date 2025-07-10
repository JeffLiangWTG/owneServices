using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BranchProxyCollection))]
	sealed class BranchProxyCollectionTest : RegistryProxyBusinessObjectCollectionTest<BranchProxyCollection, BranchProxy>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BranchProxyCollection GetCollectionToTest()
		{
			return new BranchProxyCollection();
		}

		#endregion
	}
}
