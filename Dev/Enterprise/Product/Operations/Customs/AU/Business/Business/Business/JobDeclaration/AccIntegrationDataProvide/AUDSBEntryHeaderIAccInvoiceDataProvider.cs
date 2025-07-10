using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.AU.Declaration.Business;

public class AUDSBEntryHeaderIAccInvoiceDataProvider : AUEntryHeaderIAccInvoiceDataProvider
{
	public AUDSBEntryHeaderIAccInvoiceDataProvider(CusEntryHeader entryHeader, bool isQuarantineChargeRatingSeparated) : base(entryHeader)
	{
		this.isQuarantineChargeRatingSeparated = isQuarantineChargeRatingSeparated;
	}
	readonly bool isQuarantineChargeRatingSeparated;

	public override EntryChargeTypeList EntryChargeTypeList
	{
		get
		{
			if (entryChargeTypeList == null)
			{
				entryChargeTypeList = new CusEntryChargeTypeList();

				if (isQuarantineChargeRatingSeparated)
				{
					entryChargeTypeList.RemoveWhere(c => c.Code == CusEntryChargeTypeList.Codes.AQISServicePaymentAmount);
				}
			}
			return entryChargeTypeList;
		}
	}
	EntryChargeTypeList entryChargeTypeList;
}
