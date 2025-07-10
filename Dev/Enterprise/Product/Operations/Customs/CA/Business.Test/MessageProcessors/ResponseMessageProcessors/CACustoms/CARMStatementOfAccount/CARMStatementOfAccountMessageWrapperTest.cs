using System.Linq;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMSOA;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public class CARMStatementOfAccountMessageWrapperTest : TestCaseWithFactory
	{
		public void TestProperties_PA()
		{
			var wrapper = GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_PAMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("File_Name", "SOA-112358145-20240711120000", wrapper.FileName);
				AssertEquals("Canada Revenue Agency Business Number", "112358145", wrapper.ImporterBusinessNumber);
				AssertEquals("File_Split", "E01", wrapper.FileSeq);
				AssertEquals("Should be PA", ZcarmsoaFileType.Pa, wrapper.FileType);
				AssertEquals("Should be CARM Statement of Account", MessageTypeList.Descriptions.CARMStatementOfAccount, wrapper.MessageSubTypeDescription);

				AssertEquals("HEADER.PER_START", new ZDateTime(2023, 9, 18), wrapper.PeriodStartDate);
				AssertEquals("HEADER.PER_END", new ZDateTime(2023, 10, 17), wrapper.PeriodEndDate);
				AssertEquals("HEADER.SOA_DATE", new ZDateTime(2023, 10, 25), wrapper.StatementDate);
				AssertEquals("HEADER.PAY_DUE", new ZDateTime(2023, 10, 31), wrapper.DueDate);
				AssertEquals("HEADER.GRAND_TOT", 12820.0m, wrapper.StatementAmount);
				AssertEquals("HEADER.PARTY.BN9", "112358145", wrapper.ImporterBusinessNumber);

				AssertEquals("Note that language is EN", "SoA - The CARM Client Portal is now live. Check the CBSA Website for more information.", wrapper.MessageEN);
				AssertEquals("Note that language is FR", "SoA - Le Portail client de la GCRA est maintenant disponible. Verifier le site Web de lASFC pour plus dinformation.", wrapper.MessageFR);
				AssertEquals("The count of program acccount should be 1", 1, wrapper.ProgramAccount.Count());

				AssertEquals("SUMMARY.LAST_TOT_A", 1000.1m, wrapper.PreviousStatementBalance);
				AssertEquals("SUMMARY.CORR_LAST_B", 2000.1m, wrapper.CorrectionsToPreviousStatementBalance);
				AssertEquals("SUMMARY.PAY_LAST_C", 3000.1m, wrapper.PaymentsReceivedAfterPreviousSOA);
				AssertEquals("SUMMARY.DISB_D", 4000.1m, wrapper.Disburesements);
				AssertEquals("SUMMARY.INTEREST_E", 5000.1m, wrapper.InterestAndPenaltiesSumTotal);
				AssertEquals("SUMMARY.DEBIT_F", 6000.1m, wrapper.CurrentPeriodCharges);
				AssertEquals("SUMMARY.CREDIT_G", 0m, wrapper.CurrentPeriodCredit);
				AssertEquals("SUMMARY.TOTPAY_H", 21000.6m, wrapper.CurrentStatementBalance);

				AssertEquals("SUMMARY.REV_DIST.DUTIES", 100.1m, wrapper.Duties);
				AssertEquals("SUMMARY.REV_DIST.EXCISE", 200.1m, wrapper.Excise);
				AssertEquals("SUMMARY.REV_DIST.EXCISEDUTIES", 300.1m, wrapper.ExciseDuties);
				AssertEquals("SUMMARY.REV_DIST.SIMA", 400.1m, wrapper.SIMA);
				AssertEquals("SUMMARY.REV_DIST.GST", 500.1m, wrapper.GST);
				AssertEquals("SUMMARY.REV_DIST.HST", 600.1m, wrapper.HST);
				AssertEquals("SUMMARY.REV_DIST.PST", 700.1m, wrapper.PST);
				AssertEquals("SUMMARY.REV_DIST.INTEREST", 800.1m, wrapper.Interest);
				AssertEquals("SUMMARY.REV_DIST.PENALTIES", 900.1m, wrapper.Penalties);
				AssertEquals("SUMMARY.REV_DIST.PAYMENTS", 1000.1m, wrapper.Payments);
				AssertEquals("SUMMARY.REV_DIST.OTHERS", 0m, wrapper.Others);
				AssertEquals("SUMMARY.REV_DIST.TOTALS", 5501m, wrapper.Totals);
			});
		}

		public void TestProperties_LE()
		{
			var wrapper = GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_LEMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("File_Name", "SOA-112358145-20231222114905", wrapper.FileName);
				AssertEquals("Canada Revenue Agency Business Number", "112358145", wrapper.ImporterBusinessNumber);
				AssertEquals("File_Split", "E01", wrapper.FileSeq);
				AssertEquals("Should be LE", ZcarmsoaFileType.Le, wrapper.FileType);
				AssertEquals("Should be CARM Statement of Account", MessageTypeList.Descriptions.CARMStatementOfAccount, wrapper.MessageSubTypeDescription);

				AssertEquals("HEADER.PER_START", new ZDateTime(2023, 9, 18), wrapper.PeriodStartDate);
				AssertEquals("HEADER.PER_END", new ZDateTime(2023, 10, 17), wrapper.PeriodEndDate);
				AssertEquals("HEADER.SOA_DATE", new ZDateTime(2023, 10, 25), wrapper.StatementDate);
				AssertEquals("HEADER.PAY_DUE", new ZDateTime(2023, 10, 31), wrapper.DueDate);
				AssertEquals("HEADER.GRAND_TOT", 27280.0m, wrapper.StatementAmount);
				AssertEquals("HEADER.PARTY.BN9", "112358145", wrapper.ImporterBusinessNumber);

				AssertEquals("Note that language is EN", "SoA - The CARM Client Portal is now live. Check the CBSA Website for more information.", wrapper.MessageEN);
				AssertEquals("Note that language is FR", "SoA - Le Portail client de la GCRA est maintenant disponible. Verifier le site Web de lASFC pour plus dinformation.", wrapper.MessageFR);
				AssertEquals("The count of program account should be 2", 2, wrapper.ProgramAccount.Count());

				AssertEquals("SUMMARY.LAST_TOT_A", 1000.1m, wrapper.PreviousStatementBalance);
				AssertEquals("SUMMARY.CORR_LAST_B", 2000.1m, wrapper.CorrectionsToPreviousStatementBalance);
				AssertEquals("SUMMARY.PAY_LAST_C", 3000.1m, wrapper.PaymentsReceivedAfterPreviousSOA);
				AssertEquals("SUMMARY.DISB_D", 4000.1m, wrapper.Disburesements);
				AssertEquals("SUMMARY.INTEREST_E", 5000.1m, wrapper.InterestAndPenaltiesSumTotal);
				AssertEquals("SUMMARY.DEBIT_F", 6000.1m, wrapper.CurrentPeriodCharges);
				AssertEquals("SUMMARY.CREDIT_G", 0m, wrapper.CurrentPeriodCredit);
				AssertEquals("SUMMARY.TOTPAY_H", 21000.6m, wrapper.CurrentStatementBalance);

				AssertEquals("SUMMARY.REV_DIST.DUTIES", 100.1m, wrapper.Duties);
				AssertEquals("SUMMARY.REV_DIST.EXCISE", 200.1m, wrapper.Excise);
				AssertEquals("SUMMARY.REV_DIST.EXCISEDUTIES", 300.1m, wrapper.ExciseDuties);
				AssertEquals("SUMMARY.REV_DIST.SIMA", 400.1m, wrapper.SIMA);
				AssertEquals("SUMMARY.REV_DIST.GST", 500.1m, wrapper.GST);
				AssertEquals("SUMMARY.REV_DIST.HST", 600.1m, wrapper.HST);
				AssertEquals("SUMMARY.REV_DIST.PST", 700.1m, wrapper.PST);
				AssertEquals("SUMMARY.REV_DIST.INTEREST", 800.1m, wrapper.Interest);
				AssertEquals("SUMMARY.REV_DIST.PENALTIES", 900.1m, wrapper.Penalties);
				AssertEquals("SUMMARY.REV_DIST.PAYMENTS", 1000.1m, wrapper.Payments);
				AssertEquals("SUMMARY.REV_DIST.OTHERS", 0m, wrapper.Others);
				AssertEquals("SUMMARY.REV_DIST.TOTALS", 5501m, wrapper.Totals);
			});
		}

		public void TestProperties_PT()
		{
			var wrapper = GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_PTMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("File_Name", "SOA-112358145-20231222114854", wrapper.FileName);
				AssertEquals("Canada Revenue Agency Business Number", "112358145", wrapper.ImporterBusinessNumber);
				AssertEquals("File_Split", "E01", wrapper.FileSeq);
				AssertEquals("Should be PT", ZcarmsoaFileType.Pt, wrapper.FileType);
				AssertEquals("Should be CARM Statement of Account", MessageTypeList.Descriptions.CARMStatementOfAccount, wrapper.MessageSubTypeDescription);

				AssertEquals("HEADER.PER_START", new ZDateTime(2023, 9, 18), wrapper.PeriodStartDate);
				AssertEquals("HEADER.PER_END", new ZDateTime(2023, 10, 17), wrapper.PeriodEndDate);
				AssertEquals("HEADER.SOA_DATE", new ZDateTime(2023, 10, 25), wrapper.StatementDate);
				AssertEquals("HEADER.PAY_DUE", new ZDateTime(2023, 10, 31), wrapper.DueDate);
				AssertEquals("HEADER.GRAND_TOT", 21640.0m, wrapper.StatementAmount);
				AssertEquals("HEADER.PARTY.BN9", "112358145", wrapper.ImporterBusinessNumber);

				AssertEquals("Note that language is EN", "SoA - The CARM Client Portal is now live. Check the CBSA Website for more information.", wrapper.MessageEN);
				AssertEquals("Note that language is FR", "SoA - Le Portail client de la GCRA est maintenant disponible. Verifier le site Web de lASFC pour plus dinformation.", wrapper.MessageFR);
				AssertEquals("The count of program account should be 2", 2, wrapper.ProgramAccount.Count());

				AssertEquals("SUMMARY.LAST_TOT_A", 1000.1m, wrapper.PreviousStatementBalance);
				AssertEquals("SUMMARY.CORR_LAST_B", 2000.1m, wrapper.CorrectionsToPreviousStatementBalance);
				AssertEquals("SUMMARY.PAY_LAST_C", 3000.1m, wrapper.PaymentsReceivedAfterPreviousSOA);
				AssertEquals("SUMMARY.DISB_D", 4000.1m, wrapper.Disburesements);
				AssertEquals("SUMMARY.INTEREST_E", 5000.1m, wrapper.InterestAndPenaltiesSumTotal);
				AssertEquals("SUMMARY.DEBIT_F", 6000.1m, wrapper.CurrentPeriodCharges);
				AssertEquals("SUMMARY.CREDIT_G", 0m, wrapper.CurrentPeriodCredit);
				AssertEquals("SUMMARY.TOTPAY_H", 21000.6m, wrapper.CurrentStatementBalance);

				AssertEquals("SUMMARY.REV_DIST.DUTIES", 100.1m, wrapper.Duties);
				AssertEquals("SUMMARY.REV_DIST.EXCISE", 200.1m, wrapper.Excise);
				AssertEquals("SUMMARY.REV_DIST.EXCISEDUTIES", 300.1m, wrapper.ExciseDuties);
				AssertEquals("SUMMARY.REV_DIST.SIMA", 400.1m, wrapper.SIMA);
				AssertEquals("SUMMARY.REV_DIST.GST", 500.1m, wrapper.GST);
				AssertEquals("SUMMARY.REV_DIST.HST", 600.1m, wrapper.HST);
				AssertEquals("SUMMARY.REV_DIST.PST", 700.1m, wrapper.PST);
				AssertEquals("SUMMARY.REV_DIST.INTEREST", 800.1m, wrapper.Interest);
				AssertEquals("SUMMARY.REV_DIST.PENALTIES", 900.1m, wrapper.Penalties);
				AssertEquals("SUMMARY.REV_DIST.PAYMENTS", 1000.1m, wrapper.Payments);
				AssertEquals("SUMMARY.REV_DIST.OTHERS", 0m, wrapper.Others);
				AssertEquals("SUMMARY.REV_DIST.TOTALS", 5501m, wrapper.Totals);
			});
		}

		public void TestITableInterpretation()
		{
			var wrapper = GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_LEMessageText());
			var tableInterpretation = (ITableInterpretation)wrapper;
			AssertEquals(ZString.Empty, tableInterpretation.Caption);

			var titles = tableInterpretation.Titles.ToList();
			AssertEquals("Duties", titles[0]);
			AssertEquals("Excise", titles[1]);
			AssertEquals("Excise Duties", titles[2]);
			AssertEquals("SIMA", titles[3]);
			AssertEquals("GST", titles[4]);
			AssertEquals("HST", titles[5]);
			AssertEquals("PST", titles[6]);
			AssertEquals("Interest", titles[7]);
			AssertEquals("Penalties", titles[8]);
			AssertEquals("Payments", titles[9]);
			AssertEquals("Others", titles[10]);
			AssertEquals("Totals", titles[11]);

			var values = tableInterpretation.Values.ToList();
			AssertEquals("Duties", "100.10", values[0]);
			AssertEquals("Excise", "200.10", values[1]);
			AssertEquals("Excise Duties", "300.10", values[2]);
			AssertEquals("SIMA", "400.10", values[3]);
			AssertEquals("GST", "500.10", values[4]);
			AssertEquals("HST", "600.10", values[5]);
			AssertEquals("PST", "700.10", values[6]);
			AssertEquals("Interest", "800.10", values[7]);
			AssertEquals("Penalties", "900.10", values[8]);
			AssertEquals("Payments", "1,000.10", values[9]);
			AssertEquals("Others", "0.00", values[10]);
			AssertEquals("Totals", "5,501.00", values[11]);
		}

		public static CARMStatementOfAcccountMessageWrapper GetCARMStatementOfAccountMessageWrapper(BusinessObjectFactory factory, ZString messageSubType, ZString messageText)
		{
			var message = factory.New<CARMStatementOfAccountMessage>();
			message.EM_MessageText = messageText;
			message.EM_MessageSubType = messageSubType;

			return new CARMStatementOfAcccountMessageWrapper(message);
		}
	}
}
