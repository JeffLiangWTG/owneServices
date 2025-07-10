namespace Enterprise.Customs.CH.Business;

public class AdditionalCodeDataCollection : SingleCusCodeDataCollection<AdditionalCodeData>
{
	public AdditionalCodeDataCollection(JobComInvoiceLine parent)
		: base(parent, CusCodeDataTypeList.Codes.AdditionalCode)
	{
	}
}
