using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.AUS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.AUS.GUI
{
	public partial class ClientAusProductImportRegistryForm : ZForm
	{
		public ClientAusProductImportRegistryForm(ClientAUSProductImportRegistry importRegistry) : base(importRegistry)
		{
			InitializeComponent();
			this.ImportRegistry = importRegistry;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			MinimumSize = Size;
		}

		public readonly ClientAUSProductImportRegistry ImportRegistry;

		public override string FormCaption
		{
			get { return "Product Import/Export Registry"; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref PostingButtonsUserControl, MainStatusBar.Top - PostingButtonsUserControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
		}

		#region Browse Directories

		void BrowseDataButton_Click(object sender, EventArgs e)
		{
			SetFolderLocationFromBrowseDialog(DirToStoreTextBox, "Browse/Select Directory to Store Files");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Default directory for Dialog form")]
		void SetFolderLocationFromBrowseDialog(ZTextBox fileTextBox, string directoryWanted)
		{
			using (ZFolderBrowserDialog folderDialog = new ZFolderBrowserDialog())
			{
				folderDialog.SelectedPath = "C:\\"; // Default directory for Dialog form
				folderDialog.ShowNewFolderButton = false;
				folderDialog.Description = directoryWanted;
				DialogResult result = folderDialog.ShowDialog();

				if (result == DialogResult.OK)
				{
					fileTextBox.Text = folderDialog.UnmappedSelectedPath;
				}
			}
		}

		void BrowseImportedDirectoryButton_Click(object sender, EventArgs e)
		{
			SetFolderLocationFromBrowseDialog(DirImportProductsTextBox, "Browse/Select Directory to for Imported Products Log");
		}

		void BrowseRejectedDirectoryButton_Click(object sender, EventArgs e)
		{
			SetFolderLocationFromBrowseDialog(DirRejectedProductsTextBox, "Browse/Select Directory to for Rejected Products Log");
		}

#endregion
	}
}
