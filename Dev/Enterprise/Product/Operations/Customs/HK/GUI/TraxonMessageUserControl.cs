using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.Customs.HK.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.HK.GUI
{
	public partial class TraxonMessageUserControl : ZUserControl
	{
		public TraxonMessageUserControl()
		{
			InitializeComponent();
		}

		void Traxon_MessageStatusBoundTextBox4_UpdateColor(object sender, EventArgs e)
		{
			UpdateMessageStatusBoundTextBox4Color();
		}

		void UpdateMessageStatusBoundTextBox4Color()
		{
			ZString status = Traxon_MessageStatusBoundTextBox.Text;
			if (status.Left(12) == "Acknowledged")
			{
				Traxon_MessageStatusBoundTextBox.BackColor = Color.ForestGreen;
				Traxon_MessageStatusBoundTextBox.ForeColor = Color.White;
			}
			else if (status == TraxonConsolStatus.NoMessagesSent)
			{
				Traxon_MessageStatusBoundTextBox.BackColor = SystemColors.Control;
				Traxon_MessageStatusBoundTextBox.ForeColor = SystemColors.WindowText;
			}
			else
			{
				Traxon_MessageStatusBoundTextBox.BackColor = Color.LightCoral;
				Traxon_MessageStatusBoundTextBox.ForeColor = Color.White;
			}
		}
	}
}
