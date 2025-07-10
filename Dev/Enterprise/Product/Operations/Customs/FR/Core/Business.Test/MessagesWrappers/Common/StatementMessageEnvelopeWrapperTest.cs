using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	public class StatementMessageEnvelopeWrapperTest : TestCaseWithFactory
	{
		public void TestSchemaID()
		{
			AssertEquals("MessageDcg", wrapper.SchemaID);
		}

		public void TestSchemaVersion()
		{
			AssertEquals("18122012", wrapper.SchemaVersion);
		}

		public void TestPartnerId()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ReportingPeriodList.Codes.TEN, "CAC2581C");
			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Import;
			statement.B2_OH_Importer = importer.PK;
			statement.B2_EntryFilerCode = "DGI002";
			AssertEquals("CAC2581C", statement.DeltaAgreementAccountRepresentativeID);
			AssertEquals("CAC2581C", wrapper.PartnerId);
		}

		public void TestTransactionId()
		{
			AssertEquals(ZString.Empty, wrapper.TransactionId);
		}

		public void TestNumSeq()
		{
			statement.Messages.AddNew().IsTransmitMessage = true;
			statement.Messages.AddNew().IsTransmitMessage = true;
			statement.Messages.AddNew().IsTransmitMessage = true;
			statement.Messages.AddNew().IsTransmitMessage = false;
			AssertEquals((short)3, wrapper.NumSeq);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statement = Factory.New<CusStatementHeader>();
			wrapper = new StatementMessageEnvelopeWrapper(statement);
		}

		CusStatementHeader statement;
		StatementMessageEnvelopeWrapper wrapper;
	}
}
