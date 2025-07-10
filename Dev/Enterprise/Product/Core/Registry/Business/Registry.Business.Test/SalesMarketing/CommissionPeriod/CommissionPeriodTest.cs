using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CommissionPeriod))]
	sealed class CommissionPeriodTest : RegistryBusinessObjectTest
	{
		#region Properties

		protected override int ExpectedDefaultMaxCodeLength
		{
			get { return OrgCommissionAgreementRecipientRateSchema.CAT_CommissionPeriod.MaxLength; }
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new CommissionPeriod();

			result.Code = "0-24";
			result.EnglishDescription = "First 2 years only";
			result.Start = 0;
			result.End = 24;
			result.IsEnabled = true;

			return result;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("0-24", clone.Code);
			AssertEquals("First 2 years only", clone.Description);
			AssertEquals(0, ((CommissionPeriod)clone).Start);
			AssertEquals(24, ((CommissionPeriod)clone).End);
			AssertEquals(true, ((CommissionPeriod)clone).IsEnabled);
		}

		#endregion

		#region Validation

		public void TestValidateStart()
		{
			var period = new CommissionPeriod();
			period.End = 24;

			period.Start = -1;
			AssertHasError(period.StartInfo, "Must be between 0 and 240 inclusive.");

			period.Start = 241;
			AssertHasError(period.StartInfo, "Must be between 0 and 240 inclusive.");

			period.Start = 0;
			AssertNoErrors(period.StartInfo);

			period.Start = 12;
			AssertNoErrors(period.StartInfo);

			period.Start = 36;
			AssertHasError(period.StartInfo, "Must be smaller than End.");

			period.End = 0;
			period.ValidateStart();
			AssertNoErrors(period.StartInfo);
		}

		public void TestValidateEnd()
		{
			var period = new CommissionPeriod();
			period.Start = 24;

			period.End = -1;
			AssertHasError(period.EndInfo, "Must be between 0 and 240 inclusive.");

			period.End = 241;
			AssertHasError(period.EndInfo, "Must be between 0 and 240 inclusive.");

			period.End = 0;
			AssertNoErrors(period.EndInfo);

			period.End = 12;
			AssertHasError(period.EndInfo, "Must be larger than Start.");

			period.End = 36;
			AssertNoErrors(period.EndInfo);
		}

		public void TestRunPreSaveValidation()
		{
			var period = new CommissionPeriod();
			period.Start = -1;
			period.End = -1;
			period.RunPreSaveValidation();

			AssertHasError(period.StartInfo, "Must be between 0 and 240 inclusive.");
			AssertHasError(period.EndInfo, "Must be between 0 and 240 inclusive.");
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CommissionPeriod();
		}

		#endregion
	}
}
