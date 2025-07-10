using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.Module
{
	public partial class SendACDAOperationActionControl : ZUserControl
	{
		public SendACDAOperationActionControl()
		{
			InitializeComponent();

			if (!Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed)
			{
				SendWithErrorsRadioButton.Enabled = false;
			}
		}
	}
}
