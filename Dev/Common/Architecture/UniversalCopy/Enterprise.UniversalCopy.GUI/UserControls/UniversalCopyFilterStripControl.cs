using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.UniversalCopy.GUI
{
	partial class UniversalCopyFilterStripControl : StripControl
	{
		public UniversalCopyFilterStripControl(FilterStripBusinessObject filterStrip)
			: base(filterStrip)
		{
			InitializeComponent();

			ToolStripFindDropButton.Visible = false;
			ToolStripSaveLayoutButton.Visible = false;
			ToolStripManageDropButton.Visible = false;
			ToolStripColourPicker.Visible = false;
			ToolStripBackColourButton.Visible = false;
			ToolStripForeColourButton.Visible = false;
			FilterStripsPanel.Dock = DockStyle.Fill;
		}

		internal void ForceRebuildFilterStrips()
		{
			InitializeStrips();
			ResetFilterStrips();
			if (FilterBusinessObject.FilterStrips.Count > 0)
			{
				RebuildFilterStrips();
				AlreadyLoaded = true;
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			this.GetFrontMostActiveControl();
			bool result;

			switch (keyData)
			{
				case Keys.Control | Keys.E:
					AddStrip();
					result = true;
					break;

				default:
					result = base.ProcessDialogKey(keyData);
					break;
			}

			return result;
		}
	}
}
