using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(MalaysiaEInvoicingPreEligibilityProvider))]
	public class MalaysiaEInvoicingPreEligibilityProviderTest : EInvoicingPreEligibilityProviderTest
	{
		protected override string CountryCode => CountryCodes.Malaysia;

		protected override bool ExpectedCanEvaluateByTransaction_IsInDatabase => true;

		[TestDate(2023, 3, 11, 15, 12, 12)]
		public override void TestCanEvaluateByComplianceDate()
		{
			TestDateAttribute.UseUNLOCO = false;
			AssertCanEvaluateByComplianceDate_ShouldUseCreatedDate(typeof(ARInvoice));
			AssertCanEvaluateByComplianceDate_ShouldUseCreatedDate(typeof(APInvoice));
		}

		public void AssertCanEvaluateByComplianceDate_ShouldUseCreatedDate(Type invoiceType)
		{
			var invoiceInDB = TestObjectCreator.CreateInvoiceWithLine(invoiceType, "INV_INDB", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			invoiceInDB.AH_PostDate = new ZDateTime(2023, 2, 22, 15, 12, 12);
			Factory.Save();

			invoiceInDB.Branch.GB_RL_NKHomePort = "MYKLA";
			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("AH_PostDate.", new ZDateTime(2023, 02, 22, 15, 12, 12), invoiceInDB.AH_PostDate);
				AssertEquals("AH_SystemCreateTimeUtc.", new ZDateTime(2023, 3, 11, 15, 12, 12), invoiceInDB.AH_SystemCreateTimeUtc);
				AssertEquals("AH_SystemCreateTimeLocal, time zone for MYKLA, is UTC+8.", new DateTime(2023, 3, 11, 23, 12, 12), invoiceInDB.AH_SystemCreateTimeUtc.ToLocationTime(invoiceInDB.Branch.HomePort).ToDateTime());
			});

			var eInvoicingPreEligibilityProvider = new MalaysiaEInvoicingPreEligibilityProvider();

			CombineAssertions("Results: ", () =>
			{
				AssertEquals(false, eInvoicingPreEligibilityProvider.CanEvaluateByComplianceDate(invoiceInDB, DateTime.MinValue));
				AssertEquals("CanNotEvaluateByComplianceDate, when AH_SystemCreateTimeLocal is before EReportingComplianceDate(2023-03-21).", false, eInvoicingPreEligibilityProvider.CanEvaluateByComplianceDate(invoiceInDB, new DateTime(2023, 3, 21, 00, 00, 00)));
				AssertEquals("CanEvaluateByComplianceDate, when AH_SystemCreateTimeLocal is equal or after EReportingComplianceDate(2023-03-21).", true, eInvoicingPreEligibilityProvider.CanEvaluateByComplianceDate(invoiceInDB, new DateTime(2023, 3, 1, 00, 00, 00)));
			});
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
