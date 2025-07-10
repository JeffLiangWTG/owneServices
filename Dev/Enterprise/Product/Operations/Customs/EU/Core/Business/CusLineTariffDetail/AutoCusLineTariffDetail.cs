using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public abstract class AutoCusLineTariffDetail : Customs.Business.CusLineTariffDetail, Customs.Business.IAddInfoManager
	{
		protected AutoCusLineTariffDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region AddInfo
		protected AddInfoCusLineTariffDetail AddInfo
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
		AddInfoCusLineTariffDetail fAddInfo;

		protected virtual AddInfoCusLineTariffDetail GetNewAddInfo()
		{
			return new AddInfoCusLineTariffDetail(BZ_NAddInfoInfo);
		}

		public AddInfoCusLineTariffDetailLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public AddInfoCusLineTariffDetailValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo Customs.Business.IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion
	}
}
