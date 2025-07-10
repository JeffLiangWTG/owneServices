using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class CreateOrAttachLoadListForm : ZChildForm
	{
		public CreateOrAttachLoadListForm(CFSLoadListConsol consol) : base(consol)
		{
			this.Consol = consol;
			zGrid1.GridId = "GridLayout1gjPwJa8I1VxBav3CF7EiQ==";
			zGrid2.GridId = "GridLayoutZi+B58dWA+Vzji5gJ3Kw/w==";
		}

		public readonly CFSLoadListConsol Consol;

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

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateControlButtons();
		}

		public override string FormVerb
		{
			get
			{
				return ZString.Empty;
			}
		}

		internal void BackButton_Click(object sender, EventArgs e)
		{
			if (mainTabControl.SelectedIndex > 0)
			{
				mainTabControl.SelectedIndex--;
			}
			UpdateControlButtons();
		}

		internal void NextButton_Click(object sender, EventArgs e)
		{
			if (mainTabControl.SelectedIndex < mainTabControl.TabPages.Count - 1)
			{
				mainTabControl.SelectedIndex++;
			}
			else if (mainTabControl.SelectedIndex == mainTabControl.TabPages.Count - 1)
			{
				Consol.RunPreSaveValidation();
				if (!Consol.HasErrors)
				{
					DialogResult = DialogResult.OK;
					Close();
					return;
				}
				else
				{
					Globals.Message.ShowError("Please correct all errors before finishing this wizard.");
				}
			}
			UpdateControlButtons();
		}

		void UpdateControlButtons()
		{
			backButton.Enabled = mainTabControl.SelectedIndex != 0;
			if (mainTabControl.SelectedIndex == (mainTabControl.TabPages.Count - 1))
			{
				nextButton.Text = "Finish";
			}
			else
			{
				nextButton.Text = "&Next >";
			}
			if (Consol != null)
			{
				Consol.RunPreSaveValidation();
			}
		}

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateControlButtons();
		}
	}
}
