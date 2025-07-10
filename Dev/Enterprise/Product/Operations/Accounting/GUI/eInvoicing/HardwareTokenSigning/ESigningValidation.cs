using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning
{
	public class ESigningValidation : ZValidation
	{
		public ESigningValidation(ESigningBusinessObject parent) : base(parent)
		{
			this.Parent = parent;
		}

		public override Type AutoValidationType => GetType();

		public void ValidateChipsetType()
		{
			Parent.ChipsetTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.ChipsetTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ChipsetTypeInfo);
		}

		public void ValidateCertificateCode()
		{
			Parent.CertificateCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.CertificateCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CertificateCodeInfo);
		}

		public void ValidateEnteredPin()
		{
			if (!Parent.IsPinRequired)
			{
				return;
			}
			Parent.EnteredPinInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.EnteredPinInfo);
			if (Parent.EnteredPin.Length < 4)
			{
				Parent.EnteredPinInfo.AddError(Res.GetString("768d7e30-8faa-410f-ad0b-bda20914b653", "Pin should have minimum length of 4 digits."));
			}
		}

		public override void ValidateAll()
		{
			ValidateChipsetType();
			ValidateCertificateCode();
			ValidateEnteredPin();
		}

		readonly ESigningBusinessObject Parent;
	}
}
