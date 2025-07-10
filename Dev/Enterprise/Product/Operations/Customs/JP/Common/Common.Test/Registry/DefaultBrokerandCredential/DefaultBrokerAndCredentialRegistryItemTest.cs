using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(DefaultBrokerAndCredentialRegistryItem))]
	sealed class DefaultBrokerAndCredentialRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<DefaultBrokerAndCredential>
	{
		protected override StronglyTypedRegistryItem<DefaultBrokerAndCredential, DefaultBrokerAndCredential> GetNewRegistryItem() => new DefaultBrokerAndCredentialRegistryItem("", null, null, null, RegistryStorageFlags.BranchDepartment);

		protected override DefaultBrokerAndCredential ValidValue
		{
			get
			{
				var result = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
				result.DefaultBrokerCode = "AN";
				result.DefaultCredentialSEA = "Test3003";

				return result;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			new DefaultBrokerAndCredentialTestHelper().CreateBrokerStaff();
		}
	}
}
