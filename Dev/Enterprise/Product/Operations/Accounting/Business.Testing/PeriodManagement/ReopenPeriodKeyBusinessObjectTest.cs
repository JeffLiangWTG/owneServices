using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PeriodManagement.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ReopenPeriodKeyBusinessObject))]
	internal sealed class ReopenPeriodKeyBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			AccPeriodManagement period = Factory.NewWithValidTestData<AccPeriodManagement>();
			return new ReopenPeriodKeyBusinessObject(ZString.Empty, period);
		}

		public void TestReopenPeriod()
		{
			AccPeriodManagement period = Factory.NewWithValidTestData<AccPeriodManagement>();
			period.AM_IsSubLedgerClosed = true;
			period.AM_IsGeneralLedgerClosed = true;
			period.AM_IsSubledgerClosedForAdjustments = true;
			Factory.Save();
			AssertEquals("Period Subledger is closed", period.AM_IsSubLedgerClosed, true);
			AssertEquals("Period General Ledger is closed", period.AM_IsGeneralLedgerClosed, true);
			AssertEquals("Period Subledger For Adjustments is closed", period.AM_IsSubledgerClosedForAdjustments, true);
			ReopenPeriodKeyGenerator generator = new ReopenPeriodKeyGenerator();
			string key = generator.GenerateKey(ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode, GlbCompany.CurrentCompany.GC_Code, GlbStaff.CurrentUser.GS_Code, period.AM_Period);
			ReopenPeriodKeyBusinessObject bo = new ReopenPeriodKeyBusinessObject(ZString.Empty, period);
			ZString message = bo.ReopenPeriod(key, true, false, false);
			AssertEquals("Message result", string.Format("Period {0} is Reopened for: Sub ledger.", period.AM_Period.ToString()), message);
			AssertEquals("Period Subledger is closed", period.AM_IsSubLedgerClosed, false);
			AssertEquals("Period General Ledger is closed", period.AM_IsGeneralLedgerClosed, true);
			AssertEquals("Period Subledger For Adjustments is closed", period.AM_IsSubledgerClosedForAdjustments, true);
			message = bo.ReopenPeriod(key, false, true, false);
			AssertEquals("Message result", string.Format("Period {0} is Reopened for: General ledger.", period.AM_Period.ToString()), message);
			AssertEquals("Period Subledger is closed", period.AM_IsSubLedgerClosed, false);
			AssertEquals("Period General Ledger is closed", period.AM_IsGeneralLedgerClosed, false);
			AssertEquals("Period Subledger For Adjustments is closed", period.AM_IsSubledgerClosedForAdjustments, true);
			message = bo.ReopenPeriod(key, false, false, true);
			AssertEquals("Message result", string.Format("Period {0} is Reopened for: Adjustments.", period.AM_Period.ToString()), message);
			AssertEquals("Period Subledger is closed", period.AM_IsSubLedgerClosed, false);
			AssertEquals("Period General Ledger is closed", period.AM_IsGeneralLedgerClosed, false);
			AssertEquals("Period Subledger For Adjustments is closed", period.AM_IsSubledgerClosedForAdjustments, false);
			message = bo.ReopenPeriod(key, true, true, true);
			AssertEquals("Message result", "Nothing to reopen!", message);
			message = bo.ReopenPeriod(key, false, false, false);
			AssertEquals("Message result", "Nothing to reopen!", message);
			message = bo.ReopenPeriod("abc", true, true, true);
			AssertEquals("Message result", "You have entered an invalid key.\r\n\r\nNote that a key:\r\n - Can only be used within 24 hours of it being issued\r\n - Must be used to reopen the period for which it was issued\r\n - Must be used by the user it was issued to\r\n\r\nPlease contact CargoWise support for assistance.", message);
		}
	}
}
