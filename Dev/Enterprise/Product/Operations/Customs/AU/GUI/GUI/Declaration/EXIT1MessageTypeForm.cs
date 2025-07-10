using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.Declaration.GUI
{
	/// <summary>
	/// Summary description for ReplacementTypeForEXIT1.
	/// </summary>
	public partial class EXIT1MessageTypeForm : ZChildForm
	{
		public EXIT1MessageTypeForm(EXIT1MessageType messageType) : base(messageType)
		{
		}

		public override string FormCaption
		{
			get { return "Message type for EXIT 1"; }
		}

		private void OKBoundButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void CancelBoundButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
