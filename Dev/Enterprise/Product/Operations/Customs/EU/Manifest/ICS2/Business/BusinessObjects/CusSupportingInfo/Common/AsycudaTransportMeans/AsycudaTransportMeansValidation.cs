using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaTransportMeansValidation : CusTransportMeansValidation
	{
		public AsycudaTransportMeansValidation(CusTransportMeans parent) : base(parent)
		{
		}

		protected override void CheckTPM_IdentificationNumber()
		{
			base.CheckTPM_IdentificationNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TPM_IdentificationNumberInfo);
		}

		protected override void CheckTPM_TypeOfIdentification()
		{
			base.CheckTPM_TypeOfIdentification();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TPM_TypeOfIdentificationInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.TPM_TypeOfIdentificationInfo);
		}

		protected override void CheckTPM_TypeOfTransportMeans()
		{
			base.CheckTPM_TypeOfTransportMeans();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TPM_TypeOfTransportMeansInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.TPM_TypeOfTransportMeansInfo);
		}

		protected override void CheckTPM_RN_NKTransportNationality()
		{
			var specificCircumstanceIndicator = Pack?.Bill?.Header?.SpecificCircumstanceIndicator ?? Bill?.Header?.SpecificCircumstanceIndicator;

			if (!specificCircumstanceIndicator.GetValueOrDefault().EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F51))
			{
				base.CheckTPM_RN_NKTransportNationality();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TPM_RN_NKTransportNationalityInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.TPM_RN_NKTransportNationalityInfo);
		}

		protected AsycudaPack Pack => (Parent as AsycudaTransportMeans)?.Pack;

		protected AsycudaBill Bill => (Parent as AsycudaTransportMeans)?.Bill;
	}
}
