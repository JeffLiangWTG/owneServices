using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusEntryLineFee : EU.Business.Declaration.CusEntryLineFee, Integration.Customs.DE.ICusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override IFeeRounder GetNewChargeAmountRounder() => new TwoDigitsChargeAmountRounder();
	}
}
