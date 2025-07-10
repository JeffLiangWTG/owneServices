using CargoWise.Windows.UI;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class Question5To7GroupBoxUserControl : ZUserControl
	{
		public Question5To7GroupBoxUserControl()
		{
			InitializeComponent();
		}

		public new JobComInvoiceHeader CurrentDataItem => base.CurrentDataItem as JobComInvoiceHeader;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			UpdateCaptions();
		}

		void UpdateCaptions()
		{
			if (CurrentDataItem != null)
			{
				if (CurrentDataItem.JobDeclaration.JE_MessageType == KRJobMessageTypeList.Codes.ValuationDeclaration)
				{
					Question5GroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(200);

					Question5GroupBox.CaptionResourceString = Res.GetData("2ba8a648-ba32-4277-a3cd-be99254dfdc6", "Question 5");
					Question6GroupBox.CaptionResourceString = Res.GetData("ae7ef6dd-f95b-4018-9988-ee2666ee019f", "Question 6");
					Question7GroupBox.CaptionResourceString = Res.GetData("c8d18906-e50a-461f-90c6-27308570932d", "Question 7");
				}
				else
				{
					Question5GroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(180);

					Question5GroupBox.CaptionResourceString = Res.GetData("E0CF992A-FE04-4DFB-9CED-28C9E3330F5F", "Question 7");
					Question6GroupBox.CaptionResourceString = Res.GetData("1DE3FBB5-7233-4935-8057-BC7C1B2CC035", "Question 8");
					Question7GroupBox.CaptionResourceString = Res.GetData("C488B6FC-57E1-4CAA-B812-013B9B530037", "Question 9");
				}
			}
		}
		public void ChangeBindingTo5SM()
		{
			Question5Panel.UpdateLayout(new Question5Layout());
			Question6Panel.UpdateLayout(new Question6Layout());
			Question7Panel.UpdateLayout(new Question7Layout());
		}
		public void ChangeBindingTo934()
		{
			Question5Panel.UpdateLayout(new ValuationDeclarationQuestion7Layout());
			Question6Panel.UpdateLayout(new ValuationDeclarationQuestion8Layout());
			Question7Panel.UpdateLayout(new ValuationDeclarationQuestion9Layout());
		}
		public void ChangeBindingToMessageSendingObject()
		{
			Question5Panel.UpdateLayout(new Question7MessageSendingLayout());
			Question6Panel.UpdateLayout(new Question8MessageSendingLayout());
			Question7Panel.UpdateLayout(new Question9MessageSendingLayout());
		}
	}
}
