using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class SuspensionDrawbackValidation : Customs.Business.CusSupportingInfoValidation
	{
		public SuspensionDrawbackValidation(SuspensionDrawback parent) : base(parent)
		{
		}

		public new SuspensionDrawback Parent => (SuspensionDrawback)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			if (!Parent.CSI_ReferenceNumber2.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			}

			if (!Parent.CSI_ReferenceNumber.IsEmpty && !CNPJValidator.ValidateCNPJ(Parent.CSI_ReferenceNumber))
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("77E0545F-60E4-4317-A859-65D132F2620A", "The entered CNPJ is not valid."));
			}
		}
	}
}
