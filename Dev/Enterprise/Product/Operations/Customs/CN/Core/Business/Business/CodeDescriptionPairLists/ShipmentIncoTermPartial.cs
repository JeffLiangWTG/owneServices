namespace Enterprise.Customs.CN.Business
{
	public partial class ShipmentIncoTerm
	{
		public static string MapIncoTermCode(string cnTncoTerm) => cnTncoTerm switch
		{
			CNInvoiceHeaderIncoTermList.Codes.CIF => Codes.CIF,
			CNInvoiceHeaderIncoTermList.Codes.CFR => Codes.CAF,
			CNInvoiceHeaderIncoTermList.Codes.FOB => Codes.FOB,
			CNInvoiceHeaderIncoTermList.Codes.CAI => Codes.CAI,
			CNInvoiceHeaderIncoTermList.Codes.ExWorks => Codes.ExWorks,
			_ => string.Empty
		};
	}
}
