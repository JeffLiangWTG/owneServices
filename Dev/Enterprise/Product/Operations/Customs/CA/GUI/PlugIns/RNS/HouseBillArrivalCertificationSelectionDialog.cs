using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.PlugIns
{
	public partial class HouseBillArrivalCertificationSelectionDialog : ZChildForm
	{
		public HouseBillArrivalCertificationSelectionDialog(RNSRequestBOCollection rnsRequestBos)
			: base(rnsRequestBos)
		{
		}

		RNSRequestBOCollection RNSRequestBOs
		{
			get { return (RNSRequestBOCollection)BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return Res.GetString("1031615B-820B-45C1-8E82-4F5618B3E5E7", "Sending Messages"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			var selectedRNSRequestBOs = RNSRequestBOs.GetSelectedRequestBOs();
			if (selectedRNSRequestBOs.Any())
			{
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				Globals.Message.ShowInformation("Please select the required RNS Request information from the grid first, in order to proceed.", "Select RNS Request Information");
			}
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SelectAll_Button_Click(object sender, EventArgs e)
		{
			RNSRequestBOs.SelectAll();
		}

		void DeselectAll_Button_Click(object sender, EventArgs e)
		{
			RNSRequestBOs.DeselectAll();
		}
	}
}
