namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7MessagesUserControl : ASYCUDA.GUI.MessagesUserControl
	{
		protected override void CustomizeLayoutCore()
		{
			MessageTextTabPage.SuspendLayout();

			BindingSource.SetBindingMember(this.MessageTextTextBox, "Messages.EM_MessageTextIndentedXml");
			MessageTextTextBox.WordWrap = false;
			MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;

			MessageTextTabPage.ResumeLayout(false);
			MessageTextTabPage.PerformLayout();
		}
	}
}
