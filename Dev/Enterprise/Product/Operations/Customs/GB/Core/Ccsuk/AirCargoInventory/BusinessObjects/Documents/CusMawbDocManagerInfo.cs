using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class CusMawbDocManagerInfo : DocManagerInfo, Freight.Business.ShipmentDocManagerInfo.IHaveEDocsChildren
	{
		public CusMawbDocManagerInfo(CusMAWB mawb)
			: base(mawb, Enterprise.Core.Constants.DocManagerCodes.AirCargoMaster)
		{ }

		protected override BusinessObject[] GetRelatedObjects()
		{
			var list = new List<BusinessObject>();
			list.AddRange(Mawb.Messages);
			foreach (CusHAWB hawb in Mawb.ChildBills)
			{
				list.Add(hawb);
			}
			return list.ToArray();
		}

		CusMAWB Mawb
		{
			get { return (CusMAWB)BusinessEntity; }
		}

		public BusinessObject[] GetEDocsChildrenForAFreightJobToDisplay()
		{
			var list = new List<BusinessObject>();
			list.AddRange(Mawb.Messages);
			list.AddRange(Mawb.ChildBills);
			return list.ToArray();
		}
	}
}
