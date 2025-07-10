using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class AlertForm : ZChildForm
	{
		public AlertForm()
		{
		}

		public AlertForm(Alert nonPersistentBusinessObject)
			: base(nonPersistentBusinessObject)
		{
		}

		public Alert Alert
		{
			get { return (Alert)BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return "Alerts"; }
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
