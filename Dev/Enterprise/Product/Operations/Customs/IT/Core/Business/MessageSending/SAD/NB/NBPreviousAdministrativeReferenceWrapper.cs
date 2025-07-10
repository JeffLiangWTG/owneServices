using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class NBPreviousAdministrativeReferenceWrapper : SADPreviousAdministrativeReferenceWrapper
{
	public NBPreviousAdministrativeReferenceWrapper(ZString register, ZString referenceNumber, ZString referenceCin, ZDate date, ZString series, ZString customsOffice, ZInt? itemNumber)
		: base(register, referenceNumber, referenceCin, date, series, customsOffice, itemNumber)
	{
	}

	public override ZString CustomsOffice => base.CustomsOffice.SubstringSafe(2);

	public override ZInt? ItemNumber => GetItemNumber(base.ItemNumber);

	ZInt? GetItemNumber(ZInt? itemNumber) => itemNumber.HasValue && itemNumber.Value > 0 ? itemNumber : null;
}
