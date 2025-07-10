using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	public sealed class SetOrgAllowMixedCaseAttribute : TestSetupAttribute
	{
		public SetOrgAllowMixedCaseAttribute(bool state)
		{
			AllowMixedCaseState = state;
		}

		public override void SetUp(TestCase testCase)
		{
			originalMixedCaseState = Env.Registry.OrgAllowMixedCase;

			if (AllowMixedCaseState != Env.Registry.OrgAllowMixedCase)
			{
				Env.Registry.SetOrgAllowMixedCase(AllowMixedCaseState);
			}
		}

		public override void TearDown(TestCase testCase)
		{
			if (originalMixedCaseState != Env.Registry.OrgAllowMixedCase)
			{
				Env.Registry.SetOrgAllowMixedCase(originalMixedCaseState);
			}
		}

		public ZBool AllowMixedCaseState { get; private set; }

		ZBool originalMixedCaseState;
	}
}
