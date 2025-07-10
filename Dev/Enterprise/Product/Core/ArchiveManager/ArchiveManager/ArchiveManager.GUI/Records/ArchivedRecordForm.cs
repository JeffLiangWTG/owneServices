using System;
using System.Windows.Forms;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ArchiveManager.GUI.Records
{
	public partial class ArchivedRecordForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ArchivedRecordForm()
		{
			InitializeComponent();
			MissingResourceStringChecker.ExcludeFromTest(offlineLabel);
		}

		public ArchivedRecordForm(ArchiveStorageMain bo)
			: base(bo)
		{
			InitializeComponent();

			previewDisplayControl = new GraphicalDisplayControl(true);
			previewPanel.Controls.Add(previewDisplayControl);

			storageDocsGrid.AfterBind += new EventHandler(storageDocsGrid_AfterBind);

			storageDocsGrid.RebuildContextMenu();

			DisplayMode = ODisplayMode.ReadOnly;
			MissingResourceStringChecker.ExcludeFromTest(offlineLabel);
		}

		public override string FormCaption
			=> ((ArchiveStorageMain)BusinessEntity).MainReference;

		ArchiveStorageMain ArchiveStorageMainEntity
			=> (ArchiveStorageMain)BusinessEntity;

		void storageDocsGrid_AfterBind(object sender, EventArgs e)
			=> storageDocsGrid.ListManager.CurrentChanged += new EventHandler(SelectedStorageDocChanged);

		void SelectedStorageDocChanged(object sender, EventArgs e)
			=> UpdatePreviewDisplayControl();

		[ThreadStatic]
		static string lastLocationOfArchiveVolume;

		public static void SetLastLocationOfArchiveVolumeForTest(string lastlocation)
			=> lastLocationOfArchiveVolume = lastlocation;

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (ArchiveStorageMainEntity.SM_OffLine.IsValid && !ArchiveStorageMainEntity.SM_OffLine.IsEmpty)
			{
				ArchiveVolume volume = null;
				var volumeSelection = new VolumeSelection(ArchiveStorageMainEntity.SM_CD1);

				if (!string.IsNullOrEmpty(lastLocationOfArchiveVolume))
				{
					volumeSelection.VolumeLocation = lastLocationOfArchiveVolume;
					if (volumeSelection.VolumeManager.VolumeExists(volumeSelection.VolumeNoToFind, VolumeState.Closed))
					{
						volumeSelection.VolumeManager.LoadVolume(volumeSelection.VolumeNoToFind, VolumeState.Closed);
						volume = volumeSelection.VolumeManager.CurrentVolume;
					}
				}

				try
				{
					if (volume != null)
					{
						ArchiveStorageMainEntity.RestoreFrom(volume);
						UpdatePreviewDisplayControl();
						UpdateOfflineLabel(volume.VolumePath);
					}
					else
					{
						var result = ZFormModaliser.ShowDialogAndDispose(new VolumeSelectionForm(volumeSelection));
						if (result == DialogResult.OK)
						{
							ArchiveStorageMainEntity.RestoreFrom(volumeSelection.VolumeManager.CurrentVolume);
							UpdatePreviewDisplayControl();
							UpdateOfflineLabel(volumeSelection.VolumeManager.CurrentVolume.VolumePath);

							if (lastLocationOfArchiveVolume != volumeSelection.VolumeLocation)
							{
								lastLocationOfArchiveVolume = volumeSelection.VolumeLocation;
							}
						}
						else
						{
							Close();
						}
					}
				}
				catch (InvalidOperationException)
				{
					Globals.Message.ShowError(Res.GetString("671856A0-FE3F-4EAF-924D-D2EC2AC3B950", "Unable to find the file in this volume.\r\nPlease specify the correct volume."));
				}
			}
		}

		void UpdateOfflineLabel(string volumePath)
		{
			offlineLabel.Text = Res.GetString("a5cd6961-c1f0-481d-8fbd-9da01a936a1e", "Offline images temporarily restored from file: {0}", volumePath);
			offlineLabel.ForeColor = System.Drawing.Color.Red;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdatePreviewDisplayControl();
			BusinessEntity.SuspendValidation();
		}

#if DEBUG
		internal
#endif
		readonly GraphicalDisplayControl previewDisplayControl;
#if DEBUG
		public
#endif
		ImageManager previewPaneImageManager;

		void UpdatePreviewDisplayControl()
		{
			string filename = null;
			previewPanel.SuspendLayout();
			try
			{
				var selectedItem = storageDocsGrid.CurrentElement;

				previewPaneImageManager?.Dispose();

				if (selectedItem == null)
				{
					previewDisplayControl.Close();
				}
				else
				{
					filename = selectedItem.SaveToTempFile();
					if (!PreviewableDocumentHelper.IsSupported(selectedItem.SC_DataType))
					{
						var badLoadReason = Res.GetString("F0354985-DECE-4C40-9866-F77E360DF39A", "This document format is not supported for previewing.");
						var imageFile = new DummyPreviewable(badLoadReason);
						previewPaneImageManager = new ImageManager(filename, imageFile, selectedItem, previewDisplayControl);
					}
					else
					{
						previewPaneImageManager = new ImageManager(filename, selectedItem, previewDisplayControl);
					}

					previewPaneImageManager.ImageDialogClose += new EventHandler(Manager_ImageDialogClose);
					previewPaneImageManager.Tag = selectedItem.PK;

					previewDisplayControl.Document = selectedItem;
				}
			}
			catch (Exception ex) when (ex is CorruptedDocumentException || ex is ExternalStorageException)
			{
				TempFile.Delete(filename, reportException: false);

				previewDisplayControl?.Close();
				previewPaneImageManager?.Dispose();

				if (storageDocsGrid.CurrentElement != lastSelectedItemForErrorMessage)
				{
					if (ex is ExternalStorageException external)
					{
						Globals.Message.ShowError(external.UnableToAccessStorageFriendlyMessage);
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("2a356b88-8831-4b7d-8323-10007041b7b4", "An image could not be opened due to unsupported format or corrupted file.\r\nError message: {0}", ex.Message));
					}

					lastSelectedItemForErrorMessage = storageDocsGrid.CurrentElement;
				}
			}
			finally
			{
				previewPanel.ResumeLayout(false);
			}
		}

		StorageDocsBase lastSelectedItemForErrorMessage;

		void okButton_Click(object sender, EventArgs e)
			=> Close();

		void CleanUpManager(ImageManager manager)
		{
			if (manager != null)
			{
				manager.ImageDialogClose -= new EventHandler(Manager_ImageDialogClose);
				manager.Close();
				manager.Dispose();
			}
		}

		void Manager_ImageDialogClose(object sender, EventArgs e)
			=> CleanUpManager((ImageManager)sender);

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			CleanUpManager(previewPaneImageManager);
		}
	}
}
