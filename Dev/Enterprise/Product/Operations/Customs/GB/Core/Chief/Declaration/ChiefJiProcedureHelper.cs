using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Chief
{
	public class ChiefJiProcedureHelper : JiProcedureHelper
	{
		public ChiefJiProcedureHelper(JobComInvoiceLine jobComInvoiceLine) : base(jobComInvoiceLine) { }

		public override void EnsureBox47TaxLineForVatRateWhenSettingTaxType(string vatRateCode)
		{
			bool validCode = true;
			base.EnsureBox47TaxLineForVatRateWhenSettingTaxType(vatRateCode);

			var taxTypeVat = FeeTypeList.Codes.B00;
			var taxRate = "";

			switch (vatRateCode)
			{
				case "666":
					taxRate = TaxRateVATDutyListImport.Codes.VATTheGoodsAreLiableToVATAtTheStandardRate;
					break;
				case "650":
					taxRate = TaxRateVATDutyListImport.Codes.VAT5PercentLowerRateForCertainGoods;
					break;
				case "654":
					taxRate = TaxRateVATDutyListImport.Codes.VATTheGoodsAreExemptFromVAT;
					break;
				case "673":
					taxRate = TaxRateVATDutyListImport.Codes.VATTheGoodsAreZeroRated;
					break;
				default:
					validCode = false;
					return;
			}

			if (validCode)
			{
				var tax = invoiceLine.Taxes.Cast<Business.JobComInvoiceLineTax>().FirstOrDefault(t => t.Data.G4_Type == taxTypeVat) ?? invoiceLine.Taxes.AddNew();
				tax.Data.G4_Type = taxTypeVat;
				tax.Data.G4_RateDuty = taxRate;
			}
		}
	}
}
