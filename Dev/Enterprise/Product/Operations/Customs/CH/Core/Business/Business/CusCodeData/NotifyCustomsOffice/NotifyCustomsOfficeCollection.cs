using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class NotifyCustomsOfficeCollection : CusCodeDataCollection<NotifyCustomsOffice>
{
	public NotifyCustomsOfficeCollection(JobComInvoiceLine parent) : base(parent, CusCodeDataTypeList.Codes.NotifyCustomsOffice)
	{
		MaxCountValidationEnable(MaxNumberOfAllowedEntries, Res.GetString("Enterprise.Customs.CH.Business.NotifyCustomsOfficeCollection|NotifyCustomsOfficeCollection", "Only {0} Notify Customs Offices are allowed.", MaxNumberOfAllowedEntries));
	}

	const int MaxNumberOfAllowedEntries = 9;
}
