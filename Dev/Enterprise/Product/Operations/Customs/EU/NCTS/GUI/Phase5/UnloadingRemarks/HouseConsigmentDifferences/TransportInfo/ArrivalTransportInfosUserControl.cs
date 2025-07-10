using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	internal partial class ArrivalTransportInfosUserControl : ZUserControl
	{
		protected new NctsHeader DataSource => (NctsHeader)base.DataSource;
		internal ZUserControl ArrivalTransportInfosGridUserControl { get; private set; }

		public ArrivalTransportInfosUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			InitializeTransportInfoGrid();
		}

		void InitializeTransportInfoGrid()
		{
			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			var arrivalTransportInfoGridType = provider.ArrivalTransportInfoGridType;
			ArrivalTransportInfosGridUserControl = (ZUserControl)Activator.CreateInstance(arrivalTransportInfoGridType);
			TransportInfoGroupBox.Controls.Add(ArrivalTransportInfosGridUserControl);
			BindingSource.SetBindingMember(ArrivalTransportInfosGridUserControl, ".");
			ArrivalTransportInfosGridUserControl.Dock = DockStyle.Fill;
		}
	}
}
