using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public class CARMStatementOfAccountMessageDaySummaryWrapperTest : TestCaseWithFactory
	{
		public void TestProperties_PA()
		{
			var wrapper = CARMStatementOfAccountMessageWrapperTest.GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_PAMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("The count of program account should be 1", 1, wrapper.ProgramAccount.Count());
				var programAccount = wrapper.ProgramAccount.ToList()[0];
				AssertEquals("Should be 3", 3, programAccount.DaySummary.Count());

				var daySummary = programAccount.DaySummary.ToList()[0];
				AssertCARMStatementOfAcccountMessageDaySummaryWrapper(daySummary, "1", new ZDate(2023, 08, 01), new ZDateTime(2023, 08, 01), 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, new ZDateTime(2023, 10, 31), 0m, 551.0m);

				daySummary = programAccount.DaySummary.ToList()[1];
				AssertCARMStatementOfAcccountMessageDaySummaryWrapper(daySummary, "2", new ZDate(2023, 08, 02), new ZDateTime(2023, 08, 02), 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, new ZDateTime(2023, 10, 31), 0m, 552.0m);

				daySummary = programAccount.DaySummary.ToList()[2];
				AssertCARMStatementOfAcccountMessageDaySummaryWrapper(daySummary, "3", new ZDate(2023, 08, 03), new ZDateTime(2023, 08, 03), 10.3m, 20.3m, 30.3m, 40.3m, 50.3m, 60.3m, 70.3m, 80.3m, 90.3m, 100.3m, new ZDateTime(2023, 10, 31), 0m, 553.0m);
			});
		}

		public void TestProperties_LE()
		{
			var wrapper = CARMStatementOfAccountMessageWrapperTest.GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_LEMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("The count of program account should be 2", 2, wrapper.ProgramAccount.Count());
				var programAccount = wrapper.ProgramAccount.ToList()[0];
				AssertEquals("Should be 2", 2, programAccount.DaySummary.Count());

				var daySummary = programAccount.DaySummary.ToList()[0];
				AssertCARMStatementOfAcccountMessageDaySummaryWrapper(daySummary, "1", new ZDate(2023, 08, 01), new ZDateTime(2023, 08, 01), 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, new ZDateTime(2023, 10, 31), 0m, 551.0m);

				daySummary = programAccount.DaySummary.ToList()[1];
				AssertCARMStatementOfAcccountMessageDaySummaryWrapper(daySummary, "2", new ZDate(2023, 08, 02), new ZDateTime(2023, 08, 02), 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, new ZDateTime(2023, 10, 31), 0m, 552.0m);

				programAccount = wrapper.ProgramAccount.ToList()[1];
				daySummary = programAccount.DaySummary.ToList()[0];
				AssertCARMStatementOfAcccountMessageDaySummaryWrapper(daySummary, "1", new ZDate(2023, 08, 01), new ZDateTime(2023, 08, 01), 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, new ZDateTime(2023, 10, 31), 0m, 551.0m);

				daySummary = programAccount.DaySummary.ToList()[1];
				AssertCARMStatementOfAcccountMessageDaySummaryWrapper(daySummary, "2", new ZDate(2023, 08, 02), new ZDateTime(2023, 08, 02), 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, new ZDateTime(2023, 10, 31), 0m, 552.0m);
			});
		}

		public void TestProperties_PT()
		{
			var wrapper = CARMStatementOfAccountMessageWrapperTest.GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_PTMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("The count of program account should be 2", 2, wrapper.ProgramAccount.Count());
				var programAccount = wrapper.ProgramAccount.ToList()[0];
				AssertEquals("Should be 2", 2, programAccount.DaySummary.Count());

				var daySummary = programAccount.DaySummary.ToList()[0];
				AssertCARMStatementOfAcccountMessageDaySummaryWrapper(daySummary, "1", new ZDate(2023, 08, 01), new ZDateTime(2023, 08, 01), 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, new ZDateTime(2023, 10, 31), 0m, 551.0m);

				daySummary = programAccount.DaySummary.ToList()[1];
				AssertCARMStatementOfAcccountMessageDaySummaryWrapper(daySummary, "2", new ZDate(2023, 08, 02), new ZDateTime(2023, 08, 02), 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, new ZDateTime(2023, 10, 31), 0m, 552.0m);

				programAccount = wrapper.ProgramAccount.ToList()[1];
				daySummary = programAccount.DaySummary.ToList()[0];
				AssertCARMStatementOfAcccountMessageDaySummaryWrapper(daySummary, "1", new ZDate(2023, 08, 01), new ZDateTime(2023, 08, 01), 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, new ZDateTime(2023, 10, 31), 0m, 551.0m);

				daySummary = programAccount.DaySummary.ToList()[1];
				AssertCARMStatementOfAcccountMessageDaySummaryWrapper(daySummary, "2", new ZDate(2023, 08, 02), new ZDateTime(2023, 08, 02), 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, new ZDateTime(2023, 10, 31), 0m, 552.0m);
			});
		}

		void AssertCARMStatementOfAcccountMessageDaySummaryWrapper(CARMStatementOfAccountMessageDaySummaryWrapper daySummary, ZString id, ZDate releaseDate, ZDateTime accountingDate, ZDecimal duties, ZDecimal excise, ZDecimal exciseDuties,
			ZDecimal sima, ZDecimal gST, ZDecimal hST, ZDecimal pST, ZDecimal interest, ZDecimal penalties, ZDecimal payments, ZDateTime? paymentDueDate, ZDecimal others, ZDecimal totals)
		{
			AssertEquals(id + " DAY_SUMMARY.RELEASE_DATE", releaseDate, daySummary.ReleaseDate);
			AssertEquals(id + " DAY_SUMMARY.ACCOUNTING_DATE", accountingDate, daySummary.AccountingDate);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.DUTIES", duties, daySummary.Duties);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.EXCISE", excise, daySummary.Excise);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.EXCISEDUTIES", exciseDuties, daySummary.ExciseDuties);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.SIMA", sima, daySummary.SIMA);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.GST", gST, daySummary.GST);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.HST", hST, daySummary.HST);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.PST", pST, daySummary.PST);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.INTEREST", interest, daySummary.Interest);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.PENALTIES", penalties, daySummary.Penalties);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.PAYMENTS", payments, daySummary.Payments);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.PAYMENT_DUE_DATE", paymentDueDate, daySummary.PaymentDueDate);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.OTHERS", others, daySummary.Others);
			AssertEquals(id + " DAY_SUMMARY.LINEITEM.TOTALS", totals, daySummary.Totals);
		}

		public void TestITableInterpretation()
		{
			var wrapper = CARMStatementOfAccountMessageWrapperTest.GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_LEMessageText());
			var tableInterpretation = (ITableInterpretation)wrapper.ProgramAccount.ToList()[0].DaySummary.ToList()[0];
			AssertEquals("112358145RM0001", tableInterpretation.Caption);

			var titles = tableInterpretation.Titles.ToList();
			AssertEquals("ID", titles[0]);
			AssertEquals("Release Date", titles[1]);
			AssertEquals("Accounting Date", titles[2]);
			AssertEquals("Duties", titles[3]);
			AssertEquals("Excise", titles[4]);
			AssertEquals("Excise Duties", titles[5]);
			AssertEquals("SIMA", titles[6]);
			AssertEquals("GST", titles[7]);
			AssertEquals("HST", titles[8]);
			AssertEquals("PST", titles[9]);
			AssertEquals("Interest", titles[10]);
			AssertEquals("Penalties", titles[11]);
			AssertEquals("Payments", titles[12]);
			AssertEquals("Others", titles[13]);
			AssertEquals("Totals", titles[14]);
			AssertEquals("Payment Due Date", titles[15]);

			var values = tableInterpretation.Values.ToList();
			AssertEquals("ID", "1", values[0]);
			AssertEquals("Release Date", new ZDate(2023, 08, 01), values[1]);
			AssertEquals("Accounting Date", new ZDateTime(2023, 08, 01), values[2]);
			AssertEquals("Duties", "10.10", values[3]);
			AssertEquals("Excise", "20.10", values[4]);
			AssertEquals("Excise Duties", "30.10", values[5]);
			AssertEquals("SIMA", "40.10", values[6]);
			AssertEquals("GST", "50.10", values[7]);
			AssertEquals("HST", "60.10", values[8]);
			AssertEquals("PST", "70.10", values[9]);
			AssertEquals("Interest", "80.10", values[10]);
			AssertEquals("Penalties", "90.10", values[11]);
			AssertEquals("Payments", "100.10", values[12]);
			AssertEquals("Others", "0.00", values[13]);
			AssertEquals("Totals", "551.00", values[14]);
			AssertEquals("Payment Due Date", new ZDateTime(2023, 10, 31), values[15]);
		}
	}
}
