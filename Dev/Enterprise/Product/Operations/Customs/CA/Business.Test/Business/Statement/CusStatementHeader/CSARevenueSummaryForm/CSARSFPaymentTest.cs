using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CSARSFPayment))]
	sealed class CSARSFPaymentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var payment = GetNewBusinessObject() as CSARSFPayment;
			AssertNotNull(payment);
			AssertEquals("Payment Type", CSARSFPaymentTypes.Codes.Debit, payment.Type);
			AssertEquals("Payment Code ID", CSARSFDebitCodes.Codes._490101, payment.CodeID);
			AssertEquals("Payment Code", CSARSFDebitCodes.Descriptions._490101.ToString().Substring(0, 5), payment.Code);
			AssertEquals("Payment Code Description", CSARSFDebitCodes.Descriptions._490101.ToString().Substring(5), payment.Description);
			AssertEquals("Payment Amount", 0m, payment.Amount);

			var charge = rSFLine.Charges.Cast<CusStatementLineCharge>().FirstOrDefault(x => x.B4_ChargeType == CSARSFDebitCodes.Codes._490101);
			charge.B4_ChargeAmount = 12m;
			AssertEquals("Amount will change depends on CusStatementLineCharge value.", 12m, payment.Amount);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CSARSFPayment(rSF.GetStatementLineDependsOnPaymentType(CSARSFPaymentTypes.Codes.Debit), CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._490101, CSARSFDebitCodes.Descriptions._490101);
		}

		protected override void SetUp()
		{
			base.SetUp();
			rSF = Factory.New<CusStatementHeader>();
			rSF.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			rSFLine = rSF.DebitLine;
			csaRSFPayment = new CSARSFPayment(rSFLine, CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._490101, CSARSFDebitCodes.Descriptions._490101);
		}

		#region TEST ICSARSFItem

		public void TestICSARSFItemProperties()
		{
			csaRSFPayment.Amount = 23m;

			var item = csaRSFPayment as ICSARSFItem;

			AssertEquals(CSARSFDebitCodes.Descriptions._490101.ToString().Substring(0, 5), item.LineItemNumber);
			AssertEquals(CSARSFDebitCodes.Codes._490101, item.CodeID);
			AssertEquals(ZString.Empty, item.PortCode);
			AssertEquals(CSARSFPaymentTypes.Codes.Debit, item.Type);
			AssertEquals(23m, item.MonetaryAmount);
		}

		#endregion

		CSARSFPayment csaRSFPayment;
		CusStatementHeader rSF;
		CusStatementLine rSFLine;
	}
}
