using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class CusEntryHeaderCharges : Customs.Business.CusEntryHeaderCharges, Integration.Customs.KR.ICusEntryHeaderCharges
	{
		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		protected override Customs.Business.CusEntryHeaderChargesValidation GetNewValidation() => new CusEntryHeaderChargesValidation(this);
		protected override bool ShouldResetDataOnMergingCore => C1_RateOverrideReasonCode.IsEmpty;
		protected override void CheckChargeTypeUniqueness(ZString value) { }
	}
}
