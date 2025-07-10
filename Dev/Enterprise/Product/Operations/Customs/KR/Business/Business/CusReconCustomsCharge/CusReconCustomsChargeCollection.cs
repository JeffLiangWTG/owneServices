using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconCustomsChargeCollection : ActiveBusinessObjectCollection<CusReconCustomsCharge>
	{
		public CusReconCustomsChargeCollection(CusReconEntryLine entryLine)
		: base(entryLine.Factory, entryLine, new ZQuery(), CusReconCustomsChargeSchema.CRC_CRL_Line)
		{
		}
	}
}
