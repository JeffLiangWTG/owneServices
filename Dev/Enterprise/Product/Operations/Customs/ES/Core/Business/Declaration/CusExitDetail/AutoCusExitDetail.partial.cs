using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Declaration
{
	[UniversalCopyAddInfo(CusExitDetailSchema.Constants.Prefix, EU.Business.AddInfo.Schema.Prefix)]
	public abstract partial class AutoCusExitDetail
	{
		protected AddInfoCusExitDetail AddInfo
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
		AddInfoCusExitDetail fAddInfo;

		protected virtual AddInfoCusExitDetail GetNewAddInfo() => new AddInfoCusExitDetail(CED_AddInfoInfo);
	}
}
