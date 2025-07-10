using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class EuCustomsValuationCalculatorTest : TestCaseWithFactory
	{
		public virtual void TestCalculateCifForEu()
		{
			var dec = GetNewJobDeclaration();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var inv = dec.Invoices.AddNew();
			inv.JZ_IncoTerm = "FOB";
			inv.JZ_InvoiceAmount = 1000m;
			inv.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
			var invLine = inv.InvoiceLines.AddNew();
			invLine.JI_LinePrice = inv.JZ_InvoiceAmount;
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)dec.IncoTermAndChargeFactory;
			var chargeFreightBefore = invLine.Charges.AddNew();
			chargeFreightBefore.J7_ChargeType = chargeFactory.FreightToEUBorderCode;
			chargeFreightBefore.J7_Amount = 80m;
			chargeFreightBefore.J7_IsDutiable = true;
			var chargeFreightAfter = invLine.Charges.AddNew();
			chargeFreightAfter.J7_ChargeType = chargeFactory.FreightAfterEUBorderCode;
			chargeFreightAfter.J7_Amount = 20m;
			chargeFreightAfter.J7_IsDutiable = false;
			AssertEquals("Pre-req: customs value is 1080", 1080m, invLine.JI_CustomsValue);
			AssertEquals("CIF is customs value plus freight after border", 1100m, invLine.JI_Calc_CIF);
		}

		protected virtual JobDeclaration GetNewJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
