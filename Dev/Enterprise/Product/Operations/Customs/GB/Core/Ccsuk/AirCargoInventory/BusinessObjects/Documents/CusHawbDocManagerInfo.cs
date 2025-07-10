using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class CusHawbDocManagerInfo : DocManagerInfo, Freight.Business.ShipmentDocManagerInfo.IHaveEDocsChildren
	{
		public CusHawbDocManagerInfo(CusHAWB hawb)
			: base(hawb, Enterprise.Core.Constants.DocManagerCodes.AirCargoHouse)
		{ }

		protected override BusinessObject[] GetRelatedObjects()
		{
			var list = new List<BusinessObject>();
			foreach (var baseMessage in Hawb.Messages)
			{
				list.Add(baseMessage.Factory.Load<GbEDIMessage>(baseMessage.PK));
			}
			list.AddRange(Hawb.Messages);
			if (Hawb.MAWB != null)
			{
				list.Add(Hawb.MAWB);
			}
			return list.ToArray();
		}

		CusHAWB Hawb
		{
			get { return (CusHAWB)BusinessEntity; }
		}

		public BusinessObject[] GetEDocsChildrenForAFreightJobToDisplay()
		{
			var list = new List<BusinessObject>();
			list.AddRange(Hawb.Messages);
			if (Hawb.MAWB != null)
			{
				list.Add(Hawb.MAWB);
			}
			return list.ToArray();
		}
	}
}
