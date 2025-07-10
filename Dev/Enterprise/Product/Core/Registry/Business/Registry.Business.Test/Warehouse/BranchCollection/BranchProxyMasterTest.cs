using System;
using CargoWise.Application;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BranchProxyMaster))]
	sealed class BranchProxyMasterTest : RegistryProxyBusinessObjectMasterTest<BranchProxy>
	{
		#region TestFindBoxCollection

		protected override Type ExpectedFindBoxCollectionType
		{
			get { return ObjectFactory.GetType<IGlbBranchCollection>(); }
		}

		#endregion

		#region Implementation

		protected override RegistryProxyBusinessObjectMaster<BranchProxy> GetBusinessObjectToClone()
		{
			return new BranchProxyMaster();
		}

		protected override RegistryProxyBusinessObjectMaster<BranchProxy> GetBusinessObjectToSerialise()
		{
			return new BranchProxyMaster();
		}

		#endregion
	}
}
