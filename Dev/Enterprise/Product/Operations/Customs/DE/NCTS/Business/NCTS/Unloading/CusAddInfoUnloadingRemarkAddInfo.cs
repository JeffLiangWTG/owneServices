using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class CusAddInfoUnloadingRemarkAddInfo : CusAddInfo<EU.NCTS.Business.UnloadingRemarkAddInfo>
	{
		public CusAddInfoUnloadingRemarkAddInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new UnloadingRemarkAddInfo Data => (UnloadingRemarkAddInfo)base.Data;

		protected override Type AddInfoType => typeof(UnloadingRemarkAddInfo);
	}
}
