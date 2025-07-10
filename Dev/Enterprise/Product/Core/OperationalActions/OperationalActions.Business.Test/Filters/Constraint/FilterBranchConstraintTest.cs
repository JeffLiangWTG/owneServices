using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	sealed class FilterBranchConstraintTest : ConstraintTest<FilterBranchConstraint>
	{
		public override void TestGetValue()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CCC";
			company.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "BBB";
			branch.GB_RL_NKHomePort = "AUCNS";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("BBB", Constraint.GetValue());
			}
		}

		#region Implementation

		protected override string ExpectedName => FilterConstants.Branch;

		protected override string ExpectedSingularValueName => "branch";

		protected override string ExpectedPluralValueName => "branches";

		protected override bool ExpectGetValueToReturnGetDefaultStringValue => true;

		#endregion
	}
}
