using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CSARSFMessageWrapperTest : TestCaseWithFactory
	{
		[TestDate(2020, 11, 11)]
		public void TestICSARevenueSummaryFormProperties()
		{
			cSARevenueSummaryForm.B2_PrintDate = new ZDateTime(2020, 11, 11);
			cSARevenueSummaryForm.B2_PeriodStartDate = new ZDate(2020, 09, 19);
			cSARevenueSummaryForm.B2_PeriodEndDate = new ZDate(2020, 10, 18);
			cSARevenueSummaryForm.B2_ImporterCustomsID = "ICID0001";

			var messageWrapper = new CSARSFMessageWrapper(cSARevenueSummaryForm);
			AssertEquals(ZString.Empty, cSARevenueSummaryForm.B2_Status);
			AssertEquals(ZDateTime.Empty, cSARevenueSummaryForm.B2_DueDate);

			var wrapper = messageWrapper as ICSARevenueSummaryForm;
			messageWrapper.PreProcessBeforeSendMessage(MessageSubTypes.Create);
			AssertEquals(new ZDateTime(2020, 11, 11), wrapper.DocumentMessageDateTime);
			AssertEquals(new ZDateTime(2020, 09, 19), wrapper.PeriodStartDateTime);
			AssertEquals(new ZDateTime(2020, 10, 18), wrapper.PeriodEndDateTime);
			AssertEquals(new ZDateTime(2020, 10, 18), wrapper.RSFMonth);
			AssertEquals("ICID0001", wrapper.BusinessNumber);
			AssertEquals(0m, wrapper.VFD);
			AssertEquals(0m, wrapper.TotalPayment);

			AssertEquals(MessageStatusList.Codes.AwaitingOriginal, cSARevenueSummaryForm.B2_Status);
			AssertEquals(new ZDateTime(2020, 11, 11), cSARevenueSummaryForm.B2_DueDate);

			AssertEquals(cSARevenueSummaryForm.Debits.Count, wrapper.Debits.Count());
			AssertArrayEqualsByElements(cSARevenueSummaryForm.Debits.ToArray(), wrapper.Debits.ToArray());

			AssertEquals(cSARevenueSummaryForm.Credits.Count, wrapper.Credits.Count());
			AssertArrayEqualsByElements(cSARevenueSummaryForm.Credits.ToArray(), wrapper.Credits.ToArray());

			AssertEquals(cSARevenueSummaryForm.InterimPayments.Count, wrapper.InterimPayments.Count());
			AssertArrayEqualsByElements(cSARevenueSummaryForm.InterimPayments.ToArray(), wrapper.InterimPayments.ToArray());

			AssertEquals(cSARevenueSummaryForm.CustomsAssessments.Count, wrapper.CustomsAssessments.Count());
			AssertArrayEqualsByElements(cSARevenueSummaryForm.CustomsAssessments.ToArray(), wrapper.CustomsAssessments.ToArray());

			messageWrapper.UndoPreProcess();
			AssertEquals(ZString.Empty, cSARevenueSummaryForm.B2_Status);
			AssertEquals(ZDateTime.Empty, cSARevenueSummaryForm.B2_DueDate);

			messageWrapper.PreProcessBeforeSendMessage(MessageSubTypes.Change);
			AssertEquals(MessageStatusList.Codes.AwaitingChange, cSARevenueSummaryForm.B2_Status);
			AssertEquals(new ZDateTime(2020, 11, 11), cSARevenueSummaryForm.B2_DueDate);

			messageWrapper.UndoPreProcess();
			AssertEquals(ZString.Empty, cSARevenueSummaryForm.B2_Status);
			AssertEquals(ZDateTime.Empty, cSARevenueSummaryForm.B2_DueDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cSARevenueSummaryForm = Factory.New<CusStatementHeader>();
			cSARevenueSummaryForm.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
		}

		CusStatementHeader cSARevenueSummaryForm;
	}
}
