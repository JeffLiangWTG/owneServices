using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(DefaultBrokerAndCredentialRegistryDataType))]
	sealed class DefaultBrokerAndCredentialRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultBrokerAndCredentialRegistryDataType>
	{
		protected override DefaultBrokerAndCredentialRegistryDataType GetNewDataType() => new DefaultBrokerAndCredentialRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();

			var item1 = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item1.DefaultBrokerCode = "AN";
			item1.DefaultCredentialSEA = "Test3003";

			var item2 = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item2.DefaultBrokerCode = "AN";
			item2.DefaultCredentialAIR = "Test3003";

			var item3 = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item3.DefaultBrokerCode = "AN";
			item3.ForwarderManifestSEA = "Test3003";

			var item4 = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item4.DefaultBrokerCode = "AN";
			item4.ForwarderManifestAIR = "Test3003";

			return new[] { new ValidSampleAndBinaryValueInDB(item1, new DefaultBrokerAndCredentialRegistryDataType().Serialise(item1)),
							new ValidSampleAndBinaryValueInDB(item2, new DefaultBrokerAndCredentialRegistryDataType().Serialise(item2)),
							new ValidSampleAndBinaryValueInDB(item3, new DefaultBrokerAndCredentialRegistryDataType().Serialise(item3)),
							new ValidSampleAndBinaryValueInDB(item4, new DefaultBrokerAndCredentialRegistryDataType().Serialise(item4)) };
		}

		protected override string ExpectedEditorName => "DefaultBrokerAndCredentialRegistryItemEditor";

		protected override void SetUp()
		{
			base.SetUp();
			new DefaultBrokerAndCredentialTestHelper().CreateBrokerStaff();
		}
	}
}
