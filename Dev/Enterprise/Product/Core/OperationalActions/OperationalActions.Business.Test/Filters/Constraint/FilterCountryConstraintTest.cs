using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	sealed class FilterCountryConstraintTest : ConstraintTest<FilterCountryConstraint>
	{
		public override void TestGetValue()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Constraint.GetValue());
		}

		#region Implementation

		protected override string ExpectedName => FilterConstants.Country;

		protected override string ExpectedSingularValueName => "country/region";

		protected override string ExpectedPluralValueName => "countries/regions";

		protected override bool ExpectGetValueToReturnGetDefaultStringValue => true;

		#endregion
	}
}
