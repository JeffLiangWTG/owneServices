using System;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class AcceptabilityBandFiltersControl : ZUserControl
	{
		public AcceptabilityBandFiltersControl()
		{
			InitializeComponent();

			SQLGroupBox.AllowOverlap(SupersetFilterStripsGroupBox);
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			if (DataSource != null)
			{
				BAB_TypeInfo_ValueChanged();
				DataSource.BAB_TypeInfo.ValueChanged += BAB_TypeInfo_ValueChanged;
			}
		}

		public new BMComponentAcceptabilityBand DataSource
		{
			get { return (BMComponentAcceptabilityBand)BindingSource.Current; }
		}

		void BAB_TypeInfo_ValueChanged(object sender = null, EventArgs e = null)
		{
			filterCustomisationControl.Visible = !DataSource.IsFilterStripsDisabled;
			SQLTextBox.Visible = !DataSource.IsSqlDisabled;
			SQLGroupBox.Visible = !DataSource.IsPercentageBand;
			supersetFilterCustomisationControl.Visible = !SQLGroupBox.Visible;
		}
	}
}
