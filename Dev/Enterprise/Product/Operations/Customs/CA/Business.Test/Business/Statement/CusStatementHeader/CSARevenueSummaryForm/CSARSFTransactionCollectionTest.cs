using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CSARSFTransactionCollection))]
	sealed class CSARSFTransactionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CSARSFTransactionCollection>
	{
		public void TestLoad()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			var line1 = header.StatementLines.AddNew();
			var line2 = header.StatementLines.AddNew();
			var line3 = header.StatementLines.AddNew();
			var line4 = header.StatementLines.AddNew();

			line1.B3_EntryType = JobMessageTypeList.Codes.Import;
			line2.B3_EntryType = JobMessageTypeList.Codes.XTypeEntry;
			line3.B3_EntryType = CSARSFPaymentTypes.Codes.Debit;
			line3.B3_EntryType = CSARSFPaymentTypes.Codes.Credit;

			AssertEquals(2, header.CSARSFTransactions.Count);
		}

		protected override CSARSFTransactionCollection GetCollectionToTest()
		{
			return new CSARSFTransactionCollection(rSF);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = rSF.StatementLines.AddNew();
			line.B3_EntryType = JobMessageTypeList.Codes.Import;
			return new CSARSFTransaction(line);
		}

		protected override void SetUp()
		{
			base.SetUp();
			rSF = Factory.New<CusStatementHeader>();
			var line1 = rSF.StatementLines.AddNew();
			line1.B3_EntryType = JobMessageTypeList.Codes.Import;
			var line2 = rSF.StatementLines.AddNew();
			line2.B3_EntryType = JobMessageTypeList.Codes.Import;
			var line3 = rSF.StatementLines.AddNew();
			line3.B3_EntryType = JobMessageTypeList.Codes.XTypeEntry;
		}

		CusStatementHeader rSF;
	}
}
