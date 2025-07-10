using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[UniversalCopyAddInfo(JobComInvoiceLineSchema.Constants.Prefix, Business.AddInfo.Schema.Prefix)]
	public abstract partial class AutoJobComInvoiceLine
	{
		protected AddInfoJobComInvoiceLine AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = GetNewAddInfo();
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		AddInfoJobComInvoiceLine fAddInfo;

		protected virtual AddInfoJobComInvoiceLine GetNewAddInfo() => new AddInfoJobComInvoiceLine(JI_AddInfoInfo);
	}
}
