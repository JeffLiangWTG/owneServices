namespace Enterprise.Customs.CN.Business
{
	public class IncoTermConverter : Customs.Business.IncoTermConverter
	{
		public override string GetConvertedIncoTerm(string incoTerm, bool isImport) => incoTerm switch
		{
			Core.Constants.IncoTerms.CostInsuranceAndFreight => CNInvoiceHeaderIncoTermList.Codes.CIF,
			Core.Constants.IncoTerms.CarriageAndInsurancePaidTo => CNInvoiceHeaderIncoTermList.Codes.CIF,
			Core.Constants.IncoTerms.DeliveredDutyPaid => CNInvoiceHeaderIncoTermList.Codes.CIF,
			Core.Constants.IncoTerms.DeliveredAtPlace => CNInvoiceHeaderIncoTermList.Codes.CIF,
			Core.Constants.IncoTerms.DeliveredAtTerminal => CNInvoiceHeaderIncoTermList.Codes.CIF,
			Core.Constants.IncoTerms.CostAndFreight => CNInvoiceHeaderIncoTermList.Codes.CFR,
			Core.Constants.IncoTerms.CostFreightWithAmpersand => CNInvoiceHeaderIncoTermList.Codes.CFR,
			Core.Constants.IncoTerms.CarriagePaidTo => CNInvoiceHeaderIncoTermList.Codes.CFR,
			Core.Constants.IncoTerms.ExWorks => CNInvoiceHeaderIncoTermList.Codes.ExWorks,
			Core.Constants.IncoTerms.FreeCarrier => CNInvoiceHeaderIncoTermList.Codes.FOB,
			Core.Constants.IncoTerms.FreeAlongsideShip => CNInvoiceHeaderIncoTermList.Codes.FOB,
			Core.Constants.IncoTerms.FreeOnBoard => CNInvoiceHeaderIncoTermList.Codes.FOB,
			Core.Constants.IncoTerms.CostAndInsurance => CNInvoiceHeaderIncoTermList.Codes.CAI,
			_ => string.Empty
		};
	}
}
