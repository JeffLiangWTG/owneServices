using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[UniversalCopyAddInfo(CusEntryInstructionSchema.Constants.Prefix, Business.AddInfo.Schema.Prefix)]
	public partial class AutoCusEntryInstruction : IAddInfoChildSupporter
	{
		protected AddInfoCusEntryInstruction AddInfo
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
		AddInfoCusEntryInstruction fAddInfo;

		protected virtual AddInfoCusEntryInstruction GetNewAddInfo() => new AddInfoCusEntryInstruction(CEI_AddInfoInfo);
	}
}
