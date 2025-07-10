namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects;

public class DeclarationExporterMaxLength
{
	public DeclarationExporterMaxLength(int placeLength, int supplierDetailsLength)
	{
		PlaceInDocument = placeLength;
		SupplierDetailsInDocument = supplierDetailsLength;
	}

	public int PlaceInDocument { get; set; }
	public int SupplierDetailsInDocument { get; set; }
}
