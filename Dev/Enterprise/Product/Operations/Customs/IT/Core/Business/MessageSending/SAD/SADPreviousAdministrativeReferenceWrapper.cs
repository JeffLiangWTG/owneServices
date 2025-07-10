using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADPreviousAdministrativeReferenceWrapper : IPreviousAdministrativeReference
{
	public SADPreviousAdministrativeReferenceWrapper(ZString register, ZString referenceNumber, ZString referenceCin, ZDate date, ZString series, ZString customsOffice, ZInt? itemNumber)
	{
		Register = register;
		ReferenceNumber = referenceNumber;
		ReferenceCIN = referenceCin;
		Date = date;
		Series = series;
		CustomsOffice = customsOffice;
		ItemNumber = itemNumber;
	}

	public static SADPreviousAdministrativeReferenceWrapper Empty() => new SADPreviousAdministrativeReferenceWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZDate.Empty, ZString.Empty, ZString.Empty, null);

	public ZString Register { get; }

	public ZString ReferenceNumber { get; }

	public ZString ReferenceCIN { get; }

	public ZDate Date { get; }

	public ZString Series { get; }

	public virtual ZString CustomsOffice { get; }

	public virtual ZInt? ItemNumber { get; }
}
