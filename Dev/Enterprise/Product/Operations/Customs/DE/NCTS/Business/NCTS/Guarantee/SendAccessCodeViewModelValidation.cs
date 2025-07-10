using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class SendAccessCodeViewModelValidation : ZValidation
	{
		public SendAccessCodeViewModelValidation(SendAccessCodeViewModel parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly SendAccessCodeViewModel parent;

		public override void ValidateAll()
		{
			ValidateOfficeOfGuarantee();
			ValidateNewMainAccessCode();
		}

		public override Type AutoValidationType => typeof(SendAccessCodeViewModelValidation);

		public void ValidateOfficeOfGuarantee()
		{
			ValidateCalculatedProperty(parent.OfficeOfGuaranteeInfo);
		}

		public void ValidateNewMainAccessCode()
		{
			ValidateCalculatedProperty(parent.NewMainAccessCodeInfo);
		}

		protected void CheckOfficeOfGuarantee()
		{
			MandatoryValidation.CheckEntered(parent.OfficeOfGuaranteeInfo);
			ListValidation.ErrorIfInvalidCode(parent.OfficeOfGuaranteeInfo);
		}

		protected void CheckNewMainAccessCode()
		{
			var length = parent.NewMainAccessCode.Length;

			if (length != 0 && length != 4)
			{
				parent.NewMainAccessCodeInfo.AddError(Res.GetString("21674b77-cf36-4aa9-b26b-ed30d2f0168f", "The new Main Access Code must have 4 digits."));
			}
		}
	}
}
