using System;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(ClientCompanyCollection))]
	class ClientCompanyCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientCompanyCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)Collection).AllowNew);
		}

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ClientCompanyCollection);
		}

		protected override ClientCompanyCollection GetCollectionToTest()
		{
			return new ClientCompanyCollection(Factory);
		}

		#endregion
	}
}
