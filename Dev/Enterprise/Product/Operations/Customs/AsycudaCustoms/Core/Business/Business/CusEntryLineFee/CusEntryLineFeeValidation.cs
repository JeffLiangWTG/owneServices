using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusEntryLineFeeValidation : Customs.Business.CusEntryLineFeeValidation
	{
		public CusEntryLineFeeValidation(AutoCusEntryLineFee parent) : base(parent)
		{
		}
		protected new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

		protected override void CheckCF_ChargeType()
		{
			base.CheckCF_ChargeType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CF_ChargeTypeInfo);
		}
	}
}
