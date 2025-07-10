using System;
using System.Windows.Forms;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ArchiveManager.GUI.Records
{
	public partial class VolumeSelectionForm : ZChildForm
	{
		public VolumeSelectionForm()
		{
			InitializeComponent();
		}

		public VolumeSelectionForm(VolumeSelection volumeSelection)
			: base(volumeSelection)
		{
			InitializeComponent();
		}

		public override string FormHeading
			=> Res.GetString("3ba5f062-36c9-4346-b317-852bdb5253b9", "Restore Offline Record");

		VolumeSelection VolumeSelectionEntity
			=> (VolumeSelection)BusinessEntity;

		void selectButton_Click(object sender, EventArgs e)
		{
			ValidateAll(CargoWise.EntityFramework.ValidationType.Full);
			if (!VolumeSelectionEntity.HasErrors)
			{
				VolumeSelectionEntity.VolumeManager.LoadVolume(VolumeSelectionEntity.VolumeNoToFind, VolumeState.Closed);
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void folderSelectButton_Click(object sender, EventArgs e)
		{
			folderBrowserDialog.RequireMappablePath = true;
			var result = folderBrowserDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				try
				{
					VolumeSelectionEntity.VolumeLocation = folderBrowserDialog.MappedSelectedPath;
				}
				catch (NotSupportedException)
				{
					VolumeSelectionEntity.VolumeLocation = Res.GetString("261cf085-7b17-47ca-b811-1050903894ae", "Invalid Folder Selected.");
				}
			}
		}
	}
}
