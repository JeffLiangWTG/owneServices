using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public abstract class AutoBill : Customs.Business.Bill, Customs.Business.IAddInfoManager
	{
		protected AutoBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region AddInfo/Validation/Lookups objects

		public AddInfoHouseBillLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public AddInfoHouseBillValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		protected AddInfoHouseBill AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AddInfoHouseBill(CU_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		AddInfoHouseBill fAddInfo;

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo Customs.Business.IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion
	}
}
