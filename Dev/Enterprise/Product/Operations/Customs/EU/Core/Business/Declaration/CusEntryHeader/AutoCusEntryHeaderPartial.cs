using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract partial class AutoCusEntryHeader : Customs.Business.CusEntryHeader, Customs.Business.IAddInfoManager
	{
		protected AddInfoCusEntryHeader AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = GetNewAddInfoCusEntryHeader();
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		AddInfoCusEntryHeader fAddInfo;

		protected virtual AddInfoCusEntryHeader GetNewAddInfoCusEntryHeader()
		{
			return new AddInfoCusEntryHeader(CH_AddInfoInfo);
		}

		#region IAddInfoChildSupporter Members

		protected override BusinessObject GetAddInfoChild() => AddInfoChild;
		protected override SchemaGuidColumn GetChildForeignKeyColumn() => CusEUEntryHeaderSchema.EUH_CH;

		#endregion
	}
}
