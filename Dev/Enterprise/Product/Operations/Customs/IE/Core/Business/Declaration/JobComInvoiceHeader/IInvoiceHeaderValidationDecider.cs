using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public interface IRuleCD8051ForJZ_ValuationCodeDecider
	{
		bool IsActive(JobComInvoiceHeader header);
	}
}
