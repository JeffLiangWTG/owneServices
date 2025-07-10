using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	class TestHelpErrorLogValidation : BusinessObjectValidationTestCase
	{
		public void TestCheckTestValidateHE_FixedDateRange()
		{
			AssertEquals("PreCondition: No Assigned Date", ZDateTime.Empty, Log.HE_FixedDateLocal);

			Log.Validation.ValidateHE_FixedDate();
			AssertEquals("Now should be ok", false, Log.HE_FixedDateLocalInfo.HasErrors());

			Log.HE_FixedDateLocal = ZDateTime.Now.AddYears(1).AddDays(-1);
			AssertEquals("less than 1 year should be ok", false, Log.HE_FixedDateLocalInfo.HasErrors());
			AssertEquals("less than 1 year should be ok", false, Log.HE_FixedDateLocalInfo.HasWarnings());

			Log.HE_FixedDateLocal = ZDateTime.Now.AddYears(1).AddDays(1);
			AssertEquals("just over 1 year should have warning not error", false, Log.HE_FixedDateLocalInfo.HasErrors());
			AssertEquals("just over 1 year should have warning not error", true, Log.HE_FixedDateLocalInfo.HasWarnings());

			Log.HE_FixedDateLocal = ZDateTime.Now.AddYears(5).AddDays(-1);
			AssertEquals("just under 5 years should have warning not error", false, Log.HE_FixedDateLocalInfo.HasErrors());
			AssertEquals("just under 5 years should have warning not error", true, Log.HE_FixedDateLocalInfo.HasWarnings());

			Log.HE_FixedDateLocal = ZDateTime.Now.AddYears(5).AddDays(1);
			AssertEquals("just over 5 years should have error", true, Log.HE_FixedDateLocalInfo.HasErrors());
			AssertEquals("just over 5 years should have error", false, Log.HE_FixedDateLocalInfo.HasWarnings());

			Log.HE_FixedDateLocal = ZDateTime.Now.AddYears(-1).AddDays(1);
			AssertEquals("less than 1 year past should be ok", false, Log.HE_FixedDateLocalInfo.HasErrors());
			AssertEquals("less than 1 year past should be ok", false, Log.HE_FixedDateLocalInfo.HasWarnings());

			Log.HE_FixedDateLocal = ZDateTime.Now.AddYears(-1).AddDays(-1);
			AssertEquals("just over 1 year past should have warning not error", false, Log.HE_FixedDateLocalInfo.HasErrors());
			AssertEquals("just over 1 year past should have warning not error", true, Log.HE_FixedDateLocalInfo.HasWarnings());

			Log.HE_FixedDateLocal = ZDateTime.Now.AddYears(-10).AddDays(1);
			AssertEquals("just under 10 years past should have warning not error", false, Log.HE_FixedDateLocalInfo.HasErrors());
			AssertEquals("just under 10 years past should have warning not error", true, Log.HE_FixedDateLocalInfo.HasWarnings());

			Log.HE_FixedDateLocal = ZDateTime.Now.AddYears(-10).AddDays(-1);
			AssertEquals("just over 10 years past should have error", true, Log.HE_FixedDateLocalInfo.HasErrors());
			AssertEquals("just over 10 years past should have error", false, Log.HE_FixedDateLocalInfo.HasWarnings());
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2018, 2, 28, 14, 1, 1)]
		public void TestCheckTestValidateLocalDateRangeWatchLeapYears()
		{
			TestDateAttribute.UseUNLOCO = true;

			Log.Validation.ValidateHE_FixedDate();
			AssertEquals("Default Should be ok", false, Log.HE_FixedDateLocalInfo.HasErrors());

			Log.HE_FixedDateLocal = ZDateTime.Now.AddYears(-10).AddDays(1);
			Log.Validation.ValidateHE_FixedDate();
			AssertEquals("just under 10 years past should have warning not error", false, Log.HE_FixedDateLocalInfo.HasErrors());
			AssertEquals("just under 10 years past should have warning not error", true, Log.HE_FixedDateLocalInfo.HasWarnings());

			Log.HE_FixedDateLocal = ZDateTime.Now.AddYears(-10).AddDays(-1);
			Log.Validation.ValidateHE_FixedDate();
			AssertEquals("just over 10 years past should have error", true, Log.HE_FixedDateLocalInfo.HasErrors());
			AssertEquals("just over 10 years past should have error", false, Log.HE_FixedDateLocalInfo.HasWarnings());
		}

		EdiHelpErrorLog Log
		{
			get { return log ?? (log = Factory.New<EdiHelpErrorLog>()); }
		}
		EdiHelpErrorLog log;
	}
}