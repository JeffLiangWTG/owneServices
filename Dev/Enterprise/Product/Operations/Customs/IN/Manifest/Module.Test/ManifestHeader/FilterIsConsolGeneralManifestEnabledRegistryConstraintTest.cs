using System;
using Enterprise.Customs.IN.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Module.Testing;

[TestedType(typeof(FilterIsConsolGeneralManifestEnabledRegistryConstraint))]
sealed class FilterIsConsolGeneralManifestEnabledRegistryConstraintTest : ConstraintTest<FilterIsConsolGeneralManifestEnabledRegistryConstraint>
{
	protected override string ExpectedName => "IsConsolGeneralManifestEnabled";

	protected override string ExpectedSingularValueName => "value";

	protected override string ExpectedPluralValueName => "values";

	protected override bool ExpectGetValueToReturnGetDefaultStringValue => true;

	public override void TestGetValue()
	{
		using (INCustomsDataRegistry.Instance.INEnableConsolGeneralManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			AssertEquals("Y", Constraint.GetValue());
		}

		using (INCustomsDataRegistry.Instance.INEnableConsolGeneralManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			AssertEquals("N", Constraint.GetValue());
		}
	}
}
