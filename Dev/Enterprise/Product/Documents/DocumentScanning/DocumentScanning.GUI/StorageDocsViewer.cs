using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public class StorageDocsViewer : IStorageDocsViewer
	{
		public StorageDocsViewer()
		{
			disposables = new ConcurrentBag<IDisposable>();
		}

		readonly ConcurrentBag<IDisposable> disposables;

		public void Initialise(Control parentControl, IEDocsPlugIn plugIn)
		{
			this.parentControl = parentControl ?? throw new ArgumentNullException(nameof(parentControl), @"StorageDocsViewer requires a parent control to be specified");
			this.plugIn = plugIn as eDocsPlugIn;
		}

		Control parentControl;

#if DEBUG
		internal
#endif
		eDocsPlugIn plugIn;

		ZForm parentForm;

		private protected Thread threadForViewingFile;

		public ZForm ParentForm
		{
			get
			{
				if (parentForm == null)
				{
					parentForm = (ZForm)parentControl.FindForm();

					if (parentForm != null)
					{
						parentForm.Closed += OnFormClosed;
					}
				}

				return parentForm;
			}
		}

		#region View

		public void View(IeDocBase objectToView, bool readOnly)
		{
#if !WINZOR
			if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly && !RemoteDesktopServices.Server.RemoteFile.IsSupported)
			{
				Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);

				return;
			}
#endif

			if (objectToView is StorageDocsBase eDocsBase)
			{
				try
				{
					if (SystemDataRegistry.Instance.EDocsStorageProvider.Value == Core.Constants.EDocsStorageProviders.Code.DB && eDocsBase.SC_ImageData.Length == 0)
					{
						var errorMessageBuilder = new StringBuilder();
						errorMessageBuilder.AppendLine(Res.GetString("6757F69E-5C51-423B-97A7-50C9109DFF88", "This eDoc has a file size of 0B (bytes) and cannot be opened. eDocs Storage is currently set to SQL Server DocManager database."));
						errorMessageBuilder.AppendLine();
						errorMessageBuilder.Append(Res.GetString("36D3D070-CCFE-4279-A515-FA3259857254", "Please contact your System Administrator to confirm if the file was previously stored using another eDocs Storage configuration option, such as S3 compatible storage. To verify this, review the Change Log in the Registry for System > DocManager > eDocs Storage."));

						Globals.Message.ShowError(errorMessageBuilder.ToString());

						return;
					}

					switch (objectToView)
					{
						case StorageFile file:
							ViewFile(file, readOnly);
							break;
						case StorageDocs image:
							ViewImage(image, readOnly);
							break;
					}

					objectToView.NotifyReadByUser();
				}
				catch (ExternalStorageException ex)
				{
					Globals.Message.ShowError(ex.UnableToAccessStorageFriendlyMessage);
				}
				catch (VirusDetectedException ex)
				{
					Globals.Message.ShowError(ex.VirusDetectedFriendlyMessage);
				}
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "We need to instantiate a factory by using a getter")]
		void ViewFile(StorageFile file, bool readOnly)
		{
			if (file != null)
			{
				file.ReadOnly = readOnly;
				var lockObject = file.Lock();

				if (lockObject != null)
				{
					var form = parentControl.FindForm();
					if (form != null)
					{
						form.UseWaitCursor = true;
					}

					file.LaunchProcessThrowWin32Exception += File_NoProgramAssociation;
					file.RemoteDesktopConnectionError += File_RemoteDesktopConnectionError;
					var currentContext = SynchronizationContext.Current ?? throw new InvalidOperationException("SynchronizationContext.Current should not be null");

					//WI00211331 - Normally this is already instantiated on the main thread, but for some forms (like Customer Service (Legacy)) it is not.
					//If we don't do this, the secondary thread will, and this will lead to an issue later.
					//Secondary thread does not actually use rowfactory/etc of this factory, so this is safe to do.
					_ = file.MasterFactory.FactoryForEverythingExceptEDocs;

					threadForViewingFile = new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							SynchronizationContext.SetSynchronizationContext(currentContext);
							try
							{
								var openFile = file.OpenForEdit();

								currentContext.Post(delegate
								{
									if (openFile != null)
									{
										if (!parentControl.IsDisposed && !parentControl.Disposing)
										{
											disposables.Add(openFile);
										}
										else
										{
											openFile.Dispose();
										}
									}
								}, null);
							}
							catch (IOException e)
							{
#if DEBUG
								file.ViewFileExceptionExpose(e);
#endif
								File_IOException(e, EventArgs.Empty);
							}
							catch (ExternalStorageException ex)
							{
								Globals.Message.ShowError(ex.UnableToAccessStorageFriendlyMessage);
							}
							catch (VirusDetectedException ex)
							{
								Globals.Message.ShowError(ex.VirusDetectedFriendlyMessage);
							}
							catch (Exception ex) when (ex is ZBlobReadException || ex is SqlStreamReaderRowNotFoundException)
							{
								Globals.Message.ShowWarning(Res.GetString("5E88C60A-0577-4CC8-A332-CB0182FC1E91", "Whilst you were working another user has deleted some information you are attempting to see. Please close this form and retry your action."));
							}

							file.LaunchProcessThrowWin32Exception -= File_NoProgramAssociation;
							file.RemoteDesktopConnectionError -= File_RemoteDesktopConnectionError;
#if WINZOR
							if (form != null && !form.Disposing && !form.IsDisposed)
							{
								form.Invoke(() => form.UseWaitCursor = false);
							}
#else
							form.UseWaitCursor = false;
#endif
							lockObject.Dispose();
						}
					});
					threadForViewingFile.Start();
				}
			}
		}

		void ViewImage(StorageDocs doc, bool readOnly)
		{
			if (doc != null)
			{
				if (SystemDataRegistry.Instance.UseDefaultWindowsImageViewer.Value)
				{
					doc.ReadOnly = true;
					doc.WasOpenInExternalEditor = true;
					try
					{
						disposables.Add(doc.OpenForEdit());
					}
					catch (IOException e)
					{
						File_IOException(e, EventArgs.Empty);
					}
				}
				else if (plugIn != null || readOnly)
				{
					OpenImage(doc, readOnly);
				}
			}
		}

		void File_NoProgramAssociation(StorageDocsBase storageFile, FilenameEventArgs e)
		{
			if (parentControl.Disposing || !parentControl.IsHandleCreated)
			{
				return;
			}

			if (parentControl.InvokeRequired)
			{
				parentControl.BeginInvoke(new FilenameEventHandler(File_NoProgramAssociation), storageFile, e);
				return;
			}

			FileSaveToOpenForm.ShowDialog(stream =>
			{
				try
				{
					using (stream)
					{
						storageFile.SaveToStream(stream);
					}
				}
				catch (ExternalStorageException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
				catch (VirusDetectedException ex)
				{
					Globals.Message.ShowError(ex.VirusDetectedFriendlyMessage);
				}
			}, storageFile.SC_FileNameWithExtension);
		}

		void File_RemoteDesktopConnectionError(object sender, EventArgs e)
		{
			if (parentControl.Disposing || !parentControl.IsHandleCreated)
			{
				return;
			}

			if (parentControl.InvokeRequired)
			{
				parentControl.BeginInvoke(new EventHandler(File_RemoteDesktopConnectionError), sender, e);
				return;
			}

			Globals.Message.ShowError(Res.GetString("33f50a28-103e-440e-9e2e-d06e9ad1b5e5", "Failed to open file due to an interruption in the Remote Desktop Services connection. Restart {0} and try again.", Core.Constants.ProductName));
		}

		void File_IOException(object sender, EventArgs e)
		{
			if (parentControl.Disposing || !parentControl.IsHandleCreated)
			{
				return;
			}

			if (parentControl.InvokeRequired)
			{
				parentControl.BeginInvoke(new EventHandler(File_IOException), sender, e);
				return;
			}

			var exception = (IOException)sender;
			var message = exception.Message;

			if (exception is PathTooLongException && CargoWise.IO.Temp.TempPath.Length > 150)
			{
				message += System.Environment.NewLine + Res.GetString("4272227f-148f-469e-9f19-9b21642bc8f6", "Note: Temporary directory path is: {0}", CargoWise.IO.Temp.TempPath);
			}

			Globals.Message.ShowError(message);
		}

#endregion

		#region GUI Setup

		public void UpdateGraphicalDisplayFile(ZGuid documentPK, string newImagePath)
		{
			if (GraphicDisplayChildForms.TryGetValue(documentPK, out var manager))
			{
				File.Copy(newImagePath, manager.ManagedImageFilename, true);
				manager.ReloadImage();
			}
		}

		Dictionary<ZGuid, ImageManager> graphicDisplayChildForms;
		protected Dictionary<ZGuid, ImageManager> GraphicDisplayChildForms
		{
			get
			{
				return graphicDisplayChildForms ?? (graphicDisplayChildForms = new Dictionary<ZGuid, ImageManager>());
			}
		}

		#endregion

		#region Image Displays

		protected void OpenImage(StorageDocsBase targetImage, bool readOnly)
		{
			if (targetImage.IsImageFile)
			{
				if (GraphicDisplayChildForms.ContainsKey(targetImage.PK))
				{
					var manager = GraphicDisplayChildForms[targetImage.PK];
					manager.Activate();
				}
				else
				{
					var displayForm = new GraphicalDisplayForm(readOnly || targetImage.SC_IsSystemGenerated);
					string filename = null;
					ImageManager manager = null;

					try
					{
						filename = targetImage.SaveToTempFile();
						manager = new ImageManager(filename, targetImage, displayForm);
						manager.ImageDialogClose += Manager_ImageDialogClose;
						manager.Tag = targetImage.PK;
						GraphicDisplayChildForms.Add(targetImage.PK, manager);

						if (!readOnly)
						{
							manager.ManagedFileChanged += Manager_ManagedFileChanged;
							manager.DocumentEmpty += Manager_DocumentEmpty;
							manager.MoveToNewDocument += Manager_MoveToNewDocument;
						}

						displayForm.Show();
						displayForm.BringToFront();

						Cursor.Current = Cursors.Default;
					}
					catch (Exception ex)
					{
						if (!string.IsNullOrEmpty(filename))
						{
							TempFile.Delete(filename, false);
						}

						manager?.Dispose();
						displayForm?.Dispose();

						if (ex is CorruptedDocumentException)
						{
							Globals.Message.ShowError(Res.GetString("5219933f-730c-4034-aad6-b26ac7d2a82e", "An image could not be opened due to unsupported format or corrupted file.\r\nError message: {0}", ex.Message));
						}
						else
						{
							throw;
						}
					}
				}
			}
		}

		void Manager_ManagedFileChanged(object sender, DocumentChangedEventArgs e)
		{
			var manager = (ImageManager)sender;
			plugIn.UpdateDocumentImage((ZGuid)manager.Tag, manager.ManagedImageFilename, e);
		}

		void Manager_DocumentEmpty(object sender, EventArgs e)
		{
			var manager = (ImageManager)sender;
			plugIn.DeleteDocument((ZGuid)manager.Tag);
			manager.Close();
		}

		void Manager_MoveToNewDocument(object sender, MoveToNewDocumentEventArgs e)
		{
			((IDragDropSupport)plugIn).Add(e.SerializableEDocs);
		}

		void CleanUpManager(ImageManager manager)
		{
			manager.ImageDialogClose -= Manager_ImageDialogClose;
			manager.Close();
			manager.Dispose();
		}

		void Manager_ImageDialogClose(object sender, EventArgs e)
		{
			CleanUpManager((ImageManager)sender);
			GraphicDisplayChildForms.Remove((ZGuid)((ImageManager)sender).Tag);
		}

		public void NotifyDocumentDeleted(ZGuid documentPk)
		{
			if (GraphicDisplayChildForms.TryGetValue(documentPk, out var manager))
			{
				manager.Close();
			}
		}

		#endregion

		#region Dispose

		void OnFormClosed(object sender, EventArgs e)
		{
			CleanUpChildGraphicalForms();
		}

		bool hasCleanedUpChildGraphicalForms;
		public void CleanUpChildGraphicalForms()
		{
			if ((parentControl.Site == null || !parentControl.Site.DesignMode) && !hasCleanedUpChildGraphicalForms)
			{
				IDictionaryEnumerator managerEnumerator = GraphicDisplayChildForms.GetEnumerator();

				while (managerEnumerator.MoveNext())
				{
					CleanUpManager((ImageManager)managerEnumerator.Value);
				}

				GraphicDisplayChildForms.Clear();

				if (parentForm != null)
				{
					parentForm.Closed -= OnFormClosed;
					parentForm = null;
				}

				hasCleanedUpChildGraphicalForms = true;
			}
		}

		#endregion

		public void Dispose()
		{
			//empty the bag - prevent memory leak
			while (disposables.TryTake(out var dummy))
			{
				dummy.Dispose();
			}

			CleanUpChildGraphicalForms();
		}
	}
}
