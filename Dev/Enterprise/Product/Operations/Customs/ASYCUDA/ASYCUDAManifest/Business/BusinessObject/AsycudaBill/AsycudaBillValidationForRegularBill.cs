using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDAManifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSADOfficeCode();
		}

		public void ValidateSADOfficeCode()
		{
			ValidateCalculatedProperty(Parent.SADOfficeCodeInfo);
		}

		protected void CheckSADOfficeCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.SADOfficeCodeInfo);
		}

		protected override void CheckCustomsEntryNumberType()
		{
			if (Parent.CusEntryNumber != null && Parent.CusEntryNumber.CE_EntryType == ASYCUDA.Business.Constants.CustomsEntryType.SAD)
			{
				return;
			}
			base.CheckCustomsEntryNumberType();
		}
	}
}
