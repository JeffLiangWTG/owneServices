using System;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(DefaultBrokerAndCredential))]
	sealed class DefaultBrokerAndCredentialTest : RegistryBusinessObjectTemplateTestCase<DefaultBrokerAndCredential>
	{
		public void TestDefaultValue()
		{
			Assert(defaultBrokerAndCredential.DefaultCredentialSEA.Equals(string.Empty));
			Assert(defaultBrokerAndCredential.DefaultCredentialAIR.Equals(string.Empty));
			Assert(defaultBrokerAndCredential.ForwarderManifestSEA.Equals(string.Empty));
			Assert(defaultBrokerAndCredential.ForwarderManifestAIR.Equals(string.Empty));
		}

		public void TestLookupsType()
		{
			AssertType<DefaultBrokerAndCredentialLookups>(defaultBrokerAndCredential.Lookups);
		}

		public void TestValidationType()
		{
			AssertType<DefaultBrokerAndCredentialValidation>(defaultBrokerAndCredential.Validation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			new DefaultBrokerAndCredentialTestHelper().CreateBrokerStaff();
			defaultBrokerAndCredential = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
		}

		DefaultBrokerAndCredential defaultBrokerAndCredential;

		protected override DefaultBrokerAndCredential GetBusinessObjectToClone() => (DefaultBrokerAndCredential)GetNewBusinessObject();

		protected override DefaultBrokerAndCredential GetBusinessObjectToSerialise() => (DefaultBrokerAndCredential)GetNewBusinessObject();

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;
	}
}
