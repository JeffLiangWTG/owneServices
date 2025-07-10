using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class InvoicingFormPlugin : InvoicingPluginToFreight
	{
		public InvoicingFormPlugin(IBusiness hostEntity) : base(hostEntity, false)
		{
			InitializePlugIn(hostEntity);
		}

		protected override bool AddUserControlIfRequired()
		{
			var userControl = UserControl as JobInvoicingUserControl;
			userControl.Dock = DockStyle.Fill;
			userControl.AllowOutsideOfParent();

			var controlPanel = Form.Controls.Find("MainPanel", true).FirstOrDefault() as ZPanel;
			controlPanel.Controls.Add(userControl);

			return true;
		}
	}
}
