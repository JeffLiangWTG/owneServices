using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoDepotOutturnUserControl : ZUserControl
	{
		public SeaCargoDepotOutturnUserControl()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!doneFirstBind)
			{
				outturnMessagesControl.SetBindPrepend("Outturns.");
				doneFirstBind = true;
			}
			base.SetDataBinding(dataSource, dataMember);
		}
		bool doneFirstBind;

		#region Nil Outturn

		void NilOutturnButton_Click(object sender, EventArgs e)
		{
			CusOutturnHeader header = CurrentDataItem as CusOutturnHeader;
			if (header != null && (Globals.Message.Show("Are you sure you want to perform a nil outturn?", "Nil Outturn", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes))
			{
				header.NilOutturn();
			}
		}

		#endregion
	}
}
