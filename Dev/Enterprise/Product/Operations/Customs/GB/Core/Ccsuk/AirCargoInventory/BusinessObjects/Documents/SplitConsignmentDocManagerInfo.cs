using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class SplitConsignmentDocManagerInfo : DocManagerInfo, Freight.Business.ShipmentDocManagerInfo.IHaveEDocsChildren
	{
		public SplitConsignmentDocManagerInfo(SplitConsignment split)
			: base(split, Enterprise.Core.Constants.DocManagerCodes.AirCargoHouse)
		{ }

		protected override BusinessObject[] GetRelatedObjects()
		{
			var list = new List<BusinessObject>();
			foreach (var baseMessage in Split.Messages)
			{
				list.Add(baseMessage.Factory.Load<GbEDIMessage>(baseMessage.PK));
			}
			list.AddRange(Split.Messages);
			return list.ToArray();
		}

		SplitConsignment Split
		{
			get { return (SplitConsignment)BusinessEntity; }
		}

		public BusinessObject[] GetEDocsChildrenForAFreightJobToDisplay()
		{
			var list = new List<BusinessObject>();
			list.AddRange(Split.Messages);
			return list.ToArray();
		}
	}
}
