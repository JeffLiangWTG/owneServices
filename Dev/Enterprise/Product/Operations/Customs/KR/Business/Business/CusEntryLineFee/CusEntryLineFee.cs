using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class CusEntryLineFee : Customs.Business.CusEntryLineFee, Integration.Customs.KR.ICusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool ShouldResetDataOnMergingCore => CF_RateOverrideReasonCode.IsEmpty;
	}
}
