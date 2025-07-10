using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MXMessageChooserValidation : MessageChooserValidation
	{
		public MXMessageChooserValidation(MXMessageChooser parent) : base(parent)
		{
		}

		MXMessageChooser Chooser => (MXMessageChooser)Parent;

		public void ValidateReason()
		{
			ValidateCalculatedProperty(Chooser.ReasonInfo);
		}

		protected void CheckReason()
		{
			var chooser = Chooser;

			ListValidation.MessageErrorIfInvalidCode(chooser.ReasonInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateReason();
		}
	}
}
