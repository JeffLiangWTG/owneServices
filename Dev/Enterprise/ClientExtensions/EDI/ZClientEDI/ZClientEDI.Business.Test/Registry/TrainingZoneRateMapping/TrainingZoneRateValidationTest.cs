using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	public class TrainingZoneRateValidationTest : BusinessObjectValidationTestCase
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			zoneForTest = Factory.NewWithValidTestData<RefZoneHeader>();
			zoneForTest.FZ_ZoneType = EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training;
			Factory.Save();
		}

		RefZoneHeader zoneForTest;

		#endregion

		#region Zone

		public void TestValidateZonePK_MandatoryAndValidPK()
		{
			TrainingZoneRate rate = new TrainingZoneRate();

			rate.ZonePK = new ZGuid();
			AssertEquals(1, rate.ZonePKInfo.GetErrors().Count());
			AssertHasError(rate.ZonePKInfo, "Please enter a Zone.");

			rate.ZonePK = ZGuid.Invalid;
			AssertEquals(1, rate.ZonePKInfo.GetErrors().Count());
			AssertHasError(rate.ZonePKInfo, "Enter a valid Zone.");

			rate.ZonePK = ZGuid.NewZGuid();
			AssertEquals(1, rate.ZonePKInfo.GetErrors().Count());
			AssertHasError(rate.ZonePKInfo, "Enter a valid Zone.");

			rate.ZonePK = zoneForTest.PK;
			AssertEquals(0, rate.ZonePKInfo.GetErrors().Count());
		}

		#endregion

		#region Rate Amount

		public void TestValidateRateAmount()
		{
			TrainingZoneRate rate = new TrainingZoneRate();

			rate.RateAmount = (ZDecimal)(-100m);
			AssertEquals(1, rate.RateAmountInfo.GetErrors().Count());
			AssertHasError(rate.RateAmountInfo, "Please enter a 'Rate Amount' greater than 0.");

			rate.RateAmount = 0;
			AssertEquals(1, rate.RateAmountInfo.GetErrors().Count());
			AssertHasError(rate.RateAmountInfo, "Please enter a 'Rate Amount' greater than 0.");

			rate.RateAmount = (ZDecimal)100m;
			AssertEquals(0, rate.RateAmountInfo.GetErrors().Count());
		}

		#endregion

		#region Currency

		public void TestValidateCurrencyCode()
		{
			TrainingZoneRate rate = new TrainingZoneRate();

			rate.CurrencyCode = "(*#";
			AssertEquals(1, rate.CurrencyCodeInfo.GetErrors().Count());
			AssertHasError(rate.CurrencyCodeInfo, "Enter a valid Currency.");

			rate.CurrencyCode = "";
			AssertEquals(1, rate.CurrencyCodeInfo.GetErrors().Count());
			AssertHasError(rate.CurrencyCodeInfo, "Please enter a Currency.");

			rate.CurrencyCode = "AUD";
			AssertEquals(0, rate.CurrencyCodeInfo.GetErrors().Count());
		}

		#endregion

		public void TestValidateAll()
		{
			TrainingZoneRate rate = new TrainingZoneRate();
			rate.Validation.ValidateAll();
			AssertHasErrors(rate.ZonePKInfo);
			AssertHasErrors(rate.CurrencyCodeInfo);
			AssertHasErrors(rate.RateAmountInfo);
		}
	}
}
