using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	internal class ClientFeatureRequestValueAndContributionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEBV()
		{
			Parent.SetShouldValidateMandatoryEBVDelegate(delegate
			{ return true; });
			Parent.Validation.ValidateT9_EBV();
			AssertMandatoryValidationError(Parent.T9_EBVInfo, true);

			Parent.SetShouldValidateMandatoryEBVDelegate(delegate
			{ return false; });
			Parent.Validation.ValidateT9_EBV();
			AssertMandatoryValidationError(Parent.T9_EBVInfo, false);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Parent = Factory.New<ClientFeatureRequestValueAndContribution>();
		}

		ClientFeatureRequestValueAndContribution Parent;

		#endregion
	}
}