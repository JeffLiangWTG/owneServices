using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Import5FNMessageSendingFormBuilder : ImportOriginalMessageSendingFormBuilder
	{
		public override ZTextBoxColumnStyleInfo GetSendColumnStyle() => null;
		public override ZUserControl GetUserControl() => new Import5FNMessageEntryLinesUserControl();
		public override ResourceStringData GetUserControlGroupBoxCaption() => Res.GetData("373A5766-62D3-496B-B384-512910A108CA", "Entry Lines");

		public override ZTabPage[] GetAdditionalTabPages(ZGrid messageSendingObjectsGrid)
		{
			var userControl = new Import5FNMessageDetailsUserControl();
			userControl.Dock = System.Windows.Forms.DockStyle.Fill;

			var tabPage = new ZTabPage();
			tabPage.CaptionResourceString = Res.GetData("6060101E-2860-4A7F-9140-77549B1F676D", "Details");
			tabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			tabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 350, true);
			tabPage.Controls.Add(userControl);

			return new ZTabPage[] { tabPage };
		}

		public override void ChangeValidationErrorBindingIfNeeded(KBindingSource bindingSource, ZTextBox validationErrorsTextBox)
		{
			bindingSource.SetBindingMember(validationErrorsTextBox, nameof(JobDeclarationMiscMessageSendingObjectParent.SendingObjectsCollection) + "." + nameof(JobDeclarationMiscMessageSendingObject.MessageSendingEntryLines) + "." + nameof(MessageSendingEntryLineObject.BizObjValidationMessageErrors));
		}

		public override int[] GetFormSize() => new int[] { 900, 725 };
		public override int Panel1MinSize => 240;
		public override int Panel2MinSize => 330;
	}
}
