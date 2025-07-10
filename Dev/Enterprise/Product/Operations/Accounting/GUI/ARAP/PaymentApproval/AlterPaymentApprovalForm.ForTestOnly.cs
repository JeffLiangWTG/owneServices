#if DEBUG

using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class AlterPaymentApprovalForm
	{
		public void ZForm_Closing_ForTestOnly(object sender, CancelEventArgs e)
		{
			ZForm_Closing(sender, e);
		}

		public ContinueWithSave ValidateAndSave_ForTestOnly()
		{
			return ValidateAndSave();
		}
	}
}

#endif
