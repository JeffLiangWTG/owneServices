using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;

namespace Enterprise.Customs.AU.Declaration.Business
{
	internal sealed class CMRCusSCAOceanBillDataEventContextReader : CusSCAOceanBillDataEventContextReader
	{
		public CMRCusSCAOceanBillDataEventContextReader(BaseCusSCAOceanBill oceanBill)
			: base(oceanBill)
		{
		}
	}
}
