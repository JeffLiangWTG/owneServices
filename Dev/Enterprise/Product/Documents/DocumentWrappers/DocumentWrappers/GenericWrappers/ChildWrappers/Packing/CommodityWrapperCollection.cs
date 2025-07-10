using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public sealed class CommodityWrapperCollection : GenericWrapperCollection<CommodityWrapper>
	{
		public CommodityWrapperCollection(CommonContainer containerBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (containerBO != null)
			{
				List<string> list = new List<string>();
				list.Add(containerBO.JC_RH_NKContainerCommodityCode);

				foreach (PackLine line in containerBO.PackLines)
				{
					list.Add(line.JL_RH_NKCommodityCode);
				}

				RefCommodityCode[] commodities = Factory.Load<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, list));
				foreach (RefCommodityCode commodity in commodities)
				{
					CommodityWrapper wrapper = new CommodityWrapper(commodity, Factory);

					if (commodity.RH_Code == containerBO.JC_RH_NKContainerCommodityCode)
					{
						containerCommodity = wrapper;
					}

					this.Add(wrapper);
				}

				Sort(CommodityWrapper.Schema.Code);
			}
		}

		public CommodityWrapperCollection(RateOneOffContainers containerBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (containerBO != null && containerBO.Parent != null)
			{
				var query = new ZQuery(RefCommodityCodeSchema.RH_Code, containerBO.Parent.TT_RH_NKCommodity);
				var commodityCode = Factory.LoadTop1<RefCommodityCode>(query);

				if (commodityCode != null)
				{
					containerCommodity = new CommodityWrapper(commodityCode, Factory);
					Add(containerCommodity);
				}
			}
		}

		internal CommodityWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String for matching data")]
		protected override IBODocDataProvider GetRow(ZString index)
		{
			if (index.EqualsIgnoringCase("container"))
			{
				return containerCommodity;
			}
			else
			{
				return base.GetRow(index);
			}
		}

		readonly CommodityWrapper containerCommodity;
	}
}
