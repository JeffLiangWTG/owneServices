using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Customs.AU.Declaration.GUI.Res;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUCOLSDetailsUserControl : ZUserControl
	{
		public AUCOLSDetailsUserControl()
		{
			InitializeComponent();
		}

		void DirectionsResetButton_Click(object sender, EventArgs e)
		{
			if (DataSource is QuarantineColsHeader colsHeader)
			{
				var promptResult = Globals.Message.Show(
					Res.GetString("27AC4198-6BA4-482F-B3BE-4F64067C0515", "This will delete all Directions in the grid. Do you want to continue?"),
					"Reset Directions", MessageBoxButtons.YesNo, DialogResult.No);
				if (promptResult == DialogResult.Yes)
				{
					colsHeader.Directions.RemoveAndDeleteAll();
				}
			}
		}
	}
}
