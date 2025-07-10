using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsUnloadingMovementHeader : EU.NCTS.Business.NctsUnloadingMovementHeader,
		Integration.Customs.ES.IUnloadingMovementHeader
	{
		public NctsUnloadingMovementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new EU.NCTS.Business.INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc> GoodsItems => (EU.NCTS.Business.INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>)base.GoodsItems;

		protected override EU.NCTS.Business.INctsCommonCargoDescCollection<EU.NCTS.Business.NctsCommonCargoDesc> CreateGoodsItems() => new EU.NCTS.Business.NctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>(this);

		protected override Type CusInBondCargoDescTypeCore => typeof(NctsArrivalAndUnloadingCargoDesc);
	}
}
