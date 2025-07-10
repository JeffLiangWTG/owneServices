using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoicingLineExtensionsTest : TestCaseWithFactory
	{
		public void TestIsARIsAP()
		{
			void assertAP(InvoicingLineBase line)
			{
				Assert(line.IsAP());
				Assert(!line.IsAR());
			}

			void assertAR(InvoicingLineBase line)
			{
				Assert(line.IsAR());
				Assert(!line.IsAP());
			}

			assertAP(Factory.NewWithValidTestData<UACreditNoteLine>());
			assertAP(Factory.NewWithValidTestData<UAInvoiceLine>());
			assertAP(Factory.NewWithValidTestData<APAdjustmentNoteLine>());
			assertAP(Factory.NewWithValidTestData<APCreditNoteLine>());
			assertAP(Factory.NewWithValidTestData<APInvoiceLine>());

			assertAR(Factory.NewWithValidTestData<ARAdjustmentNoteLine>());
			assertAR(Factory.NewWithValidTestData<ARCreditNoteLine>());
			assertAR(Factory.NewWithValidTestData<ARInvoiceLine>());
		}

		public void TestLogLineDescriptionChangedOnInvoice()
		{
			var transacitonLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			AssertEquals("Precondition", Guid.Empty, transacitonLine.AL_AH);
			transacitonLine.LogLineDescriptionChangedOnInvoice(string.Empty, string.Empty);
			AssertNull("No log added as it does not have header!", GetEditLogs(transacitonLine));

			var transactionHeader = Factory.NewWithValidTestData<ARInvoice>();
			transacitonLine.AL_AH = transactionHeader.PK;
			transacitonLine.LogLineDescriptionChangedOnInvoice("S001001", "FRT");
			AssertNotNull("Log added", GetEditLogs(transacitonLine));
			AssertEquals("Log message correct", "Line Description edited for line with: Job: S001001, Charge: FRT, Local Amount: 0.00.", GetEditLogs(transacitonLine).First().SL_Reference);
		}

		IEnumerable<StmALog> GetEditLogs(InvoicingLineBase line)
		{
			var invoice = Factory.Load<InvoicingBase>(line.AL_AH);
			if (invoice != null)
			{
				return invoice.Logs.GetAllLogs().Cast<StmALog>().Where(s => s.SL_SE_NKEvent == Events.EditedARecord.Code);
			}
			else
			{
				return null;
			}
		}
	}
}
