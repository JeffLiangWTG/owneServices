using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class ParcelValidation : CusSupportingInfoValidation
	{
		public ParcelValidation(Parcel parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			if (isMailDeclarationValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CodeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (isMailDeclarationValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
				if (Parent.CSI_ReferenceNumber.Length != Parcel.Schema.MAI_ReferenceNumberMaxLength)
				{
					Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("A6652787-55FB-458B-A8A4-59EE6988B0EA", "This parcel customs number must be 14 characters long."));
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			if (isMailDeclarationValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumber2Info);
			}
		}

		public new Parcel Parent => (Parcel)base.Parent;
		bool isMailDeclarationValidationOn => Parent.Parent?.JobDeclaration?.IsMailDeclarationValidationOn ?? false;
	}
}
