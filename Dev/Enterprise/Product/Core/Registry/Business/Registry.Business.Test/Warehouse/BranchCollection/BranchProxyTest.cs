using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BranchProxy))]
	sealed class BranchProxyTest : RegistryProxyBusinessObjectTest<BranchProxy>
	{
		#region TestBranchCode

		public void TestBranchCode()
		{
			var branchProxy = new BranchProxy();
			AssertEquals("Invalid Branch", branchProxy.BranchCode);

			branchProxy.ProxyPK = EnvProxy.Instance.CurrentBranch.PK;
			AssertEquals("BNE", branchProxy.BranchCode);
		}

		#endregion

		#region TestName

		public void TestName()
		{
			var branchProxy = new BranchProxy();
			AssertEquals("Invalid Branch", branchProxy.Name);

			branchProxy.ProxyPK = EnvProxy.Instance.CurrentBranch.PK;
			AssertEquals("BN - AUBNE", branchProxy.Name);
		}

		#endregion

		#region TestDelete

		protected override RegistryProxyBusinessObjectCollection<BranchProxy> GetNewCollection()
		{
			return new BranchProxyCollection();
		}

		#endregion
	}
}
