using System;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Business.Testing;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	sealed class FilterIsDeltaIEEnabledForImportsOrExportsConstraintTest : ConstraintTest<FilterIsDeltaIEEnabledForImportsOrExportsConstraint>
	{
		protected override string ExpectedName => "IsDeltaIEEnabledForImportsOrExports";

		protected override string ExpectedSingularValueName => "value";

		protected override string ExpectedPluralValueName => "values";

		protected override bool ExpectGetValueToReturnGetDefaultStringValue => true;

		public override void TestGetValue()
		{
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			{
				AssertEquals("Y", Constraint.GetValue());
			}

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, false))
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			{
				AssertEquals("Y", Constraint.GetValue());
			}

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, false))
			{
				AssertEquals("Y", Constraint.GetValue());
			}

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, false))
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, false))
			{
				AssertEquals("N", Constraint.GetValue());
			}
		}
	}
}
