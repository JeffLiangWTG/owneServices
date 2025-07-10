using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[UniversalCopyAddInfo(CusEntryLineSchema.Constants.Prefix, Business.AddInfo.Schema.Prefix)]
	public abstract partial class AutoCusEntryLine
	{
		protected AddInfoCusEntryLine AddInfo
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
		AddInfoCusEntryLine addInfo;

		protected virtual AddInfoCusEntryLine GetNewAddInfo() => new AddInfoCusEntryLine(CL_AddInfoInfo);
	}
}
