using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DCG.Testing
{
	public class DCGStatementWrapperTest : TestCaseWithFactory
	{
		public void TestMessageEnvelope()
		{
			AssertType<StatementMessageEnvelopeWrapper>(wrapper.MessageEnvelope);
		}

		public void TestDeltaAgreementNumber()
		{
			statement.B2_EntryFilerCode = "00001930";
			AssertEquals("00001930", wrapper.DeltaAgreementNumber);
		}

		public void TestDefermentAccountNumber()
		{
			statement.B2_CheckNo = "AUPK";
			AssertEquals("AUPK", wrapper.DefermentAccountNumber);
		}

		public void TestOperationalRepresentative()
		{
			statement.B2_ImporterCustomsID = "FR4021885690004";
			AssertEquals("FR4021885690004", wrapper.OperationalRepresentative);
		}

		public void TestPaymentType()
		{
			statement.B2_PaymentType = "R";
			AssertEquals("R", wrapper.PaymentType);
		}

		public void TestPeriodStartDate()
		{
			statement.B2_PeriodStartDate = new ZDate(2021, 08, 01);
			AssertEquals(new ZDate(2021, 08, 01), wrapper.PeriodStartDate);
		}

		public void TestFrequency()
		{
			statement.B2_StatementType = "M";
			AssertEquals("M", wrapper.Frequency);
		}

		public void TestMessageType()
		{
			AssertEquals("DCG", wrapper.MessageType);
		}

		public void TestDirection()
		{
			statement.B2_BranchDesignation = "IMP";
			AssertEquals("IMP", wrapper.Direction);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statement = Factory.New<CusStatementHeader>();
			var objectToSend = new StatementMessageSendingObject(statement);
			wrapper = new DCGStatementWrapper(objectToSend);
		}

		CusStatementHeader statement;
		DCGStatementWrapper wrapper;
	}
}
