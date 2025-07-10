using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	public partial class GuaranteeAccessCodesUserSelectionForm : EU.NCTS.GUI.GuaranteeAccessCodesUserSelectionForm
	{
		public GuaranteeAccessCodesUserSelectionForm(EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObjectCollection guarantees) : base(guarantees)
		{
		}

		public static void ShowForm(NctsHeader header)
		{
			var guarantees = new EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObjectCollection(header);
			guarantees.PopulateElements();
			using (var messageSendingForm = new GuaranteeAccessCodesUserSelectionForm(guarantees))
			{
				ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm);
			}
		}

		protected override void ConfirmButton_ClickCore(EU.Business.CusGuaranteeHeader guarantee)
		{
			GuaranteeAccessCodesSendingForm.ShowForm((CusGuaranteeHeader)guarantee);
			Close();
		}
	}
}
