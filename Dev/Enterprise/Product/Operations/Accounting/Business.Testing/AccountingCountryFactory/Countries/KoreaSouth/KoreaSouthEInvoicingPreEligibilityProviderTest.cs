using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingPreEligibilityProvider))]
	class KoreaSouthEInvoicingPreEligibilityProviderTest : EInvoicingPreEligibilityProviderTest
	{
		protected override string CountryCode => CountryCodes.KoreaSouth;

		[TestDate(2023, 3, 11, 15, 12, 12)]
		public override void TestCanEvaluateByComplianceDate()
		{
			TestDateAttribute.UseUNLOCO = false;

			var invoiceInDB = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV_INDB", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			invoiceInDB.AH_PostDate = new ZDateTime(2023, 01, 01, 00, 00, 00);
			Factory.Save();

			invoiceInDB.Branch.GB_RL_NKHomePort = "KRASA";
			AssertEquals("PreCondition, AH_PostDate."
				, new ZDateTime(2023, 1, 1, 0, 0, 0)
				, invoiceInDB.AH_PostDate
			);
			AssertEquals("PreCondition, AH_SystemCreateTimeUtc."
				, new ZDateTime(2023, 3, 11, 15, 12, 12)
				, invoiceInDB.AH_SystemCreateTimeUtc
			);
			AssertEquals("PreCondition, AH_SystemCreateTimeLocal, time zone for KRASA, is UTC+9."
				, new DateTime(2023, 3, 12, 0, 12, 12)
				, invoiceInDB.AH_SystemCreateTimeUtc.ToLocationTime(invoiceInDB.Branch.HomePort).ToDateTime()
			);

			var eInvoicingPreEligibilityProvider = new KoreaSouthEInvoicingPreEligibilityProvider();

			AssertEquals(false, eInvoicingPreEligibilityProvider.CanEvaluateByComplianceDate(invoiceInDB, DateTime.MinValue));

			AssertEquals("CanEvaluateByComplianceDate, when AH_SystemCreateTimeLocal is less than EReportingComplianceDate(2023-03-13)."
				, false
				, eInvoicingPreEligibilityProvider.CanEvaluateByComplianceDate(invoiceInDB, new DateTime(2023, 3, 13, 00, 00, 00))
			);
			AssertEquals("CanEvaluateByComplianceDate, when AH_SystemCreateTimeLocal equal to EReportingComplianceDate(2023-03-12)."
				, true
				, eInvoicingPreEligibilityProvider.CanEvaluateByComplianceDate(invoiceInDB, new DateTime(2023, 3, 12, 00, 00, 00))
			);
			AssertEquals("CanEvaluateByComplianceDate, when AH_SystemCreateTimeLocal is greater than(or equal to) EReportingComplianceDate(2023-03-11)."
				, true
				, eInvoicingPreEligibilityProvider.CanEvaluateByComplianceDate(invoiceInDB, new DateTime(2023, 3, 11, 00, 00, 00))
			);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
