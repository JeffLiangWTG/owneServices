using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ISupportingDocument
{
	ZString Type { get; }
	ZString CountryOfIssue { get; }
	ZString YearOfIssue { get; }
	ZString ReferenceNumber { get; }
	ZDecimal Quantity { get; }
	ZString UnitOfQuantity { get; }
	ZString Status { get; }
}
