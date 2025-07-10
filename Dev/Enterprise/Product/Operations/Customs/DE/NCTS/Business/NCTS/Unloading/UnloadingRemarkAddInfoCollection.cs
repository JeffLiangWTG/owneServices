using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class UnloadingRemarkAddInfoCollection : CusAddInfoCollection<EU.NCTS.Business.UnloadingRemarkAddInfo>
	{
		public UnloadingRemarkAddInfoCollection(BusinessObject master) : base(master)
		{
		}

		public new CusAddInfoUnloadingRemarkAddInfo this[int index] => (CusAddInfoUnloadingRemarkAddInfo)Elements[index];

		public new CusAddInfoUnloadingRemarkAddInfo AddNew()
		{
			return (CusAddInfoUnloadingRemarkAddInfo)base.AddNew();
		}

		protected new CusAddInfoUnloadingRemarkAddInfo AddNew(Type type) => (CusAddInfoUnloadingRemarkAddInfo)base.AddNew(type);

		protected override BusinessObject AddNewCore() => AddNew(typeof(CusAddInfoUnloadingRemarkAddInfo));

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var cusAddInfoUnloadingRemarkAddInfo = child as CusAddInfoUnloadingRemarkAddInfo;
			if (cusAddInfoUnloadingRemarkAddInfo != null)
			{
				cusAddInfoUnloadingRemarkAddInfo.Data.G9_UnloadingCompletion = YesNoList.Codes.Yes;
			}
		}
	}
}
