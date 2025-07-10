using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[UniversalCopyAddInfo(JobComInvoiceHeaderSchema.Constants.Prefix, Business.AddInfo.Schema.Prefix)]
	public abstract partial class AutoJobComInvoiceHeader
	{
		protected AddInfoJobComInvoiceHeader AddInfo
		{
			get
			{
				if (addInfo == null)
				{
					addInfo = GetNewAddInfo();
					RegisterEditableChildObject(addInfo);
					RegisterListChangedCalledRefreshBinding(addInfo);
				}
				return addInfo;
			}
		}
		AddInfoJobComInvoiceHeader addInfo;

		protected virtual AddInfoJobComInvoiceHeader GetNewAddInfo() => new AddInfoJobComInvoiceHeader(JZ_AddInfoInfo);
	}
}
