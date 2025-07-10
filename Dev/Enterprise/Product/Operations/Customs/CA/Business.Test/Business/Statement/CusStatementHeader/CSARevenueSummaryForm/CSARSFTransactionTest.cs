using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CSARSFTransaction))]
	sealed class CSARSFTransactionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreateCSARSFTransaction()
		{
			var header = Factory.New<CusStatementHeader>();
			var line1 = header.StatementLines.AddNew();
			var line2 = header.StatementLines.AddNew();
			line1.B3_EntryType = JobMessageTypeList.Codes.Import;
			line2.B3_EntryType = JobMessageTypeList.Codes.XTypeEntry;
			var transaction1 = new CSARSFTransaction(line1);
			var transaction2 = new CSARSFTransaction(line2);
			CombineAssertions(() =>
			{
				AssertEquals(0m, transaction1.Duties);
				AssertEquals(0m, transaction1.GST);
				AssertEquals(0m, transaction1.SIMA);
				AssertEquals(0m, transaction1.Excise);
				AssertEquals(0m, transaction1.Total);

				AssertEquals(0m, transaction2.Duties);
				AssertEquals(0m, transaction2.GST);
				AssertEquals(0m, transaction2.SIMA);
				AssertEquals(0m, transaction2.Excise);
				AssertEquals(0m, transaction2.Total);
			});

			var charge1 = line1.Charges.AddNew();
			charge1.B4_ChargeType = CSARSFDebitCodes.Codes._490101;
			charge1.B4_ChargeAmount = 12m;
			var charge2 = line1.Charges.AddNew();
			charge2.B4_ChargeType = CSARSFDebitCodes.Codes._491211;
			charge2.B4_ChargeAmount = 10.5m;
			var charge3 = line1.Charges.AddNew();
			charge3.B4_ChargeType = CSARSFDebitCodes.Codes._49011;
			charge3.B4_ChargeAmount = 2m;
			var charge4 = line1.Charges.AddNew();
			charge4.B4_ChargeType = CSARSFDebitCodes.Codes._49475;
			charge4.B4_ChargeAmount = 0.03m;

			var charge5 = line2.Charges.AddNew();
			charge5.B4_ChargeType = CSARSFCreditCodes.Codes._49017;
			charge5.B4_ChargeAmount = 23m;
			var charge6 = line2.Charges.AddNew();
			charge6.B4_ChargeType = CSARSFCreditCodes.Codes._49018;
			charge6.B4_ChargeAmount = -12m;

			var transaction3 = new CSARSFTransaction(line1);
			var transaction4 = new CSARSFTransaction(line2);

			CombineAssertions(() =>
			{
				AssertEquals(12m, transaction3.Duties);
				AssertEquals(10.5m, transaction3.GST);
				AssertEquals(2m, transaction3.SIMA);
				AssertEquals(0.03m, transaction3.Excise);
				AssertEquals(24.53m, transaction3.Total);

				AssertEquals(23m, transaction4.Duties);
				AssertEquals(0m, transaction4.GST);
				AssertEquals(-12m, transaction4.SIMA);
				AssertEquals(0m, transaction4.Excise);
				AssertEquals(11m, transaction4.Total);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CSARSFTransaction(cusStatementLine);
		}

		protected override void SetUp()
		{
			base.SetUp();
			rSF = Factory.New<CusStatementHeader>();
			cusStatementLine = rSF.StatementLines.AddNew();
			cusStatementLine.B3_EntryType = JobMessageTypeList.Codes.Import;
		}

		CusStatementHeader rSF;
		CusStatementLine cusStatementLine;
	}
}
