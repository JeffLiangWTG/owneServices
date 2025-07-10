using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class CLMessageChooserValidation : MessageChooserValidation
	{
		public CLMessageChooserValidation(CLMessageChooser parent) : base(parent)
		{
		}
		CLMessageChooser Chooser => (CLMessageChooser)Parent;

		public void ValidateReason()
		{
			ValidateCalculatedProperty(Chooser.ReasonInfo);
		}

		protected void CheckReason()
		{
			var chooser = Chooser;
			MandatoryValidation.MessageErrorIfNotEntered(chooser.ReasonInfo);
		}

		public void ValidateAmendReason()
		{
			ValidateCalculatedProperty(Chooser.AmendReasonInfo);
		}

		protected void CheckAmendReason()
		{
			var chooser = Chooser;
			if (Chooser.IsAmendment)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(chooser.AmendReasonInfo);
			}
		}

		public void ValidateAmendType()
		{
			ValidateCalculatedProperty(Chooser.AmendTypeInfo);
		}

		protected void CheckAmendType()
		{
			var chooser = Chooser;
			if (Chooser.IsAmendment)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(chooser.AmendTypeInfo);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateReason();
			ValidateAmendReason();
			ValidateAmendType();
		}
	}
}
