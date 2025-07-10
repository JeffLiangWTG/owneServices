using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public class NctsDepartureCargoDescWrapperCollection : DocBaseWrapperCollection<NctsDepartureCargoDescWrapper>
	{
		public NctsDepartureCargoDescWrapperCollection(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(factory)
		{
			AddGoodsItems(nctsHeader.DepartureGoodsItems);
		}

		public NctsDepartureCargoDescWrapperCollection(NctsBill nctsBill, BusinessObjectFactory factory) : base(factory)
		{
			AddGoodsItems(nctsBill?.GoodsItems);
		}

		void AddGoodsItems(IEnumerable<NctsDepartureCargoDesc> goodsItems)
		{
			if (goodsItems != null)
			{
				foreach (var goodsItem in goodsItems)
				{
					Add(NctsDepartureCargoDescWrapper.New(goodsItem));
				}
			}
		}
	}
}
