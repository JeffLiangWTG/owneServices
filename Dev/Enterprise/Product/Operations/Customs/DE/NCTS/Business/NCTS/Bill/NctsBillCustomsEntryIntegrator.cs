using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.DE.NCTS.Business
{
	sealed class NctsBillCustomsEntryIntegrator : EU.NCTS.Business.NctsBillCustomsEntryIntegrator
	{
		public NctsBillCustomsEntryIntegrator(NctsBill houseConsignment) : base(houseConsignment)
		{
		}

		protected override void CopyCustomsSecondQuantity(CusEntryLine entryLine, EU.NCTS.Business.NctsDepartureCargoDesc goodsItem)
		{
		}
	}
}
