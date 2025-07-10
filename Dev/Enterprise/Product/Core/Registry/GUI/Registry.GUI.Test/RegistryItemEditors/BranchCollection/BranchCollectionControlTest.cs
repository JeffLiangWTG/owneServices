using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BranchCollectionControl))]
	sealed class BranchCollectionControlTest : ModuleButtonGridControlTest<BranchProxy>
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new BranchProxyMaster();
		}

		#endregion
	}
}
