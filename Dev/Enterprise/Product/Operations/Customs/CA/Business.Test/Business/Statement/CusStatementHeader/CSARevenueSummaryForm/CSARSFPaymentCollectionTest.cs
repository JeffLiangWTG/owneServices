using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CSARSFPaymentCollection))]
	sealed class CSARSFPaymentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CSARSFPaymentCollection>
	{
		public void TestCalculatePayments()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			var line1 = header.StatementLines.AddNew();
			var line2 = header.StatementLines.AddNew();
			var line3 = header.StatementLines.AddNew();
			line1.B3_EntryType = JobMessageTypeList.Codes.Import;
			line2.B3_EntryType = JobMessageTypeList.Codes.XTypeEntry;
			line3.B3_EntryType = JobMessageTypeList.Codes.XTypeEntry;

			var charge = line1.Charges.AddNew();
			charge.B4_ChargeType = CSARSFDebitCodes.Codes._490101;
			charge.B4_ChargeAmount = 12m;

			charge = line1.Charges.AddNew();
			charge.B4_ChargeType = CSARSFDebitCodes.Codes._491211;
			charge.B4_ChargeAmount = 10.5m;

			charge = line1.Charges.AddNew();
			charge.B4_ChargeType = CSARSFDebitCodes.Codes._49011;
			charge.B4_ChargeAmount = 2m;

			charge = line1.Charges.AddNew();
			charge.B4_ChargeType = CSARSFDebitCodes.Codes._49475;
			charge.B4_ChargeAmount = 0.03m;

			charge = line2.Charges.AddNew();
			charge.B4_ChargeType = CSARSFDebitCodes.Codes._490102;
			charge.B4_ChargeAmount = -23m;

			charge = line2.Charges.AddNew();
			charge.B4_ChargeType = CSARSFDebitCodes.Codes._491212;
			charge.B4_ChargeAmount = -12m;

			charge = line2.Charges.AddNew();
			charge.B4_ChargeType = CSARSFCreditCodes.Codes._49018;
			charge.B4_ChargeAmount = 0.1m;

			charge = line3.Charges.AddNew();
			charge.B4_ChargeType = CSARSFCreditCodes.Codes._49017;
			charge.B4_ChargeAmount = -2m;

			charge = line3.Charges.AddNew();
			charge.B4_ChargeType = CSARSFCreditCodes.Codes._49121;
			charge.B4_ChargeAmount = -22m;

			var debits = header.Debits.Cast<CSARSFPayment>();
			var payment = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._49443);
			payment.Amount = 13.1m;

			payment = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._49454);
			payment.Amount = 21.3m;

			payment = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._49555);
			payment.Amount = 3m;

			var credits = header.Credits.Cast<CSARSFPayment>();
			payment = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49010);
			payment.Amount = 4m;

			payment = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49019);
			payment.Amount = 5.1m;

			payment = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49409);
			payment.Amount = 6m;

			payment = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49437);
			payment.Amount = 7.01m;

			payment = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49443);
			payment.Amount = 8m;

			payment = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49555);
			payment.Amount = 9m;

			var interims = header.InterimPayments.Cast<CSARSFPayment>();
			payment = interims.FirstOrDefault(x => x.CodeID == CSARSFInterimPaymentCodes.Codes._49010);
			payment.Amount = 132m;

			payment = interims.FirstOrDefault(x => x.CodeID == CSARSFInterimPaymentCodes.Codes._49121);
			payment.Amount = 234m;

			header.Debits.CalculatePayments();
			header.Credits.CalculatePayments();
			header.InterimPayments.CalculatePayments();

			CombineAssertions(() =>
			{
				AssertEquals(CSARSFDebitCodes.Codes._490101, 12m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._490101));
				AssertEquals(CSARSFDebitCodes.Codes._490102, -23m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._490102));
				AssertEquals(CSARSFDebitCodes.Codes._49011, 2m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._49011));
				AssertEquals(CSARSFDebitCodes.Codes._491211, 10.5m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._491211));
				AssertEquals(CSARSFDebitCodes.Codes._491212, -12m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._491212));
				AssertEquals(CSARSFDebitCodes.Codes._49443, 13.1m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._49443));
				AssertEquals(CSARSFDebitCodes.Codes._49454, 21.3m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._49454));
				AssertEquals(CSARSFDebitCodes.Codes._49475, 0.03m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._49475));
				AssertEquals(CSARSFDebitCodes.Codes._49555, 3m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._49555));

				AssertEquals(CSARSFCreditCodes.Codes._49010, 4m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49010));
				AssertEquals(CSARSFCreditCodes.Codes._49017, -2m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49017));
				AssertEquals(CSARSFCreditCodes.Codes._49018, 0.1m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49018));
				AssertEquals(CSARSFCreditCodes.Codes._49019, 5.1m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49019));
				AssertEquals(CSARSFCreditCodes.Codes._49121, 0m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49121));
				AssertEquals(CSARSFCreditCodes.Codes._49409, 6m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49409));
				AssertEquals(CSARSFCreditCodes.Codes._49437, 7.01m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49437));
				AssertEquals(CSARSFCreditCodes.Codes._49443, 8m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49443));
				AssertEquals(CSARSFCreditCodes.Codes._49555, 9m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49555));

				AssertEquals(CSARSFInterimPaymentCodes.Codes._49010, 132m, GetPaymentAmount(interims, CSARSFInterimPaymentCodes.Codes._49010));
				AssertEquals(CSARSFInterimPaymentCodes.Codes._49121, 234m, GetPaymentAmount(interims, CSARSFInterimPaymentCodes.Codes._49121));
			});
		}

		ZDecimal GetPaymentAmount(IEnumerable<CSARSFPayment> payments, ZString paymentType)
		{
			return payments.FirstOrDefault(x => x.CodeID == paymentType).Amount;
		}

		public void TestElements()
		{
			var collection = GetCollectionToTest();
			AssertEquals(9, collection.Count);
			foreach (CSARSFPayment element in collection)
			{
				AssertEquals(CSARSFPaymentTypes.Codes.Debit, element.Type);
			}
		}

		protected override CSARSFPaymentCollection GetCollectionToTest()
		{
			return rSF.Debits;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CSARSFPayment(rSF.GetStatementLineDependsOnPaymentType(CSARSFPaymentTypes.Codes.Debit), CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._490101, CSARSFDebitCodes.Descriptions._490101);
		}

		protected override void SetUp()
		{
			base.SetUp();
			rSF = Factory.New<CusStatementHeader>();
			rSF.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
		}

		CusStatementHeader rSF;
	}
}
