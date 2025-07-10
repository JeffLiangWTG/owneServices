using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationImporterDmExtensionsWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationImporterDmExtensions>
	{
		public void TestNewOrNull()
		{
			AssertNotNull(Provider);
		}

		public void TestAddress()
		{
			AssertEquals(ZString.Empty, Provider.Address);
		}

		public void TestName()
		{
			AssertEquals(ZString.Empty, Provider.Name);
		}

		public void TestRoleCode()
		{
			AssertEquals("4", Provider.RoleCode.Value);
		}

		public void TestEntitlementTypeCode()
		{
			AssertNull(Provider.EntitlementTypeCode);
		}

		public void TestIssueLocation()
		{
			AssertNull(Provider.IssueLocation);
		}

		protected override IDeclarationImporterDmExtensions GetProvider()
		{
			return DeclarationImporterDmExtensionsWrapper.NewOrNull();
		}
	}
}
