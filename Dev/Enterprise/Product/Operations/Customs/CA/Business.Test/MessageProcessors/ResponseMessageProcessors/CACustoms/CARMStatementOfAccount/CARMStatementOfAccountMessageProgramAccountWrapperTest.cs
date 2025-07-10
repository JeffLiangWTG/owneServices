using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public class CARMStatementOfAccountMessageProgramAccountWrapperTest : TestCaseWithFactory
	{
		public void TestProperties_PA()
		{
			var wrapper = CARMStatementOfAccountMessageWrapperTest.GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_PAMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("The count of program account should be 1", 1, wrapper.ProgramAccount.Count());
				var programAccount = wrapper.ProgramAccount.ToList()[0];
				AssertEquals("ACCOUNT", "112358145RM0004", programAccount.Acccount);
				AssertEquals("LINETOTALS.DUTIES", 30.6m, programAccount.Duties);
				AssertEquals("LINETOTALS.EXCISE", 60.6m, programAccount.Excise);
				AssertEquals("LINETOTALS.EXCISEDUTIES", 90.6m, programAccount.ExciseDuties);
				AssertEquals("LINETOTALS.SIMA", 120.6m, programAccount.SIMA);
				AssertEquals("LINETOTALS.GST", 150.6m, programAccount.GST);
				AssertEquals("LINETOTALS.HST", 180.6m, programAccount.HST);
				AssertEquals("LINETOTALS.PST", 210.6m, programAccount.PST);
				AssertEquals("LINETOTALS.INTEREST", 240.6m, programAccount.Interest);
				AssertEquals("LINETOTALS.PENALTIES", 270.6m, programAccount.Penalties);
				AssertEquals("LINETOTALS.PAYMENTS", 300.6m, programAccount.Payments);
				AssertEquals("LINETOTALS.OTHERS", 0m, programAccount.Others);
				AssertEquals("LINETOTALS.TOTALS", 1656.0m, programAccount.Totals);
			});
		}

		public void TestProperties_LE()
		{
			var wrapper = CARMStatementOfAccountMessageWrapperTest.GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_LEMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("The count of program account should be 2", 2, wrapper.ProgramAccount.Count());
				var programAccount = wrapper.ProgramAccount.ToList()[0];
				AssertEquals("ACCOUNT", "112358145RM0001", programAccount.Acccount);
				AssertEquals("LINETOTALS.DUTIES", 20.3m, programAccount.Duties);
				AssertEquals("LINETOTALS.EXCISE", 40.3m, programAccount.Excise);
				AssertEquals("LINETOTALS.EXCISEDUTIES", 60.3m, programAccount.ExciseDuties);
				AssertEquals("LINETOTALS.SIMA", 80.3m, programAccount.SIMA);
				AssertEquals("LINETOTALS.GST", 100.3m, programAccount.GST);
				AssertEquals("LINETOTALS.HST", 120.3m, programAccount.HST);
				AssertEquals("LINETOTALS.PST", 140.3m, programAccount.PST);
				AssertEquals("LINETOTALS.INTEREST", 160.3m, programAccount.Interest);
				AssertEquals("LINETOTALS.PENALTIES", 180.3m, programAccount.Penalties);
				AssertEquals("LINETOTALS.PAYMENTS", 200.3m, programAccount.Payments);
				AssertEquals("LINETOTALS.OTHERS", 0m, programAccount.Others);
				AssertEquals("LINETOTALS.TOTALS", 1103.0m, programAccount.Totals);

				var programAccount2 = wrapper.ProgramAccount.ToList()[1];
				AssertEquals("ACCOUNT", "112358145RM0002", programAccount2.Acccount);
				AssertEquals("LINETOTALS.DUTIES", 20.3m, programAccount2.Duties);
				AssertEquals("LINETOTALS.EXCISE", 40.3m, programAccount2.Excise);
				AssertEquals("LINETOTALS.EXCISEDUTIES", 60.3m, programAccount2.ExciseDuties);
				AssertEquals("LINETOTALS.SIMA", 80.3m, programAccount2.SIMA);
				AssertEquals("LINETOTALS.GST", 100.3m, programAccount2.GST);
				AssertEquals("LINETOTALS.HST", 120.3m, programAccount2.HST);
				AssertEquals("LINETOTALS.PST", 140.3m, programAccount2.PST);
				AssertEquals("LINETOTALS.INTEREST", 160.3m, programAccount2.Interest);
				AssertEquals("LINETOTALS.PENALTIES", 180.3m, programAccount2.Penalties);
				AssertEquals("LINETOTALS.PAYMENTS", 200.3m, programAccount2.Payments);
				AssertEquals("LINETOTALS.OTHERS", 0m, programAccount2.Others);
				AssertEquals("LINETOTALS.TOTALS", 1103.0m, programAccount2.Totals);
			});
		}

		public void TestProperties_PT()
		{
			var wrapper = CARMStatementOfAccountMessageWrapperTest.GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_PTMessageText());
			CombineAssertions(() =>
			{
				AssertEquals("The count of program account should be 2", 2, wrapper.ProgramAccount.Count());
				var programAccount = wrapper.ProgramAccount.ToList()[0];
				AssertEquals("ACCOUNT", "112358145RM0003", programAccount.Acccount);
				AssertEquals("LINETOTALS.DUTIES", 20.3m, programAccount.Duties);
				AssertEquals("LINETOTALS.EXCISE", 40.3m, programAccount.Excise);
				AssertEquals("LINETOTALS.EXCISEDUTIES", 60.3m, programAccount.ExciseDuties);
				AssertEquals("LINETOTALS.SIMA", 80.3m, programAccount.SIMA);
				AssertEquals("LINETOTALS.GST", 100.3m, programAccount.GST);
				AssertEquals("LINETOTALS.HST", 120.3m, programAccount.HST);
				AssertEquals("LINETOTALS.PST", 140.3m, programAccount.PST);
				AssertEquals("LINETOTALS.INTEREST", 160.3m, programAccount.Interest);
				AssertEquals("LINETOTALS.PENALTIES", 180.3m, programAccount.Penalties);
				AssertEquals("LINETOTALS.PAYMENTS", 200.3m, programAccount.Payments);
				AssertEquals("LINETOTALS.OTHERS", 0m, programAccount.Others);
				AssertEquals("LINETOTALS.TOTALS", 1103.0m, programAccount.Totals);

				var programAccount2 = wrapper.ProgramAccount.ToList()[1];
				AssertEquals("ACCOUNT", "112358145RM0004", programAccount2.Acccount);
				AssertEquals("LINETOTALS.DUTIES", 20.3m, programAccount2.Duties);
				AssertEquals("LINETOTALS.EXCISE", 40.3m, programAccount2.Excise);
				AssertEquals("LINETOTALS.EXCISEDUTIES", 60.3m, programAccount2.ExciseDuties);
				AssertEquals("LINETOTALS.SIMA", 80.3m, programAccount2.SIMA);
				AssertEquals("LINETOTALS.GST", 100.3m, programAccount2.GST);
				AssertEquals("LINETOTALS.HST", 120.3m, programAccount2.HST);
				AssertEquals("LINETOTALS.PST", 140.3m, programAccount2.PST);
				AssertEquals("LINETOTALS.INTEREST", 160.3m, programAccount2.Interest);
				AssertEquals("LINETOTALS.PENALTIES", 180.3m, programAccount2.Penalties);
				AssertEquals("LINETOTALS.PAYMENTS", 200.3m, programAccount2.Payments);
				AssertEquals("LINETOTALS.OTHERS", 0m, programAccount2.Others);
				AssertEquals("LINETOTALS.TOTALS", 1103.0m, programAccount2.Totals);
			});
		}

		public void TestITableValues()
		{
			var wrapper = CARMStatementOfAccountMessageWrapperTest.GetCARMStatementOfAccountMessageWrapper(Factory, MessageTypeList.Codes.CARMStatementOfAccount, CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_LEMessageText());
			var programAccount = wrapper.ProgramAccount.First();
			var tableValues = (ITableValues)programAccount;
			var values = tableValues.Values.ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Line Total", ((CellWithFormatting)values[0]).CellValue);
				AssertEquals("Duties", "20.30", values[1]);
				AssertEquals("Excise", "40.30", values[2]);
				AssertEquals("ExciseDuties", "60.30", values[3]);
				AssertEquals("SIMA", "80.30", values[4]);
				AssertEquals("GST", "100.30", values[5]);
				AssertEquals("HST", "120.30", values[6]);
				AssertEquals("PST", "140.30", values[7]);
				AssertEquals("Interest", "160.30", values[8]);
				AssertEquals("Penalties", "180.30", values[9]);
				AssertEquals("Payments", "200.30", values[10]);
				AssertEquals("Others", "0.00", values[11]);
				AssertEquals("Totals", "1,103.00", values[12]);
			});
		}
	}
}
