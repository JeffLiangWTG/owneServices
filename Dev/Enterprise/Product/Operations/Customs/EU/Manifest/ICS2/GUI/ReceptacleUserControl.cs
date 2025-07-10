using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public partial class ReceptacleUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public ReceptacleUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		protected void ReceptaclesEditButton_Click(object sender, System.EventArgs e)
		{
			var manifestHeader = (AsycudaManifestHeader)CurrentDataItem;
			if (manifestHeader != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new ReceptacleForm(manifestHeader));
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		Control IExtendedControl.Host => this;

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; }

		string IResourceStringBindingMember.ResourceStringBindingMember => nameof(AsycudaManifestHeader.ReceptacleId);

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			Extensions.Dispose();
			base.Dispose(disposing);
		}
	}
}
