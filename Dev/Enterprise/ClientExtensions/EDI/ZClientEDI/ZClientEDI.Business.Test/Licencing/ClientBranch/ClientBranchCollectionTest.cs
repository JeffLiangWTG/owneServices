using System;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(ClientBranchCollection))]
	class ClientBranchCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientBranchCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)Collection).AllowNew);
		}

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ClientBranchCollection);
		}

		protected override ClientBranchCollection GetCollectionToTest()
		{
			return new ClientBranchCollection(Factory);
		}

		#endregion
	}
}
