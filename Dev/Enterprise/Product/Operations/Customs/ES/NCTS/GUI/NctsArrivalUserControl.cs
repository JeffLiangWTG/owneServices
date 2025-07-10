using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class NctsArrivalUserControl : EU.NCTS.GUI.NctsArrivalUserControl, IExtendedControl
	{
		public NctsArrivalUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			UnhookEvents();

			base.OnCurrentDataItemChanged(e);

			HookEvents();
			var header = (NctsHeader)CurrentDataItem;
			if (header != null)
			{
				ShowParentUnloadingTab();
			}
		}

		#region Hook / Unhook Events

		void HookEvents()
		{
			var header = (NctsHeader)CurrentDataItem;
			if (header != null)
			{
				header.CombinedMessageInfo.ValueChanged += CombinedMessageInfo_ValueChanged;
			}
		}

		void UnhookEvents()
		{
			var header = (NctsHeader)CurrentDataItem;
			if (header != null)
			{
				header.CombinedMessageInfo.ValueChanged -= CombinedMessageInfo_ValueChanged;
			}
		}

		#endregion

		void CombinedMessageInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowParentUnloadingTab();
		}

		void ShowParentUnloadingTab()
		{
			if (ParentForm is NctsMovementForm nctsParentForm)
			{
				nctsParentForm.ShowUnloadingTab(false);
			}
			else if (ParentForm is ShipmentForm shipmentForm)
			{
				((NctsUserControlForPlugin)shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.EU.NctsMovementController).UserControl).ShowUnloadingTab(false);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			header = dataSource as NctsHeader;
			if (header != null)
			{
				header.ESNctsHeader.CEN_TIRArrivalInfo.ValueChanged -= CEN_TIRArrivalInfo_ValueChanged;
				header.ESNctsHeader.CEN_TIRArrivalInfo.ValueChanged += CEN_TIRArrivalInfo_ValueChanged;
				CEN_TIRArrivalInfo_ValueChanged(null, EventArgs.Empty);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (header != null)
			{
				header.ESNctsHeader.CEN_TIRArrivalInfo.ValueChanged -= CEN_TIRArrivalInfo_ValueChanged;
			}
			if (disposing && components != null)
			{
				components.Dispose();
			}
			extensions?.Dispose();
			base.Dispose(disposing);
		}

		void CEN_TIRArrivalInfo_ValueChanged(object sender, EventArgs e)
		{
			if (header != null && header.ESNctsHeader.CEN_TIRArrival)
			{
				TIRPartialUnloadingCheckBox.Visible = true;
				TIRCarnetPageIntEdit.Visible = true;
			}
			else
			{
				TIRPartialUnloadingCheckBox.Visible = false;
				TIRCarnetPageIntEdit.Visible = false;
			}
		}
		NctsHeader header;

		#region IExtendedControl Implementation

		Control IExtendedControl.Host => this;

		IControlExtensionCollection IExtendedControl.Extensions => extensions ?? (extensions = new ControlExtensionCollection(this) { new ReadOnlyCacheExtension(new string[] { nameof(CertificateDropEdit), nameof(BrokerFindBox) }) });
		IControlExtensionCollection extensions;

		#endregion
	}
}
