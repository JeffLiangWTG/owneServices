using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public partial class FinalDestinationUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public FinalDestinationUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		public string ResourceStringBindingMember => nameof(AsycudaBill.ABL_RL_NKFinalDestination);

		AsycudaBill Bill => (AsycudaBill)base.CurrentDataItem;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is AsycudaManifestHeader header)
			{
				header.AMA_TransportModeInfo.ValueChanged -= AMA_TransportModeInfo_ValueChanged;
			}
			else
			{
				header = null;
			}
			base.SetDataBinding(dataSource, dataMember);
			if (header != null)
			{
				header.AMA_TransportModeInfo.ValueChanged -= AMA_TransportModeInfo_ValueChanged;
				header.AMA_TransportModeInfo.ValueChanged += AMA_TransportModeInfo_ValueChanged;
			}
			AMA_TransportModeInfo_ValueChanged(null, null);
		}

		void AMA_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			FinalDestinationTextBox.Visible = Bill?.IsAir ?? false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
				components?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
