using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	[TestedType(typeof(CusStatementChargesDetail))]
	class CusStatementChargesDetailTest : CusStatementLineTest
	{
		public void TestStatement()
		{
			var chargeDetail = Factory.New<CusStatementChargesDetail>();
			AssertNull(chargeDetail.Statement);

			var statement = Factory.New<CusStatementHeader>();
			chargeDetail.B3_B2 = statement.PK;
			AssertSame(statement, chargeDetail.Statement);
		}

		public void TestDelete()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = StatementPeriodicityList.Codes.Day;
			header.ChargesDetail.Charges.AddNew();
			header.ChargesDetail.B3_BrokerReference = "B00000002";
			Factory.Save();
			AssertEquals(1, Factory.GetDatabaseCount(typeof(CusStatementLineCharge)));

			header.ChargesDetail.Delete();
			Factory.Save();
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusStatementLineCharge)));
		}

		public void TestLoadOrCreate()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_StatementType = StatementPeriodicityList.Codes.Day;
			var createdChargesDetail = statementHeader.ChargesDetail;
			createdChargesDetail.B3_BrokerReference = "B00000002";
			Factory.Save();
			var loadedChargesDetail = CusStatementChargesDetail.LoadOrCreate(statementHeader);
			AssertSame("2nd attempt of LoadOrCreate should return the charge detail created at first attempt", loadedChargesDetail, createdChargesDetail);
		}

		public void TestLoadOrCreate_ShouldLoadDCGEntry()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_StatementType = StatementPeriodicityList.Codes.Day;
			Factory.Save();

			statementHeader.ChargesDetail.Delete();
			Factory.Save();

			var exist = Factory.Exists(typeof(CusStatementLine), new ZQuery(CusStatementLineSchema.B3_B2, statementHeader.PK));
			AssertEquals("PRE: there is no statement line in the db.", false, exist);

			var statementLine1 = Factory.New<CusStatementEntry>();
			statementLine1.B3_B2 = statementHeader.PK;
			statementLine1.B3_EntryType = StatementEntryTypeList.Codes.Import;
			statementLine1.B3_BrokerReference = "123";
			Factory.Save();

			var loadedChargesDetail = CusStatementChargesDetail.LoadOrCreate(statementHeader);
			AssertNotEquals("The existing statement line is not type DCG, so we create a new one instead.", statementLine1, loadedChargesDetail);

			loadedChargesDetail.Delete();
			Factory.Save();

			var statementLine2 = Factory.New<CusStatementChargesDetail>();
			statementLine2.B3_B2 = statementHeader.PK;
			statementLine2.B3_EntryType = StatementEntryTypeList.Codes.DCG;
			statementLine2.B3_BrokerReference = "456";
			Factory.Save();

			loadedChargesDetail = CusStatementChargesDetail.LoadOrCreate(statementHeader);
			AssertEquals("The existing statement line is type DCG, so we load it as ChargesDetail.", statementLine2, loadedChargesDetail);
		}

		protected override BusinessObject GetNewBusinessObject() => chargesDetail;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => chargesDetail;

		protected override void SetUp()
		{
			base.SetUp();
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
			chargesDetail = statement.ChargesDetail;
			chargesDetail.B3_BrokerReference = "B00000001";
		}

		CusStatementChargesDetail chargesDetail;
	}
}
