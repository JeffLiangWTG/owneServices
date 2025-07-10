using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	sealed class FilterDepartmentConstraintTest : ConstraintTest<FilterDepartmentConstraint>
	{
		public override void TestGetValue()
		{
			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "DDD";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, department.PK.ToGuid()))
			{
				AssertEquals("DDD", Constraint.GetValue());
			}
		}

		#region Implementation

		protected override string ExpectedName => FilterConstants.Department;

		protected override string ExpectedSingularValueName => "department";

		protected override string ExpectedPluralValueName => "departments";

		protected override bool ExpectGetValueToReturnGetDefaultStringValue => true;

		#endregion
	}
}
